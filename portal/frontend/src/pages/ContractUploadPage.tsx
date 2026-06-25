import { useState, useRef } from 'react';
import { submitContract } from '../api';
import type { ContractAnalysisResult } from '../types';

interface Props {
  onSubmitted: () => void;
}

const riskColor = (r: string) => ({
  Low: '#107c10', Medium: '#ff8c00', High: '#d83b01', Critical: '#a80000'
}[r] ?? '#333');

export default function ContractUploadPage({ onSubmitted }: Props) {
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<ContractAnalysisResult | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const handleSubmit = async () => {
    if (!file) return;
    setLoading(true);
    setError(null);
    try {
      const buffer = await file.arrayBuffer();
      const base64 = btoa(String.fromCharCode(...new Uint8Array(buffer)));
      const res = await submitContract({
        fileName: file.name,
        fileContent: base64,
        contentType: file.type || 'application/octet-stream',
      });
      setResult(res);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  };

  if (result) {
    return (
      <div>
        <h2>Analysis Complete</h2>
        <p><strong>Contract:</strong> {result.fileName}</p>
        <p><strong>Status:</strong> {result.status}</p>
        <p><strong>Overall Risk:</strong> <span style={{ color: riskColor(result.overallRisk), fontWeight: 'bold' }}>{result.overallRisk}</span></p>
        <p><strong>Summary:</strong> {result.summary}</p>
        <h3>Risk Findings ({result.findings.length})</h3>
        {result.findings.map((f, i) => (
          <div key={i} style={{ border: '1px solid #ddd', borderRadius: 6, padding: '0.75rem', marginBottom: '0.5rem' }}>
            <strong style={{ color: riskColor(f.riskLevel) }}>[{f.riskLevel}]</strong> {f.clause}
            <p style={{ margin: '0.25rem 0' }}>{f.description}</p>
            <em>Recommendation: {f.recommendation}</em>
          </div>
        ))}
        <button onClick={onSubmitted} style={{ marginTop: '1rem', padding: '0.5rem 1.5rem', background: '#0078d4', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          View All Contracts
        </button>
      </div>
    );
  }

  return (
    <div>
      <h2>Submit Contract for Analysis</h2>
      <p>Upload a contract file (PDF, DOCX, or TXT) to analyze compliance risks.</p>
      <div style={{ border: '2px dashed #0078d4', borderRadius: 8, padding: '2rem', textAlign: 'center', marginBottom: '1rem' }}>
        <input
          ref={inputRef}
          type="file"
          accept=".pdf,.docx,.txt"
          style={{ display: 'none' }}
          onChange={e => setFile(e.target.files?.[0] ?? null)}
        />
        <button onClick={() => inputRef.current?.click()} style={{ padding: '0.5rem 1.5rem', background: '#f3f3f3', border: '1px solid #ccc', borderRadius: 4, cursor: 'pointer' }}>
          Choose File
        </button>
        {file && <p style={{ marginTop: '0.5rem' }}>Selected: <strong>{file.name}</strong> ({Math.round(file.size / 1024)} KB)</p>}
      </div>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <button
        onClick={handleSubmit}
        disabled={!file || loading}
        style={{ padding: '0.6rem 2rem', background: file && !loading ? '#0078d4' : '#ccc', color: '#fff', border: 'none', borderRadius: 4, cursor: file && !loading ? 'pointer' : 'default', fontSize: '1rem' }}
      >
        {loading ? 'Analyzing...' : 'Submit for Analysis'}
      </button>
    </div>
  );
}
