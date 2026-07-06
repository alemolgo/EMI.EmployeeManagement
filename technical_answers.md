# Technical Answers — EMI Employee Management

## 1. What are some common performance issues in .NET applications and how can you address them?

### Common issues and mitigations

| Issue | Impact | How to address |
|-------|--------|----------------|
| **N+1 queries** (EF Core loads parent, then one query per child) | High DB load, slow APIs | Use `.Include()`, projections, or a single LINQ join |
| **Tracking overhead on read-only queries** | Extra memory and CPU in EF Core | Use `.AsNoTracking()` for queries that only read data |
| **Loading full entities when only a few fields are needed** | More data transferred and materialized | Project to DTOs with `.Select()` |
| **Synchronous I/O** (`SaveChanges`, HTTP, file access) | Thread pool starvation under load | Use `async`/`await` end-to-end |
| **Missing indexes / full table scans** | Slow SQL at scale | Analyze execution plans; add indexes on FK and filter columns |
| **Excessive logging** | I/O overhead, noisy logs | Tune log levels; structured logging with Serilog |
| **Large Docker images / cold starts** | Slow deploys and startup | Multi-stage builds; runtime-only base images |
| **Blocking calls in middleware** | Latency spikes | Measure per-request duration; avoid sync-over-async |
| **Missing connection pooling** | Connection churn | Use provider defaults (Npgsql pools by default) |
| **Unbounded queries** | Memory spikes | Pagination, filters, `Take`/`Skip` |

### How this solution addresses them

#### Read-only queries without change tracking

`EmployeeDAL` and `AuthDAL` use `AsNoTracking()` on queries that only read data, avoiding EF change-tracker overhead:

```csharp
// EmployeeDAL.GetByIdAsync
var e = await _context.Employees
    .AsNoTracking()
    .FirstOrDefaultAsync(emp => emp.EmployeeId == id);

// AuthDAL.AuthenticateAsync
var user = await _context.Employees
    .AsNoTracking()
    .FirstOrDefaultAsync(e => e.EmployeeName == username);
```

#### Projections instead of loading full entities

`GetAllAsync` maps directly to `EmployeeResponse` in SQL via `.Select()`, so EF does not materialize full `Employee` entities:

```csharp
return await _context.Employees
    .AsNoTracking()
    .Select(e => new EmployeeResponse
    {
        Id = e.EmployeeId,
        Name = e.EmployeeName,
        Salary = e.EmployeeSalary,
        CurrentPositionId = e.EmployeeCurrentPositionId
    })
    .ToListAsync();
```

#### Existence checks with `AnyAsync`

Before insert/update, the DAL checks related data with lightweight existence queries instead of loading rows:

```csharp
var positionExists = await _context.Positions
    .AsNoTracking()
    .AnyAsync(p => p.PositionId == employeeRequest.CurrentPositionId);
```

#### Single-query joins (avoid N+1)

`AuthDAL` loads roles in one query with a LINQ join instead of separate round-trips per role:

```csharp
var roles = await (
    from er in _context.EmployeeRoles
    join r in _context.Roles on er.RoleId equals r.RoleId
    where er.EmployeeId == user.EmployeeId
    select r.RoleName
).ToListAsync();
```

`PositionHistoryDAL.IsEmployeeManagerAsync` resolves manager status in one joined query:

```csharp
var isManager = await (
    from ph in _context.PositionHistories
    join p in _context.Positions on ph.PositionHistoryPositionId equals p.PositionId
    where ph.PositionHistoryEmployeeId == employeeId && ph.PositionHistoryIsActive
    select p.PositionIsManager
).FirstOrDefaultAsync();
```

#### Async I/O throughout the stack

Controllers, BLL, and DAL use `async`/`await` (`SaveChangesAsync`, `FirstOrDefaultAsync`, etc.), keeping threads free while waiting on PostgreSQL.

#### Transactions and batched writes

`EmployeeBLL.AddAsync` uses `UnitOfWork` to group employee creation and initial position history in one transaction, reducing partial-state risk and round-trips:

```csharp
await _uow.BeginTransactionAsync();
var id = await _uow.Employees.AddAsync(request);
await _uow.PositionHistories.CreateInitialPositionHistoryAsync(id, request.CurrentPositionId, DateTime.UtcNow);
await _uow.CommitAsync();
```

`PositionHistoryDAL.UpdateEmployeePositionAsync` wraps close-history + update employee + create history in a single DB transaction.

#### Request timing observability

`RequestLoggingMiddleware` measures each HTTP request with `Stopwatch` and logs duration — useful to spot slow endpoints in Docker (`docker compose logs -f api`):

```csharp
_logger.LogInformation(
    "Response completed in {ElapsedMilliseconds} ms with status code {StatusCode}",
    stopwatch.ElapsedMilliseconds,
    context.Response.StatusCode);
```

Example from runtime logs: login ~34 ms, failed PUT ~10–64 ms.

#### Logging tuned for production noise

`appsettings.json` sets `Microsoft` and `Microsoft.AspNetCore` to **Warning**, reducing framework log volume while keeping application logs at Information:

```json
"Override": {
  "Microsoft": "Warning",
  "Microsoft.AspNetCore": "Warning"
}
```

#### Lean Docker runtime image

The `Dockerfile` uses a **multi-stage build**: `dotnet/sdk:8.0` for compile, `dotnet/aspnet:8.0` for runtime — smaller image and faster container startup than shipping the full SDK.

#### Area for future optimization (observed in this codebase)

`EmployeeBLL.GetByIdAsync` runs **two** DB calls (employee + `IsEmployeeManagerAsync`). For a single record this is acceptable; at scale it could be merged into one query with a join — a typical finding after profiling.

---

## 2. Describe how you would profile and optimize a slow-running query in an ASP.NET Core application.

### Step-by-step approach

1. **Reproduce and measure** — Identify the slow endpoint (middleware timing, APM, or logs).
2. **Enable EF Core query logging** — Log generated SQL in Development (`LogLevel.Information` for `Microsoft.EntityFrameworkCore.Database.Command`).
3. **Capture the SQL** — Copy the statement from logs or use `ToQueryString()` in a test.
4. **Analyze in the database** — Run `EXPLAIN ANALYZE` in PostgreSQL to see seq scans, missing indexes, and row counts.
5. **Fix at the right layer** — Index, rewrite LINQ, add projection, reduce round-trips, or cache if data is read-heavy and stable.
6. **Verify** — Compare before/after latency and execution plan cost.

### Tools commonly used

| Tool | Purpose |
|------|---------|
| `RequestLoggingMiddleware` / Serilog | Per-request latency in this API |
| `docker compose logs -f api` | Runtime traces in containerized deploys |
| EF Core `LogTo` / `Database.Command` logging | See exact SQL sent to PostgreSQL |
| PostgreSQL `EXPLAIN ANALYZE` | Index and plan analysis |
| dotnet-counters / Application Insights | CPU, GC, request rate under load |
| MiniProfiler / dotTrace | Deep dive in local dev |

### Example: profiling a slow query in EMI

**Scenario:** `GET /api/Employees/{id}` feels slow.

**1. Measure (already in place)**

`RequestLoggingMiddleware` logs:

```text
[INF] Incoming Request: GET /api/Employees/3
[INF] Response completed in 20 ms with status code 200
```

If latency grows, the log line pinpoints which endpoint regressed.

**2. Trace the code path**

- `EmployeesController.GetByIdAsync` → `EmployeeBLL.GetByIdAsync` → `EmployeeDAL.GetByIdAsync` + `PositionHistoryDAL.IsEmployeeManagerAsync`

**3. Inspect generated SQL (Development)**

Add temporarily to `Program.cs` or `appsettings.Development.json`:

```json
"Microsoft.EntityFrameworkCore.Database.Command": "Information"
```

You would see two statements similar to:

```sql
SELECT ... FROM emi.employee WHERE employee_id = @id;
SELECT p.position_is_manager
FROM emi.position_history ph
INNER JOIN emi.position p ON ph.position_history_position_id = p.position_id
WHERE ph.position_history_employee_id = @id AND ph.position_history_is_active;
```

**4. Analyze in PostgreSQL**

```sql
EXPLAIN ANALYZE
SELECT * FROM emi.employee WHERE employee_id = 3;

EXPLAIN ANALYZE
SELECT p.position_is_manager
FROM emi.position_history ph
JOIN emi.position p ON ph.position_history_position_id = p.position_id
WHERE ph.position_history_employee_id = 3 AND ph.position_history_is_active;
```

The schema already defines a partial unique index for active history:

```sql
CREATE UNIQUE INDEX uq_position_history_employee_active
ON emi.position_history (position_history_employee_id)
WHERE position_history_is_active;
```

That supports fast lookup of the active position per employee.

**5. Optimize**

| Finding | Action in this solution |
|---------|-------------------------|
| Read-only employee fetch | Already uses `AsNoTracking()` in `EmployeeDAL` |
| Manager check as second query | Could merge into one DAL method with a left join (optimization candidate) |
| Full table scan on `employee_name` at login | Add index on `emi.employee(employee_name)` if auth becomes a bottleneck |
| Delete failed with FK error (runtime evidence) | Fixed in `EmployeeDAL.DeleteByIdAsync` by removing `position_history` rows before deleting the employee — avoids retry storms and 500s from `23503` violations |

**6. Verify after change**

- Re-run the endpoint; compare `Response completed in X ms` in logs.
- Re-run `EXPLAIN ANALYZE` to confirm lower cost.
- Run unit tests: `dotnet test src/EMI.EmployeeManagement.Tests --filter "EmployeeDAL"`.

### Real incident from this solution (logs as evidence)

**Problem:** `DELETE /api/Employees/3` returned `500`.

**Log evidence:**

```text
PostgresException: 23503: update or delete on table "employee" violates foreign key constraint
"position_history_employee_id_fkey" on table "position_history"
```

**Root cause:** `ON DELETE RESTRICT` on `position_history` + `DeleteByIdAsync` only removed the employee.

**Fix applied:** Delete related `position_history` records first, then the employee — verified with HTTP `204` and follow-up `GET` returning `404`.

This illustrates the full loop: **observe logs → identify DB constraint → fix data access order → verify with tests and API calls**.
