# InventoryHub — Full-Stack .NET 10 (Blazor + Minimal API)

Final project integrating the four activities: front↔back integration code,
debugging, JSON structures, and performance optimization — with Copilot assistance
documented in `docs/COPILOT-REFLECTION.md`.

| Rubric (30 pts) | Where to review |
|---|---|
| GitHub repo (5) | https://github.com/CristianMz21/InventoryHub (public) |
| Integration code front↔back (5) | `Frontend/Services/BackendClient.cs`, `Backend/Api/Endpoints/` |
| Debugging with Copilot (5) | `docs/INTEGRATION-TROUBLESHOOTING.md` (9 real errors) |
| JSON structures (5) | `Contracts/Dtos/ApiEnvelope.cs`, `docs/JSON-EXAMPLES.md` |
| Performance optimization (5) | `docs/PERFORMANCE.md`, `ProductRepository.cs`, `Products.razor` |
| Reflective summary (5) | `docs/COPILOT-REFLECTION.md` |

## Architecture

```text
InventoryHub.slnx
├── Contracts/            # shared: ApiRoutes, Limits, ApiResponse<T>, PagedResult<T>, DTOs
├── Backend/
│   ├── Domain/           # Product, Category entities (no dependencies)
│   ├── Application/      # repository/service abstractions, services, domain exceptions
│   ├── Infrastructure/   # AppDbContext (indexes + seed), EF repositories
│   └── Api/              # Minimal API endpoints, ProblemDetails handler, OutputCache
├── Frontend/             # Blazor Server: BackendClient (retry), Products + Categories pages
├── Tests/
│   ├── Application.Tests # 12 unit tests, hand-made fakes
│   └── Api.Tests         # 10 integration tests, WebApplicationFactory + SQLite per test
└── docs/                 # architecture, troubleshooting, JSON, performance, Copilot reflection
```

Rules and trade-offs: `docs/ARCHITECTURE.md`.

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

## Repository

Live at https://github.com/CristianMz21/InventoryHub (public, branch `main`).
Clone and run — no extra setup beyond the .NET 10 SDK.
