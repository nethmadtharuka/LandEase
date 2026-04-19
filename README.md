# LandEase (Backend API)

ASP.NET Core API for the LandEase platform.

## Tech stack

- .NET 10, ASP.NET Core
- EF Core + MySQL (Pomelo provider)
- JWT auth + role-based authorization
- Swagger (OpenAPI)
- SignalR hub: `/hubs/sos`

## Run locally (without Docker)

```bash
dotnet restore
dotnet run --project LandEase.API
```

Swagger UI is served at `/`.

## Local dev with Docker (recommended)

From this folder:

```bash
docker compose up --build
```

- API: `http://localhost:5153`
- DB: `localhost:3306` (MySQL)

## Health checks

- `GET /health` (includes a DB check)

## Rate limiting (AI)

AI endpoints are rate-limited via middleware policy `ai`:
- `POST /api/Ai/chat`
- `POST /api/Ai/translate`

Configure limit with:
- `RateLimiting__AiEndpointsPerMinute` (default: 10)

## Azure (simple deployment)

### 1) Azure Database for MySQL (Flexible Server)

- Create server + database + user
- Put the connection string into App Service setting:
  - `ConnectionStrings__DefaultConnection`

### 2) Azure App Service (Linux, .NET 10)

- Create App Service (runtime: .NET 10)
- Set **Application settings** (environment variables):
  - `ConnectionStrings__DefaultConnection`
  - `JwtSettings__Secret`
  - `JwtSettings__Issuer`
  - `JwtSettings__Audience`
  - `AzureStorage__ConnectionString` (if using KYC uploads)
  - `AzureStorage__ContainerName`
  - `EmailSettings__Host`, `EmailSettings__Port`, `EmailSettings__SenderEmail`, `EmailSettings__SenderName`, `EmailSettings__Password`
  - `GeminiApi__ApiKey`, `GeminiApi__Model`, `GeminiApi__BaseUrl` (if using AI features)
  - `RateLimiting__AiEndpointsPerMinute`
- Turn on **Application Insights** (recommended)
- Configure **Health check path** to `/health`

### 3) Deploy

Simplest: deploy directly from GitHub (Deployment Center) or `dotnet publish` + zip deploy.

# LandEase

Project documentation moved to the repository root:

- `README.md`

## Local dev with Docker

From `LandEase/`:

```bash
docker compose up --build
```

API will be available at `http://localhost:5153` (Swagger UI at `/`).