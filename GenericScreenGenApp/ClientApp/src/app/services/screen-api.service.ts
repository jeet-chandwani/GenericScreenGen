import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { ScreenListItem, ScreenRenderModel, ScreenValidationResult } from '../models/screen.models';

@Injectable({
  providedIn: 'root'
})
export class ScreenApiService {
  private readonly httpClient = inject(HttpClient);

  getScreens(): Observable<ScreenListItem[]> {
    return this.httpClient.get<ScreenListItem[]>('/api/screens');
  }

  getRenderModel(strScreenFileName: string): Observable<ScreenRenderModel> {
    return this.httpClient.get<ScreenRenderModel>(`/api/screens/${encodeURIComponent(strScreenFileName)}/render`);
  }

  getValidationResults(): Observable<ScreenValidationResult[]> {
    return this.httpClient.get<ScreenValidationResult[]>('/api/screens/validation');
  }
}