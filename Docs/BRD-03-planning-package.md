# BRD-03 Planning Package
> **Repository:** GenericScreenGen  
> **Branch:** feat/brd-03-new-layout-policies-and-field-types  
> **Date:** 2026-04-25  
> **Source requirements:** Docs/BRD-03.md

---

## Part 1 — Planning Discussion Record

### Original Prompt

> Don't forget to follow the copilot instructions document. If you are going to follow this automatically in every chat, let me know so I don't have to tell you every time.  
> Understand ALL requirements from BRD-03 and come up with a plan and save it in memory or a file that you can easily access.  
> Once I review the plan, I will direct you to proceed with autonomous implementation.

**Assistant commitment:** Follow repo instruction file automatically in this workspace. Analyze BRD-03 and the codebase, then ask focused clarification questions before finalizing the plan.

---

### Clarification Questions and Answers

#### Q1 — Data Source
*Where should tabular/record data come from for add/edit/delete/sort/filter/pagination?*

| Option | Description |
|--------|-------------|
| In-memory mock data | Full UX first, data resets on refresh |
| JSON file-backed persistence | Read/write data to files in API project |
| Database-backed persistence | Add repository layer and storage |
| Other | Custom preference |

**Answer:** JSON-backed file persistence for now. Store files in a new solution folder called `MyJsonDataStore`. Access persisted data via an `IDataStore` interface, with a JSON implementation initially, but designed to easily support different data store implementations (e.g. database) later. Add this as **Requirement 3.1.0** in BRD-03.

---

#### Q2 — Sort / Filter / Pagination Location
*Do you want these operations primarily on client-side or server-side?*

| Option | Description |
|--------|-------------|
| Client-side first | Fetch all rows then sort/filter/page in Angular |
| Server-side | API handles query params and returns paged rows |
| **Hybrid** ✓ | Client for now, API-ready contracts for later |

**Answer:** Hybrid

---

#### Q3 — Edit / Add Navigation
*Should record-detail be a dedicated routed page or an in-place panel/modal?*

| Option | Description |
|--------|-------------|
| **Dedicated routed page** ✓ | Clear URL/state transitions |
| Modal/Dialog | |
| Inline expandable row | |

**Answer:** Dedicated routed page

---

#### Q4 — Multi-Select Lookup UX
*For multi-select lookup, which interaction should be primary?*

| Option |
|--------|
| Searchable multi-select dropdown with removable tags |
| Dual-list picker |
| **Checkbox list with search** ✓ |

**Answer:** Checkbox list with search

---

#### Q5 — Show Original Values Display
*How should original values be displayed in record-detail?*

| Option |
|--------|
| **Side-by-side Current vs Original columns** ✓ |
| Toggle replace current with original read-only snapshot |
| Inline diff highlights per field |

**Answer:** Side-by-side Current vs Original columns

---

#### Q6 — CSV Export Scope
*Export should include which rows by default when filters are active?*

| Option |
|--------|
| **Filtered rows by default, plus option for all rows** ✓ |
| All rows always |
| Prompt each time |

**Answer:** Filtered rows by default, plus option for all rows

---

### Resulting Planning Decisions

| # | Decision |
|---|----------|
| 1 | Add BRD requirement **3.1.0** for `IDataStore`-based persistence with initial JSON implementation |
| 2 | Introduce new solution folder/project area named `MyJsonDataStore` |
| 3 | Keep design storage-agnostic — database implementation can be added later |
| 4 | Hybrid strategy for sorting, filtering, and pagination |
| 5 | Dedicated routed pages for record-detail add/edit flows |
| 6 | Checkbox-list-with-search as primary multi-select lookup UX |
| 7 | Original values displayed side-by-side with current values |
| 8 | CSV default = filtered rows; explicit all-rows option also available |

---

## Part 2 — Implementation Plan

### Summary
Extend the existing dynamic-screen architecture with two new layout policies (`tabular`, `record-detail`), a storage abstraction (`IDataStore`) with JSON-backed persistence in a new `MyJsonDataStore` solution folder, and client capabilities for table operations, CRUD navigation, and advanced lookup behavior. Delivered in phases that preserve backward compatibility for existing screen configs and APIs, with contracts that can later switch to database storage.

---

### Phase 0 — Requirement Baseline and Solution Structure
*Blocks all other phases*

1. Add BRD requirement `3.1.0` — introduce `IDataStore` abstraction, JSON-backed implementation, and solution folder `MyJsonDataStore` for persisted row data files.
2. Update `.sln` so the new structural folder is visible as a Solution Folder in .NET Explorer.
3. Define naming and data contracts for row entities and lookup option metadata (storage-agnostic).

---

### Phase 1 — Backend Contracts and Policy Registration
*Depends on Phase 0*

4. Add `CTabularLayoutPolicy` and `CRecordDetailLayoutPolicy` classes; register both in DI in `Program.cs`.
5. Extend interfaces/models for row data operations and lookup metadata while preserving all existing interfaces.
6. Extend schema and config parsing to support lookup `is-mandatory`, `is-multiple`, and enriched option metadata (value + description + optional image URL).
7. Confirm `per-line` and `flow` behavior is unchanged and backward-compatible.

---

### Phase 2 — Data Store Layer and API Surface
*Depends on Phase 1*

8. Add `IDataStore` interface and `CJsonDataStore` implementation in `MyJsonDataStore` — pluggable for future DB swap.
9. Add backend services for tabular row retrieval, filtering, sorting, pagination, create/update/delete, and CSV generation (filtered and all-rows modes).
10. Add API endpoints:
    - `GET /api/data/{screenId}` — list/query rows (sort, filter, page params)
    - `GET /api/data/{screenId}/export` — CSV export
    - `POST /api/data/{screenId}` — create row
    - `PUT /api/data/{screenId}/{rowId}` — update row
    - `DELETE /api/data/{screenId}/{rowId}` — delete row
    - `GET /api/data/{screenId}/{rowId}` — fetch single record including original values snapshot
11. Ensure contracts are hybrid-compatible: server accepts query params; client manages UX state.

---

### Phase 3 — Angular Routing and Screen Flows
*Depends on Phase 2*

12. Add dedicated routes in `app.routes.ts` for tabular list (`/screen/:id`) and record-detail (`/screen/:id/record/:rowId`, `/screen/:id/record/new`).
13. Wire navigation from tabular row click → record-detail edit, and from Add New Row → record-detail create mode.
14. Implement Save/Cancel return path from record-detail back to tabular view.

---

### Phase 4 — Tabular UI Features
*Depends on Phase 3; sub-steps 15–16 can run in parallel after base table component exists*

15. Implement tabular layout rendering — sticky headers, vertical-scroll body, horizontal scroll for wide tables; add `layout-tabular` CSS class in `LayoutPolicyService`.
16. Implement sorting (asc/desc toggle, visual indicator on active column).
17. Implement per-column case-insensitive partial-match filtering with multi-column AND composition.
18. Implement pagination controls (first/prev/next/last) — activates at >50 rows; shows current page and total pages.
19. Implement per-row delete with confirmation prompt and view refresh on confirm.
20. Implement CSV export action — filtered rows default, explicit all-rows option.

---

### Phase 5 — Record-Detail and Original Values UX
*Depends on Phase 3*

21. Implement `record-detail` layout — `per-line`-style field rendering with explicit Save and Cancel actions.
22. Implement "Show Original Values" — side-by-side Current vs Original columns per field.
23. Ensure edit mode and create mode have distinct visual state and validation feedback.

---

### Phase 6 — Lookup Enhancements
*Depends on Phase 1 for contracts; parallel with Phases 4/5 once API exists*

24. Support `is-mandatory` / `is-optional` lookup behavior in forms and validation.
25. Support single-select and multi-select lookup rendering paths.
26. Implement searchable lookup — case-insensitive partial matching.
27. Implement checkbox-list-with-search for multi-select; display selected items as removable tags/chips.
28. Display optional description and image metadata in option UI where available.

---

### Phase 7 — Samples, Docs, and Session Logging
*Depends on all prior phases*

29. Add new sample screen configs demonstrating BRD 3.1/3.2/3.3 coverage.
30. Update `Docs/BRD-03.md` to include requirement `3.1.0`.
31. Update `Other/development-session-log.txt` with branch entries and milestone changes.

---

### Phase 8 — Verification and Hardening
*Depends on all phases*

32. Run backend build; confirm no interface/implementation contract breaks.
33. Verify all new API endpoint payloads for list/query/sort/filter/page/export/CRUD.
34. Verify tabular rendering — sticky headers, scroll behavior, desktop and mobile widths.
35. Verify sorting toggle and indicator.
36. Verify multi-column AND filtering and case-insensitivity.
37. Verify pagination threshold, controls, and page count.
38. Verify CSV export — filtered default and all-rows option.
39. Verify record-detail row click routing, Save/Cancel return.
40. Verify original values — side-by-side snapshot accuracy.
41. Verify lookup single-select search and required validation.
42. Verify lookup multi-select — checkbox list, tags, removable selections.
43. Verify delete — confirmation prompt and view refresh.
44. Verify persistence — added/updated/deleted rows survive API restart.
45. Regression check — existing `per-line` and `flow` screens render exactly as before.

---

### Relevant Files

| File | Change |
|------|--------|
| `Docs/BRD-03.md` | Add requirement `3.1.0` |
| `GenericScreenGenSln.sln` | Add `MyJsonDataStore` solution folder/project |
| `GenericScreenGenApp/Program.cs` | Register new policies, data store, new endpoints |
| `GenericScreenGenApp/Schemas/ScreenConfigSchema.json` | Extend lookup schema metadata |
| `GenericScreenGenInterfacesLib/IScreenRenderFieldModel.cs` | Extend lookup metadata contract |
| `GenericScreenGenInterfacesLib/EFieldType.cs` | Evaluate if enum extension needed |
| `GenericScreenGenImplementationsLib/CScreenRenderModelFactory.cs` | Map enriched lookup controls |
| `GenericScreenGenImplementationsLib/CScreenRenderFieldModel.cs` | Add lookup/output state properties |
| `GenericScreenGenImplementationsLib/CScreenConfigProvider.cs` | Parse extended config and new layout ids |
| `GenericScreenGenClientApp/src/app/services/layout-policy.service.ts` | Add `tabular` and `record-detail` CSS mappings |
| `GenericScreenGenClientApp/src/app/components/section-renderer.component.ts` | Add tabular and record-detail rendering paths |
| `GenericScreenGenClientApp/src/app/models/screen.models.ts` | Add row and lookup metadata models |
| `GenericScreenGenClientApp/src/app/services/screen-api.service.ts` | Add row query/CRUD/export API methods |
| `GenericScreenGenClientApp/src/app/app.routes.ts` | Define tabular and record-detail routes |
| `GenericScreenGenClientApp/src/app/app.ts` | Align root navigation with routed flow |
| `GenericScreenGenApp/screen/*.json` | Add/update sample configs |
| `Other/development-session-log.txt` | Track milestones and branch entries |

---

### Further Considerations

1. **Data versioning** — define a `schema-version` field in persisted JSON rows to simplify future DB migration.
2. **Server-side scaling** — for large datasets, prioritize server-side pagination/filtering while retaining client-side visual UX state.
3. **Validation ownership** — clarify final split between backend and frontend for required fields and lookup constraints to avoid duplicated or conflicting rules.