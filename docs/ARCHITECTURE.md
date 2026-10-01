# Architecture (Clean-style layers, pragmatic edition)

## Dependency rule

Dependencies point **inward**. Nothing outside `Api`/`Frontend` knows about EF Core;
nothing outside `Infrastructure` touches SQL.

```text
Frontend ──→ Contracts ←── Api ──→ Application ──→ Domain
                  ↑                     ↑
                  └── Infrastructure ───┘
```

- **Contracts** — shared kernel, zero dependencies: routes (`ApiRoutes`), limits
  (`Limits`), JSON shapes (`ApiResponse<T>`, `PagedResult<T>`, request/response DTOs
  with `DataAnnotations`). Referenced by Api, Application, and Frontend, so a renamed
  field breaks the build instead of failing at runtime.
- **Domain** — `Product`, `Category` POCOs. No attributes, no EF, no JSON.
- **Application** — ports (`IProductService`, `ICategoryService`, `IProductRepository`,
  `ICategoryRepository`), service implementations with the business rules (paging
  clamp, search hygiene, category-exists guard), and `InvalidRequestException`.
- **Infrastructure** — `AppDbContext` (indexes, relationships, seed) + EF
  repositories. The only project allowed to reference `Microsoft.EntityFrameworkCore`.
- **Api** — composition root: DI wiring, JSON policy, OutputCache, health,
  `IExceptionHandler`, thin Minimal API endpoints (HTTP ↔ service, no logic).
- **Frontend** — Blazor Server. Talks to the back-end only through the typed
  `BackendClient`; pages never build URLs or parse envelopes by hand.
- **Tests** — `Application.Tests` targets services via fakes (fast, no I/O);
  `Api.Tests` targets HTTP behavior via `WebApplicationFactory` + throwaway SQLite.

## Decisions (with reasons)

1. **Minimal API over controllers** — 5 CRUD endpoints × 2 entities; controllers add
   ceremony with no payoff at this size. If versioning/HATEOAS ever matters,
   revisit.
2. **Envelope only on reads/404s; RFC 7807 on 400s** — Minimal API validation and the
   domain exception handler both speak `ProblemDetails` natively. Forcing one shape
   would mean fighting the framework; both shapes are documented in
   `docs/JSON-EXAMPLES.md` and handled in `BackendClient`.
3. **`EnsureCreated` instead of migrations** — school project, single SQLite file,
   reviewer runs once and sees seed data. Production path is one line away
   (`Database.Migrate()` + `dotnet ef migrations add`), noted in
   `docs/INTEGRATION-TROUBLESHOOTING.md` §2.
4. **Hand-made fakes instead of Moq/NSubstitute** — 2 interfaces, explicit behavior,
   zero new dependencies. If the port count grows past ~5, switch to a framework.
5. **Fresh server + fresh DB per integration test** — `OutputCache` and seed data make
   shared fixtures order-dependent. Cost: ~200 ms/test. Worth it (see
   `docs/INTEGRATION-TROUBLESHOOTING.md` §8).
6. **`public partial class Program`** — one 4-line block at the end of Api
   `Program.cs` to let `WebApplicationFactory<Program>` in. No test hooks anywhere
   else in production code.

## Known shortcuts (declared, not hidden)

- `ProductService.CreateAsync` does 2 DB round-trips (insert + refetch for
  `CategoryName`). Fine at this scale; project to DTO in-query if it grows.
- No `Select` projection in list queries (`Include` + map). Readability over
  micro-optimization; indexes + paging dominate latency (see `docs/PERFORMANCE.md`).
- Categories list is unpaged (3–50 rows realistic). Products are paged.
- No authN/Z — out of scope for this integration-focused project.
