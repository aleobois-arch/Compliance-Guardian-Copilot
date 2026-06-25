import React, { useState } from "react";
import ReactDOM from "react-dom/client";

// === Types ===
interface PortalContractUploadRequest {
  fileName: string;
  base64Content: string;
  businessOwner: string;
  department: string;
}
interface RiskFinding {
  clauseReference: string;
  description: string;
  level: string;
  recommendation: string;
}
interface PortalContractResult {
  contractId: string;
  summary: string;
  findings: RiskFinding[];
}
interface PortalContractSummary {
  contractId: string;
  fileName: string;
  businessOwner: string;
  department: string;
  highestRiskLevel: string;
}

// === API ===
async function submitContract(req: PortalContractUploadRequest): Promise<PortalContractResult> {
  const res = await fetch("/portal/contracts/submit", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(req),
  });
  if (!res.ok) throw new Error("Erreur analyse.");
  return res.json();
}
async function listContracts(): Promise<PortalContractSummary[]> {
  const res = await fetch("/portal/contracts");
  if (!res.ok) throw new Error("Erreur liste.");
  return res.json();
}
async function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve((reader.result as string).split(",")[1]);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

// === Upload Page ===
function ContractUploadPage() {
  const [file, setFile] = useState<File | null>(null);
  const [owner, setOwner] = useState("");
  const [dept, setDept] = useState("");
  const [result, setResult] = useState<PortalContractResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async () => {
    if (!file) return;
    setLoading(true); setError("");
    try {
      const base64 = await fileToBase64(file);
      const res = await submitContract({ fileName: file.name, base64Content: base64, businessOwner: owner, department: dept });
      setResult(res);
    } catch (e: any) { setError(e.message); }
    finally { setLoading(false); }
  };

  const riskColor = (level: string) => ({ Critical: "#dc2626", High: "#ea580c", Medium: "#ca8a04", Low: "#16a34a" }[level] || "#6b7280");

  return (
    <div style={{ padding: "2rem", maxWidth: 800, margin: "0 auto" }}>
      <h2>Deposer un contrat pour analyse</h2>
      <div style={{ display: "flex", gap: 12, flexWrap: "wrap", marginBottom: 16 }}>
        <input type="file" accept=".pdf,.doc,.docx" onChange={e => setFile(e.target.files?.[0] ?? null)} />
        <input placeholder="Responsable" value={owner} onChange={e => setOwner(e.target.value)} style={{ padding: "6px 10px", border: "1px solid #ccc", borderRadius: 4 }} />
        <input placeholder="Departement" value={dept} onChange={e => setDept(e.target.value)} style={{ padding: "6px 10px", border: "1px solid #ccc", borderRadius: 4 }} />
        <button onClick={handleSubmit} disabled={!file || loading} style={{ padding: "8px 16px", background: "#2563eb", color: "white", border: "none", borderRadius: 4, cursor: "pointer" }}>
          {loading ? "Analyse..." : "Analyser"}
        </button>
      </div>
      {error && <p style={{ color: "red" }}>{error}</p>}
      {result && (
        <div style={{ marginTop: 24 }}>
          <h3>Resume</h3>
          <p style={{ background: "#f3f4f6", padding: 12, borderRadius: 4 }}>{result.summary}</p>
          <h3>Risques identifies ({result.findings.length})</h3>
          {result.findings.map((f, i) => (
            <div key={i} style={{ borderLeft: `4px solid ${riskColor(f.level)}`, padding: "10px 16px", marginBottom: 8, background: "#fafafa", borderRadius: 4 }}>
              <strong>[{f.level}] {f.clauseReference}</strong><br />
              <span>{f.description}</span><br />
              <em style={{ color: "#374151" }}>Recommandation : {f.recommendation}</em>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// === List Page ===
function ContractsListPage() {
  const [contracts, setContracts] = useState<PortalContractSummary[]>([]);
  const [loading, setLoading] = useState(true);

  React.useEffect(() => {
    listContracts().then(setContracts).catch(console.error).finally(() => setLoading(false));
  }, []);

  const riskBadge = (level: string) => {
    const colors: Record<string, string> = { Critical: "#dc2626", High: "#ea580c", Medium: "#ca8a04", Low: "#16a34a" };
    return <span style={{ background: colors[level] || "#6b7280", color: "white", padding: "2px 8px", borderRadius: 12, fontSize: 12 }}>{level}</span>;
  };

  if (loading) return <p style={{ padding: 24 }}>Chargement...</p>;

  return (
    <div style={{ padding: "2rem", maxWidth: 900, margin: "0 auto" }}>
      <h2>Contrats analyses</h2>
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f3f4f6" }}>
            {["Fichier", "Responsable", "Departement", "Risque max"].map(h => (
              <th key={h} style={{ textAlign: "left", padding: "10px 12px", borderBottom: "2px solid #e5e7eb" }}>{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {contracts.map(c => (
            <tr key={c.contractId} style={{ borderBottom: "1px solid #e5e7eb" }}>
              <td style={{ padding: "10px 12px" }}>{c.fileName}</td>
              <td style={{ padding: "10px 12px" }}>{c.businessOwner}</td>
              <td style={{ padding: "10px 12px" }}>{c.department}</td>
              <td style={{ padding: "10px 12px" }}>{riskBadge(c.highestRiskLevel)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {contracts.length === 0 && <p style={{ color: "#6b7280", marginTop: 16 }}>Aucun contrat analyse pour l'instant.</p>}
    </div>
  );
}

// === App ===
function App() {
  const [view, setView] = useState<"upload" | "list">("upload");
  const navStyle = (active: boolean): React.CSSProperties => ({
    padding: "10px 20px", cursor: "pointer", border: "none",
    background: active ? "#2563eb" : "transparent",
    color: active ? "white" : "#374151",
    fontWeight: active ? 600 : 400,
    borderRadius: 4
  });
  return (
    <div style={{ fontFamily: "system-ui, sans-serif", minHeight: "100vh", background: "#fff" }}>
      <header style={{ background: "#1e40af", color: "white", padding: "0 2rem", display: "flex", alignItems: "center", gap: 24 }}>
        <h1 style={{ margin: "16px 0", fontSize: 20 }}>Compliance Guardian Portal</h1>
        <nav>
          <button onClick={() => setView("upload")} style={navStyle(view === "upload")}>Deposer un contrat</button>
          <button onClick={() => setView("list")} style={navStyle(view === "list")}>Contrats analyses</button>
        </nav>
      </header>
      <main>{view === "upload" ? <ContractUploadPage /> : <ContractsListPage />}</main>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")!).render(<React.StrictMode><App /></React.StrictMode>);
