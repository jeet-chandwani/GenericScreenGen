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
const RECORD_ID_FIELD_NAME = '__record-id';
const SOURCE_SCREEN_FIELD_NAME = '__source-screen';

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
  readonly navigationPrefillByFieldName = signal<Record<string, string> | null>(null);
  readonly returnScreenFileName = signal('');
  readonly selectedRecordId = signal('');
  readonly selectedRecordSourceScreenFileName = signal('');
  readonly selectedInvalidScreenValidation = signal<ScreenValidationResult | null>(null);
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

  openScreenFromStatus(objScreenStatus: IScreenListStatusItem): void {
    this.returnScreenFileName.set('');

    if (!objScreenStatus.isValid) {
      const objValidationResult = this.getValidationResultForScreen(objScreenStatus.fileName) ?? {
        screenFileName: objScreenStatus.fileName,
        isValid: false,
        issues: [
          {
            code: 'VALIDATION-UNKNOWN',
            path: '(root)',
            message: 'Screen is marked invalid but issue details are not available yet. Try Refresh Screens.'
          }
        ]
      };

      this.selectedScreenFileName.set(objScreenStatus.fileName);
      this.renderModel.set(null);
      this.selectedInvalidScreenValidation.set(objValidationResult);
      this.errorMessage.set('');
      this.actionMessage.set('');
      this.viewMode.set('screen');
      return;
    }

    this.openScreen(objScreenStatus.fileName);
  }

  openScreen(strScreenFileName: string, dictPrefillByFieldName: Record<string, string> | null = null): void {
    this.navigationPrefillByFieldName.set(dictPrefillByFieldName);
    const objValidationResult = this.getValidationResultForScreen(strScreenFileName);
    if (objValidationResult && !objValidationResult.isValid) {
      this.selectedScreenFileName.set(strScreenFileName);
      this.renderModel.set(null);
      this.selectedInvalidScreenValidation.set(objValidationResult);
      this.errorMessage.set('');
      this.actionMessage.set('');
      this.viewMode.set('screen');
      return;
    }

    this.selectedInvalidScreenValidation.set(null);
    this.selectedScreenFileName.set(strScreenFileName);
    this.loadRenderModel(strScreenFileName, true);
  }

  goToHome(): void {
    this.viewMode.set('home');
    this.navigationPrefillByFieldName.set(null);
    this.selectedRecordId.set('');
    this.selectedRecordSourceScreenFileName.set('');
    this.selectedInvalidScreenValidation.set(null);
    this.errorMessage.set('');
    this.actionMessage.set('');
  }

  reloadCurrentScreen(): void {
    if (this.selectedScreenFileName()) {
      if (this.selectedInvalidScreenValidation()) {
        this.openScreen(this.selectedScreenFileName());
        return;
      }

      this.renderModel.set(null);
      this.navigationPrefillByFieldName.set(null);
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
    if (strActionName.startsWith('navigate:')) {
      const strSourceScreenFileName = this.selectedScreenFileName();
      const strNavigatePayload = strActionName.slice('navigate:'.length);
      const arrNavigateParts = strNavigatePayload.split('|', 2);
      const strTargetScreenFileName = arrNavigateParts[0] ?? '';

      let dictPrefillByFieldName: Record<string, string> | null = null;
      if (arrNavigateParts.length > 1) {
        try {
          dictPrefillByFieldName = JSON.parse(decodeURIComponent(arrNavigateParts[1])) as Record<string, string>;
        } catch {
          dictPrefillByFieldName = null;
        }
      }

      const strRecordId = dictPrefillByFieldName?.[RECORD_ID_FIELD_NAME] ?? '';
      const strSourceScreenFromPayload = dictPrefillByFieldName?.[SOURCE_SCREEN_FIELD_NAME] ?? '';
      const strSourceForRecord = strSourceScreenFromPayload || strSourceScreenFileName;

      if (dictPrefillByFieldName) {
        const { [RECORD_ID_FIELD_NAME]: _, [SOURCE_SCREEN_FIELD_NAME]: __, ...dictCleanPrefill } = dictPrefillByFieldName;
        dictPrefillByFieldName = dictCleanPrefill;
      }

      this.selectedRecordId.set(strRecordId);
      this.selectedRecordSourceScreenFileName.set(strSourceForRecord);

      if (strSourceScreenFileName && strSourceScreenFileName !== strTargetScreenFileName) {
        this.returnScreenFileName.set(strSourceScreenFileName);
      }

      if (strRecordId && strSourceForRecord) {
        this.screenApiService
          .getRecordById(strSourceForRecord, strRecordId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (objRecordResponse) => {
              this.openScreen(strTargetScreenFileName, { ...(dictPrefillByFieldName ?? {}), ...objRecordResponse.data });
            },
            error: () => {
              this.openScreen(strTargetScreenFileName, dictPrefillByFieldName);
            }
          });
        return;
      }

      this.openScreen(strTargetScreenFileName, dictPrefillByFieldName);
    } else if (strActionName === 'navigate-back') {
      this.navigateBackToSourceOrHome();
    } else if (strActionName.startsWith('save-record:')) {
      const strRecordId = this.selectedRecordId();
      const strSourceScreenFileName = this.selectedRecordSourceScreenFileName() || this.returnScreenFileName();

      if (!strRecordId || !strSourceScreenFileName) {
        this.errorMessage.set('Missing record context required to save the selected record.');
        return;
      }

      const strEncodedPayload = strActionName.slice('save-record:'.length);
      let dictRecordData: Record<string, string> = {};

      try {
        dictRecordData = JSON.parse(decodeURIComponent(strEncodedPayload)) as Record<string, string>;
      } catch {
        this.errorMessage.set('Failed to parse record data for save operation.');
        return;
      }

      this.errorMessage.set('');
      this.screenApiService
        .saveRecordById(strSourceScreenFileName, strRecordId, dictRecordData)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.actionMessage.set(`Record ${strRecordId} saved.`);
            this.navigateBackToSourceOrHome();
          },
          error: () => {
            this.errorMessage.set(`Failed to save record ${strRecordId}.`);
          }
        });
    } else if (strActionName === 'save') {
      this.actionMessage.set('Save action invoked.');
    } else {
      this.actionMessage.set(`Action invoked: ${strActionName}`);
    }
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
    this.selectedInvalidScreenValidation.set(null);

    this.screenApiService
      .getRenderModel(strScreenFileName)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (objRenderModel) => {
          this.renderModel.set(objRenderModel);
          this.selectedInvalidScreenValidation.set(null);

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
      if (objSelectedScreenStatus && !objSelectedScreenStatus.isValid) {
        this.openScreen(strSelectedScreenFileName);
      } else {
        this.goToHome();
        this.selectedScreenFileName.set('');
        this.renderModel.set(null);
      }
    }
  }

  private getValidationResultForScreen(strScreenFileName: string): ScreenValidationResult | null {
    return this.validationResults().find(objValidationResult => objValidationResult.screenFileName === strScreenFileName) ?? null;
  }

  private navigateBackToSourceOrHome(): void {
    const strReturnScreenFileName = this.returnScreenFileName();
    this.returnScreenFileName.set('');
    this.selectedRecordId.set('');
    this.selectedRecordSourceScreenFileName.set('');

    if (strReturnScreenFileName) {
      this.openScreen(strReturnScreenFileName, null);
    } else {
      this.goToHome();
    }
  }
}
