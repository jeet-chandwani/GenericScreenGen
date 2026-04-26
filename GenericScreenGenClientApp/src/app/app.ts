import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, retry, switchMap } from 'rxjs';

import { SectionRendererComponent } from './components/section-renderer.component';
import { ScreenListItem, ScreenRenderModel, ScreenValidationResult } from './models/screen.models';
import { ScreenApiService } from './services/screen-api.service';

interface IScreenListStatusItem {
  fileName: string;
  displayName: string;
  isValid: boolean;
}

type EAppViewMode = 'home' | 'screen';

const RETRY_COUNT = 3;
const RETRY_DELAY_MS = 1000;

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
  readonly showAboutPanel = signal(false);
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
    this.showAboutPanel.set(false);
  }

  toggleAboutPanel(): void {
    this.showAboutPanel.update(fShow => !fShow);
  }

  reloadCurrentScreen(): void {
    if (this.selectedScreenFileName()) {
      this.renderModel.set(null);
      this.loadRenderModel(this.selectedScreenFileName());
    }
  }

  refreshScreens(): void {
    this.errorMessage.set('');

    this.screenApiService
      .refreshScreens()
      .pipe(
        switchMap(() =>
          forkJoin({
            screens: this.screenApiService.getScreens(),
            validationResults: this.screenApiService.getValidationResults()
          })
        ),
        retry({ count: RETRY_COUNT, delay: RETRY_DELAY_MS }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ screens, validationResults }) => {
          this.screens.set(screens);
          this.validationResults.set(validationResults);
          this.ensureCurrentScreenIsValidAfterRefresh();
        },
        error: () => {
          this.errorMessage.set('Failed to refresh screens and validation results.');
        }
      });
  }

  onActionInvoked(strActionName: string): void {
    this.actionMessage.set(`Action invoked: ${strActionName}`);
  }

  private loadInitialState(): void {
    this.screenApiService
      .getScreens()
      .pipe(
        retry({ count: RETRY_COUNT, delay: RETRY_DELAY_MS }),
        takeUntilDestroyed(this.destroyRef)
      )
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
      .pipe(
        retry({ count: RETRY_COUNT, delay: RETRY_DELAY_MS }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (arrValidationResults) => {
          this.validationResults.set(arrValidationResults);
        },
        error: () => {
          this.errorMessage.set('Failed to load schema validation results.');
        }
      });
  }

  private ensureCurrentScreenIsValidAfterRefresh(): void {
    const strSelectedScreenFileName = this.selectedScreenFileName();

    if (!strSelectedScreenFileName) {
      return;
    }

    const objSelectedScreenStatus: IScreenListStatusItem | undefined = this.screenListStatusItems()
      .find((objScreenStatus) => objScreenStatus.fileName === strSelectedScreenFileName);

    if (!objSelectedScreenStatus || !objSelectedScreenStatus.isValid) {
      this.goToHome();
      this.selectedScreenFileName.set('');
      this.renderModel.set(null);
    }
  }
}
