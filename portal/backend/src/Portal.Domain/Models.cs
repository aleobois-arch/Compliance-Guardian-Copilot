using System.ComponentModel.DataAnnotations;

namespace Portal.Domain;

// === Requests ===
public class PortalContractUploadRequest
{
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = default!;

    [Required]
    public string Base64Content { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    public string BusinessOwner { get; set; } = default!;

    [Required]
    [MaxLength(200)]
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

// === Database Entities ===
public class PortalContractEntity
{
    [Key]
    [MaxLength(36)]
    public string ContractId { get; set; } = default!;

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = default!;

    [MaxLength(200)]
    public string BusinessOwner { get; set; } = default!;

    [MaxLength(200)]
    public string Department { get; set; } = default!;

    [MaxLength(20)]
    public string HighestRiskLevel { get; set; } = default!;

    public string Summary { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    // Navigation property for persisted findings
    public List<PortalRiskFindingEntity> Findings { get; set; } = new();
}

public class PortalRiskFindingEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(36)]
    public string ContractId { get; set; } = default!;

    [MaxLength(500)]
    public string ClauseReference { get; set; } = default!;

    public string Description { get; set; } = default!;

    [MaxLength(20)]
    public string Level { get; set; } = default!;

    public string Recommendation { get; set; } = default!;
}
