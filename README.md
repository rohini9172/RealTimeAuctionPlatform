# Real-Time Auction Platform

ASP.NET Core 9 API and React/Vite client for scheduled auctions, authenticated bidding, watchlists, notifications, and SignalR updates. SQL Server is the source of truth; create the schema with the scripts under `database/` before starting the API.

## Requirements

- .NET 9 SDK
- SQL Server 2019 or later
- Node.js and npm

## Database

Run these scripts in order against SQL Server:

1. `database/01_CreateDatabase.sql`
2. `database/02_CreateTables.sql`
3. `database/03_IndexesAndConstraints.sql`
4. `database/04_SeedData.sql`

Configure local API secrets (PowerShell example):

```powershell
dotnet user-secrets init --project Auction.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=RealTimeAuctionDb;Trusted_Connection=True;TrustServerCertificate=True" --project Auction.Api
dotnet user-secrets set "Jwt:SecretKey" "YOUR_RANDOM_SECRET_AT_LEAST_32_BYTES" --project Auction.Api
```

The same settings can be supplied with `ConnectionStrings__DefaultConnection` and `Jwt__SecretKey` environment variables. The API intentionally stops at startup if either setting is missing or the JWT key is shorter than 32 bytes. Never commit local secrets.

The SQL scripts are authoritative. After changing the schema, re-scaffold rather than adding EF migrations:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.5
dotnet ef dbcontext scaffold "$env:ConnectionStrings__DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer --project Auction.Infrastructure/Auction.Infrastructure.csproj --startup-project Auction.Api/Auction.Api.csproj --context AuctionDbContext --context-dir Persistence --output-dir Persistence/Entities --no-onconfiguring --force
```

## Run

Start the API from the repository root:

```powershell
dotnet run --project Auction.Api
```

With the HTTP launch profile, Swagger is at `http://localhost:5263/swagger`; the Hangfire dashboard is at `http://localhost:5263/hangfire` in Development and uses Hangfire's local-request restriction. Start the client in another terminal:

```powershell
cd auction.client
npm install
npm run dev
```

In development, the client uses relative API and SignalR URLs through Vite's proxy, which targets `http://localhost:5263` and avoids browser CORS preflights. Set `VITE_API_BASE_URL` when the API is hosted on a separate origin; update `auction.client/vite.config.js` if you change the API launch port.

## Example workflow

1. Register a `Seller` or `Bidder` through `POST /api/auth/register`; public registration does not permit `Admin`.
2. Log in through `POST /api/auth/login` and use the returned bearer token for protected endpoints.
3. As a Seller, create an auction with `POST /api/auctions`. Start time must be in the future and the category must exist.
4. After its start time, a different user with the Bidder role can post to `POST /api/auctions/{id}/bids`. The next accepted amount must meet the current price plus the minimum increment.
5. Clients on an auction details page join its SignalR group and receive `NewBid` after the bid transaction commits.

## Development checks

```powershell
dotnet restore RealTimeAuction.sln
dotnet build RealTimeAuction.sln
dotnet test RealTimeAuction.sln
cd auction.client
npm install
npm run build
```

The automated test project covers application service rules; SQL Server-specific transaction and rowversion behavior must be verified against SQL Server, not EF Core's in-memory provider.

## Architecture and limitations

- `Auction.Domain`: shared enums and domain exceptions.
- `Auction.Application`: DTOs, service/repository contracts, validators, and mapping.
- `Auction.Infrastructure`: scaffolded EF entities, persistence, repositories, services, and lifecycle jobs.
- `Auction.Api`: HTTP endpoints, JWT auth, SignalR, middleware, and hosting configuration.
- `auction.client`: JavaScript React application.
- `Auction.Tests`: unit tests for service-level rules.

JWTs are stored in browser local storage for this portfolio implementation; an XSS vulnerability could expose them. Production deployments should consider secure, HttpOnly cookies and appropriate CSRF protection. The app requires a reachable SQL Server database for both EF Core and Hangfire. SQL Server concurrency and multi-worker lifecycle behavior are not covered by in-memory tests.