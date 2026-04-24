import { Injectable } from '@angular/core';

/**
 * Maps layout policy identifiers to the corresponding CSS class applied to a section body.
 * To add a new layout policy: register a new entry in the map below and add the
 * corresponding CSS class to the section-renderer component (or a shared stylesheet).
 */
@Injectable({
  providedIn: 'root'
})
export class LayoutPolicyService {
  private readonly m_mapPolicyCssClasses = new Map<string, string>([
    ['per-line', 'layout-per-line'],
  ]);

  private readonly m_strFallbackCssClass = 'layout-per-line';

  /**
   * Returns the CSS class name for the given layout policy identifier.
   * Falls back to the per-line class when the policy is unknown.
   */
  getCssClass(strPolicyId: string): string {
    return this.m_mapPolicyCssClasses.get(strPolicyId) ?? this.m_strFallbackCssClass;
  }
}
