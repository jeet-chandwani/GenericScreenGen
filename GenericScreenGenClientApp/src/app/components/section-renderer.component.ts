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
        @for (objField of section.fields; track objField.id) {
          <div class="field-row" [style.maxWidth]="objField.width">
            @if (objField.isActionField) {
              <button type="button" class="field-action" [title]="objField.description" (click)="emitAction(objField)">
                {{ objField.name }}
              </button>
            } @else {
              <label>
                <span>{{ objField.name }}</span>
                <input [type]="objField.inputType" [placeholder]="objField.description" [title]="objField.description" />
              </label>
            }
            <small>{{ objField.description }}</small>
          </div>
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
        gap: 16px;
        padding: 18px;
      }

      .section-body.layout-per-line {
        display: grid;
      }

      .section-body.hidden {
        display: none;
      }

      .field-row {
        display: grid;
        gap: 8px;
      }

      .field-row label {
        display: grid;
        gap: 6px;
        font-weight: 700;
      }

      input,
      .field-action {
        font: inherit;
        border-radius: 12px;
        border: 1px solid rgba(94, 63, 34, 0.22);
        padding: 10px 12px;
      }

      .field-action {
        width: fit-content;
        background: #8f4e2f;
        color: #fffaf2;
      }

      small {
        color: #65584d;
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