# Compliance Guardian Portal

A React + .NET 8 portal for submitting and reviewing contracts analyzed by the Compliance-Guardian-Copilot engine.

## Architecture

```
portal/
  backend/src/
    Portal.Api/          # .NET 8 Web API (JWT/Entra ID auth, EF Core)
    Portal.Domain/       # Domain models and interfaces
    Portal.Infrastructure/ # EF Core DbContext + PortalContractService
    Portal.sln           # Visual Studio solution
  frontend/
    src/
      pages/             # ContractUploadPage, ContractsListPage
      App.tsx            # Root component + navigation
      api.ts             # JWT-authenticated API client
      types.ts           # TypeScript interfaces
      main.tsx           # Vite entry point
    index.html
    package.json
    vite.config.ts
    tsconfig.json
  deployment/docker/
    Dockerfile.backend
    Dockerfile.frontend
    docker-compose.yml
  README.md
  start.sh              # Linux/Mac local dev launcher
  start.ps1             # Windows local dev launcher
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) *(for Docker mode)*
- Azure AD tenant *(for Entra ID auth — optional for local dev)*

## Quick Start (Local Dev)

### Linux / Mac
```bash
bash portal/start.sh
```

### Windows (PowerShell)
```powershell
.\portal\start.ps1
```

This automatically:
1. Installs frontend npm dependencies if missing
2. Restores .NET NuGet packages via `Portal.sln`
3. Starts the backend API on **http://localhost:5000**
4. Starts the Vite dev server on **http://localhost:3000**

## Docker Compose (All Services)

```bash
cd portal/deployment/docker
docker-compose up --build
```

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| SQL Server | localhost:1433 |

## Configuration

Update `portal/backend/src/Portal.Api/appsettings.json`:

```json
{
  "Auth": {
    "Authority": "https://login.microsoftonline.com/{YOUR_TENANT_ID}/v2.0",
    "Audience": "{YOUR_CLIENT_ID}"
  },
  "CoreApi": {
    "BaseUrl": "https://your-compliance-engine-api"
  },
  "ConnectionStrings": {
    "PortalDatabase": "Server=localhost,1433;Database=PortalDb;User=sa;Password=YourStrongPassword123!;"
  }
}
```

## EF Core Migrations

First time setup — run from repo root:

```bash
cd portal/backend/src
dotnet ef migrations add InitialCreate --project Portal.Infrastructure --startup-project Portal.Api
dotnet ef database update --project Portal.Infrastructure --startup-project Portal.Api
```

## CI/CD

GitHub Actions workflow at `.github/workflows/ci.yml` runs on every push/PR to `main`:

1. **build-backend** — restore + build + test via `Portal.sln`
2. **build-frontend** — `npm ci` + `npm run build`
3. **docker-build** — verify Docker images build for both services

## API Endpoints

All endpoints require `Authorization: Bearer <token>` with the `ComplianceUser` policy.

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/portal/contracts/submit` | Upload contract file (base64) for analysis |
| `GET` | `/portal/contracts` | List all submitted contracts |
| `GET` | `/portal/contracts/{id}` | Get contract details + risk findings |

## Risk Levels

| Level | Color | Meaning |
|-------|-------|---------|
| Low | Green | Minor clause concerns |
| Medium | Orange | Review recommended |
| High | Red-Orange | Legal review required |
| Critical | Dark Red | Immediate action needed |

## Next Steps

- [ ] Connect real Azure SQL DB and apply EF Core migrations
- [ ] Integrate SharePoint document library as upload source
- [ ] Add Teams notification on high-risk contract detection
- [ ] Extend Copilot Studio plugin to surface portal data
- [ ] Add Microsoft Purview sensitivity label support
- [ ] Add contract detail page with full risk breakdown
- [ ] Implement MSAL.js for proper Entra ID token acquisition
