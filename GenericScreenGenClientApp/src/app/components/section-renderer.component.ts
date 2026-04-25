import { CommonModule } from '@angular/common';
import { Component, EventEmitter, inject, Input, Output, signal } from '@angular/core';

import { ScreenRenderFieldModel, ScreenRenderSectionModel } from '../models/screen.models';
import { LayoutPolicyService } from '../services/layout-policy.service';

@Component({
  selector: 'app-section-renderer',
  standalone: true,
  imports: [CommonModule],
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
                      <th [title]="objField.description">{{ objField.name }}</th>
                    }
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    @for (objField of section.fields; track objField.id) {
                      <td>
                        @if (objField.isActionField) {
                          <button type="button" class="field-action" [title]="objField.description" (click)="emitAction(objField)">
                            {{ objField.name }}
                          </button>
                        } @else if (objField.controlType === 'textarea') {
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
                      </td>
                    }
                  </tr>
                </tbody>
              </table>
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
        text-align: left;
        font-weight: 700;
        color: #4f4135;
        white-space: nowrap;
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
export class SectionRendererComponent {
  private readonly m_itfLayoutPolicyService = inject(LayoutPolicyService);

  @Input({ required: true }) section!: ScreenRenderSectionModel;
  @Output() readonly actionInvoked = new EventEmitter<string>();

  readonly collapsed = signal(false);

  get layoutCssClass(): string {
    return 'section-body ' + this.m_itfLayoutPolicyService.getCssClass(this.section.layoutPolicy);
  }

  get isTabularLayout(): boolean {
    return this.section.layoutPolicy === 'tabular';
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
