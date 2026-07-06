# EMI Employee Management — Docker

Run the API and PostgreSQL with Docker Compose. Database schema and seed data are applied automatically from the SQL scripts in `database/Table Scripts/`.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Docker Compose v2)
- No local .NET SDK required to run the containers (only to develop locally)

## Quick start

1. Copy the environment template (optional — defaults work for local development):

   ```bash
   cp .env.example .env
   ```

2. Build and start all services:

   ```bash
   docker compose up --build
   ```

3. Open the API:

   | Resource | URL |
   |----------|-----|
   | Swagger UI | http://localhost:8080/swagger |
   | API base | http://localhost:8080/api |
   | Postman | Import [`EMI.postman_collection.json`](EMI.postman_collection.json) (see below) |

4. Authenticate in Swagger (required for most endpoints):

   1. Expand **POST /api/Auth/login** and execute it with the seed credentials below.
   2. Copy the `token` value from the response.
   3. Click **Authorize** (top right), paste the token **without** the `Bearer` prefix, and confirm.
   4. Call protected endpoints — Swagger sends `Authorization: Bearer <token>` automatically.

   | Username | Password | Role |
   |----------|----------|------|
   | `admin` | `admin1234` | Admin |

5. Stop services:

   ```bash
   docker compose down
   ```

   To remove the PostgreSQL data volume as well (full cluster reset):

   ```bash
   docker compose down -v
   ```

## Services

| Service | Container | Port | Description |
|---------|-----------|------|-------------|
| `api` | `emi-api` | 8080 | ASP.NET Core 8 Web API |
| `db` | `emi-postgres` | 5432 | PostgreSQL 16 |

## Configuration

Settings are defined in [`.env.example`](.env.example) and overridden via a `.env` file or environment variables.

| Variable | Default | Description |
|----------|---------|-------------|
| `POSTGRES_DB` | `emi` | Database name |
| `POSTGRES_USER` | `postgresqluser` | PostgreSQL user |
| `POSTGRES_PASSWORD` | `postgressqlpass` | PostgreSQL password |
| `POSTGRES_PORT` | `5432` | Host port for PostgreSQL |
| `API_PORT` | `8080` | Host port for the API |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Enables Swagger UI |
| `JWT_KEY` | (see `.env.example`) | JWT signing key |
| `JWT_ISSUER` | `Co.Appinit.API` | JWT issuer claim |
| `JWT_AUDIENCE` | `Co.Appinit.Client` | JWT audience claim |
| `JWT_EXPIRE_MINUTES` | `60` | Token lifetime in minutes |

.NET configuration keys are passed as environment variables with `__` separators, for example:

```
ConnectionStrings__PostgreSql=Host=db;Port=5432;...
Jwt__Key=your-secret-key
Jwt__Issuer=Co.Appinit.API
Jwt__Audience=Co.Appinit.Client
Jwt__ExpireMinutes=60
```

## API authentication & Swagger

The API uses **JWT Bearer** authentication. Swagger UI is enabled when `ASPNETCORE_ENVIRONMENT=Development` (the Docker default).

### Obtaining a token

```http
POST /api/Auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin1234"
}
```

Response:

```json
{
  "token": "<jwt>"
}
```

### Using the token

Send the token on every protected request:

```http
Authorization: Bearer <jwt>
```

In Swagger UI, use **Authorize** once; authorization is persisted across requests for the session.

### Role-based access

Protected endpoints document their required roles in Swagger (lock icon, description, and `401`/`403` responses). Summary:

| Endpoint | Roles |
|----------|-------|
| `POST /api/Employees` | Admin |
| `GET /api/Employees`, `GET /api/Employees/{id}` | Admin, User |
| `PUT /api/Employees/{id}`, `DELETE /api/Employees/{id}` | Admin |
| `PUT /api/PositionHistory/{id}` | Admin |
| `POST /api/Auth/login` | None (anonymous) |

The seed user `admin` has the **Admin** role and can access all endpoints.

### Swagger implementation

Swagger is configured in `src/EMI.EmployeeManagement.API/Extensions/`:

- `SwaggerExtensions.cs` — OpenAPI document, Bearer security scheme, Swagger UI
- `AuthorizeCheckOperationFilter.cs` — applies JWT and role requirements to `[Authorize]` endpoints

## Postman collection

A ready-to-import collection is available at [`EMI.postman_collection.json`](EMI.postman_collection.json). Use it to exercise the API without Swagger.

### Import

1. Open [Postman](https://www.postman.com/downloads/) (desktop or web).
2. **Import** → select `EMI.postman_collection.json` from the repository root.
3. Ensure the API is running (`docker compose up`) at `http://localhost:8080`.

### Workflow

1. Run **Login** (`POST /api/Auth/login`) with the seed credentials:

   | Username | Password |
   |----------|----------|
   | `admin` | `admin1234` |

2. Copy the `token` from the response.
3. For protected requests, set the collection or request **Authorization** to **Bearer Token** and paste the token (or update the bearer token on each request).
4. Execute the remaining requests.

### Included requests

| Request | Method | Endpoint | Auth |
|---------|--------|----------|------|
| Login | POST | `/api/Auth/login` | None |
| Add Employee | POST | `/api/Employees` | Admin |
| Get Employee By Id | GET | `/api/Employees/{id}` | Admin, User |
| Get All Employees | GET | `/api/Employees` | Admin, User |
| Update Employee | PUT | `/api/Employees/{id}` | Admin |
| Delete Employee | DELETE | `/api/Employees/{id}` | Admin |
| Update Position | PUT | `/api/PositionHistory/{id}` | Admin |

> **Note:** Sample bodies in the collection may use example usernames or HTTPS URLs. For Docker, use `http://localhost:8080` and the seed user `admin` / `admin1234`.

## Database initialization

On every **`db` container start**, the database is dropped, recreated, and initialized from the SQL scripts in `database/Table Scripts/`:

1. `Position.sql` — schema `emi`, positions + seed data  
2. `role.sql` — roles (Admin, User)  
3. `Employee.sql` — employee table  
4. `employee_role.sql` — employee–role mapping  
5. `Position_History.sql` — position history table  

Scripts are executed in that order via `database/docker-init/apply_schema.sh`, orchestrated by `database/docker-init/entrypoint.sh`. The API waits until the schema is ready (healthcheck marker + `pg_isready`).

> **Warning:** All data in the `emi` database is **lost** on each `db` container start.

| Action | Database reset? |
|--------|-----------------|
| `docker compose up` (first time / after `down`) | Yes |
| `docker compose restart db` | Yes |
| `docker compose up --force-recreate db` | Yes |
| `docker compose up` while stack already running | No (containers unchanged) |

To re-apply scripts while the stack is already running:

```bash
docker compose restart db
# or
docker compose up --force-recreate db
```

To reset the entire PostgreSQL cluster (not just the `emi` database), remove the volume: `docker compose down -v`.

## Project layout (Docker files)

```
EMI.EmployeeManagement/
├── Dockerfile              # Multi-stage build for the API
├── docker-compose.yml      # API + PostgreSQL
├── EMI.postman_collection.json  # Postman requests for the API
├── .env.example            # Environment template
├── .dockerignore
├── src/
│   └── EMI.EmployeeManagement.API/
│       └── Extensions/     # Swagger + JWT documentation helpers
└── database/
    ├── docker-init/        # DB init entrypoint
    └── Table Scripts/      # SQL schema scripts
```

## Unit tests

Unit tests live in `src/EMI.EmployeeManagement.Tests/`. They use xUnit with EF Core InMemory — **no database or Docker stack is required**.

**Prerequisite:** [.NET 8 SDK](https://dotnet.microsoft.com/download) (or later).

From the repository root:

```bash
# Restore and run all tests
dotnet test src/EMI.EmployeeManagement.sln

# Run with detailed output
dotnet test src/EMI.EmployeeManagement.sln --verbosity normal
```

From the `src` folder:

```bash
cd src

# Run all tests
dotnet test EMI.EmployeeManagement.Tests/EMI.EmployeeManagement.Tests.csproj

# Run only DAL tests
dotnet test EMI.EmployeeManagement.Tests/EMI.EmployeeManagement.Tests.csproj --filter "FullyQualifiedName~DAL"

# Run only BLL tests
dotnet test EMI.EmployeeManagement.Tests/EMI.EmployeeManagement.Tests.csproj --filter "FullyQualifiedName~BLL"

# Run only API controller tests
dotnet test EMI.EmployeeManagement.Tests/EMI.EmployeeManagement.Tests.csproj --filter "FullyQualifiedName~API"
```

See [`src/EMI.EmployeeManagement.Tests/README.md`](src/EMI.EmployeeManagement.Tests/README.md) for project structure and IDE instructions.

### Run unit tests with Docker

Only Docker is required — no local .NET SDK and no running `api`/`db` stack.

```bash
# Build the test image and run all tests
docker compose --profile test run --rm --build tests

# Run with verbose output
docker compose --profile test run --rm tests -- --verbosity normal

# Run only DAL tests
docker compose --profile test run --rm tests -- --filter "FullyQualifiedName~DAL"

# Run only BLL tests
docker compose --profile test run --rm tests -- --filter "FullyQualifiedName~BLL"

# Run only API controller tests
docker compose --profile test run --rm tests -- --filter "FullyQualifiedName~API"

# Rebuild the test image without running tests
docker compose --profile test build tests
```

## Useful commands

```bash
# Build images only
docker compose build

# Run in background
docker compose up -d --build

# View API logs
docker compose logs -f api

# View database logs
docker compose logs -f db

# Rebuild API after code changes
docker compose up --build api
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Port 5432 or 8080 already in use | Change `POSTGRES_PORT` or `API_PORT` in `.env` |
| API cannot connect to DB | Wait for `db` healthcheck; ensure `docker compose ps` shows `healthy` |
| Schema not created | Check `docker compose logs db`; restart with `docker compose restart db` |
| `permission denied` on init script | On Linux/macOS: `chmod +x database/docker-init/entrypoint.sh database/docker-init/apply_schema.sh` |
| HTTPS redirect in browser | API runs HTTP on port 8080 in Development/Docker |
| `401 Unauthorized` on API calls | Obtain a JWT via `POST /api/Auth/login` and set it in Swagger **Authorize**, or send `Authorization: Bearer <token>` |
| `403 Forbidden` | Token is valid but the user lacks the required role (see **Role-based access** above) |
| Swagger lock icon on login | Login is anonymous — no token needed for `POST /api/Auth/login` |

## Security note

Default passwords and JWT keys in `.env.example` are for **local development only**. Change them before any shared or production deployment.
