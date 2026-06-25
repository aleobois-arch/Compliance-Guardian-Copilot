import type { ContractAnalysisResult, ContractSummary, SubmitContractRequest } from './types';
import { msalInstance, loginRequest } from './authConfig';

// Fix: BASE_URL aligned with Vite proxy (/portal -> http://localhost:5000)
const BASE_URL = '/portal';

/**
 * Fix: Replace localStorage token stub with proper MSAL token acquisition.
 * Acquires a token silently first; falls back to redirect if needed.
 */
async function getAuthToken(): Promise<string> {
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length === 0) {
    // No active account - trigger redirect login
    await msalInstance.loginRedirect(loginRequest);
    return '';
  }

  try {
    const response = await msalInstance.acquireTokenSilent({
      ...loginRequest,
      account: accounts[0],
    });
    return response.accessToken;
  } catch {
    // Silent acquisition failed - fall back to redirect
    await msalInstance.acquireTokenRedirect(loginRequest);
    return '';
  }
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
  return apiFetch<ContractAnalysisResult>('/contracts/submit', {
    method: 'POST',
    body: JSON.stringify(req),
  });
}

export async function getContracts(): Promise<ContractSummary[]> {
  return apiFetch<ContractSummary[]>('/contracts');
}

export async function getContractById(id: string): Promise<ContractAnalysisResult> {
  return apiFetch<ContractAnalysisResult>(`/contracts/${id}`);
}
