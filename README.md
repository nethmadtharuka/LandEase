# LandEase API (Backend)

LandEase is a full-stack **migration support platform** built as an internship-ready portfolio project. This repository contains the **ASP.NET Core backend API**.

## Project overview

Migrants often face fragmented support: finding trusted local help, navigating immigration requirements, verifying service providers, and accessing help quickly in emergencies. LandEase brings these into one platform with **role-based workflows**, **real-time SOS alerts**, and **AI-assisted features** (chat/translation, recommendations, moderation, and recognition).

## Features

### User features (Migrant)
- **Authentication**: register, login, view profile (JWT)
- **KYC**: submit verification documents and track status
- **Services marketplace**: browse service listings and view service details
- **Bookings**: create bookings and manage booking status updates
- **Reviews**: submit reviews for service providers
- **Community**: browse communities, join/leave, view members, read/create posts
- **SOS emergency system**: trigger SOS, view active alerts, view SOS history
- **AI**:
  - AI chat assistant
  - Translation endpoint (used by voice translator UI)
  - Personalized service recommendations + explanation per service
  - Immigration profile analysis (supports multipart document upload)

### Provider features (Helper)
- **Service listings**: create/update services
- **Bookings**: view incoming bookings
- **SOS**: acknowledge SOS alerts (helper role)

### Admin / Agency features (Agency)
- **KYC review**: view pending KYC records and approve/reject
- **Trust & safety**: view/resolve fraud flags, AI-assisted review moderation
- **Services**: deactivate services (where authorized)
- **SOS**: acknowledge SOS alerts (agency role)

### Platform features
- **Role-based authorization** (Migrant/Helper/Agency)
- **Real-time updates** via **SignalR** (`/hubs/sos`)
- **Swagger UI** for API exploration
- **Health checks** (`/health`) including a DB check
- **Rate limiting** for AI endpoints via middleware policy

## Tech stack

- **Backend**: ASP.NET Core (.NET 10), C#
- **Database**: MySQL (Pomelo provider), EF Core + migrations
- **Auth**: JWT + role-based authorization
- **Realtime**: SignalR
- **Validation**: FluentValidation
- **AI / external services**: Gemini integration (via `GeminiAiService`), email + blob storage integrations
- **DevOps**: Docker + docker-compose for local environments, GitHub Actions CI

## System architecture (high level)

The backend follows a clean-ish layered approach:

- **`LandEase.API`**: controllers, middleware, auth, DI, SignalR hub mapping
- **`LandEase.Application`**: DTOs, interfaces, validators
- **`LandEase.Domain`**: entities / core domain types
- **`LandEase.Infrastructure`**: EF Core DbContext, service implementations, external integrations

Request flow:
1. Frontend calls `/api/*` endpoints (JWT attached).
2. Controllers call Application-layer interfaces.
3. Infrastructure implements interfaces using EF Core + external services.
4. SOS events also fan out via SignalR hub for real-time clients.

## Setup & installation

### Prerequisites
- .NET SDK 10.x
- MySQL 8.x (or Docker)

### Run locally (without Docker)

From this folder:

```bash
dotnet restore
dotnet run --project LandEase.API
```

- Swagger UI: `http://localhost:5153/`
- Health check: `http://localhost:5153/health`

### Run locally (Docker + MySQL) — recommended

```bash
docker compose up --build
```

- API: `http://localhost:5153/`
- MySQL: `localhost:3306`

Note: Docker build requires Docker Engine running (Docker Desktop on Windows).

## Usage

- **Developers**: use Swagger UI to explore endpoints and test JWT-protected routes.
- **Users** (via frontend): register/login, complete KYC, browse services, book helpers, post in communities, trigger SOS, and use AI chat/translation/recommendations.

## API endpoints (major)

Base path: `/api`

### Auth
- `POST /Auth/register` — register account
- `POST /Auth/login` — login and receive JWT
- `GET /Auth/profile` — current user profile (auth)

### KYC
- `POST /Kyc/submit` — submit KYC (multipart, auth)
- `GET /Kyc/status` — view my KYC status (auth)
- `GET /Kyc/pending` — list pending KYC (Agency)
- `PUT /Kyc/{id}/review` — approve/reject KYC (Agency)

### Services
- `GET /Services` — list services (filters)
- `GET /Services/{id}` — service detail
- `GET /Services/provider/{providerId}` — services by provider
- `POST /Services` — create service (Helper)
- `PUT /Services/{id}` — update service (Helper)
- `DELETE /Services/{id}` — deactivate service (Helper/Agency)

### Bookings
- `POST /Bookings` — create booking (Migrant)
- `GET /Bookings/mine` — my bookings (auth)
- `GET /Bookings/incoming` — incoming bookings (Helper)
- `GET /Bookings/{id}` — booking details (auth)
- `PUT /Bookings/{id}/status` — update booking status (auth)

### Reviews
- `POST /Reviews` — create review (Migrant)
- `GET /Reviews/provider/{providerId}` — reviews for provider

### Community
- `GET /Community` — list communities (paged)
- `GET /Community/{id}` — community detail
- `POST /Community/{id}/join` — join (auth)
- `POST /Community/{id}/leave` — leave (auth)
- `GET /Community/{id}/members` — members (paged)
- `GET /Community/{id}/posts` — posts (paged)
- `POST /Community/{id}/posts` — create post (auth)

### SOS (real-time + REST)
- `POST /Sos/trigger` — trigger SOS (Migrant)
- `GET /Sos/active` — active alerts (Helper/Agency/Migrant)
- `GET /Sos/history` — my SOS history (Migrant)
- `PUT /Sos/{id}/acknowledge` — acknowledge (Helper/Agency)
- `PUT /Sos/{id}/resolve` — resolve event (auth)
- SignalR hub: `GET /hubs/sos`

### AI
- `POST /Ai/chat` — chat (auth, rate-limited)
- `GET /Ai/chat/history` — chat history (auth)
- `DELETE /Ai/chat/history` — clear history (auth)
- `POST /Ai/translate` — translate text (auth, rate-limited)
- `GET /Ai/recommendations` — recommendations (Migrant)
- `GET /Ai/recommendations/explain/{serviceId}` — explanation (Migrant)

### Place recognition
- `POST /Place/recognize` — recognize place from base64 image (auth)

### Immigration predictor
- `POST /Immigration/analyze` — profile analysis (multipart, auth)
- `GET /Immigration/visa-types` — supported visa types (public)
- `GET /Immigration/countries` — supported countries (public)

### Admin
- `GET /Admin/fraud-flags` — active fraud flags (Agency)
- `PUT /Admin/fraud-flags/{id}/resolve` — resolve (Agency)
- `POST /Admin/moderate/review` — AI moderation (Agency)

## Configuration

Local defaults are in `LandEase.API/appsettings.json` and `LandEase.API/appsettings.example.json`.

In production (Azure App Service), configure settings via **Application settings** (environment variables), e.g.:
- `ConnectionStrings__DefaultConnection`
- `JwtSettings__Secret`
- `AzureStorage__ConnectionString`
- `GeminiApi__ApiKey`
- `RateLimiting__AiEndpointsPerMinute`

## Future improvements

- Replace local JWT storage on the frontend with a more secure session strategy (httpOnly cookies + CSRF)
- Add integration tests for key flows (auth → KYC → booking → SOS)
- Add environment-based CORS configuration (dev/staging/prod)
- Introduce distributed rate limiting (Redis) for multi-instance deployments
- Add background jobs for notifications and moderation workflows
- Improve AI quality with evaluation datasets and cost/latency monitoring

## Author / credits

- **Author**: Nethma D. Tharuka (`@nethmadtharuka`)