import { useState } from 'react';
import ContractUploadPage from './pages/ContractUploadPage';
import ContractsListPage from './pages/ContractsListPage';

type View = 'upload' | 'list';

export default function App() {
  const [view, setView] = useState<View>('upload');

  return (
    <div style={{ fontFamily: 'sans-serif', maxWidth: 900, margin: '0 auto', padding: '1rem' }}>
      <header style={{ borderBottom: '2px solid #0078d4', paddingBottom: '1rem', marginBottom: '1.5rem' }}>
        <h1 style={{ color: '#0078d4', margin: 0 }}>Compliance Guardian Portal</h1>
        <nav style={{ marginTop: '0.75rem' }}>
          <button
            onClick={() => setView('upload')}
            style={{
              marginRight: '1rem',
              padding: '0.5rem 1rem',
              background: view === 'upload' ? '#0078d4' : '#f3f3f3',
              color: view === 'upload' ? '#fff' : '#333',
              border: 'none',
              borderRadius: 4,
              cursor: 'pointer',
            }}
          >
            Submit Contract
          </button>
          <button
            onClick={() => setView('list')}
            style={{
              padding: '0.5rem 1rem',
              background: view === 'list' ? '#0078d4' : '#f3f3f3',
              color: view === 'list' ? '#fff' : '#333',
              border: 'none',
              borderRadius: 4,
              cursor: 'pointer',
            }}
          >
            View Contracts
          </button>
        </nav>
      </header>
      <main>
        {view === 'upload' ? (
          <ContractUploadPage onSubmitted={() => setView('list')} />
        ) : (
          <ContractsListPage />
        )}
      </main>
    </div>
  );
}
