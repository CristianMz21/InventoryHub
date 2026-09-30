# JSON structures (Actividad 3)

Every response is `camelCase` (`ConfigureHttpJsonOptions`, `Program.cs`).
Success reads and 404s use the envelope; validation/domain failures use RFC 7807.

## 1. Envelope — `ApiResponse<T>` → `{ success, data, error, traceId }`

### GET `/products?page=1&pageSize=2` → 200 (real output)

```json
{
  "success": true,
  "data": {
    "items": [
      { "id": 1, "name": "Laptop", "categoryId": 1, "categoryName": "Electronics", "quantity": 12 },
      { "id": 2, "name": "Mouse", "categoryId": 1, "categoryName": "Electronics", "quantity": 50 }
    ],
    "page": 1,
    "pageSize": 2,
    "totalCount": 6,
    "totalPages": 3
  },
  "traceId": "0HNOV77SHPLCU:00000001"
}
```

### GET `/categories` → 200 (real output)

```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Electronics" },
    { "id": 2, "name": "Office" },
    { "id": 3, "name": "Groceries" }
  ],
  "traceId": "0HNOV77SHPLCV:00000001"
}
```

### POST `/products` `{ "name": "Webcam", "categoryId": 1, "quantity": 7 }` → 201 (real output)

```json
{
  "success": true,
  "data": { "id": 7, "name": "Webcam", "categoryId": 1, "categoryName": "Electronics", "quantity": 7 },
  "traceId": "0HNOV77SHPLD0:00000001"
}
```

### GET `/products/999` → 404 (real output)

```json
{ "success": false, "data": null, "error": "Product 999 not found.", "traceId": "0HNOV781NJTF5:00000001" }
```

## 2. Failures — RFC 7807 `ProblemDetails`

### POST invalid DTO → 400 (real output, Minimal API validation filter)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Quantity": ["The field Quantity must be between 0 and 2147483647."]
  },
  "traceId": "00-654b3974cba97f208853e7dab8180e63-31cd6f92b9121193-00"
}
```

### POST unknown category → 400 (real output, `InvalidRequestExceptionHandler`)

```json
{ "title": "Bad Request", "status": 400, "detail": "Category 99 does not exist." }
```

## 3. Contract source of truth

`Contracts/Dtos/ApiEnvelope.cs` (`ApiResponse<T>`, `PagedResult<T>`),
`ProductDtos.cs`, `CategoryDtos.cs`, plus `ApiRoutes.cs` and `Limits.cs`
(page clamp 1–100, search trim/max 100). Both projects reference the same
assembly, so front-end and back-end can never disagree on a field name.
