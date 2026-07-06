# EMI.EmployeeManagement.Tests

Unit and integration tests for the **EMI.EmployeeManagement** solution. This project validates the business logic (BLL), data access layer (DAL), and API controllers.

## Prerequisites

| Requirement | Version |
|-------------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 or later |
| IDE (optional) | Visual Studio 2022, VS Code, or Rider |

No database is required to run these tests. DAL tests use **EF Core InMemory** with an isolated database per test.

## Project structure

```
EMI.EmployeeManagement.Tests/
├── API/
│   └── Controllers/          # Controller unit tests (mocked BLL)
├── BLL/
│   ├── Validators/           # Pure validation tests (no mocks)
│   ├── EmployeeBLLTests.cs
│   ├── AuthBLLTests.cs
│   └── PositionHistoryBLLTests.cs
├── DAL/                      # DAL integration tests (EF InMemory)
├── Helpers/
│   ├── TestDataBuilder.cs    # DTO factories and entity seed helpers
│   ├── BllMockSetup.cs       # Shared Moq setup for BLL tests
│   └── DbContextFactory.cs   # In-memory AppDbContext per test
└── GlobalUsings.cs
```

## Dependencies

| Package | Purpose |
|---------|---------|
| xUnit | Test framework |
| Moq | Mocking `I*DAL`, `I*BLL`, `IUnitOfWork` |
| FluentAssertions | Readable assertions |
| Microsoft.EntityFrameworkCore.InMemory | DAL tests against `AppDbContext` |

## Setup

1. Clone or open the repository.

2. Restore NuGet packages from the solution folder:

   ```bash
   cd src
   dotnet restore EMI.EmployeeManagement.sln
   ```

3. Build the test project:

   ```bash
   dotnet build EMI.EmployeeManagement.Tests/EMI.EmployeeManagement.Tests.csproj
   ```

No additional configuration files or environment variables are needed for the test project.

## Running tests

### Command line

From the `src` folder:

```bash
# Run all tests
dotnet test EMI.EmployeeManagement.Tests/EMI.EmployeeManagement.Tests.csproj

# Run with detailed output
dotnet test EMI.EmployeeManagement.Tests/EMI.EmployeeManagement.Tests.csproj --verbosity normal

# Run tests matching a filter (example: only DAL tests)
dotnet test EMI.EmployeeManagement.Tests/EMI.EmployeeManagement.Tests.csproj --filter "FullyQualifiedName~DAL"
```

From the solution (runs all projects that contain tests):

```bash
dotnet test EMI.EmployeeManagement.sln
```

### Visual Studio

1. Open `src/EMI.EmployeeManagement.sln`.
2. Open **Test Explorer** (`Test` → `Test Explorer`).
3. Click **Run All** or run individual test classes/methods.

### Visual Studio Code

1. Install the **.NET Core Test Explorer** or **C# Dev Kit** extension.
2. Open the solution folder and use the Testing panel to run tests.

## Test coverage overview

| Layer | Approach | Examples |
|-------|----------|----------|
| **Validators** | Direct static calls, no mocks | `NewEmployeeValidator`, `UpdateEmployeeValidator` |
| **BLL** | Moq for DAL and UnitOfWork | Employee CRUD, bonus calculation, auth flow |
| **DAL** | EF Core InMemory + seed data | CRUD, position history, authentication queries |
| **API Controllers** | Moq for BLL, direct controller instantiation | HTTP status codes, route parameter binding |

## DAL tests and InMemory limitations

DAL tests use an in-memory database that does **not** fully replicate PostgreSQL behavior:

- **Sequences** (`nextval`) are ignored; EF assigns IDs automatically.
- **Filtered unique indexes** (e.g. one active position history per employee) are not enforced at the database level; business rules are validated in DAL code.
- **Transactions** are simulated; `RollbackAsync` is tested for successful completion, not for undoing already persisted changes from `SaveChangesAsync`.

For full database fidelity, consider a separate integration test project with PostgreSQL (e.g. Testcontainers).

## Running the API (optional)

To run the main application alongside or before manual testing:

1. Configure PostgreSQL in `EMI.EmployeeManagement.API/appsettings.json`:

   ```json
   "ConnectionStrings": {
     "PostgreSql": "Host=localhost;Port=5432;Database=emi;Username=your_user;Password=your_password"
   }
   ```

2. Apply database migrations if required, then start the API:

   ```bash
   cd src
   dotnet run --project EMI.EmployeeManagement.API/EMI.EmployeeManagement.API.csproj
   ```

3. Open Swagger UI at `https://localhost:<port>/swagger` (port shown in the console output).

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Tests not visible in Test Explorer | Rebuild the solution and ensure `EMI.EmployeeManagement.Tests` is included in the `.sln`. |
| Package restore errors | Run `dotnet restore --force` from the `src` folder. |
| `TransactionIgnoredWarning` in DAL tests | Already handled in `DbContextFactory`; ensure you use `DbContextFactory.Create()` instead of creating `AppDbContext` manually. |

## Conventions

- Test class naming: `{ClassUnderTest}Tests`
- Test method naming: `{Method}_{Scenario}_Returns{ExpectedResult}`
- Pattern: **Arrange → Act → Assert**
- Use `[Theory]` + `[InlineData]` for parameterized validation tests
