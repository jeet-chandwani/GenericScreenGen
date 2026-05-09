# Java Migration Plan — GenericScreenGenSln

Assessment of the difficulty and effort required to rewrite the ASP.NET Core (.NET 8) backend in Java.

---

## Codebase Size

| Project | Files | Lines |
|---------|-------|-------|
| GenericScreenGenApp (API host) | 1 | 438 |
| GenericScreenGenFactoryLib | 2 | 122 |
| GenericScreenGenImplementationsLib | 24 | 2,107 |
| GenericScreenGenInterfacesLib | 23 | 570 |
| GenericScreenGenUtilsLib | 2 | 64 |
| MyDataStoreProviders | 2 | 334 |
| **Total** | **54** | **3,635** |

No automated tests exist (`MyTestConsoleApp` is a manual console harness, not xUnit/NUnit).

---

## External Dependencies (only 2 NuGet packages)

| Package | Version | Purpose | Java equivalent |
|---------|---------|---------|-----------------|
| `JsonSchema.Net` | 9.2.0 | Validates screen config JSON against JSON Schema | `networknt/json-schema-validator` or `everit-org/json-schema` |
| `Microsoft.Data.SqlClient` | 6.1.1 | Raw ADO.NET SQL Server access | `mssql-jdbc` |

Everything else is built into the .NET runtime (System.Text.Json, ASP.NET Minimal API, DI container, file I/O).

---

## What Maps Easily to Java

| .NET pattern | Java equivalent | Effort |
|---|---|---|
| Interfaces (`IScreenConfigProvider` etc.) | Java interfaces — 1:1 | Trivial |
| `ACanInitBase` template method | Abstract class — 1:1 | Trivial |
| Factory pattern (`CGenericScreenGenFactory`) | Same pattern in Spring | Low |
| Registry pattern (4 registries) | Spring `@Component` + `@Autowired` | Low |
| `CJsonDataStore` (file I/O) | `Files.readString` / `Files.writeString` | Low |
| `CDbDataStore` (raw ADO.NET SQL) | JDBC `PreparedStatement` — same pattern | Low |
| LINQ chains | Java Streams | Low |
| String interpolation | `String.format()` or formatted strings (Java 15+) | Low |
| CORS config | Spring Security CORS — same concepts | Low |
| 10 REST endpoints | `@RestController` / `@GetMapping` / `@PutMapping` | Low–Medium |

---

## The Hard Parts (honest blockers)

### 1. `out` parameters / Try-parse pattern — **Medium effort**

Every method returns `bool` + `out T result` + `out string error` (144 occurrences across the codebase). Java has no `out` parameters.

Two options:
- **Result wrapper class** — `class Result<T> { T value; String error; boolean ok; }` — cleanest approach, requires touching every call site
- **Checked exceptions** — architectural shift, changes error-handling philosophy throughout

### 2. `JsonSchema.Net` → Java JSON Schema validation — **Medium–High effort**

Used in `CScreenSchemaValidator` and `CFieldTypeRegistry` to validate screen config files at startup.
- `networknt/json-schema-validator` (most complete, actively maintained) is the best Java match
- API is different; rewriting the validation wrapper takes ~1 week
- The JSON Schema files themselves (`Schemas/*.json`) are format-agnostic and reuse as-is

### 3. ASP.NET Minimal API → Spring Boot — **Medium effort**

ASP.NET Minimal API lets you write `app.MapGet("/path", (IService svc) => { ... })` with inline lambdas and automatic DI injection into parameters. Spring Boot uses `@RestController` classes with `@Autowired` — structurally similar but more verbose.

- All 10 endpoints need to become controller methods
- DI wiring moves from `Program.cs` singleton registrations to `@Bean` / `@Component` annotations
- `Results.Ok()`, `Results.NotFound()`, `Results.Problem()` → `ResponseEntity<T>`
- `Results.File()` for the schema endpoint → `ResponseEntity<Resource>`

### 4. `IReadOnlyList` / `IReadOnlyDictionary` — **Low effort but pervasive**

95 occurrences. Java uses `List.copyOf()`, `Map.copyOf()`, or `Collections.unmodifiableList()`. Mechanical substitution.

---

## What Does NOT Need to Change

- All JSON screen config files (`screen/Screen-*.json`) — format-agnostic
- All JSON Schema files (`Schemas/*.json`)
- DataStore config / mapping JSON files
- The Angular frontend — it talks HTTP to the API, no coupling to .NET
- Docker Compose — the `api` service just points to a different image

---

## Recommended Java Stack

```
Spring Boot 3.x                      — web framework, DI, CORS
Jackson (bundled with Spring Boot)   — JSON serialization (replaces System.Text.Json)
networknt/json-schema-validator      — JSON Schema validation (replaces JsonSchema.Net)
mssql-jdbc                           — SQL Server (replaces Microsoft.Data.SqlClient)
Java 21                              — Records (replaces C# records), pattern matching
Maven or Gradle                      — build tool
```

---

## Effort Estimate

| Work area | Weeks |
|-----------|-------|
| Interfaces + constants + factory | 0.5 |
| 4 registries + `ACanInitBase` base class | 1.0 |
| Screen config provider + render model factory | 1.0 |
| Schema validator (`JsonSchema.Net` port) | 1.0 |
| Data stores (JSON file + SQL) | 0.5 |
| Spring Boot controller + DI + CORS | 1.0 |
| `Result<T>` wrapper + touch all call sites | 1.0 |
| Integration testing + Docker image | 1.0 |
| **Total** | **~7 weeks** |

- With a Java developer who already knows Spring Boot well: **4–5 weeks** realistic
- With a developer new to Spring Boot: **8–10 weeks**

---

## Verdict

**Medium difficulty.** The codebase is small (~3,600 lines), has only 2 external dependencies, no authentication, no background services, and a clean layered architecture. The architecture patterns (factory, registry, interface-driven) translate directly to Java.

The main friction is mechanical: the `out`-parameter pattern needs a `Result<T>` wrapper threaded through ~144 call sites, and the JSON Schema library needs a Java replacement. Neither is conceptually hard — just time-consuming.

If the goal is simply to run on the JVM (e.g., for deployment consistency), **consider Kotlin instead** — Kotlin on the JVM is nearly a 1:1 translation from C# with almost no friction on the `out`-parameter problem (Kotlin has `Pair<T, E>` and destructuring), and Spring Boot supports Kotlin natively.
