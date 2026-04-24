import { InjectionToken } from '@angular/core';

export interface RuntimeClientConfig {
  apiBaseUrl?: string;
}

declare global {
  interface Window {
    __GSG_CONFIG__?: RuntimeClientConfig;
  }
}

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

const DEFAULT_API_BASE_URL = '/api';

export function resolveApiBaseUrl(): string {
  const strConfiguredApiBaseUrl = window.__GSG_CONFIG__?.apiBaseUrl;

  if (!strConfiguredApiBaseUrl || strConfiguredApiBaseUrl.trim().length === 0) {
    return DEFAULT_API_BASE_URL;
  }

  return strConfiguredApiBaseUrl.replace(/\/$/, '');
}
