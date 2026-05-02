# This file has my requirements for adding new screen attributes and field types.

**Req 5.1.** Each screen should have a new mandatory attribute called "key". It can be left empty if there is no unique key for a record or not known at this point Or the value could be the list of field-ids (1 or more). NOTE -- change all existing screen configs to add this new attribute with empty value.

**Req 5.2.** When a screen displays multiple records in tabular or other layout, and selecting a record for edit/delete via different screen - we need to pass the record id - so that new screen can save/edit the record to persistent layer. This record id should be displayed on right top of new screen used for further updates.

**Req 5.3.** For defining actions about "selection" (click/double-click) on a record, the child screen id to handle record-details should be mentioned along with record-id.

**Req 5.4.** In `selection-actions`, rename attribute `target-screen` to `target-screen-id` and use child screen `id` value instead of screen file name in all screen config files.

**Req 5.5.** Reduce vertical spacing in screen display for all screens irrespective of sections and layout policies. Specifically, reduce space between top border and screen title, and reduce space between screen title and first section so UI feels compact and uses more display area.

