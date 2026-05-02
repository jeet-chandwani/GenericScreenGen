using System.Text.Json;
using System.Text.Json.Serialization;
using GenericScreenGenInterfacesLib;
using GenericScreenGenUtilsLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Loads and caches screen definitions from JSON configuration files.
    /// </summary>
    public sealed class CScreenConfigProvider : ACanInitBase, IScreenConfigProvider
    {
        private static readonly string[] s_arrDefaultScreenFeatures = ["save", "cancel", "show-original-values"];

        private readonly JsonSerializerOptions m_objSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ILayoutPolicyRegistry m_itfLayoutPolicyRegistry;
        private string? m_strScreenFolderPath;

        /// <summary>
        /// Initialises the provider with the supplied layout policy registry used to validate
        /// <c>layout-policy</c> values in screen configuration files.
        /// </summary>
        /// <param name="itfLayoutPolicyRegistry">Registry of all registered layout policies.</param>
        public CScreenConfigProvider(ILayoutPolicyRegistry itfLayoutPolicyRegistry)
        {
            m_itfLayoutPolicyRegistry = itfLayoutPolicyRegistry;
        }

        private readonly Dictionary<string, IScreenDefinition> m_dictScreenDefinitions = new Dictionary<string, IScreenDefinition>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public bool TryGetAvailableScreens(out IReadOnlyList<string> lstScreenFileNames, out string strError)
        {
            lstScreenFileNames = m_dictScreenDefinitions.Keys.OrderBy(strItem => strItem, StringComparer.OrdinalIgnoreCase).ToList();
            strError = string.Empty;
            return true;
        }

        /// <inheritdoc />
        public bool TryGetScreenDefinition(string strScreenFileName, out IScreenDefinition? itfScreenDefinition, out string strError)
        {
            if (string.IsNullOrWhiteSpace(strScreenFileName))
            {
                itfScreenDefinition = null;
                strError = "Screen file name is required.";
                return false;
            }

            if (!m_dictScreenDefinitions.TryGetValue(strScreenFileName, out itfScreenDefinition))
            {
                strError = $"Screen definition '{strScreenFileName}' was not found.";
                return false;
            }

            strError = string.Empty;
            return true;
        }

        /// <inheritdoc />
        public bool TryReloadScreens(out string strError)
        {
            if (string.IsNullOrWhiteSpace(m_strScreenFolderPath))
            {
                strError = "Screen folder path was not initialized.";
                return false;
            }

            if (!Directory.Exists(m_strScreenFolderPath))
            {
                strError = $"Screen folder '{m_strScreenFolderPath}' was not found.";
                return false;
            }

            return TryLoadAllScreenDefinitions(m_strScreenFolderPath, out strError);
        }

        /// <inheritdoc />
        protected override bool TryInitCore(object objInputParam, out string strError)
        {
            if (objInputParam is not string strScreenFolderPath || string.IsNullOrWhiteSpace(strScreenFolderPath))
            {
                strError = "Screen folder path must be provided as a non-empty string.";
                return false;
            }

            if (!Directory.Exists(strScreenFolderPath))
            {
                strError = $"Screen folder '{strScreenFolderPath}' was not found.";
                return false;
            }

            m_strScreenFolderPath = strScreenFolderPath;

            return TryLoadAllScreenDefinitions(strScreenFolderPath, out strError);
        }

        /// <inheritdoc />
        protected override bool InitAfterLoad(out string strError)
        {
            if (string.IsNullOrWhiteSpace(m_strScreenFolderPath))
            {
                strError = "Screen folder path was not initialized.";
                return false;
            }

            strError = string.Empty;
            return true;
        }

        private bool TryLoadAllScreenDefinitions(string strScreenFolderPath, out string strError)
        {
            Dictionary<string, IScreenDefinition> dictLoadedScreenDefinitions = new Dictionary<string, IScreenDefinition>(StringComparer.OrdinalIgnoreCase);
            string[] arrScreenFiles = Directory.GetFiles(strScreenFolderPath, "Screen-*.json", SearchOption.TopDirectoryOnly);

            foreach (string strJsonFilePath in arrScreenFiles)
            {
                if (!TryLoadScreenDefinition(strJsonFilePath, out IScreenDefinition? itfScreenDefinition, out strError) || itfScreenDefinition is null)
                {
                    return false;
                }

                dictLoadedScreenDefinitions[itfScreenDefinition.ScreenFileName] = itfScreenDefinition;
            }

            m_dictScreenDefinitions.Clear();

            foreach (KeyValuePair<string, IScreenDefinition> kvpLoadedScreen in dictLoadedScreenDefinitions)
            {
                m_dictScreenDefinitions[kvpLoadedScreen.Key] = kvpLoadedScreen.Value;
            }

            strError = string.Empty;
            return true;
        }

        private bool TryLoadScreenDefinition(string strJsonFilePath, out IScreenDefinition? itfScreenDefinition, out string strError)
        {
            try
            {
                string strJsonContent = File.ReadAllText(strJsonFilePath);
                CScreenDocumentDto? objScreenDocument = JsonSerializer.Deserialize<CScreenDocumentDto>(strJsonContent, m_objSerializerOptions);

                if (objScreenDocument?.Sections is null || objScreenDocument.Sections.Count == 0)
                {
                    itfScreenDefinition = null;
                    strError = $"Screen file '{Path.GetFileName(strJsonFilePath)}' must contain at least one section.";
                    return false;
                }

                List<IScreenSectionDefinition> lstSections = new List<IScreenSectionDefinition>();

                foreach (CScreenSectionDto objSection in objScreenDocument.Sections)
                {
                    if (!TryCreateSectionDefinition(objSection, out IScreenSectionDefinition? itfSectionDefinition, out strError) || itfSectionDefinition is null)
                    {
                        itfScreenDefinition = null;
                        return false;
                    }

                    lstSections.Add(itfSectionDefinition);
                }

                string strScreenFileName = Path.GetFileName(strJsonFilePath);
                string strDisplayName = objScreenDocument.Name.Trim();
                string strScreenId = objScreenDocument.Id.Trim();
                IReadOnlyList<string> lstKey = NormalizeKeyFieldIds(objScreenDocument.Key);
                IReadOnlyList<string> lstFeatures = NormalizeFeatures(objScreenDocument.Features);

                itfScreenDefinition = new CScreenDefinition(strScreenId, strScreenFileName, strDisplayName, lstSections, lstKey, lstFeatures);
                strError = string.Empty;
                return true;
            }
            catch (JsonException objJsonException)
            {
                itfScreenDefinition = null;
                strError = $"Failed to parse screen file '{Path.GetFileName(strJsonFilePath)}'. {objJsonException.Message}";
                return false;
            }
            catch (Exception objException)
            {
                itfScreenDefinition = null;
                strError = $"Failed to load screen file '{Path.GetFileName(strJsonFilePath)}'. {objException.Message}";
                return false;
            }
        }

        private bool TryCreateSectionDefinition(CScreenSectionDto objSection, out IScreenSectionDefinition? itfScreenSectionDefinition, out string strError)
        {
            List<IScreenFieldDefinition> lstFields = new List<IScreenFieldDefinition>();
            List<IScreenSectionDefinition> lstSections = new List<IScreenSectionDefinition>();
            List<IScreenSelectionActionDefinition> lstSelectionActions = new List<IScreenSelectionActionDefinition>();

            if (objSection.Fields is not null)
            {
                if (objSection.Fields.Count == 0)
                {
                    itfScreenSectionDefinition = null;
                    strError = $"Section '{objSection.Name}' has a fields array but it is empty. At least one field is required when the fields array is present.";
                    return false;
                }

                foreach (CScreenFieldDto objField in objSection.Fields)
                {
                    if (!TryCreateFieldDefinition(objField, out IScreenFieldDefinition? itfFieldDefinition, out strError) || itfFieldDefinition is null)
                    {
                        itfScreenSectionDefinition = null;
                        return false;
                    }

                    lstFields.Add(itfFieldDefinition);
                }
            }

            if (objSection.Sections is not null)
            {
                foreach (CScreenSectionDto objChildSection in objSection.Sections)
                {
                    if (!TryCreateSectionDefinition(objChildSection, out IScreenSectionDefinition? itfChildSectionDefinition, out strError) || itfChildSectionDefinition is null)
                    {
                        itfScreenSectionDefinition = null;
                        return false;
                    }

                    lstSections.Add(itfChildSectionDefinition);
                }
            }

            string strSectionName = objSection.Name;
            string strLayoutPolicy = objSection.LayoutPolicy;

            if (objSection.SelectionActions is not null)
            {
                foreach (CScreenSelectionActionDto objSelectionAction in objSection.SelectionActions)
                {
                    string strSelectionEvent = objSelectionAction.Event.Trim().ToLowerInvariant();
                    if (strSelectionEvent != "click" && strSelectionEvent != "double-click")
                    {
                        itfScreenSectionDefinition = null;
                        strError = $"Section '{strSectionName}' contains unsupported selection action event '{objSelectionAction.Event}'. Supported values: click, double-click.";
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(objSelectionAction.TargetScreen))
                    {
                        itfScreenSectionDefinition = null;
                        strError = $"Section '{strSectionName}' has a selection action with empty target-screen.";
                        return false;
                    }

                    if (lstSelectionActions.Any(itfAction => string.Equals(itfAction.Event, strSelectionEvent, StringComparison.OrdinalIgnoreCase)))
                    {
                        itfScreenSectionDefinition = null;
                        strError = $"Section '{strSectionName}' contains duplicate selection action for event '{strSelectionEvent}'.";
                        return false;
                    }

                    lstSelectionActions.Add(new CScreenSelectionActionDefinition(
                        strSelectionEvent,
                        objSelectionAction.TargetScreen.Trim(),
                        objSelectionAction.IncludeRecordId));
                }
            }

            if (string.Equals(strLayoutPolicy, "tabular", StringComparison.OrdinalIgnoreCase) && lstSelectionActions.Count == 0)
            {
                itfScreenSectionDefinition = null;
                strError = $"Section '{strSectionName}' uses tabular layout and must define at least one selection-action (click or double-click).";
                return false;
            }

            if (!m_itfLayoutPolicyRegistry.IsValidPolicyId(strLayoutPolicy))
            {
                itfScreenSectionDefinition = null;
                strError = $"Unknown layout-policy '{strLayoutPolicy}' in section '{strSectionName}'. " +
                           $"Valid policies are: {string.Join(", ", m_itfLayoutPolicyRegistry.GetAllPolicies().Select(p => p.PolicyId))}";
                return false;
            }

            itfScreenSectionDefinition = new CScreenSectionDefinition(
                strSectionName,
                strLayoutPolicy,
                objSection.IsCollapsible,
                lstFields,
                lstSections,
                lstSelectionActions);
            strError = string.Empty;
            return true;
        }

        private static bool TryCreateFieldDefinition(CScreenFieldDto objField, out IScreenFieldDefinition? itfFieldDefinition, out string strError)
        {
            if (string.IsNullOrWhiteSpace(objField.Id) || string.IsNullOrWhiteSpace(objField.Name) ||
                string.IsNullOrWhiteSpace(objField.Description) || string.IsNullOrWhiteSpace(objField.Width))
            {
                itfFieldDefinition = null;
                strError = "Each field must define id, name, description, and width values.";
                return false;
            }

            if (!TryParseFieldType(objField.Type, out EFieldType enuFieldType))
            {
                itfFieldDefinition = null;
                strError = $"Unsupported field type '{objField.Type}'.";
                return false;
            }

            itfFieldDefinition = new CScreenFieldDefinition(
                objField.Id,
                objField.Name,
                objField.Description,
                enuFieldType,
                objField.TypeInfo ?? string.Empty,
                objField.Width,
                objField.MaxWidth ?? string.Empty,
                objField.IsMandatory,
                objField.IsSearchable);
            strError = string.Empty;
            return true;
        }

        private static IReadOnlyList<string> NormalizeFeatures(List<string> lstRawFeatures)
        {
            if (lstRawFeatures.Count == 0)
            {
                return Array.Empty<string>();
            }

            HashSet<string> setKnownFeatures = new HashSet<string>(s_arrDefaultScreenFeatures, StringComparer.OrdinalIgnoreCase);
            HashSet<string> setNormalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> lstNormalized = new List<string>();

            foreach (string strRawFeature in lstRawFeatures)
            {
                if (string.IsNullOrWhiteSpace(strRawFeature))
                {
                    continue;
                }

                string strNormalizedFeature = strRawFeature.Trim().ToLowerInvariant();

                if (!setKnownFeatures.Contains(strNormalizedFeature) || !setNormalized.Add(strNormalizedFeature))
                {
                    continue;
                }

                lstNormalized.Add(strNormalizedFeature);
            }

            return lstNormalized;
        }

        private static IReadOnlyList<string> NormalizeKeyFieldIds(List<string> lstRawKeyFieldIds)
        {
            if (lstRawKeyFieldIds.Count == 0)
            {
                return Array.Empty<string>();
            }

            HashSet<string> setNormalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> lstNormalized = new List<string>();

            foreach (string strRawKeyFieldId in lstRawKeyFieldIds)
            {
                if (string.IsNullOrWhiteSpace(strRawKeyFieldId))
                {
                    continue;
                }

                string strNormalizedKeyFieldId = strRawKeyFieldId.Trim().ToLowerInvariant();

                if (!setNormalized.Add(strNormalizedKeyFieldId))
                {
                    continue;
                }

                lstNormalized.Add(strNormalizedKeyFieldId);
            }

            return lstNormalized;
        }

        private static bool TryParseFieldType(string strFieldType, out EFieldType enuFieldType)
        {
            enuFieldType = EFieldType.Text;

            if (string.IsNullOrWhiteSpace(strFieldType))
            {
                return false;
            }

            string strNormalizedFieldType = strFieldType.Trim().Replace("_", "-", StringComparison.Ordinal).ToLowerInvariant();

            if (strNormalizedFieldType == "date-time" || strNormalizedFieldType == "datetime")
            {
                enuFieldType = EFieldType.DateTime;
                return true;
            }

            if (strNormalizedFieldType == "date")
            {
                enuFieldType = EFieldType.Date;
                return true;
            }

            if (strNormalizedFieldType == "lookup")
            {
                enuFieldType = EFieldType.Lookup;
                return true;
            }

            return Enum.TryParse(strFieldType, true, out enuFieldType);
        }

        private sealed class CScreenDocumentDto
        {
            [JsonPropertyName("id")]
            [JsonRequired]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            [JsonRequired]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("features")]
            [JsonRequired]
            public List<string> Features { get; set; } = new List<string>();

            [JsonPropertyName("key")]
            [JsonRequired]
            public List<string> Key { get; set; } = new List<string>();

            [JsonPropertyName("sections")]
            public List<CScreenSectionDto>? Sections { get; set; }
        }

        private sealed class CScreenSectionDto
        {
            [JsonPropertyName("name")]
            [JsonRequired]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("layout-policy")]
            [JsonRequired]
            public string LayoutPolicy { get; set; } = string.Empty;

            [JsonPropertyName("is-collapsible")]
            [JsonRequired]
            public bool IsCollapsible { get; set; }

            [JsonPropertyName("selection-actions")]
            public List<CScreenSelectionActionDto>? SelectionActions { get; set; }

            [JsonPropertyName("fields")]
            public List<CScreenFieldDto>? Fields { get; set; }

            [JsonPropertyName("sections")]
            public List<CScreenSectionDto>? Sections { get; set; }
        }

        private sealed class CScreenSelectionActionDto
        {
            [JsonPropertyName("event")]
            [JsonRequired]
            public string Event { get; set; } = string.Empty;

            [JsonPropertyName("target-screen")]
            [JsonRequired]
            public string TargetScreen { get; set; } = string.Empty;

            [JsonPropertyName("include-record-id")]
            public bool IncludeRecordId { get; set; } = true;
        }

        private sealed class CScreenFieldDto
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            [JsonRequired]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            [JsonRequired]
            public string Type { get; set; } = string.Empty;

            [JsonPropertyName("type-info")]
            public string? TypeInfo { get; set; }

            [JsonPropertyName("width")]
            [JsonRequired]
            public string Width { get; set; } = string.Empty;

            [JsonPropertyName("max-width")]
            public string? MaxWidth { get; set; }

            [JsonPropertyName("is-mandatory")]
            public bool IsMandatory { get; set; }

            [JsonPropertyName("is-searchable")]
            public bool IsSearchable { get; set; }
        }
    }
}