# Release Notes — BRD-03 Req 3.1: Tabular Layout Policy

## Overview
A new `tabular` layout policy is now available for screen definitions. It renders all records in an interactive data-table, replacing the need for hand-coded grid UIs. Screens simply declare `"layout-policy": "tabular"` in their configuration to opt in.

---

## Feature List

### Req 3.1 — Tabular layout policy registration
- The backend now exposes a `tabular` policy ID via the `ILayoutPolicy`/`ILayoutPolicyRegistry` system.
- The Angular client maps `tabular` to the `layout-tabular` CSS class so components can style themselves appropriately.
- **Benefit**: Any screen configuration can instantly adopt a table layout with no front-end code changes.

---

### Req 3.1.1 — Baseline table rendering
- Each record is shown as a row; field labels become sticky column headers.
- The table responds to viewport changes, making it usable on any screen width.
- **Benefit**: Data that was previously hard to scan in a vertical form is now easy to compare across records.

---

### Req 3.1.2 — Vertical scroll with sticky headers
- When more rows are present than the visible area can display, the table scrolls vertically.
- Column headers stay fixed at the top during scroll.
- **Benefit**: Users never lose track of which column they are reading, even with large datasets.

---

### Req 3.1.3 — Horizontal scroll with column min-widths
- When the total column width exceeds the viewport, the table scrolls horizontally.
- Each column enforces a minimum width so content is never cramped or truncated by default.
- **Benefit**: Wide schemas remain fully readable without wrapping or overflowing the page layout.

---

### Req 3.1.4 — Column sorting (ascending / descending)
- Clicking any column header sorts all rows by that column's value.
- Clicking again toggles the direction; the active column and sort arrow are highlighted.
- **Benefit**: Users can instantly reorder data to find the highest, lowest, or alphabetically first value without writing any query.

---

### Req 3.1.5 — Per-column filter inputs
- A search input appears beneath each column header.
- Rows are filtered in real-time as the user types; matching is case-insensitive and supports partial strings.
- **Benefit**: Users can zero in on the exact records they need without leaving the table.

---

### Req 3.1.6 — Multi-column AND filtering
- Filters from multiple columns combine with AND logic.
- Only rows that satisfy every active filter are shown.
- **Benefit**: Complex look-ups (e.g., "status = active AND city contains 'New'") are answered interactively, with no backend round-trip.

---

### Req 3.1.7 — Pagination (threshold > 50 rows)
- When the filtered result set exceeds 50 rows, pagination controls appear automatically.
- Controls include First, Previous, Next, Last buttons plus a "Page X of Y" indicator.
- **Benefit**: Large tables stay performant and legible; users are never overwhelmed by thousands of rows at once.

---

### Req 3.1.8 — CSV export (filtered and all rows)
- An **Export Filtered** button downloads only the currently visible (filtered) rows.
- An **Export All** button downloads the full dataset regardless of active filters.
- Both exports include column headers.
- **Benefit**: Users can hand off a focused slice of data to spreadsheet tools with a single click, without needing developer assistance.

---

### Req 3.1.9 — Row-click navigation to edit view
- Clicking anywhere on a data row opens an inline edit form for that record.
- The edit form renders the fields according to the screen's own layout policy and supports Save / Cancel.
- **Benefit**: Editing a record is a natural one-click action from the table, eliminating separate navigation steps.

---

### Req 3.1.10 — Add new row screen
- An **Add New Row** button at the top of the table opens a blank entry form.
- The form uses the same field definitions as the table.
- Save appends the new record; Cancel returns to the table without changes.
- **Benefit**: Users can create new records without leaving the screen, maintaining context.

---

### Req 3.1.11 — Delete row with confirmation
- Each row has a **Delete** button.
- Clicking it opens a browser confirmation prompt before any data is removed.
- Confirming removes the row from the in-memory dataset and immediately refreshes the table.
- **Benefit**: Accidental deletions are prevented by the mandatory confirmation step, while legitimate removals are quick and immediate.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Tabular layout | Compare many records side-by-side at a glance |
| Sticky headers + scroll | Maintain orientation in large datasets |
| Sorting | Quickly find extremes or alphabetical order |
| Per-column + multi-column filter | Pinpoint exact records without backend queries |
| Pagination | Stay performant with large record sets |
| CSV export | Share or analyse data in any spreadsheet tool |
| Inline edit | Modify records with one click from the table |
| Add new row | Create records without losing table context |
| Delete with confirm | Safe, immediate record removal |
