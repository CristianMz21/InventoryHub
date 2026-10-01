# Integration troubleshooting (Actividad 2)

Real errors found while integrating the Blazor front-end with the Minimal API back-end,
how they were diagnosed, and the fix. Use this as a checklist when the UI shows
"Could not reach the backend".

## 1. `Backend:BaseUrl is not configured` — frontend fails at startup

**Symptom:** `InvalidOperationException: Backend:BaseUrl is not configured.`
**Cause:** `Frontend/Program.cs` reads `builder.Configuration["Backend:BaseUrl"]`.
That key only exists in `appsettings.Development.json`. Running the compiled DLL
without `ASPNETCORE_ENVIRONMENT=Development` (or without env var override) crashes on boot.
**Fix:** run with `ASPNETCORE_ENVIRONMENT=Development`, or set the env var
`Backend__BaseUrl=http://localhost:5200`. With Copilot I generated the explicit
exception message pointing to this doc.

## 2. `SQLite Error 1: no such table: Products` — empty database file

**Symptom:** every endpoint returns 500 `no such table`, although the app starts fine.
**Cause (found twice during this project):**
- (a) `Database.Migrate()` with **zero EF migrations** creates the `.db` file with
  only the migrations-history table — no entity tables. The stale file then blocks
  any later strategy because the file "already exists".
- (b) Running `Api.dll` with the wrong content root: the relative connection string
  `Data Source=inventoryhub.db` resolves against the content root. With no
  `appsettings.json` found, the connection string is empty and SQLite uses a
  **temporary database that is deleted when the connection closes** — tables created
  at startup vanish before the first request.
**Fix:** `Database.EnsureCreated()` (no migrations in this school project) +
always run via `dotnet run --project Backend/Api` (content root = project dir) or
pass an absolute `--contentRoot`. Delete stale `inventoryhub.db` after switching strategies.

## 3. HTTP 400 validation vs domain errors — two different JSON shapes

**Symptom:** reviewer expects the envelope but gets `{"type":...,"title":"One or more
validation errors occurred.",...}`.
**Explanation (not a bug):** DataAnnotations failures (empty name, negative quantity)
short-circuit in the Minimal API validation filter and return RFC 7807
`ProblemDetails`. Domain violations (category 99 doesn't exist) go through
`InvalidRequestExceptionHandler` and also return `ProblemDetails` with `detail`.
Only **successful reads and 404s** use the `ApiResponse<T>` envelope — see
`docs/JSON-EXAMPLES.md`. Front-end `BackendClient` handles both shapes.

## 4. Blazor `@rendermode InteractiveServer` — `CS0103: InteractiveServer does not exist`

**Symptom:** build error in generated `*_razor.g.cs`.
**Cause:** the `InteractiveServer` property lives in
`Microsoft.AspNetCore.Components.Web.RenderMode`; razor pages need
`@using static Microsoft.AspNetCore.Components.Web.RenderMode`.
**Fix:** added to `Components/_Imports.razor` (one line, all pages covered).

## 5. Wrong `ProjectReference` paths — `CS0234/CS0246` on every contract type

**Symptom:** dozens of `CategoryResponse could not be found` errors after creating projects.
**Cause:** relative paths miscounted (`..\..\..\Contracts` instead of `..\..\Contracts`).
Rule: from `Backend/<X>/<X>.csproj` use `..\..\Contracts\Contracts.csproj`;
from `Frontend/Frontend.csproj` use `..\Contracts\Contracts.csproj`.
**Fix:** corrected paths + `RootNamespace=InventoryHub.Frontend` so
`InventoryHub.Frontend.Services` resolves.

## 6. Port already in use — a previous test server is still alive

**Symptom:** curl returns responses from an **old build** (stale behavior, confusing).
**Fix:** reuse one terminal per server, kill by exact PID (`kill $(cat srv.pid)`).
Never `pkill -f <pattern>` with a pattern that also appears in your own shell command.

## 7. Commit rejected by pre-commit hook — `dotnet format` whitespace

**Symptom:** `git commit` aborts with `error WHITESPACE ... Run 'dotnet format' to fix`.
**Cause:** this repo enforces formatting via hook (e.g. expanded accessor braces in
entities — `get;`/`set;` on their own lines — which no human writes by hand).
**Fix:** `dotnet format InventoryHub.slnx` before committing, then
`dotnet format ... --verify-no-changes` to confirm exit 0. Never `--no-verify`
your way past it; the CI/reviewer sees the same diff.

## 8. Integration tests influence each other — shared server/DB/cache

**Symptom:** tests pass alone, fail in suite (or counts drift): `GET /products`
returns a cached list from a previous test because of `OutputCache` (30 s), and
seed assertions break once another test inserts rows.
**Fix:** `Tests/Api.Tests/InventoryHubFactory.cs` spins a fresh
`WebApplicationFactory` + unique temp SQLite file **per test** and deletes it on
dispose. Costs ~200 ms/test; buys total isolation. Rule: never assert absolute
list counts against a shared server.

## 9. `WebApplicationFactory<Program>` doesn't compile — Program is internal

**Symptom:** `CS0122`/`CS0246`: test project can't see the minimal-hosting `Program`.
**Cause:** top-level statements generate an `internal Program` class.
**Fix:** append `public partial class Program { }` to `Backend/Api/Program.cs`
(the only test hook in production code). No `InternalsVisibleTo` needed.

## Quick smoke test (after any change)

```bash
dotnet format InventoryHub.slnx --verify-no-changes
dotnet build InventoryHub.slnx
dotnet test InventoryHub.slnx
rm -f Backend/Api/inventoryhub.db
dotnet run --project Backend/Api --urls http://localhost:5200 &
sleep 8
curl -s http://localhost:5200/health
curl -s "http://localhost:5200/products?page=1&pageSize=2"
```
