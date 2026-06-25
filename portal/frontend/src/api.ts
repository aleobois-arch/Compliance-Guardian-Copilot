import type { ContractAnalysisResult, ContractSummary, SubmitContractRequest } from './types';

const BASE_URL = '/api';

async function getAuthToken(): Promise<string> {
  // Replace with your MSAL or token acquisition logic
  return localStorage.getItem('access_token') ?? '';
}

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const token = await getAuthToken();
  const res = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      ...options?.headers,
    },
  });
  if (!res.ok) {
    const msg = await res.text();
    throw new Error(`API error ${res.status}: ${msg}`);
  }
  return res.json() as Promise<T>;
}

export async function submitContract(req: SubmitContractRequest): Promise<ContractAnalysisResult> {
  return apiFetch<ContractAnalysisResult>('/portal/contracts/submit', {
    method: 'POST',
    body: JSON.stringify(req),
  });
}

export async function getContracts(): Promise<ContractSummary[]> {
  return apiFetch<ContractSummary[]>('/portal/contracts');
}

export async function getContractById(id: string): Promise<ContractAnalysisResult> {
  return apiFetch<ContractAnalysisResult>(`/portal/contracts/${id}`);
}
