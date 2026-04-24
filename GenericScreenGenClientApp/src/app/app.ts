import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { SectionRendererComponent } from './components/section-renderer.component';
import { ScreenListItem, ScreenRenderModel, ScreenValidationResult } from './models/screen.models';
import { ScreenApiService } from './services/screen-api.service';

interface IScreenListStatusItem {
  fileName: string;
  displayName: string;
  isValid: boolean;
}

type EAppViewMode = 'home' | 'screen';

@Component({
  selector: 'app-root',
  imports: [SectionRendererComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly screenApiService = inject(ScreenApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly screens = signal<ScreenListItem[]>([]);
  readonly selectedScreenFileName = signal('');
  readonly renderModel = signal<ScreenRenderModel | null>(null);
  readonly validationResults = signal<ScreenValidationResult[]>([]);
  readonly errorMessage = signal('');
  readonly actionMessage = signal('');
  readonly viewMode = signal<EAppViewMode>('home');
  readonly isSchemaHealthy = computed(() => this.validationResults().every((objResult) => objResult.isValid));
  readonly screenListStatusItems = computed<IScreenListStatusItem[]>(() => {
    const mapValidationByScreen = new Map(
      this.validationResults().map((objValidationResult) => [objValidationResult.screenFileName, objValidationResult.isValid])
    );

    return this.screens().map((objScreen) => ({
      fileName: objScreen.fileName,
      displayName: objScreen.displayName,
      isValid: mapValidationByScreen.get(objScreen.fileName) ?? false
    }));
  });

  constructor() {
    this.loadInitialState();
  }

  openScreen(strScreenFileName: string): void {
    this.selectedScreenFileName.set(strScreenFileName);
    this.loadRenderModel(strScreenFileName, true);
  }

  goToHome(): void {
    this.viewMode.set('home');
    this.errorMessage.set('');
    this.actionMessage.set('');
  }

  reloadCurrentScreen(): void {
    if (this.selectedScreenFileName()) {
      this.loadRenderModel(this.selectedScreenFileName());
    }
  }

  refreshValidation(): void {
    this.loadValidation();
  }

  onActionInvoked(strActionName: string): void {
    this.actionMessage.set(`Action invoked: ${strActionName}`);
  }

  private loadInitialState(): void {
    this.screenApiService
      .getScreens()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (arrScreens) => {
          this.screens.set(arrScreens);
        },
        error: () => {
          this.errorMessage.set('Failed to load screen list from the backend.');
        }
      });

    this.loadValidation();
  }

  private loadRenderModel(strScreenFileName: string, fNavigateOnSuccess = false): void {
    this.errorMessage.set('');
    this.actionMessage.set('');

    this.screenApiService
      .getRenderModel(strScreenFileName)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (objRenderModel) => {
          this.renderModel.set(objRenderModel);

          if (fNavigateOnSuccess) {
            this.viewMode.set('screen');
          }
        },
        error: () => {
          this.errorMessage.set(`Failed to load render model for ${strScreenFileName}.`);
        }
      });
  }

  private loadValidation(): void {
    this.screenApiService
      .getValidationResults()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (arrValidationResults) => {
          this.validationResults.set(arrValidationResults);
        },
        error: () => {
          this.errorMessage.set('Failed to load schema validation results.');
        }
      });
  }
}
