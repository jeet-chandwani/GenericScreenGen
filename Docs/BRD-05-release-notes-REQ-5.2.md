# Release Notes - BRD-05 Req 5.2: Record-ID Propagation and Persistence

## Overview
Record selection from multi-record screens now propagates a dedicated GUID record ID to the target detail screen. The selected record ID is displayed on the top-right of the detail screen, and save operations persist updates by record ID through new backend data endpoints.

---

## Feature List

### Req 5.2 - Pass record ID to detail screen and persist edits

#### Backend API (`Program.cs`)
- Added `GET /api/data/{screenFileName}/{recordId}` to fetch persisted record data by record ID.
- Added `PUT /api/data/{screenFileName}/{recordId}` to upsert persisted record data by record ID.
- Stored record IDs using reserved row key `__record-id`.
- Added row lookup/update helpers to keep record-id-based persistence logic centralized.

#### Angular API Service (`screen-api.service.ts`)
- Added `getRecordById(...)` for detail-screen hydration.
- Added `saveRecordById(...)` for record-detail save persistence.

#### Angular Navigation and Save Flow (`app.ts`)
- Added record context state:
  - `selectedRecordId`
  - `selectedRecordSourceScreenFileName`
- Enhanced `navigate:` action handling to:
  - read `__record-id` and `__source-screen` from payload,
  - load persisted record data when available,
  - fallback to prefill payload when no persisted row exists.
- Added `save-record:` action handling to persist detail form values by record ID.

#### Angular UI (`app.html`, `app.css`)
- Added right-top record badge in screen header:
  - `Record ID: {guid}`
- Styled as `record-id-pill` for clear visual identification.

#### Section Renderer (`section-renderer.component.ts`)
- Tabular row click now ensures each row has a GUID record ID.
- Navigation payload now includes:
  - `__record-id`
  - `__source-screen`
- Record-detail save now emits `save-record:{payload}` with the edited field dictionary.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| GUID record ID passed during row selection | Detail screens can track exactly which record is being edited |
| Record ID shown on screen header | Users can confirm they are editing the expected record |
| Save by record ID into persistence layer | Edits are tied to a stable identifier and can be reloaded accurately |
| Persisted data hydration on navigation | Detail screen can restore previously saved values for that record |
