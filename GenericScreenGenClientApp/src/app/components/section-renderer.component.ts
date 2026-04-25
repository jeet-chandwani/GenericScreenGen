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
        <button type="button" class="section-header" (click)="toggle()" [disabled]="!section.isCollapsible">
          <span>{{ section.name }}</span>
          <span>{{ section.isCollapsible ? (collapsed() ? '+' : '-') : '•' }}</span>
        </button>
      }

      <div class="section-body" [class]="layoutCssClass" [class.hidden]="collapsed()">
        @if (isTabularLayout) {
          <div class="tabular-shell">
            <div class="tabular-scroll">
              <table class="tabular-table" [attr.aria-label]="section.name + ' table view'">
                <thead>
                  <tr>
                    @for (objField of section.fields; track objField.id) {
                      <th [title]="objField.description" [style.min-width]="objField.width">
                        <button
                          type="button"
                          class="tabular-sort-button"
                          [disabled]="objField.isActionField"
                          (click)="sortByColumn(objField.id)"
                        >
                          <span>{{ objField.name }}</span>
                          <span class="tabular-sort-indicator">{{ getSortIndicator(objField.id) }}</span>
                        </button>
                      </th>
                    }
                  </tr>
                  <tr class="tabular-filter-row">
                    @for (objField of section.fields; track objField.id) {
                      <th [style.min-width]="objField.width">
                        @if (!objField.isActionField) {
                          <input
                            class="tabular-filter-input"
                            type="text"
                            [placeholder]="'Filter ' + objField.name"
                            [value]="getColumnFilter(objField.id)"
                            (input)="setColumnFilter(objField.id, $any($event.target).value)"
                          />
                        }
                      </th>
                    }
                  </tr>
                </thead>
                <tbody>
                  @for (objRow of pagedTabularRows(); track $index) {
                  <tr>
                    @for (objField of section.fields; track objField.id) {
                      <td [style.min-width]="objField.width">
                        @if (objField.isActionField) {
                          <button type="button" class="field-action" [title]="objField.description" (click)="emitAction(objField)">
                            {{ objField.name }}
                          </button>
                        } @else if (objField.controlType === 'textarea') {
                          <textarea
                            class="field-input"
                            [style.width]="'100%'"
                            [rows]="objField.lines"
                            [placeholder]="objField.description + ' ' + ($index + 1)"
                            [title]="objField.description"
                            [ngModel]="objRow[objField.id]"
                            (ngModelChange)="updateCellValue(objRow, objField.id, $event)"
                            [attr.minlength]="objField.minChars > 0 ? objField.minChars : null"
                            [attr.maxlength]="objField.maxChars > 0 ? objField.maxChars : null"
                          ></textarea>
                        } @else if (objField.controlType === 'select') {
                          <select
                            class="field-input"
                            [style.width]="'100%'"
                            [title]="objField.description"
                            [ngModel]="objRow[objField.id]"
                            (ngModelChange)="updateCellValue(objRow, objField.id, $event)"
                          >
                            @for (strLookupValue of objField.lookupValues; track strLookupValue) {
                              <option [value]="strLookupValue">{{ strLookupValue }}</option>
                            }
                          </select>
                        } @else {
                          <input
                            class="field-input"
                            [style.width]="'100%'"
                            [type]="objField.inputType"
                            [placeholder]="objField.description + ' ' + ($index + 1)"
                            [title]="objField.description"
                            [ngModel]="objRow[objField.id]"
                            (ngModelChange)="updateCellValue(objRow, objField.id, $event)"
                            [attr.minlength]="objField.minChars > 0 ? objField.minChars : null"
                            [attr.maxlength]="objField.maxChars > 0 ? objField.maxChars : null"
                          />
                        }
                      </td>
                    }
                  </tr>
                  }
                </tbody>
              </table>
            </div>

            @if (shouldShowPagination()) {
              <div class="tabular-pagination">
                <button type="button" class="tabular-page-btn" [disabled]="!canGoToPreviousPage()" (click)="goToFirstPage()">First</button>
                <button type="button" class="tabular-page-btn" [disabled]="!canGoToPreviousPage()" (click)="goToPreviousPage()">Previous</button>
                <span class="tabular-page-status">Page {{ currentPageNumber() }} of {{ totalPageCount() }}</span>
                <button type="button" class="tabular-page-btn" [disabled]="!canGoToNextPage()" (click)="goToNextPage()">Next</button>
                <button type="button" class="tabular-page-btn" [disabled]="!canGoToNextPage()" (click)="goToLastPage()">Last</button>
              </div>
            }
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
                    <select class="field-input" [style.width]="objField.width" [title]="objField.description">
                      @for (strLookupValue of objField.lookupValues; track strLookupValue) {
                        <option [value]="strLookupValue">{{ strLookupValue }}</option>
                      }
                    </select>
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
              <small class="field-description">{{ objField.description }}</small>
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
      }

      .section-card.borderless {
        border: none;
        background: transparent;
      }

      .section-header {
        width: 100%;
        display: flex;
        justify-content: space-between;
        gap: 16px;
        padding: 14px 18px;
        border: none;
        background: rgba(216, 192, 163, 0.4);
        color: inherit;
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
      }

      .tabular-shell {
        width: 100%;
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
        padding: 10px;
        border-bottom: 1px solid rgba(94, 63, 34, 0.16);
        vertical-align: top;
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
        white-space: nowrap;
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 8px;
        padding: 10px;
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

      .tabular-filter-row th {
        background: #fff7ee;
        padding: 8px;
      }

      .tabular-filter-input {
        width: 100%;
        min-width: 120px;
        border-radius: 8px;
        border: 1px solid rgba(94, 63, 34, 0.24);
        padding: 7px 8px;
        font: inherit;
      }

      .tabular-pagination {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 8px;
        margin-top: 10px;
      }

      .tabular-page-btn {
        border: 1px solid rgba(94, 63, 34, 0.22);
        background: #fffaf2;
        border-radius: 8px;
        padding: 6px 10px;
        font: inherit;
      }

      .tabular-page-btn:disabled {
        opacity: 0.55;
      }

      .tabular-page-status {
        color: #5a4b3e;
        font-weight: 600;
      }

      .tabular-table td .field-input,
      .tabular-table td .field-action {
        width: 100%;
        min-width: 120px;
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
  readonly pageSize = 50;

  ngOnChanges(objChanges: SimpleChanges): void {
    if (!objChanges['section'] || !this.isTabularLayout) {
      return;
    }

    let strCurrentShapeSignature = this.section.fields.map(objField => objField.id).join('|');
    if (this.m_strTabularShapeSignature === strCurrentShapeSignature) {
      return;
    }

    this.m_strTabularShapeSignature = strCurrentShapeSignature;
    this.columnFilters.set({});
    this.sortColumnId.set(null);
    this.sortDirection.set('asc');
    this.currentPageNumber.set(1);
    this.allTabularRows.set(this.createInitialRows());
  }

  get layoutCssClass(): string {
    return 'section-body ' + this.m_itfLayoutPolicyService.getCssClass(this.section.layoutPolicy);
  }

  get isTabularLayout(): boolean {
    return this.section.layoutPolicy === 'tabular';
  }

  getSortIndicator(strColumnId: string): string {
    if (this.sortColumnId() !== strColumnId) {
      return '↕';
    }

    return this.sortDirection() === 'asc' ? '▲' : '▼';
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
