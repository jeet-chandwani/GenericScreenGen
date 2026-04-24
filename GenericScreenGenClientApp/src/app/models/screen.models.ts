export interface ScreenListItem {
  fileName: string;
  displayName: string;
}

export interface ScreenRenderFieldModel {
  id: string;
  name: string;
  description: string;
  type: string;
  typeInfo: string;
  width: string;
  controlType: string;
  inputType: string;
  minChars: number;
  maxChars: number;
  lines: number;
  lookupValues: string[];
  isActionField: boolean;
}

export interface ScreenRenderSectionModel {
  name: string;
  layoutPolicy: string;
  isCollapsible: boolean;
  showBorder: boolean;
  fields: ScreenRenderFieldModel[];
  sections: ScreenRenderSectionModel[];
}

export interface ScreenRenderModel {
  screenId: string;
  screenFileName: string;
  displayName: string;
  sections: ScreenRenderSectionModel[];
}

export interface ScreenValidationIssue {
  code: string;
  path: string;
  message: string;
}

export interface ScreenValidationResult {
  screenFileName: string;
  isValid: boolean;
  issues: ScreenValidationIssue[];
}