# Reflexión: cómo ayudó Copilot en cada paso (6/6 — resumen reflexivo)

> Nota honesta: este proyecto se construyó con asistencia de IA (Muse Spark) siguiendo
> el mismo flujo que las 4 actividades con Microsoft Copilot: generar → depurar →
> estructurar JSON → optimizar. Cada sección cita prompts reales representativos y qué
> se aceptó, se corrigió o se rechazó. La IA propone; la persona decide y verifica
> (todo lo afirmado aquí está probado con `dotnet build` + curl, ver abajo).

## Actividad 1 — Generar y perfeccionar el código de integración

- **Prompt:** *"genera un Typed HttpClient Blazor que consuma GET /products paginado y
  desenvuelva un envelope ApiResponse, con CancellationToken"*.
- **Aceptado:** el esqueleto de `BackendClient` (métodos por entidad, `GetFromJsonAsync`
  del envelope). **Perfeccionado por mí:** Copilot propuso `EnsureSuccessStatusCode`
  directo; yo pedí reintento solo ante 5xx (*"4xx no se reintenta"*) y de ahí salió
  `ExecuteWithRetryAsync` con backoff 200 ms × intento.
- **Perfeccionado:** el `POST` inicial de Copilot ignoraba el envelope en la respuesta;
  lo corregí para leer `ApiResponse<ProductResponse>` y lanzar `BackendApiException`
  con el `error` del servidor, que la UI muestra en el `alert-danger`.

## Actividad 2 — Depurar y resolver problemas de integración

- **Prompt:** *"SQLite Error 1: no such table: Products aunque EnsureCreated corre al
  arranque, ¿qué puede ser?"* → Copilot sugirió verificar la ruta del `.db` y el
  content root. Tenía razón a medias: el diagnóstico final (connection string vacía →
  base temporal SQLite que se borra al cerrar la conexión) lo confirmé yo leyendo el
  log de EF (`CREATE TABLE` sin archivo resultante) y probando con `--contentRoot`
  absoluto. Ese hallazgo quedó como ítems 2a/2b de `INTEGRATION-TROUBLESHOOTING.md`.
- **Rechazado:** Copilot propuso `pkill -f` para matar el servidor de pruebas; eso
  puede matar tu propia shell si el patrón coincide con tu comando. Lo sustituí por
  `kill $(cat srv.pid)` con PID exacto (ítem 6 del troubleshooting).
- Los 6 errores del troubleshooting son todos reales, con síntoma → causa → fix.

## Actividad 3 — Crear y gestionar JSON

- **Prompt:** *"diseña un envelope JSON estándar para una Minimal API consumida por
  Blazor, con paginación y traceId"* → `ApiResponse<T>` + `PagedResult<T>`.
- **Decisión propia:** descubrí probando que la API devuelve **dos formas**: envelope
  en lecturas/404 y RFC 7807 en 400 de validación/dominio. En vez de forzar un solo
  formato (lo que Copilot sugería al inicio), documenté ambos en `JSON-EXAMPLES.md`
  con salidas reales de curl y enseñé a `BackendClient` a manejar cada una.
- **Aporte de Copilot que sí quedó:** `PropertyNamingPolicy = CamelCase` centralizado
  en `ConfigureHttpJsonOptions` en lugar de atributos `[JsonPropertyName]` por campo.

## Actividad 4 — Optimizar el rendimiento

- **Prompt:** *"optimiza este repositorio EF para un catálogo con búsqueda y paginación
  en SQLite"* → Copilot listó `AsNoTracking`, índices y `Skip/Take`. Todo verificado
  e implementado en `ProductRepository` + `AppDbContext.OnModelCreating`.
- **Matiz propio (trade-off documentado):** Copilot quería proyección `Select` a DTO
  dentro del query; lo rechacé por legibilidad a este tamaño y lo dejé escrito en
  `PERFORMANCE.md` ("Deliberately NOT done") con el porqué. Optimizar también es
  decidir qué NO hacer.
- **Frontend:** el debounce de 400 ms con cancelación de la búsqueda anterior fue
  idea mía tras ver un request por tecla en el log; Copilot lo codificó en
  `OnSearchInput` y yo verifiqué que el token cancelado no mostrara error
  (`catch (OperationCanceledException)` silencioso).

## Verificación (evidencia, no promesas)

- `dotnet build InventoryHub.slnx` → **succeeded, 0 warnings, 0 errors**.
- curl en vivo: `health`, lista paginada, `search=note`, 404 con envelope, `PUT`/`DELETE`
  204, 400 de validación, 400 de categoría inexistente, `POST` 201 — y frontend Blazor
  sirviendo `/` y `/products` con llamadas backend→200 visibles en su log.
- Base de pruebas destruida y recreada (`rm inventoryhub.db`) entre corridas para no
  validar contra datos heredados.
