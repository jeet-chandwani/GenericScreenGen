# Sequence Diagrams — GenericScreenGenSln

> Mermaid source. Renders in GitHub, VS Code, and any Mermaid-aware viewer.
> PlantUML equivalents: `sequences.puml`

---

## 1 — Application Startup (DI Wiring)

Shows how `Program.cs` constructs and wires all singleton services at boot time.

```mermaid
sequenceDiagram
    participant P  as Program.cs
    participant FAC as CGenericScreenGenFactory
    participant LPR as CLayoutPolicyRegistry
    participant FTR as CFieldTypeRegistry
    participant SCP as CScreenConfigProvider
    participant SSV as CScreenSchemaValidator
    participant SRF as CScreenRenderModelFactory
    participant DSR as CDataStoreRegistry
    participant SDM as CScreenDataStoreMappingRegistry
    participant FS  as File System

    P->>FAC: new CGenericScreenGenFactory()
    P->>FAC: Init(screenFolderPath)
    Note over FAC: Stores screenFolderPath.<br/>Derives schemaFilePath = .../Schemas/ScreenConfigSchema.json

    P->>LPR: new CLayoutPolicyRegistry()
    Note over LPR: Registers 4 layout policies:<br/>per-line · flow · tabular · record-detail

    P->>FTR: new CFieldTypeRegistry()
    P->>FTR: Init(contentRootPath)
    FTR->>FS: read screen/registry-field-types.json
    FS-->>FTR: field type definitions (id, parameters, validators)

    P->>FAC: TryCreateScreenConfigProvider(lpr, ftr)
    FAC->>SCP: new CScreenConfigProvider(lpr, ftr)
    FAC->>SCP: Init(screenFolderPath)
    SCP->>FS: GetFiles("Screen-*.json")
    FS-->>SCP: screen JSON files
    Note over SCP: Deserialises each file → validates layout<br/>policies + field types → caches IScreenDefinition

    P->>FAC: TryCreateScreenSchemaValidator()
    FAC->>SSV: new CScreenSchemaValidator()
    FAC->>SSV: Init(schemaFilePath)
    SSV->>FS: read Schemas/ScreenConfigSchema.json

    P->>FAC: TryCreateScreenRenderModelFactory()
    FAC->>SRF: new CScreenRenderModelFactory()
    FAC->>SRF: Init("")

    P->>DSR: new CDataStoreRegistry()
    P->>DSR: Init(contentRootPath)
    DSR->>FS: GetFiles("datastore.*.config.json")
    FS-->>DSR: config files (id, provider-type, parameters)
    Note over DSR: Creates CJsonDataStore or CDbDataStore<br/>per config entry; keyed by id

    P->>SDM: new CScreenDataStoreMappingRegistry(dsr)
    P->>SDM: Init(contentRootPath)
    SDM->>FS: GetFiles("screen-datastore-mapping.*.json")
    FS-->>SDM: mapping files (datastore-id, screen-ids[])
    SDM->>DSR: TryGetDataStore(id) — validate each reference
    Note over SDM: Builds screenId → dataStoreId dictionary

    P->>P: app.Build() — DI container sealed
    P->>P: app.Run()  — listening on :5074
```

---

## 2 — Screen Render Request

Shows the flow for `GET /api/screens/{fileName}/render` — the primary endpoint consumed by the Angular client.

```mermaid
sequenceDiagram
    participant C   as Client (Browser)
    participant API as ASP.NET API
    participant SCP as IScreenConfigProvider
    participant SRF as IScreenRenderModelFactory

    C->>API: GET /api/screens/{fileName}/render

    API->>SCP: TryGetScreenDefinition(fileName)
    Note over SCP: Dictionary lookup (case-insensitive).<br/>Returns cached IScreenDefinition.

    alt Screen not found
        SCP-->>API: false + error message
        API-->>C: 404 { error }
    end

    SCP-->>API: IScreenDefinition
    Note over API: screenId, displayName, theme,<br/>features[], key[], sections[]

    API->>SRF: TryCreateRenderModel(screenDefinition)
    Note over SRF: Per field:<br/>· resolves controlType (textarea / select / multiselect / input)<br/>· resolves inputType (text / number / date / datetime-local)<br/>· parses lookupValues from type-info string<br/>· merges registry defaults with field overrides<br/>Per section:<br/>· computes showBorder (false for "default-section")

    alt Render model creation failed
        SRF-->>API: false + error message
        API-->>C: 500 ProblemDetails
    end

    SRF-->>API: IScreenRenderModel

    API-->>C: 200 JSON<br/>{ screenId, displayName, theme, features[],<br/>  sections[{ name, layoutPolicy, isCollapsible,<br/>  showBorder, selectionActions[], fields[], sections[] }] }

    Note over C: Angular SectionRendererComponent<br/>switches rendering by layoutPolicy:<br/>  per-line      → stacked label + input rows<br/>  flow          → wrapping flex row<br/>  tabular       → sortable / filterable / paged table<br/>  record-detail → 160px label + full-width input form
```

---

## 3 — Data Read & Write

Shows the full resolution chain for data endpoints — both `GET` (load record) and `PUT` (upsert record).

```mermaid
sequenceDiagram
    participant C   as Client (Browser)
    participant API as ASP.NET API
    participant SCP as IScreenConfigProvider
    participant SDM as IScreenDataStoreMappingRegistry
    participant DSR as IDataStoreRegistry
    participant DS  as IDataStore
    participant FS  as File System / SQL Server

    rect rgb(232, 244, 255)
        Note over C,FS: READ — GET /api/data/{fileName}/{recordId}

        C->>API: GET /api/data/{fileName}/{recordId}

        Note over API: TryResolveDataStoreForScreen
        API->>SCP: TryGetScreenDefinition(fileName)
        SCP-->>API: IScreenDefinition → screenId
        API->>SDM: TryGetDataStoreIdForScreen(screenId)
        SDM-->>API: dataStoreId

        alt Screen not mapped to a data store
            API-->>C: 404 { error: "Screen is not associated with a data store" }
        end

        API->>DSR: TryGetDataStore(dataStoreId)
        DSR-->>API: IDataStore instance (CJsonDataStore or CDbDataStore)

        API->>DS: TryLoadRows(fileName)
        DS->>FS: read DataStore/{safeKey}.rows.json<br/>(or SQL SELECT)
        FS-->>DS: raw rows
        DS-->>API: IReadOnlyList‹IReadOnlyDictionary‹string,string››

        Note over API: FindRowByRecordId(__record-id == recordId)

        alt Record not found
            API-->>C: 404 { error: "Record not found" }
        end

        API-->>C: 200 { recordId, data: { fieldId: value, … } }
    end

    rect rgb(232, 255, 236)
        Note over C,FS: WRITE — PUT /api/data/{fileName}/{recordId}

        C->>API: PUT /api/data/{fileName}/{recordId}<br/>Body: { data: { fieldId: value, … } }

        alt Missing or null data body
            API-->>C: 400 { error: "Request body must contain a data object" }
        end

        Note over API: TryResolveDataStoreForScreen (same chain as READ)
        API->>SCP: TryGetScreenDefinition(fileName)
        API->>SDM: TryGetDataStoreIdForScreen(screenId)
        API->>DSR: TryGetDataStore(dataStoreId)

        API->>DS: TryLoadRows(fileName)
        DS-->>API: existing rows

        Note over API: Inject __record-id into data dict.<br/>FindRowIndexByRecordId:<br/>  ≥ 0 → replace row at index<br/>  -1  → append new row

        API->>DS: TrySaveRows(fileName, updatedRows)
        DS->>FS: write DataStore/{safeKey}.rows.json<br/>(or SQL upsert)
        FS-->>DS: OK
        DS-->>API: true

        API-->>C: 200 { screenFileName, recordId, savedAtUtc }
    end
```
