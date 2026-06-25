export interface RiskFinding {
  clause: string;
  riskLevel: 'Low' | 'Medium' | 'High' | 'Critical';
  description: string;
  recommendation: string;
}

export interface ContractAnalysisResult {
  contractId: string;
  fileName: string;
  submittedAt: string;
  status: 'Pending' | 'Analyzed' | 'Failed';
  overallRisk: 'Low' | 'Medium' | 'High' | 'Critical';
  findings: RiskFinding[];
  summary: string;
}

export interface ContractSummary {
  contractId: string;
  fileName: string;
  submittedAt: string;
  status: 'Pending' | 'Analyzed' | 'Failed';
  overallRisk: 'Low' | 'Medium' | 'High' | 'Critical';
  findingsCount: number;
}

export interface SubmitContractRequest {
  fileName: string;
  fileContent: string; // base64
  contentType: string;
}
