using System.Net.Http.Json;
using System.Text.Json;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Domain;

namespace Portal.Infrastructure;

// === Interface ===
public interface IPortalContractService
{
    Task<PortalContractResult> SubmitAndAnalyzeAsync(PortalContractUploadRequest request);
    Task<PortalContractResult?> GetAnalysisAsync(string contractId);
    Task<IReadOnlyCollection<PortalContractSummary>> ListContractsAsync();
}

// === DbContext ===
public class PortalDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public PortalDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<PortalDbContext> options)
        : base(options) { }

    public Microsoft.EntityFrameworkCore.DbSet<PortalContractEntity> Contracts =>
        Set<PortalContractEntity>();

    public Microsoft.EntityFrameworkCore.DbSet<PortalRiskFindingEntity> RiskFindings =>
        Set<PortalRiskFindingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortalRiskFindingEntity>()
            .HasOne<PortalContractEntity>()
            .WithMany(c => c.Findings)
            .HasForeignKey(f => f.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// === Service ===
public class PortalContractService : IPortalContractService
{
    private readonly HttpClient _coreClient;
    private readonly PortalDbContext _db;
    private readonly ILogger<PortalContractService> _logger;
    private readonly DocumentAnalysisClient? _documentAnalysisClient;

    public PortalContractService(
        HttpClient coreClient,
        PortalDbContext db,
        ILogger<PortalContractService> logger,
        DocumentAnalysisClient? documentAnalysisClient = null)
    {
        _coreClient = coreClient;
        _db = db;
        _logger = logger;
        _documentAnalysisClient = documentAnalysisClient;
    }

    public async Task<PortalContractResult> SubmitAndAnalyzeAsync(
        PortalContractUploadRequest request)
    {
        var contractId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting analysis for contract {ContractId}, file {FileName}",
            contractId, request.FileName);

        string rawText;
        try
        {
            rawText = await DecodeDocumentAsync(request.Base64Content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Document extraction failed for {ContractId}, falling back to raw text", contractId);
            rawText = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.Base64Content));
        }

        var coreRequest = new
        {
            id = contractId,
            title = request.FileName,
            rawText,
            sourceSystem = "Portal"
        };

        PortalContractResult portalResult;
        try
        {
            var response = await _coreClient.PostAsJsonAsync("/contracts/analyze", coreRequest);
            response.EnsureSuccessStatusCode();
            var coreResult = await response.Content.ReadFromJsonAsync<CoreContractAnalysisResult>();
            portalResult = MapCoreResult(coreResult!);
            _logger.LogInformation("Core API analysis succeeded for {ContractId}", contractId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Core API unavailable for {ContractId}, using fallback result", contractId);
            portalResult = new PortalContractResult
            {
                ContractId = contractId,
                Summary = "Analysis pending - core API unavailable.",
                Findings = new List<RiskFinding>()
            };
        }

        var entity = new PortalContractEntity
        {
            ContractId = portalResult.ContractId,
            FileName = request.FileName,
            BusinessOwner = request.BusinessOwner,
            Department = request.Department,
            HighestRiskLevel = portalResult.Findings
                .Select(f => f.Level).OrderByDescending(l => l)
                .FirstOrDefault() ?? "Low",
            Summary = portalResult.Summary,
            CreatedAt = DateTime.UtcNow,
            Findings = portalResult.Findings.Select(f => new PortalRiskFindingEntity
            {
                ContractId = portalResult.ContractId,
                ClauseReference = f.ClauseReference,
                Description = f.Description,
                Level = f.Level,
                Recommendation = f.Recommendation
            }).ToList()
        };

        _db.Contracts.Add(entity);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Persisted contract {ContractId} with {FindingCount} findings",
            contractId, entity.Findings.Count);

        return portalResult;
    }

    public async Task<PortalContractResult?> GetAnalysisAsync(string contractId)
    {
        var entity = await _db.Contracts
            .Include(c => c.Findings)
            .FirstOrDefaultAsync(c => c.ContractId == contractId);

        if (entity is null)
        {
            _logger.LogWarning("Contract {ContractId} not found", contractId);
            return null;
        }

        return new PortalContractResult
        {
            ContractId = entity.ContractId,
            Summary = entity.Summary,
            Findings = entity.Findings.Select(f => new RiskFinding
            {
                ClauseReference = f.ClauseReference,
                Description = f.Description,
                Level = f.Level,
                Recommendation = f.Recommendation
            }).ToList()
        };
    }

    public async Task<IReadOnlyCollection<PortalContractSummary>> ListContractsAsync()
    {
        var entities = await _db.Contracts
            .OrderByDescending(c => c.CreatedAt).ToListAsync();

        return entities.Select(e => new PortalContractSummary
        {
            ContractId = e.ContractId,
            FileName = e.FileName,
            BusinessOwner = e.BusinessOwner,
            Department = e.Department,
            HighestRiskLevel = e.HighestRiskLevel,
            CreatedAt = e.CreatedAt
        }).ToList();
    }

    private async Task<string> DecodeDocumentAsync(string base64Content)
    {
        if (_documentAnalysisClient is null)
        {
            _logger.LogWarning("DocumentAnalysisClient not configured, falling back to base64 decode");
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Content));
        }

        var bytes = Convert.FromBase64String(base64Content);
        using var stream = new MemoryStream(bytes);

        _logger.LogInformation("Sending document to Azure Document Intelligence");
        var operation = await _documentAnalysisClient.AnalyzeDocumentAsync(
            WaitUntil.Completed, "prebuilt-read", stream);

        var result = operation.Value;
        var text = string.Join("\n", result.Pages.SelectMany(p => p.Lines).Select(l => l.Content));
        _logger.LogInformation("Extracted {CharCount} characters from document", text.Length);
        return text;
    }

    private static PortalContractResult MapCoreResult(CoreContractAnalysisResult r) => new()
    {
        ContractId = r.ContractId,
        Summary = r.Summary,
        Findings = r.Findings.Select(f => new RiskFinding
        {
            ClauseReference = f.ClauseReference,
            Description = f.Description,
            Level = f.Level,
            Recommendation = f.Recommendation
        }).ToList()
    };

    private class CoreContractAnalysisResult
    {
        public string ContractId { get; set; } = default!;
        public string Summary { get; set; } = default!;
        public List<CoreRiskFinding> Findings { get; set; } = new();
    }

    private class CoreRiskFinding
    {
        public string ClauseReference { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Level { get; set; } = default!;
        public string Recommendation { get; set; } = default!;
    }
}
