namespace Portal.Domain;

// === Requests ===
public class PortalContractUploadRequest
{
    public string FileName { get; set; } = default!;
    public string Base64Content { get; set; } = default!;
    public string BusinessOwner { get; set; } = default!;
    public string Department { get; set; } = default!;
}

// === Results ===
public class PortalContractResult
{
    public string ContractId { get; set; } = default!;
    public string Summary { get; set; } = default!;
    public List<RiskFinding> Findings { get; set; } = new();
}

public class RiskFinding
{
    public string ClauseReference { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Level { get; set; } = default!;
    public string Recommendation { get; set; } = default!;
}

// === Summary list ===
public class PortalContractSummary
{
    public string ContractId { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string BusinessOwner { get; set; } = default!;
    public string Department { get; set; } = default!;
    public string HighestRiskLevel { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

// === DB Entity ===
public class PortalContractEntity
{
    public int Id { get; set; }
    public string ContractId { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string BusinessOwner { get; set; } = default!;
    public string Department { get; set; } = default!;
    public string HighestRiskLevel { get; set; } = default!;
    public string Summary { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
