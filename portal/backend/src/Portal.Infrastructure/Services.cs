using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
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
}

// === Service ===
public class PortalContractService : IPortalContractService
{
    private readonly HttpClient _coreClient;
    private readonly PortalDbContext _db;

    public PortalContractService(HttpClient coreClient, PortalDbContext db)
    {
        _coreClient = coreClient;
        _db = db;
    }

    public async Task<PortalContractResult> SubmitAndAnalyzeAsync(
        PortalContractUploadRequest request)
    {
        var contractId = Guid.NewGuid().ToString();
        var coreRequest = new
        {
            id = contractId,
            title = request.FileName,
            rawText = DecodeDocument(request.Base64Content),
            sourceSystem = "Portal"
        };

        PortalContractResult portalResult;
        try
        {
            var response = await _coreClient.PostAsJsonAsync("/contracts/analyze", coreRequest);
            response.EnsureSuccessStatusCode();
            var coreResult = await response.Content.ReadFromJsonAsync<CoreContractAnalysisResult>();
            portalResult = MapCoreResult(coreResult!);
        }
        catch
        {
            // Fallback si l'API principale n'est pas encore disponible
            portalResult = new PortalContractResult
            {
                ContractId = contractId,
                Summary = "Analyse en attente - API principale non disponible.",
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
            CreatedAt = DateTime.UtcNow
        };

        _db.Contracts.Add(entity);
        await _db.SaveChangesAsync();
        return portalResult;
    }

    public async Task<PortalContractResult?> GetAnalysisAsync(string contractId)
    {
        var entity = await _db.Contracts
            .FirstOrDefaultAsync(c => c.ContractId == contractId);
        if (entity is null) return null;
        return new PortalContractResult
        {
            ContractId = entity.ContractId,
            Summary = entity.Summary,
            Findings = new List<RiskFinding>()
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

    private static PortalContractResult MapCoreResult(CoreContractAnalysisResult r)
        => new()
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

    private static string DecodeDocument(string base64Content)
        => "Texte extrait (TODO: Azure Cognitive Services).";

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
