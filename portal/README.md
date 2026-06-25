# Compliance Guardian Portal

A React + .NET 8 portal for submitting and reviewing contracts analyzed by the Compliance-Guardian-Copilot engine.

## Architecture

```
portal/
  backend/
    Portal.Api/          # .NET 8 Web API (JWT auth, EF Core)
    Portal.Domain/       # Domain models and interfaces
    Portal.Infrastructure/ # EF Core DbContext, services
  frontend/              # React + TypeScript SPA (Vite)
  deployment/
    docker/
      Dockerfile.backend
      Dockerfile.frontend
      docker-compose.yml
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Azure AD tenant (for Entra ID auth)

## Local Development with Docker Compose

```bash
# From repo root
cd portal/deployment/docker
docker-compose up --build
```

Services:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **SQL Server**: localhost:1433

## Local Development (without Docker)

### Backend

```bash
cd portal/backend/Portal.Api
dotnet restore
dotnet run
```

### Frontend

```bash
cd portal/frontend
npm install
npm run dev
```

## Configuration

Update `portal/backend/Portal.Api/appsettings.json`:

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

## CI/CD

GitHub Actions workflow at `.github/workflows/ci.yml` runs on every push/PR to `main`:
1. Build & test .NET backend
2. Build React frontend
3. Build Docker images for both

## Key Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/portal/contracts/submit` | Upload contract for analysis |
| GET | `/portal/contracts` | List all submitted contracts |
| GET | `/portal/contracts/{id}` | Get contract details + risk findings |

All endpoints require a valid Bearer token with the `ComplianceUser` policy.

## Next Steps

- Connect real Azure SQL DB and run EF Core migrations
- Integrate SharePoint document library as upload source
- Add Teams notification on high-risk contract detection
- Extend Copilot Studio plugin to surface portal data
- Add Microsoft Purview sensitivity label support
