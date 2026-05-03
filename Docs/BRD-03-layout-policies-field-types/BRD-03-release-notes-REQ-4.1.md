# Release Notes — BRD-03 Req 4.1: Tabular Layout Refinements and Fixes

## Overview
This release addresses usability, layout, and compactness issues with the `tabular` layout policy. Inline editing has been removed, the screen now fits within its container, the grid is more compact, and all controls have been refined for icon-based clarity and accessibility.

---

## Feature / Fix List

### Req 4.1.1 — Inline editing disabled; row click opens record-detail edit screen
- Clicking a row in the tabular view no longer opens an inline editor within the table cells.
- Table cells now display read-only text values.
- Row click opens a dedicated **Edit Record** screen that uses the per-line field layout (consistent with the `record-detail` layout policy).
- **Benefit**: Cleaner separation between browsing and editing; the table stays compact and scannable.

---

### Req 4.1.2 — Tabular screen sizing fixed; flexes with window size
- The `.tabular-shell` wrapper now uses `width: 100%; min-width: 0; box-sizing: border-box; overflow: hidden` so it cannot grow beyond its parent container.
- The `.section-body.layout-tabular` padding has been removed (moved inward) so the scroll container has full available width.
- **Benefit**: The tabular screen no longer overflows the outer card boundary when the window is resized.

---

### Req 4.1.3 — Compact grid layout
- Table cell `padding` reduced from `10px` to `2px 6px`.
- Cell `font-size` reduced to `12px` and `line-height` set to `1.2`.
- No vertical gap between rows — the grid now appears dense, spreadsheet-style.
- **Benefit**: Significantly more rows visible at once without scrolling.

---

### Req 4.1.4 — Action buttons moved to section header; icon-only with hover tooltips
- The **Add New Row**, **Export Filtered CSV**, and **Export All CSV** buttons have been removed from inside the table area.
- They now appear as compact icon buttons (`＋`, `↓F`, `↓A`) inside the section header bar — on the same line as the screen/section title.
- Each button has a `title` attribute with the full button name, shown as a native tooltip on hover.
- Buttons are 30 × 30 px; hidden when the edit screen is open.
- **Benefit**: Saves vertical space; action buttons are always visible without scrolling.

---

### Req 4.1.5 — Horizontal scrollbar when columns exceed screen width
- The `.tabular-scroll` div already applied `overflow-x: auto`; the parent container now correctly permits horizontal scrolling without clipping.
- **Benefit**: Wide tables with many columns are fully accessible via horizontal scroll.

---

### Req 4.1.6 — Delete button replaced with trash icon
- The text **Delete** button in the Actions column is replaced with a `🗑` icon button.
- Same confirmation prompt and deletion logic.
- `title="Delete row"` provides a tooltip on hover.
- **Benefit**: Saves column width; the intent is immediately clear.

---

### Req 4.1.7 — Reload screen now re-fetches data from data store
- `reloadCurrentScreen()` in `App` now sets `renderModel` to `null` before reloading, which destroys and recreates all `SectionRendererComponent` instances.
- On recreation, `ngOnChanges` unconditionally re-initialises tabular rows (including calling `createInitialRows()`).
- **Benefit**: Clicking "Reload screen" discards any in-memory edits and resets data from the source, matching user expectations.

---

### Req 4.1.8 — Compact filter inputs with placeholder text
- Filter input `font-size` reduced to `11px`; `height` capped at `22px`; `padding` set to `2px 5px`.
- `placeholder` text updated to `"Filter by {fieldLabel}…"` to clearly communicate purpose and partial-match support.
- Filter header row `padding` reduced to `2px 4px`.
- **Benefit**: Filters consume minimal vertical space while remaining usable.

---

### Req 4.1.9 — Pagination with icon buttons
- Text labels **First / Previous / Next / Last** replaced with Unicode icons **« / ‹ / › / »**.
- `title` attributes added: *"First page"*, *"Previous page"*, *"Next page"*, *"Last page"*.
- Button `padding` reduced to `2px 8px`; `min-width: 28px`; `font-size: 13px`.
- `margin-top` reduced to `4px`.
- **Benefit**: Pagination bar occupies less space and is visually consistent with the compact design.

---

### Req 4.1.10 — Sort indicators compact and tooltip on column headers
- Sort button `padding` reduced from `10px` to `4px 6px`; `font-size` set to `12px`.
- A `title` tooltip on each sort button communicates current state: *"Click to sort ascending"*, *"Sorted ascending — click to sort descending"*, etc.
- **Benefit**: Column headers are less tall; users understand sort state without guessing.

---

### Req 4.1.11 — Separate up/down/no-icon sort indicators
- Sort indicators changed: no icon when unsorted (was `↕`), `↑` for ascending (was `▲`), `↓` for descending (was `▼`).
- The sort indicator span is only rendered when the column is actively sorted.
- **Benefit**: Clear, unambiguous visual distinction between sorted and unsorted columns.

---

### Req 4.1.12 — Detail record uses per-line layout with proper field spacing
- The **Edit Record** screen opened from the tabular view now applies the same per-line layout styling as the standalone `record-detail` policy.
- `.tabular-edit-field-label` gets `display: flex; align-items: baseline; gap: 12px; font-weight: 700;`.
- `.tabular-edit-field-name` gets `flex: 0 0 160px` — matching the 160 px label width of per-line layouts.
- Each field is wrapped in a `.tabular-edit-field-row` div.
- Similarly, `.record-detail-shell .field-row .field-label` and `.record-detail-shell .field-name` are explicitly styled for consistent 160 px labels and baseline alignment.
- **Benefit**: Field labels and controls are visually aligned; no overlapping or missing spacing between form rows.

---

### Req 4.1.13 — Edit record screen has Save, Discard, and Show Original Values
- The unified **tabular-edit-actions** bar now shows:
  - **Save** — commits edits to the in-memory row; closes the edit screen.
  - **Discard** — reverts the row to its values at the time it was opened; closes the edit screen.
  - **Show / Hide Original Values** — toggles inline display of original field values (same side-by-side pattern as the `record-detail` policy).
- The "Cancel" label is replaced with **Discard** throughout both the tabular edit screen and the record-detail layout.
- **Benefit**: Users can confidently review and abandon changes; the save/discard pattern is consistent across all edit surfaces.

---

## Summary of User-Facing Benefits

| Req | Change | Benefit |
|-----|--------|---------|
| 4.1.1 | No inline editing; row click → Edit Record screen | Cleaner table; consistent edit UX |
| 4.1.2 | Screen sizing fixed | No overflow beyond the card |
| 4.1.3 | Compact grid (12px font, minimal padding) | More rows visible at once |
| 4.1.4 | Action buttons in header; icon-only | Saves vertical space |
| 4.1.5 | Horizontal scrollbar | All columns always accessible |
| 4.1.6 | Delete → 🗑 icon | Narrower Actions column |
| 4.1.7 | Reload re-fetches data | Fresh data on demand |
| 4.1.8 | Compact filter with placeholder | Smaller footprint, clearer purpose |
| 4.1.9 | Pagination « ‹ › » icons | More compact navigation |
| 4.1.10–11 | Sort tooltips + ↑ ↓ indicators | Clear sort state at a glance |
| 4.1.12 | Per-line layout in edit screen | Properly spaced, readable form |
| 4.1.13 | Save / Discard / Show Original Values | Safe, informed editing |
