# LandEase
# LandEase — Migration Support Platform

> A full-stack migration support ecosystem connecting newly arrived migrants with verified local helpers and agencies. Built as an intensive solo internship portfolio project demonstrating Clean Architecture, AI integration, real-time systems, and cloud deployment.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?style=flat-square&logo=react)
![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?style=flat-square&logo=mysql)
![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4?style=flat-square&logo=microsoftazure)
![GitHub Actions](https://img.shields.io/badge/CI%2FCD-GitHub_Actions-2088FF?style=flat-square&logo=githubactions)

---


## Overview

LandEase is a **migration support platform** that solves a real problem: newly arrived migrants often struggle to find trusted local services, navigate their new environment, and communicate effectively. The platform provides:

- A **KYC-verified marketplace** where migrants can browse and book settlement services
- An **AI-powered chat assistant** that gives context-aware migration guidance using Google Gemini
- A **real-time SOS emergency system** with GPS location sharing via SignalR
- A **community system** connecting migrants traveling the same corridors
- **AI place recognition** — point your camera at any landmark and get instant information
- A **voice-to-voice translator** supporting 16 languages for daily situations
- A **fraud detection engine** combining rule-based checks and AI content moderation

This project was built solo over 12 weeks following Agile sprint methodology, demonstrating the full software engineering lifecycle from architecture design to cloud deployment.


## Features

### Authentication & User Management
- JWT Bearer token authentication with role-based authorization
- Three user roles: **Migrant**, **Helper**, **Agency**
- BCrypt password hashing
- User profile with migration status tracking

### KYC Verification Module
- Multipart document upload (Government ID, Selfie, Address Proof)
- Azure Blob Storage integration with private container access
- Agency review workflow (Approve / Reject with reason)
- Gmail SMTP email notifications on status change
- KYC-gated features — only verified helpers can post services

### Services Marketplace
- Full CRUD for service listings with soft delete
- Category filtering (Accommodation, Transport, Legal, Jobs, Education, Food, Banking, City Orientation)
- Country-based filtering, price filtering, full-text search
- Pagination on all list endpoints
- Provider profile with live average rating display

### Booking Lifecycle
- Full status machine: `Requested → Accepted → InProgress → Completed → Cancelled`
- Business rules enforced: only Migrants book, only Providers accept/complete
- Duplicate booking prevention
- Incoming bookings dashboard for Helpers

### Reviews & Ratings
- Review submission gated to completed bookings only
- One review per booking enforcement
- Automatic provider average rating recalculation after every review

### Community System
- 20 pre-seeded migration corridor communities (Sri Lanka→Australia, India→UK, etc.)
- Join / leave communities
- Community post feed with member-only posting
- Verified badge display on KYC-verified members
- Pagination on members and posts

### SOS Emergency System
- One-tap SOS trigger with real-time GPS coordinates
- Event types: Medical, Safety, Legal, Lost, Other
- SignalR hub broadcasts alerts to all helpers in the same destination country
- Acknowledge and resolve workflow
- Live map showing user location and active SOS events (Leaflet.js)

### AI Chat Assistant (Gemini)
- Context-aware system prompt built from user profile (origin, destination, migration status)
- Full conversation history persistence with session management
- Last 10 messages sent as context for continuity
- Paginated chat history retrieval
- In-memory rate limiting (10 requests/minute per user)
- Clear history endpoint

### AI Recommendations
- Rule-based scoring engine:
  - +30 points for country match
  - +20 points for category matching migration status
  - -10 points for recently booked categories
  - +10 points per star above 3.0 rating
- Gemini API re-ranking of top 20 services
- Personalized explanation endpoint per service

### Fraud Detection
- Rule-based immediate flags:
  - New user (< 24 hours) posting a service
  - Service price 3x above category average
  - User receiving 3+ one-star reviews in 7 days
- AI content moderation via Gemini for review text
- Rule-based checks for phone numbers and external links in reviews
- Admin dashboard for Agency to view and resolve active flags

### AI Place Recognition *(Unique Feature)*
- Live camera capture via browser `getUserMedia` API
- Image sent to Gemini Vision API as base64
- Returns: place name, country, city, description, history, famous for, travel tips
- Works on any device with a camera
- File upload fallback for non-camera devices

### Voice-to-Voice Translator *(Unique Feature)*
- 16 languages supported including Sinhala, Tamil, Hindi, Arabic, Filipino
- Hold-to-speak recording via Web Speech API
- Gemini AI translation via backend
- Text-to-Speech playback of translated text
- Quick phrases library for common migrant situations (emergency, transport, housing)
- Translation history with one-click replay

---

## Tech Stack

### Backend
| Category | Technology |
|----------|-----------|
| Language | C# 12.0 |
| Framework | ASP.NET Core Web API (.NET 8) |
| Database | MySQL 8.0 |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net |
| Validation | FluentValidation 11 |
| Real-time | ASP.NET Core SignalR |
| File Storage | Azure Blob Storage |
| Email | MailKit (Gmail SMTP) |
| AI | Google Gemini 2.5 Flash API |
| Testing | xUnit  |
| API Docs | Swagger  |
| CI/CD | GitHub Actions |
| Hosting | Azure App Service (Linux) |
| Database Host | Azure Database for MySQL Flexible Server |

### Frontend
| Category | Technology |
|----------|-----------|
| Framework | React 18 + Vite |
| Styling | Tailwind CSS |
| Routing | React Router v6 |
| HTTP Client | Axios |
| Notifications | React Hot Toast |
| Icons | Lucide React |
| Maps | Leaflet.js |
| Speech | Web Speech API (browser native) |

---

## Architecture

LandEase uses **Clean Architecture** (4-layer pattern). Dependencies flow strictly inward — the Domain layer knows nothing about infrastructure or the web framework.

```
┌─────────────────────────────────────────────┐
│                  LandEase.API               │  ← Controllers, Middleware, Program.cs
│         (depends on Application only)       │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│             LandEase.Infrastructure          │  ← EF Core, MySQL, Azure, Gemini, Email
│      (depends on Application + Domain)       │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│             LandEase.Application             │  ← DTOs, Interfaces, Validators, Services
│              (depends on Domain only)        │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│               LandEase.Domain                │  ← Entities, Enums (no dependencies)
└─────────────────────────────────────────────┘
```

### Key Architectural Decisions

**Why Clean Architecture?**
Business logic in the Application layer is completely independent of the database, web framework, and external services. This makes every service class fully unit-testable with an in-memory database.

**Why Interface Injection?**
Every service is registered behind an interface (`IKycService`, `IBookingService`, etc.), allowing Moq to mock dependencies in tests without hitting real databases or external APIs.

**Why Soft Deletes?**
Service listings use `IsActive = false` rather than hard deletes to preserve booking and review history integrity.

**Why SignalR Groups?**
SOS alerts are broadcast to groups named by `destinationCountry`, so only helpers in the relevant country receive alerts — not all connected users.

---

## Database Schema

### Core Tables

| Table | Purpose | Key Relationships |
|-------|---------|------------------|
| `Users` | All platform users with role and KYC status | Base entity |
| `KycRecords` | KYC submission documents and review status | FK → Users (cascade) |
| `ServiceListings` | Helper service postings with category and pricing | FK → Users (Provider) |
| `Bookings` | Service booking lifecycle tracking | FK → ServiceListings, Users (Migrant) |
| `Reviews` | Post-completion ratings and comments | FK → Bookings (unique), Users (Reviewer) |
| `Communities` | Migration corridor communities (20 seeded) | — |
| `CommunityMembers` | User community membership junction | FK → Communities, Users |
| `CommunityPosts` | Community feed posts | FK → Communities, Users (Author) |
| `SosEvents` | Emergency event records with GPS coordinates | FK → Users |
| `SosAlerts` | Per-helper notification records | FK → SosEvents, Users |
| `ChatHistories` | Gemini AI conversation persistence | FK → Users |
| `FraudFlags` | Rule-based and AI fraud detection flags | FK → Users |

### Important Constraints
- `Reviews.BookingId` — unique index (one review per booking)
- `CommunityMembers(CommunityId, UserId)` — composite unique index
- `Bookings.MigrantId` — `NoAction` on delete (prevents MySQL cascade cycles)
- `Reviews.ReviewerId` — `NoAction` on delete (same reason)

---

## API Reference

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | Public | Register new user |
| POST | `/api/auth/login` | Public | Login, returns JWT |
| GET | `/api/auth/profile` | Any | Get own profile |
| PUT | `/api/auth/profile` | Any | Update profile |

### KYC
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/kyc/submit` | Migrant/Helper | Submit KYC documents |
| GET | `/api/kyc/status` | Any | Check own KYC status |
| GET | `/api/kyc/pending` | Agency | List pending submissions |
| PUT | `/api/kyc/{id}/review` | Agency | Approve or reject |

### Services
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/services` | Public | Browse with filters + pagination |
| GET | `/api/services/{id}` | Public | Service detail |
| GET | `/api/services/provider/{id}` | Public | Provider's listings |
| POST | `/api/services` | KYC-verified Helper | Create listing |
| PUT | `/api/services/{id}` | Owner | Update listing |
| DELETE | `/api/services/{id}` | Owner/Agency | Soft delete |

### Bookings
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/bookings` | Migrant | Create booking |
| GET | `/api/bookings/mine` | Any | My bookings |
| GET | `/api/bookings/incoming` | Helper | Incoming bookings |
| GET | `/api/bookings/{id}` | Owner/Provider | Booking detail |
| PUT | `/api/bookings/{id}/status` | Provider/Migrant | Update status |

### Reviews
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/reviews` | Migrant | Submit review |
| GET | `/api/reviews/provider/{id}` | Public | Provider reviews |

### Community
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/community` | Public | List communities |
| GET | `/api/community/{id}` | Public | Community detail |
| POST | `/api/community/{id}/join` | Any | Join community |
| POST | `/api/community/{id}/leave` | Any | Leave community |
| GET | `/api/community/{id}/members` | Public | Member list |
| GET | `/api/community/{id}/posts` | Public | Post feed |
| POST | `/api/community/{id}/posts` | Member | Create post |

### SOS
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/sos/trigger` | Migrant | Trigger SOS with GPS |
| GET | `/api/sos/active` | Helper/Agency | Active alerts |
| PUT | `/api/sos/{id}/acknowledge` | Helper/Agency | Acknowledge |
| PUT | `/api/sos/{id}/resolve` | Any | Mark resolved |

### AI
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/ai/chat` | Any | Send chat message |
| GET | `/api/ai/chat/history` | Any | Paginated history |
| DELETE | `/api/ai/chat/history` | Any | Clear history |
| GET | `/api/ai/recommendations` | Migrant | Personalized services |
| GET | `/api/ai/recommendations/explain/{id}` | Migrant | Why this was recommended |

### Place Recognition
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/place/recognize` | Any | Identify place from image |

### Admin
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/fraud-flags` | Agency | Active fraud flags |
| PUT | `/api/admin/fraud-flags/{id}/resolve` | Agency | Resolve flag |
| POST | `/api/admin/moderate/review` | Agency | Moderate review content |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 18+](https://nodejs.org)
- [MySQL 8.0](https://dev.mysql.com/downloads/)
- [Git](https://git-scm.com)

### Backend Setup

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/LandEase.git
cd LandEase

# Restore packages
dotnet restore

# Update appsettings.json with your local config (see Environment Variables below)

# Run database migrations
cd LandEase.API
dotnet ef database update \
  --project ../LandEase.Infrastructure/LandEase.Infrastructure.csproj \
  --startup-project LandEase.API.csproj

# Run the API
dotnet run

# Swagger UI available at:
# http://localhost:5153
```

### Frontend Setup

```bash
# Navigate to frontend
cd landease-frontend

# Install dependencies
npm install

# Start development server
npm run dev

# App available at:
# http://localhost:5173
```

---

## Environment Variables

Create `LandEase.API/appsettings.json` with the following structure:

```json
{
  "JwtSettings": {
    "Secret": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "LandEaseAPI",
    "Audience": "LandEaseClient",
    "ExpiryMinutes": "60"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Database=landease_db;User=root;Password=YOUR_PASSWORD;AllowPublicKeyRetrieval=true;SslMode=none;"
  },
  "AzureStorage": {
    "ConnectionString": "YOUR_AZURE_STORAGE_CONNECTION_STRING",
    "ContainerName": "kyc-documents"
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "SenderEmail": "YOUR_GMAIL@gmail.com",
    "SenderName": "LandEase",
    "Password": "YOUR_16_CHAR_APP_PASSWORD"
  },
  "GeminiApi": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "gemini-1.5-flash",
    "BaseUrl": "https://generativelanguage.googleapis.com/v1beta/models"
  }
}
```

>  Never commit real credentials. Add `appsettings.Development.json` and `appsettings.Production.json` to `.gitignore`.

---

## Running Tests

```bash
# Run all tests
dotnet test LandEase.Tests/LandEase.Tests.csproj --verbosity normal

# Expected output:
# Test summary: total: 31, failed: 0, succeeded: 31
```

### Test Coverage

| Test Class | Tests | What It Covers |
|------------|-------|----------------|
| `KycServiceTests` | 8 | KYC submission, approval, rejection, email notifications, duplicate prevention |
| `ServiceListingServiceTests` | 13 | CRUD, KYC guard, all filter types, pagination, authorization |
| `BookingServiceTests` | 5 | Full lifecycle, status transitions, business rule enforcement |
| `ReviewServiceTests` | 5 | Completion gate, duplicate prevention, rating recalculation |

All tests use **xUnit** with **Moq** for service mocking and **EF Core InMemory** for database isolation. Each test gets its own named in-memory database to prevent state leakage.

---

## Deployment



### CI/CD Pipeline

GitHub Actions runs automatically on every pull request and push:

- **CI** (`.github/workflows/ci.yml`) — triggers on PR to `develop` or `main`
  - Restore → Build → Test
  - PR cannot be merged if tests fail

- **CD** (`.github/workflows/cd.yml`) — triggers on push to `main`
  - Build → Publish → Deploy to Azure App Service

### Branch Strategy

```
main          ← Production. Protected. Auto-deploys to Azure.
develop       ← Integration branch. All features merge here first.
feature/*     ← Individual features. PR → develop.
hotfix/*      ← Emergency fixes. PR → main + develop.
```

---

## Project Structure

```
LandEase/
├── LandEase.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── KycController.cs
│   │   ├── ServicesController.cs
│   │   ├── BookingsController.cs
│   │   ├── ReviewsController.cs
│   │   ├── CommunityController.cs
│   │   ├── SosController.cs
│   │   ├── AiController.cs
│   │   ├── PlaceController.cs
│   │   └── AdminController.cs
│   ├── Hubs/
│   │   └── SosHub.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Models/
│   │   └── ApiResponse.cs
│   └── Program.cs
│
├── LandEase.Application/
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Kyc/
│   │   ├── Services/
│   │   ├── Bookings/
│   │   ├── Reviews/
│   │   ├── Community/
│   │   ├── Sos/
│   │   ├── Ai/
│   │   └── Admin/
│   ├── Interfaces/
│   │   ├── IKycService.cs
│   │   ├── IBlobStorageService.cs
│   │   ├── IEmailService.cs
│   │   ├── IServiceListingService.cs
│   │   ├── IBookingService.cs
│   │   ├── IReviewService.cs
│   │   ├── ICommunityService.cs
│   │   ├── ISosService.cs
│   │   ├── IAiChatService.cs
│   │   ├── IRecommendationService.cs
│   │   ├── IFraudDetectionService.cs
│   │   └── IPlaceRecognitionService.cs
│   └── Validators/
│
├── LandEase.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── KycRecord.cs
│   │   ├── ServiceListing.cs
│   │   ├── Booking.cs
│   │   ├── Review.cs
│   │   ├── Community.cs
│   │   ├── CommunityMember.cs
│   │   ├── CommunityPost.cs
│   │   ├── SosEvent.cs
│   │   ├── SosAlert.cs
│   │   ├── ChatHistory.cs
│   │   └── FraudFlag.cs
│   └── Enums/
│       ├── UserRole.cs
│       ├── KycStatus.cs
│       ├── BookingStatus.cs
│       ├── ServiceCategory.cs
│       └── SosEventType.cs
│
├── LandEase.Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── ExternalServices/
│   │   └── GeminiAiService.cs
│   ├── Migrations/
│   ├── KycService.cs
│   ├── BlobStorageService.cs
│   ├── EmailService.cs
│   ├── ServiceListingService.cs
│   ├── BookingService.cs
│   ├── ReviewService.cs
│   ├── CommunityService.cs
│   ├── SosService.cs
│   ├── AiChatService.cs
│   ├── RecommendationService.cs
│   ├── FraudDetectionService.cs
│   └── PlaceRecognitionService.cs
│
├── LandEase.Tests/
│   ├── Helpers/
│   │   ├── TestDbContextFactory.cs
│   │   └── TestDataBuilder.cs
│   └── Unit/
│       ├── KycServiceTests.cs
│       ├── ServiceListingServiceTests.cs
│       ├── BookingServiceTests.cs
│       └── ReviewServiceTests.cs
│
└── landease-frontend/
    ├── src/
    │   ├── api/
    │   │   ├── axios.js
    │   │   └── endpoints.js
    │   ├── context/
    │   │   └── AuthContext.jsx
    │   ├── components/
    │   │   └── Layout.jsx
    │   └── pages/
    │       ├── auth/
    │       ├── services/
    │       ├── bookings/
    │       ├── kyc/
    │       ├── community/
    │       ├── sos/
    │       ├── ai/
    │       ├── place/
    │       ├── translator/
    │       └── admin/
    └── package.json
```

---

## Agile Sprint Summary

| Sprint | Week | Deliverable | Status |
|--------|------|-------------|--------|
| 1 | Week 1 | Clean Architecture setup, MySQL, JWT Auth | ✅ Done |
| 2 | Week 2 | KYC module, Azure Blob Storage, Gmail SMTP, FluentValidation | ✅ Done |
| 3 | Week 3 | KYC review workflow, unit tests for KycService (8 tests) | ✅ Done |
| 4 | Week 4 | Services Marketplace, filtering, pagination, unit tests (13 tests) | ✅ Done |
| 5 | Week 5 | Bookings lifecycle, Reviews, auto rating recalculation, unit tests | ✅ Done |
| 6 | Week 6 | Community system, 20 seeded corridors, posts, member listing | ✅ Done |
| 7 | Week 7 | SOS Emergency System, SignalR hub, GPS broadcasting | ✅ Done |
| 8 | Week 8 | Gemini AI Chat, context-aware prompts, history, rate limiting | ✅ Done |
| 9 | Week 9 | AI Recommendations, Gemini re-ranking, Fraud Detection, content moderation | ✅ Done |
| 10 | Week 10 | Azure Deployment — App Service, MySQL, Blob Storage | ✅ Done |
| 11 | Week 11 | GitHub Actions CI/CD pipeline, branch protection | ✅ Done |
| 12 | Week 12 | React Frontend, Place Recognition, Voice Translator, README | ✅ Done |
