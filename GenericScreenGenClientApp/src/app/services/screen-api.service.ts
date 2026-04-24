import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/api-base-url.token';
import { ScreenListItem, ScreenRenderModel, ScreenValidationResult } from '../models/screen.models';

@Injectable({
  providedIn: 'root'
})
export class ScreenApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly strApiBaseUrl = inject(API_BASE_URL);

  getScreens(): Observable<ScreenListItem[]> {
    return this.httpClient.get<ScreenListItem[]>(this.getApiUrl('/screens'));
  }

  getRenderModel(strScreenFileName: string): Observable<ScreenRenderModel> {
    return this.httpClient.get<ScreenRenderModel>(
      this.getApiUrl(`/screens/${encodeURIComponent(strScreenFileName)}/render`)
    );
  }

  getValidationResults(): Observable<ScreenValidationResult[]> {
    return this.httpClient.get<ScreenValidationResult[]>(this.getApiUrl('/screens/validation'));
  }

  private getApiUrl(strPath: string): string {
    return `${this.strApiBaseUrl}${strPath}`;
  }
}