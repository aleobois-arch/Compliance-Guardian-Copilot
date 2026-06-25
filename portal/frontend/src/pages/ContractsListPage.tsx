import { useEffect, useState } from 'react';
import { getContracts } from '../api';
import type { ContractSummary } from '../types';

const riskColor = (r: string) => ({
  Low: '#107c10', Medium: '#ff8c00', High: '#d83b01', Critical: '#a80000'
}[r] ?? '#333');

const statusBadge = (s: string) => ({
  Pending: '#605e5c', Analyzed: '#107c10', Failed: '#a80000'
}[s] ?? '#333');

export default function ContractsListPage() {
  const [contracts, setContracts] = useState<ContractSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getContracts()
      .then(setContracts)
      .catch(e => setError(e instanceof Error ? e.message : 'Unknown error'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Loading contracts...</p>;
  if (error) return <p style={{ color: 'red' }}>Error: {error}</p>;
  if (contracts.length === 0) return (
    <div>
      <h2>Contracts</h2>
      <p>No contracts submitted yet. Use the <strong>Submit Contract</strong> tab to upload one.</p>
    </div>
  );

  return (
    <div>
      <h2>Contracts ({contracts.length})</h2>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.95rem' }}>
        <thead>
          <tr style={{ background: '#f3f3f3' }}>
            <th style={th}>File</th>
            <th style={th}>Submitted</th>
            <th style={th}>Status</th>
            <th style={th}>Overall Risk</th>
            <th style={th}>Findings</th>
          </tr>
        </thead>
        <tbody>
          {contracts.map(c => (
            <tr key={c.contractId} style={{ borderBottom: '1px solid #eee' }}>
              <td style={td}>{c.fileName}</td>
              <td style={td}>{new Date(c.submittedAt).toLocaleString()}</td>
              <td style={td}>
                <span style={{ background: statusBadge(c.status), color: '#fff', padding: '2px 8px', borderRadius: 12, fontSize: '0.8rem' }}>
                  {c.status}
                </span>
              </td>
              <td style={td}>
                <strong style={{ color: riskColor(c.overallRisk) }}>{c.overallRisk}</strong>
              </td>
              <td style={td}>{c.findingsCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

const th: React.CSSProperties = {
  textAlign: 'left', padding: '0.5rem 0.75rem', fontWeight: 600, borderBottom: '2px solid #ddd'
};
const td: React.CSSProperties = {
  padding: '0.5rem 0.75rem'
};
