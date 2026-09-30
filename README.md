# InventoryHub — Full-Stack .NET 10 (Blazor + Minimal API)

Final project integrating the four activities: front↔back integration code,
debugging, JSON structures, and performance optimization — with Copilot assistance
documented in `docs/COPILOT-REFLECTION.md`.

| Rubric (30 pts) | Where to review |
|---|---|
| GitHub repo (5) | this repository (public) |
| Integration code front↔back (5) | `Frontend/Services/BackendClient.cs`, `Backend/Api/Endpoints/` |
| Debugging with Copilot (5) | `docs/INTEGRATION-TROUBLESHOOTING.md` (6 real errors) |
| JSON structures (5) | `Contracts/Dtos/ApiEnvelope.cs`, `docs/JSON-EXAMPLES.md` |
| Performance optimization (5) | `docs/PERFORMANCE.md`, `ProductRepository.cs`, `Products.razor` |
| Reflective summary (5) | `docs/COPILOT-REFLECTION.md` |

## Architecture

```text
InventoryHub.slnx
├── Contracts/            # shared: ApiRoutes, Limits, ApiResponse<T>, PagedResult<T>, DTOs
├── Backend/
│   ├── Domain/           # Product, Category entities
│   ├── Application/      # IProductService/ICategoryService, services, repos interfaces
│   ├── Infrastructure/   # AppDbContext (indexes + seed), EF repositories
│   └── Api/              # Minimal API endpoints, ProblemDetails handler, OutputCache
├── Frontend/             # Blazor Server: BackendClient (retry), Products + Categories pages
└── docs/                 # troubleshooting, JSON examples, performance, Copilot reflection
```

## Run it (2 terminals)

Requirements: .NET 10 SDK (`dotnet --version` → 10.x).

```bash
# Terminal 1 — back-end (http://localhost:5200)
dotnet run --project Backend/Api --urls http://localhost:5200

# Terminal 2 — front-end (http://localhost:5210)
cd Frontend
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls http://localhost:5210
```

Open `http://localhost:5210/products`. API docs (Development):
`http://localhost:5200/openapi/v1.json`, health: `http://localhost:5200/health`.

> SQLite file `Backend/Api/inventoryhub.db` is auto-created with seed data.
> If you change the model, delete it and restart (school-project shortcut documented
> in `docs/INTEGRATION-TROUBLESHOOTING.md` §2).

## Tests (22 passing)

```bash
dotnet test InventoryHub.slnx
```

- `Tests/Application.Tests` (12): service rules — paging clamp, search trim/truncate,
  total pages, category-exists guard, name trimming, CRUD flows. Hand-made fakes,
  no mocking framework.
- `Tests/Api.Tests` (10): `WebApplicationFactory` + fresh SQLite file per test —
  health, paged camelCase envelope, search, 404 envelope, POST 201, domain 400,
  validation 400, PUT/DELETE flows, categories seed.

## API quick tour

```bash
curl -s "http://localhost:5200/products?page=1&pageSize=2"
curl -s -X POST http://localhost:5200/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Webcam","categoryId":1,"quantity":7}'
```

## Publish to GitHub (public repo for review)

```bash
cd InventoryHub
git init && git add -A && git commit -m "feat: complete InventoryHub full-stack app"
gh repo create InventoryHub --public --source=. --push
# …or push manually:
# git remote add origin https://github.com/<tu-usuario>/InventoryHub.git
# git branch -M main && git push -u origin main
```
