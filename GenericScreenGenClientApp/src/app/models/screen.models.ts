export interface ScreenListItem {
  screenId: string;
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
  maxWidth: string;
  controlType: string;
  inputType: string;
  minChars: number;
  maxChars: number;
  lines: number;
  lookupValues: string[];
  lookupOptionDescriptions: string[];
  lookupOptionImages: string[];
  isMandatory: boolean;
  isMultiple: boolean;
  isActionField: boolean;
  isSearchable: boolean;
}

export interface ScreenSelectionActionModel {
  event: string;
  targetScreenId: string;
  includeRecordId: boolean;
}

export interface ScreenRenderSectionModel {
  name: string;
  layoutPolicy: string;
  isCollapsible: boolean;
  showBorder: boolean;
  fields: ScreenRenderFieldModel[];
  sections: ScreenRenderSectionModel[];
  selectionActions: ScreenSelectionActionModel[];
}

export interface ScreenRenderModel {
  screenId: string;
  screenFileName: string;
  displayName: string;
  theme: string;
  features: string[];
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