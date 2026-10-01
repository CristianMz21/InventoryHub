# Performance (Actividad 4)

Verified with `dotnet build` (0 warnings) + `dotnet test` (22/22) + live curl runs.
No benchmark lab here, so every item below is a concrete, reviewable code decision,
not a slogan.

## Back-end (EF Core + SQLite + Minimal API)

1. **Paged reads everywhere** — `GET /products` takes `page`/`pageSize`/`search` and
   returns `PagedResult<T>`. `ProductService` clamps (`page >= 1`, `pageSize` 1–100)
   so a malicious `pageSize=1000000` can't trigger a full-table scan.
   File: `Backend/Application/Services/ProductService.cs`.
2. **Single round-trip per page** — `CountAsync` + one `Skip/Take` query.
   File: `Backend/Infrastructure/Repositories/ProductRepository.cs`.
3. **`AsNoTracking()` on all reads** — no change-tracker overhead for GETs.
4. **DB indexes** — `Product.Name`, `Product.CategoryId` (search + join), unique
   `Category.Name`. File: `Backend/Infrastructure/Data/AppDbContext.cs`.
5. **`CancellationToken` end-to-end** — minimal API binding → service → repository →
   `*Async(ct)`. Cancelled requests stop DB work instead of finishing pointlessly.
6. **`OutputCache` on GETs** — `/products` cached 30 s, `/categories` 60 s.
   Writes (`POST/PUT/DELETE`) are not cached. File: `Backend/Api/Program.cs`,
   `ProductEndpoints.cs`, `CategoryEndpoints.cs`.
7. **Search hygiene** — `Trim()`, max 100 chars, `EF.Functions.Like` (parameterized,
   no string-concatenated SQL).

## Front-end (Blazor Server)

1. **Debounced search (400 ms)** — typing doesn't fire a request per keystroke;
   the previous pending search is cancelled via `CancellationTokenSource`.
   File: `Frontend/Components/Pages/Products.razor` (`OnSearchInput`).
2. **Typed client with retry + backoff** — `BackendClient` retries 3× (200 ms × attempt)
   only on 5xx/network errors; 4xx returns immediately (retrying is pointless).
   File: `Frontend/Services/BackendClient.cs`.
3. **`HttpClient.Timeout = 10 s`** — no hung UI spinners forever.
4. **Pagination state in the page, not in memory lists** — only the current page
   travels over the wire (`PagedResult`), the grid never holds the whole catalog.

## Deliberately NOT done (trade-offs)

- No `Select` projection to DTOs inside the query: `Include + MapToResponse` keeps the
  code readable at this catalog size; the indexes + paging dominate real latency.
- No distributed cache/Redis: `OutputCache` in-memory is the right size for a review demo.
- No response compression tuning: default Kestrel behavior is fine for JSON this small.
- No shared test fixture for integration tests: one server + SQLite file per test costs
  ~200 ms each (~2 s total) but `OutputCache` + seed data would make shared tests
  order-dependent. Correctness of the suite beats 1.5 s of speed.
