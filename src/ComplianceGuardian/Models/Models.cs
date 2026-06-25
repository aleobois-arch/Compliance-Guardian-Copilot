namespace ComplianceGuardian.Models;

public enum RiskLevel { Low, Medium, High, Critical }

public sealed record ComplianceRequest {
      public required string UserId { get; init; }
      public string? Question { get; init; }
      public string? DocumentName { get; init; }
      public string? DocumentText { get; init; }
      public DateTimeOffset SubmittedAt { get; init; } = DateTimeOffset.UtcNow;
      public bool HasDocument => !string.IsNullOrWhiteSpace(DocumentText);
}

public sealed record EnterpriseContext {
      public required string OrganizationName { get; init; }
      public IReadOnlyList<string> ApplicablePolicies { get; init; } = Array.Empty<string>();
      public IReadOnlyList<string> Jurisdictions { get; init; } = Array.Empty<string>();
      public string? BusinessUnit { get; init; }
}

public sealed record PolicyDocument {
      public required string Title { get; init; }
      public required string Source { get; init; }
      public required string Content { get; init; }
      public IReadOnlyList<string> RequiredClauses { get; init; } = Array.Empty<string>();
}

public sealed record ContractClause {
      public required string Type { get; init; }
      public required string Text { get; init; }
      public DateOnly? RelevantDate { get; init; }
}

public sealed record ContractAnalysisResult {
      public IReadOnlyList<ContractClause> Clauses { get; init; } = Array.Empty<ContractClause>();
      public IReadOnlyList<string> Obligations { get; init; } = Array.Empty<string>();
      public IReadOnlyList<DateOnly> KeyDates { get; init; } = Array.Empty<DateOnly>();
}

public sealed record RiskFinding {
      public required string Category { get; init; }
      public required RiskLevel Level { get; init; }
      public required string Description { get; init; }
      public required string BusinessImpact { get; init; }
}

public sealed record ComplianceGap {
      public required string PolicyTitle { get; init; }
      public required string MissingOrViolatedClause { get; init; }
      public required string Detail { get; init; }
      public RiskLevel Severity { get; init; }
}

public sealed record ExecutiveSummary {
      public required string Overview { get; init; }
      public IReadOnlyList<string> KeyFindings { get; init; } = Array.Empty<string>();
      public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
      public IReadOnlyList<string> ActionItems { get; init; } = Array.Empty<string>();
}

public sealed record ComplianceReport {
      public required ComplianceRequest Request { get; init; }
      public required ContractAnalysisResult ContractAnalysis { get; init; }
      public required IReadOnlyList<RiskFinding> Risks { get; init; }
      public required IReadOnlyList<ComplianceGap> Gaps { get; init; }
      public required ExecutiveSummary Summary { get; init; }
      public RiskLevel OverallRisk { get; init; }
}
