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
                string strDefaultScreenName = CScreenNameUtility.GetScreenNameFromFileName(strScreenFileName);
                string strDisplayName = string.IsNullOrWhiteSpace(objScreenDocument.Name)
                    ? strDefaultScreenName
                    : objScreenDocument.Name.Trim();
                string strScreenId = string.IsNullOrWhiteSpace(objScreenDocument.Id)
                    ? CScreenNameUtility.GetScreenIdFromFileName(strScreenFileName)
                    : objScreenDocument.Id.Trim();
                itfScreenDefinition = new CScreenDefinition(strScreenId, strScreenFileName, strDisplayName, lstSections);
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

            if (objSection.Fields is not null)
            {
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

            string strSectionName = string.IsNullOrWhiteSpace(objSection.Name)
                ? CScreenGeneratorConstants.DEFAULT_SECTION_NAME
                : objSection.Name;
            string strLayoutPolicy = string.IsNullOrWhiteSpace(objSection.LayoutPolicy)
                ? CScreenGeneratorConstants.DEFAULT_LAYOUT_POLICY
                : objSection.LayoutPolicy;

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
                objSection.IsCollapsible ?? true,
                lstFields,
                lstSections,
                objSection.DetailScreen ?? string.Empty);
            strError = string.Empty;
            return true;
        }

        private static bool TryCreateFieldDefinition(CScreenFieldDto objField, out IScreenFieldDefinition? itfFieldDefinition, out string strError)
        {
            if (string.IsNullOrWhiteSpace(objField.Id) || string.IsNullOrWhiteSpace(objField.Name))
            {
                itfFieldDefinition = null;
                strError = "Each field must define both id and name values.";
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
                objField.Description ?? string.Empty,
                enuFieldType,
                objField.TypeInfo ?? string.Empty,
                string.IsNullOrWhiteSpace(objField.Width) ? "300px" : objField.Width,
                objField.MaxWidth ?? string.Empty,
                objField.IsMandatory,
                objField.IsSearchable);
            strError = string.Empty;
            return true;
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
            public string? Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("sections")]
            public List<CScreenSectionDto>? Sections { get; set; }
        }

        private sealed class CScreenSectionDto
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("layout-policy")]
            public string? LayoutPolicy { get; set; }

            [JsonPropertyName("is-collapsible")]
            public bool? IsCollapsible { get; set; }

            [JsonPropertyName("detail-screen")]
            public string? DetailScreen { get; set; }

            [JsonPropertyName("fields")]
            public List<CScreenFieldDto>? Fields { get; set; }

            [JsonPropertyName("sections")]
            public List<CScreenSectionDto>? Sections { get; set; }
        }

        private sealed class CScreenFieldDto
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [JsonPropertyName("type-info")]
            public string? TypeInfo { get; set; }

            [JsonPropertyName("width")]
            public string? Width { get; set; }

            [JsonPropertyName("max-width")]
            public string? MaxWidth { get; set; }

            [JsonPropertyName("is-mandatory")]
            public bool IsMandatory { get; set; }

            [JsonPropertyName("is-searchable")]
            public bool IsSearchable { get; set; }
        }
    }
}