import { CommonModule } from '@angular/common';
import { Component, EventEmitter, inject, Input, OnChanges, Output, signal, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ScreenRenderFieldModel, ScreenRenderSectionModel } from '../models/screen.models';
import { LayoutPolicyService } from '../services/layout-policy.service';

type TTabularRow = Record<string, string>;
type TSortDirection = 'asc' | 'desc';

@Component({
  selector: 'app-section-renderer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="section-card" [class.borderless]="!section.showBorder">
      @if (section.showBorder) {
        <div class="section-header-bar">
          <button type="button" class="section-header" (click)="toggle()" [disabled]="!section.isCollapsible">
            <span>{{ section.name }}</span>
            @if (section.isCollapsible) { <span class="section-collapse-icon">{{ collapsed() ? '+' : '-' }}</span> }
          </button>
          @if (isTabularLayout && !editingRow()) {
            <div class="tabular-header-actions">
              <button type="button" class="tabular-icon-btn" title="Add New Row" (click)="startAddNewRow()">＋</button>
              <button type="button" class="tabular-icon-btn" title="Export Filtered Rows as CSV" (click)="exportCsv(true)">↓F</button>
              <button type="button" class="tabular-icon-btn" title="Export All Rows as CSV" (click)="exportCsv(false)">↓A</button>
            </div>
          }
        </div>
      }

      <div class="section-body" [class]="layoutCssClass" [class.hidden]="collapsed()">
        @if (isTabularLayout) {
          <div class="tabular-shell">
            @if (editingRow(); as objEditingRow) {
              <div class="tabular-edit-screen">
                <div class="tabular-edit-header">
                  <h3>{{ isCreateRowMode() ? 'Add New Row' : 'Edit Record' }}</h3>
                </div>

                <div class="tabular-edit-fields">
                  @for (objField of section.fields; track objField.id) {
                    @if (!objField.isActionField) {
                      <div class="field-row tabular-edit-field-row">
                      <label class="field-label tabular-edit-field-label">
                        <span class="field-name tabular-edit-field-name">{{ objField.name }}</span>
                        @if (objField.controlType === 'textarea') {
                          <textarea
                            class="field-input"
                            [rows]="objField.lines"
                            [placeholder]="objField.description"
                            [title]="objField.description"
                            [ngModel]="objEditingRow[objField.id]"
                            (ngModelChange)="updateCellValue(objEditingRow, objField.id, $event)"
                            [attr.minlength]="objField.minChars > 0 ? objField.minChars : null"
                            [attr.maxlength]="objField.maxChars > 0 ? objField.maxChars : null"
                          ></textarea>
                        } @else if (objField.controlType === 'select') {
                          <div class="lookup-search-wrapper">
                            <input class="lookup-search-input" type="text" placeholder="Search options…"
                              [value]="getLookupSearch(objField.id)"
                              (input)="setLookupSearch(objField.id, $any($event.target).value)" />
                            <select
                              class="field-input"
                              [title]="objField.description"
                              [ngModel]="objEditingRow[objField.id]"
                              (ngModelChange)="updateCellValue(objEditingRow, objField.id, $event)"
                            >
                              @for (opt of getFilteredLookupOptions(objField); track opt.value) {
                                <option [value]="opt.value" [title]="opt.description">
                                  {{ opt.value }}{{ opt.description ? ' — ' + opt.description : '' }}
                                </option>
                              }
                            </select>
                          </div>
                        } @else if (objField.controlType === 'multiselect') {
                          <div class="lookup-search-wrapper">
                            <input class="lookup-search-input" type="text" placeholder="Search options…"
                              [value]="getLookupSearch(objField.id)"
                              (input)="setLookupSearch(objField.id, $any($event.target).value)" />
                            <div class="lookup-multi-options">
                              @for (opt of getFilteredLookupOptions(objField); track opt.value) {
                                <label class="lookup-multi-option">
                                  <input type="checkbox"
                                    [checked]="isMultiSelected(objEditingRow, objField.id, opt.value)"
                                    (change)="toggleMultiCellValue(objEditingRow, objField.id, opt.value)" />
                                  @if (opt.image) { <img class="lookup-option-img" [src]="opt.image" [alt]="opt.value" /> }
                                  <span>{{ opt.value }}{{ opt.description ? ' — ' + opt.description : '' }}</span>
                                </label>
                              }
                            </div>
                            <div class="lookup-tags">
                              @for (strTag of getMultiCellValues(objEditingRow, objField.id); track strTag) {
                                <span class="lookup-tag">{{ strTag }}
                                  <button type="button" class="lookup-tag-remove" (click)="toggleMultiCellValue(objEditingRow, objField.id, strTag)">×</button>
                                </span>
                              }
                            </div>
                          </div>
                        } @else {
                          <input
                            class="field-input"
                            [type]="objField.inputType"
                            [placeholder]="objField.description"
                            [title]="objField.description"
                            [ngModel]="objEditingRow[objField.id]"
                            (ngModelChange)="updateCellValue(objEditingRow, objField.id, $event)"
                            [attr.minlength]="objField.minChars > 0 ? objField.minChars : null"
                            [attr.maxlength]="objField.maxChars > 0 ? objField.maxChars : null"
                          />
                        }
                      </label>
                        @if (tabularShowOriginalValues() && !isCreateRowMode()) {
                          <span class="record-detail-original-value" [title]="'Original: ' + tabularEditOriginal()[objField.id]">
                            Original: <em>{{ tabularEditOriginal()[objField.id] || '(empty)' }}</em>
                          </span>
                        }
                      </div>
                    }
                  }
                </div>

                <div class="tabular-edit-actions">
                  @if (isCreateRowMode()) {
                    <button type="button" class="record-detail-save-btn" (click)="saveNewRow()">Save</button>
                    <button type="button" class="record-detail-cancel-btn" (click)="cancelNewRow()">Discard</button>
                  } @else {
                    <button type="button" class="record-detail-save-btn" (click)="saveEditRow()">Save</button>
                    <button type="button" class="record-detail-cancel-btn" (click)="discardEditRow()">Discard</button>
                    <button type="button" class="record-detail-toggle-orig-btn" (click)="toggleTabularShowOriginalValues()">
                      {{ tabularShowOriginalValues() ? 'Hide Original Values' : 'Show Original Values' }}
                    </button>
                  }
                </div>
              </div>
            } @else {

              <div class="tabular-scroll">
                <table class="tabular-table" [attr.aria-label]="section.name + ' table view'">
                <thead>
                  <tr>
                    @for (objField of section.fields; track objField.id) {
                      <th [style.min-width]="objField.width">
                        <button
                          type="button"
                          class="tabular-sort-button"
                          [disabled]="objField.isActionField"
                          [title]="objField.isActionField ? '' : getSortTitle(objField.id)"
                          (click)="sortByColumn(objField.id)"
                        >
                          <span>{{ objField.name }}</span>
                          @if (!objField.isActionField && getSortIndicator(objField.id)) {
                            <span class="tabular-sort-indicator">{{ getSortIndicator(objField.id) }}</span>
                          }
                        </button>
                      </th>
                    }
                    <th class="tabular-action-header">Actions</th>
                  </tr>
                  <tr class="tabular-filter-row">
                    @for (objField of section.fields; track objField.id) {
                      <th [style.min-width]="objField.width">
                        @if (!objField.isActionField) {
                          <input
                            class="tabular-filter-input"
                            type="text"
                            [placeholder]="'Filter by ' + objField.name + '…'"
                            [value]="getColumnFilter(objField.id)"
                            (input)="setColumnFilter(objField.id, $any($event.target).value)"
                          />
                        }
                      </th>
                    }
                    <th class="tabular-action-header"></th>
                  </tr>
                </thead>
                <tbody>
                  @for (objRow of pagedTabularRows(); track $index) {
                  <tr class="tabular-data-row" (click)="openEditRow(objRow)">
                    @for (objField of section.fields; track objField.id) {
                      <td class="tabular-cell" [style.min-width]="objField.width">
                        @if (objField.isActionField) {
                          <button type="button" class="field-action" [title]="objField.description" (click)="$event.stopPropagation(); emitAction(objField)">
                            {{ objField.name }}
                          </button>
                        } @else {
                          {{ objRow[objField.id] }}
                        }
                      </td>
                    }
                    <td class="tabular-row-actions">
                      <button type="button" class="tabular-delete-btn" title="Delete row" (click)="$event.stopPropagation(); deleteRow(objRow)">🗑</button>
                    </td>
                  </tr>
                  }
                </tbody>
                </table>
              </div>

              @if (shouldShowPagination()) {
                <div class="tabular-pagination">
                  <button type="button" class="tabular-page-btn" title="First page" [disabled]="!canGoToPreviousPage()" (click)="goToFirstPage()">«</button>
                  <button type="button" class="tabular-page-btn" title="Previous page" [disabled]="!canGoToPreviousPage()" (click)="goToPreviousPage()">‹</button>
                  <span class="tabular-page-status">Page {{ currentPageNumber() }} of {{ totalPageCount() }}</span>
                  <button type="button" class="tabular-page-btn" title="Next page" [disabled]="!canGoToNextPage()" (click)="goToNextPage()">›</button>
                  <button type="button" class="tabular-page-btn" title="Last page" [disabled]="!canGoToNextPage()" (click)="goToLastPage()">»</button>
                </div>
              }
            }
          </div>
        } @else if (isRecordDetailLayout) {
          <div class="record-detail-shell">
            <div class="record-detail-fields">
              @for (objField of section.fields; track objField.id) {
                <div class="field-row">
                  @if (objField.isActionField) {
                    <button type="button" class="field-action" [title]="objField.description" (click)="emitAction(objField)">
                      {{ objField.name }}
                    </button>
                  } @else {
                    <label class="field-label">
                      <span class="field-name">{{ objField.name }}</span>
                      <div class="record-detail-input-group">
                        @if (objField.controlType === 'textarea') {
                          <textarea
                            class="field-input"
                            [style.width]="objField.width"
                            [rows]="objField.lines"
                            [placeholder]="objField.description"
                            [title]="objField.description"
                            [ngModel]="recordDetailValues()[objField.id]"
                            (ngModelChange)="updateRecordDetailField(objField.id, $event)"
                            [attr.minlength]="objField.minChars > 0 ? objField.minChars : null"
                            [attr.maxlength]="objField.maxChars > 0 ? objField.maxChars : null"
                          ></textarea>
                        } @else if (objField.controlType === 'select') {
                          <div class="lookup-search-wrapper">
                            <input class="lookup-search-input" type="text" placeholder="Search options…"
                              [value]="getLookupSearch(objField.id)"
                              (input)="setLookupSearch(objField.id, $any($event.target).value)" />
                            <select class="field-input" [style.width]="objField.width" [title]="objField.description"
                              [ngModel]="recordDetailValues()[objField.id]"
                              (ngModelChange)="updateRecordDetailField(objField.id, $event)">
                              @for (opt of getFilteredLookupOptions(objField); track opt.value) {
                                <option [value]="opt.value" [title]="opt.description">
                                  {{ opt.value }}{{ opt.description ? ' — ' + opt.description : '' }}
                                </option>
                              }
                            </select>
                          </div>
                        } @else if (objField.controlType === 'multiselect') {
                          <div class="lookup-search-wrapper">
                            <input class="lookup-search-input" type="text" placeholder="Search options…"
                              [value]="getLookupSearch(objField.id)"
                              (input)="setLookupSearch(objField.id, $any($event.target).value)" />
                            <div class="lookup-multi-options">
                              @for (opt of getFilteredLookupOptions(objField); track opt.value) {
                                <label class="lookup-multi-option">
                                  <input type="checkbox"
                                    [checked]="isMultiSelectedInRecord(objField.id, opt.value)"
                                    (change)="toggleMultiRecordField(objField.id, opt.value)" />
                                  @if (opt.image) { <img class="lookup-option-img" [src]="opt.image" [alt]="opt.value" /> }
                                  <span>{{ opt.value }}{{ opt.description ? ' — ' + opt.description : '' }}</span>
                                </label>
                              }
                            </div>
                            <div class="lookup-tags">
                              @for (strTag of getMultiRecordValues(objField.id); track strTag) {
                                <span class="lookup-tag">{{ strTag }}
                                  <button type="button" class="lookup-tag-remove" (click)="toggleMultiRecordField(objField.id, strTag)">×</button>
                                </span>
                              }
                            </div>
                          </div>
                        } @else {
                          <input
                            class="field-input"
                            [style.width]="objField.width"
                            [type]="objField.inputType"
                            [placeholder]="objField.description"
                            [title]="objField.description"
                            [ngModel]="recordDetailValues()[objField.id]"
                            (ngModelChange)="updateRecordDetailField(objField.id, $event)"
                            [attr.minlength]="objField.minChars > 0 ? objField.minChars : null"
                            [attr.maxlength]="objField.maxChars > 0 ? objField.maxChars : null"
                          />
                        }
                        @if (showOriginalValues() && recordDetailOriginal()[objField.id] !== undefined) {
                          <span class="record-detail-original-value" [title]="'Original: ' + recordDetailOriginal()[objField.id]">
                            Original: <em>{{ recordDetailOriginal()[objField.id] || '(empty)' }}</em>
                          </span>
                        }
                      </div>
                    </label>
                  }
                  @if (objField.description) {
                    <button type="button" class="field-info-btn" [title]="objField.description" aria-label="Field description" tabindex="-1">ℹ</button>
                  }
                </div>
              }
            </div>
            <div class="record-detail-actions">
              <button type="button" class="record-detail-save-btn" (click)="saveRecordDetail()">Save</button>
              <button type="button" class="record-detail-cancel-btn" (click)="cancelRecordDetail()">Discard</button>
              <button type="button" class="record-detail-toggle-orig-btn" (click)="toggleShowOriginalValues()">
                {{ showOriginalValues() ? 'Hide Original Values' : 'Show Original Values' }}
              </button>
            </div>
          </div>
        } @else {
          @for (objField of section.fields; track objField.id) {
            <div class="field-row">
              @if (objField.isActionField) {
                <button type="button" class="field-action" [title]="objField.description" (click)="emitAction(objField)">
                  {{ objField.name }}
                </button>
              } @else {
                <label class="field-label">
                  <span class="field-name">{{ objField.name }}</span>
                  @if (objField.controlType === 'textarea') {
                    <textarea
                      class="field-input"
                      [style.width]="objField.width"
                      [rows]="objField.lines"
                      [placeholder]="objField.description"
                      [title]="objField.description"
                      [attr.minlength]="objField.minChars > 0 ? objField.minChars : null"
                      [attr.maxlength]="objField.maxChars > 0 ? objField.maxChars : null"
                    ></textarea>
                  } @else if (objField.controlType === 'select') {
                    <div class="lookup-search-wrapper">
                      <input class="lookup-search-input" type="text" placeholder="Search options…"
                        [value]="getLookupSearch(objField.id)"
                        (input)="setLookupSearch(objField.id, $any($event.target).value)" />
                      <select class="field-input" [style.width]="objField.width" [title]="objField.description">
                        @for (strLookupValue of getFilteredLookupValues(objField); track strLookupValue) {
                          <option [value]="strLookupValue">{{ strLookupValue }}</option>
                        }
                      </select>
                    </div>
                  } @else if (objField.controlType === 'multiselect') {
                    <div class="lookup-search-wrapper">
                      <input class="lookup-search-input" type="text" placeholder="Search options…"
                        [value]="getLookupSearch(objField.id)"
                        (input)="setLookupSearch(objField.id, $any($event.target).value)" />
                      <div class="lookup-multi-options">
                        @for (strLookupValue of getFilteredLookupValues(objField); track strLookupValue) {
                          <label class="lookup-multi-option">
                            <input type="checkbox" />
                            {{ strLookupValue }}
                          </label>
                        }
                      </div>
                    </div>
                  } @else {
                    <input
                      class="field-input"
                      [style.width]="objField.width"
                      [type]="objField.inputType"
                      [placeholder]="objField.description"
                      [title]="objField.description"
                      [attr.minlength]="objField.minChars > 0 ? objField.minChars : null"
                      [attr.maxlength]="objField.maxChars > 0 ? objField.maxChars : null"
                    />
                  }
                </label>
              }
              @if (objField.description) {
                <button type="button" class="field-info-btn" [title]="objField.description" aria-label="Field description" tabindex="-1">ℹ</button>
              }
            </div>
          }
        }

        @for (objSection of section.sections; track objSection.name) {
          <app-section-renderer [section]="objSection" (actionInvoked)="forwardAction($event)"></app-section-renderer>
        }
      </div>
    </section>
  `,
  styles: [
    `
      .section-card {
        border: 1px solid rgba(94, 63, 34, 0.16);
        border-radius: 20px;
        background: rgba(255, 255, 255, 0.7);
        overflow: hidden;
        min-width: 0;
        box-sizing: border-box;
      }

      .section-card.borderless {
        border: none;
        background: transparent;
      }

      .section-header-bar {
        display: flex;
        align-items: stretch;
        background: rgba(216, 192, 163, 0.4);
      }

      .section-header {
        flex: 1;
        display: flex;
        justify-content: space-between;
        gap: 16px;
        padding: 14px 18px;
        border: none;
        background: transparent;
        color: inherit;
      }

      .section-collapse-icon {
        flex-shrink: 0;
      }

      .tabular-header-actions {
        display: flex;
        align-items: center;
        gap: 4px;
        padding: 4px 10px;
      }

      .tabular-icon-btn {
        border: 1px solid rgba(94, 63, 34, 0.24);
        background: rgba(255, 255, 255, 0.6);
        color: #4f4135;
        border-radius: 6px;
        width: 30px;
        height: 30px;
        font-size: 14px;
        font-weight: 700;
        line-height: 1;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
      }

      .tabular-icon-btn:hover {
        background: rgba(216, 192, 163, 0.6);
      }

      .section-body {
        display: grid;
        grid-template-columns: minmax(0, 1fr);
        gap: 16px;
        padding: 18px;
      }

      .section-body.layout-per-line {
        display: grid;
      }

      .section-body.layout-flow {
        display: flex;
        flex-wrap: wrap;
        justify-content: flex-start;
        align-items: flex-start;
        column-gap: 125px;
        row-gap: 12px;
      }

      .section-body.layout-flow > .field-row {
        flex: 0 0 auto;
        display: flex;
        flex-direction: column;
        align-items: flex-start;
        gap: 6px;
      }

      .section-body.layout-flow > .field-row > .field-label {
        min-width: max-content;
      }

      .section-body.layout-tabular {
        display: block;
        padding: 0;
        overflow: hidden;
      }

      .tabular-shell {
        width: 100%;
        min-width: 0;
        box-sizing: border-box;
        overflow: hidden;
      }

      .tabular-toolbar {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        margin-bottom: 10px;
      }

      .tabular-toolbar-btn {
        border: 1px solid rgba(94, 63, 34, 0.24);
        background: #f8efe2;
        color: #4f4135;
        border-radius: 8px;
        padding: 7px 10px;
        font: inherit;
        font-weight: 600;
      }

      .tabular-edit-screen {
        border: 1px solid rgba(94, 63, 34, 0.18);
        border-radius: 12px;
        background: #fffaf2;
        padding: 14px;
        margin: 8px;
      }

      .tabular-edit-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 10px;
        margin-bottom: 14px;
      }

      .tabular-edit-header h3 {
        margin: 0;
      }

      .tabular-edit-fields {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
      }

      .tabular-edit-field-row {
        display: flex;
        flex-direction: column;
        gap: 4px;
      }

      .tabular-edit-actions {
        display: flex;
        gap: 8px;
        margin-top: 12px;
      }

      .tabular-edit-field-label {
        display: flex;
        align-items: baseline;
        flex-wrap: nowrap;
        gap: 12px;
        font-weight: 700;
        min-width: 0;
      }

      .tabular-edit-field-name {
        flex: 0 0 160px;
        font-weight: 700;
      }

      .tabular-scroll {
        width: 100%;
        max-height: 360px;
        overflow-x: auto;
        overflow-y: auto;
      }

      .tabular-table {
        border-collapse: collapse;
        width: max-content;
        min-width: 100%;
      }

      .tabular-table th,
      .tabular-table td {
        padding: 2px 6px;
        border-bottom: 1px solid rgba(94, 63, 34, 0.12);
        vertical-align: middle;
        font-size: 12px;
        line-height: 1.2;
      }

      .tabular-cell {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        max-width: 200px;
      }

      .tabular-data-row {
        cursor: pointer;
      }

      .tabular-data-row:hover {
        background: rgba(216, 192, 163, 0.2);
      }

      .tabular-table th {
        position: sticky;
        top: 0;
        z-index: 1;
        background: #f8efe2;
        padding: 0;
      }

      .tabular-sort-button {
        width: 100%;
        border: none;
        background: transparent;
        color: #4f4135;
        text-align: left;
        font-weight: 700;
        font-size: 12px;
        white-space: nowrap;
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 4px;
        padding: 4px 6px;
        cursor: pointer;
      }

      .tabular-sort-button:disabled {
        opacity: 0.65;
        cursor: default;
      }

      .tabular-sort-indicator {
        color: #8f4e2f;
        min-width: 14px;
        text-align: center;
      }

      .tabular-action-header {
        min-width: 90px;
      }

      .tabular-filter-row th {
        background: #fff7ee;
        padding: 2px 4px;
      }

      .tabular-filter-input {
        width: 100%;
        min-width: 80px;
        border-radius: 6px;
        border: 1px solid rgba(94, 63, 34, 0.24);
        padding: 2px 5px;
        font-size: 11px;
        height: 22px;
        box-sizing: border-box;
      }

      .tabular-pagination {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 4px;
        margin-top: 4px;
        padding: 4px 8px;
      }

      .tabular-page-btn {
        border: 1px solid rgba(94, 63, 34, 0.22);
        background: #fffaf2;
        border-radius: 6px;
        padding: 2px 8px;
        min-width: 28px;
        font-size: 13px;
        font: inherit;
        line-height: 1.4;
      }

      .tabular-page-btn:disabled {
        opacity: 0.55;
      }

      .tabular-page-status {
        color: #5a4b3e;
        font-weight: 600;
      }

      .tabular-row-actions {
        text-align: right;
      }

      .tabular-delete-btn {
        border: none;
        background: none;
        color: #8a2525;
        border-radius: 6px;
        padding: 2px 4px;
        font-size: 15px;
        line-height: 1;
        cursor: pointer;
      }

      .tabular-delete-btn:hover {
        background: rgba(145, 39, 39, 0.1);
      }

      .tabular-table td .field-input,
      .tabular-table td .field-action {
        width: 100%;
        min-width: 80px;
      }

      /* ── record-detail layout ── */
      .record-detail-shell {
        display: flex;
        flex-direction: column;
        gap: 16px;
        padding: 18px;
      }

      .record-detail-fields {
        display: flex;
        flex-direction: column;
        gap: 12px;
      }

      .record-detail-shell .field-row .field-label {
        display: flex;
        align-items: baseline;
        flex-wrap: nowrap;
        gap: 12px;
        font-weight: 700;
        min-width: 0;
      }

      .record-detail-shell .field-name {
        flex: 0 0 160px;
      }

      .record-detail-actions {
        display: flex;
        gap: 10px;
        flex-wrap: wrap;
      }

      .record-detail-save-btn {
        border: 1px solid rgba(63, 94, 34, 0.35);
        background: #f2fff4;
        color: #2a5a2e;
        border-radius: 8px;
        padding: 8px 18px;
        font: inherit;
        font-weight: 600;
      }

      .record-detail-cancel-btn {
        border: 1px solid rgba(94, 63, 34, 0.22);
        background: #fffaf2;
        border-radius: 8px;
        padding: 8px 18px;
        font: inherit;
      }

      .record-detail-toggle-orig-btn {
        border: 1px solid rgba(63, 94, 120, 0.35);
        background: #f2f7ff;
        color: #2a4a6a;
        border-radius: 8px;
        padding: 8px 18px;
        font: inherit;
      }

      .record-detail-input-group {
        display: flex;
        flex-direction: column;
        gap: 4px;
        flex: 1 1 auto;
      }

      .record-detail-original-value {
        font-size: 0.82em;
        color: #6b5b4e;
        padding: 2px 6px;
        background: rgba(255, 240, 200, 0.6);
        border-radius: 6px;
        border-left: 3px solid rgba(180, 130, 50, 0.5);
      }

      /* ── lookup search & multi-select (Req 3.3) ── */
      .lookup-search-wrapper {
        display: flex;
        flex-direction: column;
        gap: 4px;
        flex: 1 1 auto;
      }

      .lookup-search-input {
        font: inherit;
        border-radius: 8px;
        border: 1px solid rgba(94, 63, 34, 0.22);
        padding: 6px 10px;
        font-size: 0.88em;
      }

      .lookup-multi-options {
        display: flex;
        flex-direction: column;
        gap: 4px;
        max-height: 160px;
        overflow-y: auto;
        border: 1px solid rgba(94, 63, 34, 0.18);
        border-radius: 8px;
        padding: 6px 8px;
        background: #fffdf8;
      }

      .lookup-multi-option {
        display: flex;
        align-items: center;
        gap: 6px;
        font-weight: 400;
        cursor: pointer;
      }

      .lookup-option-img {
        width: 24px;
        height: 24px;
        object-fit: cover;
        border-radius: 4px;
        flex-shrink: 0;
      }

      .lookup-tags {
        display: flex;
        flex-wrap: wrap;
        gap: 4px;
      }

      .lookup-tag {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        background: rgba(143, 78, 47, 0.12);
        border: 1px solid rgba(143, 78, 47, 0.25);
        color: #5a2e0e;
        border-radius: 20px;
        padding: 2px 8px;
        font-size: 0.85em;
      }

      .lookup-tag-remove {
        background: none;
        border: none;
        color: #8a3a1a;
        cursor: pointer;
        font-size: 1em;
        line-height: 1;
        padding: 0 2px;
      }

      .section-body.hidden {
        display: none;
      }

      .field-row {
        display: grid;
        grid-template-columns: minmax(0, 1fr);
        gap: 6px;
        align-items: start;
        min-width: 0;
      }

      .field-row .field-label {
        display: flex;
        align-items: center;
        flex-wrap: nowrap;
        gap: 12px;
        font-weight: 700;
        min-width: 0;
      }

      .field-name {
        flex: 0 0 auto;
        line-height: 1.2;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: clip;
      }

      .section-body.layout-per-line .field-name {
        flex: 0 0 160px;
      }

      .field-input {
        flex: 0 0 auto;
        min-width: 0;
      }

      input,
      textarea,
      select,
      .field-action {
        font: inherit;
        border-radius: 12px;
        border: 1px solid rgba(94, 63, 34, 0.22);
        padding: 10px 12px;
      }

      textarea {
        resize: vertical;
      }

      .field-action {
        width: fit-content;
        background: #8f4e2f;
        color: #fffaf2;
      }

      .field-description {
        color: #65584d;
        opacity: 0;
        max-height: 0;
        overflow: hidden;
        transform: translateY(-2px);
        transition: opacity 0.15s ease, max-height 0.15s ease, transform 0.15s ease;
      }

      .field-row:hover .field-description,
      .field-row:focus-within .field-description {
        opacity: 1;
        max-height: 40px;
        transform: translateY(0);
      }

      .field-info-btn {
        background: none;
        border: none;
        color: #7a6657;
        font-size: 12px;
        line-height: 1;
        cursor: default;
        padding: 0 3px;
        border-radius: 50%;
        flex-shrink: 0;
        position: relative;
      }

      .field-info-btn:hover {
        color: #4f4135;
        background: rgba(94, 63, 34, 0.1);
      }

      @media (max-width: 1024px) {
        .section-body {
          padding: 14px;
        }

        .section-body.layout-per-line .field-name {
          flex: 0 0 130px;
        }
      }

      @media (max-width: 700px) {
        .section-body.layout-flow > .field-row {
          flex: 1 1 100%;
        }

        .field-row:hover .field-description,
        .field-row:focus-within .field-description {
          max-height: 64px;
        }
      }
    `
  ]
})
export class SectionRendererComponent implements OnChanges {
  private readonly m_itfLayoutPolicyService = inject(LayoutPolicyService);
  private m_strTabularShapeSignature = '';

  @Input({ required: true }) section!: ScreenRenderSectionModel;
  @Output() readonly actionInvoked = new EventEmitter<string>();

  readonly collapsed = signal(false);
  readonly allTabularRows = signal<TTabularRow[]>([]);
  readonly columnFilters = signal<Record<string, string>>({});
  readonly sortColumnId = signal<string | null>(null);
  readonly sortDirection = signal<TSortDirection>('asc');
  readonly currentPageNumber = signal(1);
  readonly editingRow = signal<TTabularRow | null>(null);
  readonly isCreateRowMode = signal(false);
  readonly pageSize = 50;

  // ── record-detail state ──
  readonly recordDetailValues = signal<Record<string, string>>({});
  readonly showOriginalValues = signal(false);
  private m_dictRecordDetailOriginal: Record<string, string> = {};

  // ── tabular edit state (Req 4.1) ──
  readonly tabularShowOriginalValues = signal(false);
  private m_dictTabularEditOriginal: Record<string, string> = {};

  // ── lookup search state (Req 3.3.1) ──
  readonly lookupSearchTerms = signal<Record<string, string>>({});

  recordDetailOriginal(): Record<string, string> {
    return this.m_dictRecordDetailOriginal;
  }

  tabularEditOriginal(): Record<string, string> {
    return this.m_dictTabularEditOriginal;
  }

  ngOnChanges(objChanges: SimpleChanges): void {
    if (!objChanges['section']) {
      return;
    }

    if (this.isTabularLayout) {
      let strCurrentShapeSignature = this.section.fields.map(objField => objField.id).join('|');
      if (this.m_strTabularShapeSignature !== strCurrentShapeSignature || objChanges['section'].firstChange) {
        this.m_strTabularShapeSignature = strCurrentShapeSignature;
        this.columnFilters.set({});
        this.sortColumnId.set(null);
        this.sortDirection.set('asc');
        this.currentPageNumber.set(1);
        this.allTabularRows.set(this.createInitialRows());
      }
    } else if (this.isRecordDetailLayout) {
      let dictInitial: Record<string, string> = {};
      for (let objField of this.section.fields) {
        if (!objField.isActionField) {
          dictInitial[objField.id] = '';
        }
      }

      this.m_dictRecordDetailOriginal = { ...dictInitial };
      this.recordDetailValues.set({ ...dictInitial });
    }
  }

  get layoutCssClass(): string {
    return 'section-body ' + this.m_itfLayoutPolicyService.getCssClass(this.section.layoutPolicy);
  }

  get isTabularLayout(): boolean {
    return this.section.layoutPolicy === 'tabular';
  }

  get isRecordDetailLayout(): boolean {
    return this.section.layoutPolicy === 'record-detail';
  }

  updateRecordDetailField(strFieldId: string, strValue: string): void {
    this.recordDetailValues.update((dictValues) => ({ ...dictValues, [strFieldId]: strValue }));
  }

  saveRecordDetail(): void {
    // Persist current values as the new baseline; downstream consumers can subscribe to actionInvoked.
    this.m_dictRecordDetailOriginal = { ...this.recordDetailValues() };
    this.actionInvoked.emit('save');
  }

  cancelRecordDetail(): void {
    this.recordDetailValues.set({ ...this.m_dictRecordDetailOriginal });
    this.actionInvoked.emit('cancel');
  }

  toggleShowOriginalValues(): void {
    this.showOriginalValues.update(fShow => !fShow);
  }

  // ── lookup search helpers (Req 3.3.1) ──
  setLookupSearch(strFieldId: string, strTerm: string): void {
    this.lookupSearchTerms.update(dict => ({ ...dict, [strFieldId]: strTerm }));
  }

  getLookupSearch(strFieldId: string): string {
    return this.lookupSearchTerms()[strFieldId] ?? '';
  }

  getFilteredLookupValues(objField: ScreenRenderFieldModel): string[] {
    let strTerm = this.getLookupSearch(objField.id).trim().toLowerCase();
    if (!strTerm) {
      return objField.lookupValues;
    }

    return objField.lookupValues.filter(strValue => strValue.toLowerCase().includes(strTerm));
  }

  getFilteredLookupOptions(objField: ScreenRenderFieldModel): { value: string; description: string; image: string }[] {
    let strTerm = this.getLookupSearch(objField.id).trim().toLowerCase();
    return objField.lookupValues
      .map((strValue, iIndex) => ({
        value: strValue,
        description: objField.lookupOptionDescriptions?.[iIndex] ?? '',
        image: objField.lookupOptionImages?.[iIndex] ?? ''
      }))
      .filter(opt => !strTerm || opt.value.toLowerCase().includes(strTerm) || opt.description.toLowerCase().includes(strTerm));
  }

  // ── multi-select helpers for tabular rows ──
  isMultiSelected(objRow: TTabularRow, strFieldId: string, strValue: string): boolean {
    return this.getMultiCellValues(objRow, strFieldId).includes(strValue);
  }

  getMultiCellValues(objRow: TTabularRow, strFieldId: string): string[] {
    let strRaw = objRow[strFieldId] ?? '';
    return strRaw ? strRaw.split('|').filter(s => s.length > 0) : [];
  }

  toggleMultiCellValue(objRow: TTabularRow, strFieldId: string, strValue: string): void {
    let lstCurrent = this.getMultiCellValues(objRow, strFieldId);
    let lstUpdated = lstCurrent.includes(strValue)
      ? lstCurrent.filter(s => s !== strValue)
      : [...lstCurrent, strValue];
    objRow[strFieldId] = lstUpdated.join('|');
  }

  // ── multi-select helpers for record-detail ──
  isMultiSelectedInRecord(strFieldId: string, strValue: string): boolean {
    return this.getMultiRecordValues(strFieldId).includes(strValue);
  }

  getMultiRecordValues(strFieldId: string): string[] {
    let strRaw = this.recordDetailValues()[strFieldId] ?? '';
    return strRaw ? strRaw.split('|').filter(s => s.length > 0) : [];
  }

  toggleMultiRecordField(strFieldId: string, strValue: string): void {
    let lstCurrent = this.getMultiRecordValues(strFieldId);
    let lstUpdated = lstCurrent.includes(strValue)
      ? lstCurrent.filter(s => s !== strValue)
      : [...lstCurrent, strValue];
    this.updateRecordDetailField(strFieldId, lstUpdated.join('|'));
  }

  getSortIndicator(strColumnId: string): string {
    if (this.sortColumnId() !== strColumnId) {
      return '';
    }

    return this.sortDirection() === 'asc' ? '↑' : '↓';
  }

  getSortTitle(strColumnId: string): string {
    if (this.sortColumnId() === strColumnId) {
      return this.sortDirection() === 'asc' ? 'Sorted ascending — click to sort descending' : 'Sorted descending — click to remove sort';
    }

    return 'Click to sort ascending';
  }

  sortByColumn(strColumnId: string): void {
    if (!this.isTabularLayout) {
      return;
    }

    let strCurrentSortColumnId = this.sortColumnId();
    let strNextSortDirection: TSortDirection = 'asc';
    if (strCurrentSortColumnId === strColumnId) {
      strNextSortDirection = this.sortDirection() === 'asc' ? 'desc' : 'asc';
    }

    this.sortColumnId.set(strColumnId);
    this.sortDirection.set(strNextSortDirection);
    this.currentPageNumber.set(1);
  }

  displayedTabularRows(): TTabularRow[] {
    let lstRows = [...this.allTabularRows()];
    let dictFilters = this.columnFilters();
    let lstFilterColumns = Object.keys(dictFilters).filter(strFilterColumnId => (dictFilters[strFilterColumnId] ?? '').trim().length > 0);
    if (lstFilterColumns.length > 0) {
      lstRows = lstRows.filter((objRow) => {
        return lstFilterColumns.every((strFilterColumnId) => {
          let strFilterValue = (dictFilters[strFilterColumnId] ?? '').trim().toLowerCase();
          let strCellValue = String(objRow[strFilterColumnId] ?? '');
          return strCellValue.toLowerCase().includes(strFilterValue);
        });
      });
    }

    let strSortColumnId = this.sortColumnId();
    if (!strSortColumnId) {
      return lstRows;
    }

    let strSortDirection = this.sortDirection();
    return [...lstRows].sort((objLeftRow, objRightRow) =>
      this.compareValues(objLeftRow[strSortColumnId] ?? '', objRightRow[strSortColumnId] ?? '', strSortDirection)
    );
  }

  pagedTabularRows(): TTabularRow[] {
    let lstFilteredRows = this.displayedTabularRows();
    let iPageStartIndex = (this.currentPageNumber() - 1) * this.pageSize;
    return lstFilteredRows.slice(iPageStartIndex, iPageStartIndex + this.pageSize);
  }

  totalPageCount(): number {
    let iRowCount = this.displayedTabularRows().length;
    return Math.max(1, Math.ceil(iRowCount / this.pageSize));
  }

  shouldShowPagination(): boolean {
    return this.displayedTabularRows().length > this.pageSize;
  }

  canGoToPreviousPage(): boolean {
    return this.currentPageNumber() > 1;
  }

  canGoToNextPage(): boolean {
    return this.currentPageNumber() < this.totalPageCount();
  }

  goToFirstPage(): void {
    this.currentPageNumber.set(1);
  }

  goToPreviousPage(): void {
    if (!this.canGoToPreviousPage()) {
      return;
    }

    this.currentPageNumber.update(iCurrentPage => iCurrentPage - 1);
  }

  goToNextPage(): void {
    if (!this.canGoToNextPage()) {
      return;
    }

    this.currentPageNumber.update(iCurrentPage => iCurrentPage + 1);
  }

  goToLastPage(): void {
    this.currentPageNumber.set(this.totalPageCount());
  }

  openEditRow(objRow: TTabularRow): void {
    this.isCreateRowMode.set(false);
    this.m_dictTabularEditOriginal = { ...objRow };
    this.tabularShowOriginalValues.set(false);
    this.editingRow.set(objRow);
  }

  closeEditRow(): void {
    this.isCreateRowMode.set(false);
    this.editingRow.set(null);
  }

  saveEditRow(): void {
    this.m_dictTabularEditOriginal = { ...this.editingRow()! };
    this.editingRow.set(null);
  }

  discardEditRow(): void {
    let objEditingRow = this.editingRow();
    if (!objEditingRow) {
      return;
    }

    this.allTabularRows.update((lstRows) => {
      let iIndex = lstRows.findIndex(objRow => objRow === objEditingRow);
      if (iIndex < 0) {
        return lstRows;
      }

      let lstUpdated = [...lstRows];
      lstUpdated[iIndex] = { ...this.m_dictTabularEditOriginal };
      return lstUpdated;
    });

    this.editingRow.set(null);
  }

  toggleTabularShowOriginalValues(): void {
    this.tabularShowOriginalValues.update(fShow => !fShow);
  }

  startAddNewRow(): void {
    this.isCreateRowMode.set(true);
    this.editingRow.set(this.createBlankRow());
  }

  saveNewRow(): void {
    let objEditingRow = this.editingRow();
    if (!objEditingRow || !this.isCreateRowMode()) {
      return;
    }

    this.allTabularRows.update((lstRows) => [...lstRows, { ...objEditingRow }]);
    this.closeEditRow();
    this.goToLastPage();
  }

  cancelNewRow(): void {
    this.closeEditRow();
  }

  setColumnFilter(strColumnId: string, strFilterValue: string): void {
    this.columnFilters.update((dictCurrentFilters) => ({
      ...dictCurrentFilters,
      [strColumnId]: strFilterValue
    }));
    this.currentPageNumber.set(1);
  }

  getColumnFilter(strColumnId: string): string {
    return this.columnFilters()[strColumnId] ?? '';
  }

  exportCsv(fFilteredOnly: boolean): void {
    let lstExportFields = this.section.fields.filter(objField => !objField.isActionField);
    if (lstExportFields.length === 0) {
      return;
    }

    let lstRowsToExport = fFilteredOnly ? this.displayedTabularRows() : this.allTabularRows();
    let lstCsvLines: string[] = [];

    lstCsvLines.push(lstExportFields.map(objField => this.escapeCsvCell(objField.name)).join(','));
    for (let objRow of lstRowsToExport) {
      let lstCsvCells = lstExportFields.map(objField => this.escapeCsvCell(String(objRow[objField.id] ?? '')));
      lstCsvLines.push(lstCsvCells.join(','));
    }

    let strCsvContents = lstCsvLines.join('\r\n');
    let objCsvBlob = new Blob([strCsvContents], { type: 'text/csv;charset=utf-8;' });
    let strObjectUrl = URL.createObjectURL(objCsvBlob);

    let objAnchorElement = document.createElement('a');
    objAnchorElement.href = strObjectUrl;
    objAnchorElement.download = this.section.name + (fFilteredOnly ? '-filtered' : '-all') + '.csv';
    objAnchorElement.click();
    URL.revokeObjectURL(strObjectUrl);
  }

  deleteRow(objCurrentRow: TTabularRow): void {
    if (!window.confirm('Delete this row?')) {
      return;
    }

    this.allTabularRows.update((lstRows) => lstRows.filter(objRow => objRow !== objCurrentRow));
    if (this.currentPageNumber() > this.totalPageCount()) {
      this.currentPageNumber.set(this.totalPageCount());
    }
  }

  updateCellValue(objCurrentRow: TTabularRow, strFieldId: string, strValue: string): void {
    this.allTabularRows.update((lstRows) => {
      let iRowIndex = lstRows.findIndex(objRow => objRow === objCurrentRow);
      if (iRowIndex < 0 || iRowIndex >= lstRows.length) {
        return lstRows;
      }

      let lstUpdatedRows = [...lstRows];
      lstUpdatedRows[iRowIndex] = {
        ...lstUpdatedRows[iRowIndex],
        [strFieldId]: strValue
      };

      return lstUpdatedRows;
    });
  }

  private compareValues(strLeftValue: string, strRightValue: string, strSortDirection: TSortDirection): number {
    let iLeftNumber = Number(strLeftValue);
    let iRightNumber = Number(strRightValue);

    let iCompareResult: number;
    if (!Number.isNaN(iLeftNumber) && !Number.isNaN(iRightNumber)) {
      iCompareResult = iLeftNumber - iRightNumber;
    } else {
      iCompareResult = strLeftValue.localeCompare(strRightValue, undefined, { sensitivity: 'base' });
    }

    return strSortDirection === 'asc' ? iCompareResult : -iCompareResult;
  }

  private createInitialRows(): TTabularRow[] {
    let lstRows: TTabularRow[] = [];
    for (let iRowIndex = 0; iRowIndex < 120; iRowIndex++) {
      let dictRow: TTabularRow = {};

      for (let objField of this.section.fields) {
        dictRow[objField.id] = this.getInitialCellValue(objField, iRowIndex);
      }

      lstRows.push(dictRow);
    }

    return lstRows;
  }

  private getInitialCellValue(objField: ScreenRenderFieldModel, iRowIndex: number): string {
    if (objField.isActionField) {
      return '';
    }

    if (objField.controlType === 'select' && objField.lookupValues.length > 0) {
      return objField.lookupValues[iRowIndex % objField.lookupValues.length];
    }

    if (objField.inputType === 'number') {
      return String(iRowIndex + 1);
    }

    if (objField.inputType === 'date') {
      let iDayValue = (iRowIndex % 28) + 1;
      let strDayValue = iDayValue < 10 ? '0' + iDayValue : String(iDayValue);
      return '2026-04-' + strDayValue;
    }

    return objField.name + ' ' + String(iRowIndex + 1);
  }

  private createBlankRow(): TTabularRow {
    let dictRow: TTabularRow = {};
    for (let objField of this.section.fields) {
      dictRow[objField.id] = '';
    }

    return dictRow;
  }

  private escapeCsvCell(strValue: string): string {
    let strEscapedValue = strValue.replaceAll('"', '""');
    return '"' + strEscapedValue + '"';
  }

  toggle(): void {
    if (this.section.isCollapsible) {
      this.collapsed.update((fCollapsed) => !fCollapsed);
    }
  }

  emitAction(objField: ScreenRenderFieldModel): void {
    this.actionInvoked.emit(objField.name);
  }

  forwardAction(strActionName: string): void {
    this.actionInvoked.emit(strActionName);
  }
}
