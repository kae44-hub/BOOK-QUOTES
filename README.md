# Folio — books & the words that stay

A responsive book library and personal quote collection built with **Angular 20**, **ASP.NET Core 9**, and **PostgreSQL**. Bootstrap 5 and Font Awesome provide the UI foundation; a custom reading-inspired design adds light and dark themes.

## Try it

Create an account, sign in, and explore the shared library. Each account starts with five editable original quotes. Books are shared between authenticated users; quotes belong only to their owner.

The live deployment URL will be added after the Render deployment is verified.

## Features

- Register, sign in, restore a session on refresh, and sign out.
- Add, read, edit and delete books with title, author and publication date.
- Search by title/author and sort the library.
- Five starter quotes per account, plus private quote CRUD and search.
- Responsive navigation, labelled forms, keyboard focus, delete confirmation, loading/empty/error states and retry.
- Light/dark preference remembered across visits.
- Durable PostgreSQL storage through Entity Framework Core migrations.
- JWT authentication in an HttpOnly cookie, CSRF protection, hashed passwords, input validation, login rate limiting and server-side session revocation.
- Automated API tests and Angular HTTP behavior tests.

## Architecture

```text
Browser (Angular)
   │ same-origin JSON requests + cookie
   ▼
ASP.NET Core controllers
   │ authentication → authorization → validation
   ▼
EF Core / Npgsql
   │ parameterized SQL over TLS
   ▼
Neon PostgreSQL
```

In production the .NET server also serves the compiled Angular files. The Docker build compiles both projects, so only one web service is needed. Development uses an Angular proxy for /api requests.

## Run locally

Install Node.js 22.12+ (22 LTS), the .NET 9 SDK and Git. Use the required framework versions for this assignment.

From the repository root:

```powershell
dotnet tool restore
dotnet restore
dotnet user-secrets set --project server "ConnectionStrings:DefaultConnection" "Host=YOUR_HOST;Port=5432;Database=neondb;Username=YOUR_ROLE;Password=YOUR_PASSWORD;SSL Mode=Require;Channel Binding=Require"
dotnet user-secrets set --project server "Jwt:Key" "REPLACE_WITH_A_RANDOM_SECRET_AT_LEAST_32_BYTES"
dotnet ef database update --project server -- --environment Development
dotnet run --project server --launch-profile http
```

Use a cryptographically random JWT key, not the placeholder above. Keep secrets out of Git. The database setting uses Npgsql key/value format rather than a postgresql:// URL.

In a second terminal:

```powershell
cd client
npm ci
npm start
```

Open the Angular URL printed in the terminal (normally http://localhost:4200). The proxy targets http://localhost:5230. Leave both terminals running. Do not expect a UI at the API root during this two-server development workflow.

## Verify

```powershell
dotnet test tests/BookQuotes.Tests.csproj
cd client
npm test -- --watch=false --browsers=ChromeHeadless
npm run build
```

API integration tests use a fresh in-memory SQLite database for each test and never connect to Neon. They exercise authentication, password hashing, book persistence, quote ownership, CSRF, token validation and logout revocation. This is relational behavior coverage; PostgreSQL-specific migration behavior is checked separately against the deployed database.

The GitHub Actions workflow runs both test suites, a production frontend build and the Docker build.

## API overview

| Method | Path | Purpose |
|---|---|---|
| GET | /api/auth/csrf | Obtain the CSRF cookie/header token |
| POST | /api/auth/register | Create an account and its starter quotes |
| POST | /api/auth/login | Verify credentials and set JWT cookie |
| GET | /api/auth/me | Current authenticated user |
| POST | /api/auth/logout | Revoke current session and clear cookie |
| GET / POST | /api/books | List / create a book |
| GET / PUT / DELETE | /api/books/{id} | Read / update / delete a book |
| GET / POST | /api/quotes | List own quotes / create a quote |
| GET / PUT / DELETE | /api/quotes/{id} | Read / update / delete an owned quote |
| GET | /health | Process health check |

All book and quote endpoints require authentication. POST, PUT and DELETE requests require X-XSRF-TOKEN. The frontend fetches a fresh CSRF token before mutations and after authentication changes.

## Design decisions and limits

- **Shared books, private quotes:** matches the shared catalogue and personal "My Quotes" requirements. Any signed-in user can edit/delete books. The UI makes this explicit.
- **One-hour sessions:** JWTs have a fixed expiry. There is no refresh token; sign in again when the session expires. A database session record permits immediate logout revocation.
- **HttpOnly cookie:** JavaScript cannot read the authentication token. SameSite=Strict and antiforgery validation protect cookie-authenticated writes. Production cookies require HTTPS.
- **Password handling:** ASP.NET Core PasswordHasher supplies salted password hashing; passwords are never returned by the API.
- **Dates:** DateOnly represents a publication date without a timezone shift.
- **Demo scope:** no email verification, password recovery, roles or audit log. Rate limiting is in-process and uses the connecting IP; behind a reverse proxy it may group visitors. A larger deployment should add trusted proxy configuration, distributed rate limits and stronger abuse monitoring.
- **Deployment:** the free Render instance can sleep when idle. Data stays in Neon. Startup migrations suit this single-instance demonstration; a larger deployment should apply reviewed migrations in a separate release step.
- **Framework lifecycle:** .NET 9 is used because the brief explicitly requires it. Review support status and plan a supported LTS upgrade before long-term production use.
- **Frontend budget:** Bootstrap and icon CSS are bundled locally. No third-party font or image service is required.

See [deployment steps](docs/DEPLOYMENT.md), [project walkthrough](docs/WALKTHROUGH.md) and [submission checklist](docs/SUBMISSION.md).
