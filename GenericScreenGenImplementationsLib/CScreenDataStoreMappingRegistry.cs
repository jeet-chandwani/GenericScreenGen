using System.Text.Json;
using System.Text.Json.Serialization;
using GenericScreenGenInterfacesLib;
using GenericScreenGenUtilsLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Loads screen-datastore-mapping.*.json files and resolves data store ids by screen id.
    /// </summary>
    public sealed class CScreenDataStoreMappingRegistry : ACanInitBase, IScreenDataStoreMappingRegistry
    {
        private readonly IDataStoreRegistry m_itfDataStoreRegistry;
        private readonly Dictionary<string, string> m_dictScreenIdToDataStoreId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public CScreenDataStoreMappingRegistry(IDataStoreRegistry itfDataStoreRegistry)
        {
            m_itfDataStoreRegistry = itfDataStoreRegistry;
        }

        public bool TryGetDataStoreIdForScreen(string strScreenId, out string strDataStoreId, out string strError)
        {
            strDataStoreId = string.Empty;

            if (string.IsNullOrWhiteSpace(strScreenId))
            {
                strError = "Screen id cannot be empty.";
                return false;
            }

            if (!m_dictScreenIdToDataStoreId.TryGetValue(strScreenId.Trim(), out string? strResolvedDataStoreId))
            {
                strError = $"Screen '{strScreenId}' is not associated with any data store.";
                return false;
            }

            strDataStoreId = strResolvedDataStoreId ?? string.Empty;

            strError = string.Empty;
            return true;
        }

        protected override bool TryInitCore(object objInputParam, out string strError)
        {
            if (objInputParam is not string strContentRootPath || string.IsNullOrWhiteSpace(strContentRootPath))
            {
                strError = "Screen-data-store mapping registry requires a non-empty content root path.";
                return false;
            }

            string strMappingsFolderPath = Path.Combine(strContentRootPath, CScreenGeneratorConstants.DATASTORE_MAPPING_FOLDER_NAME);
            if (!Directory.Exists(strMappingsFolderPath))
            {
                strError = $"Screen-data-store mapping folder '{strMappingsFolderPath}' was not found.";
                return false;
            }

            string[] arrMappingFiles = Directory.GetFiles(strMappingsFolderPath, "screen-datastore-mapping.*.json", SearchOption.TopDirectoryOnly);
            if (arrMappingFiles.Length == 0)
            {
                strError = "At least one screen-datastore-mapping.*.json file is required.";
                return false;
            }

            m_dictScreenIdToDataStoreId.Clear();

            try
            {
                foreach (string strMappingFilePath in arrMappingFiles)
                {
                    CScreenDataStoreMappingDto? objMappingDto = JsonSerializer.Deserialize<CScreenDataStoreMappingDto>(File.ReadAllText(strMappingFilePath));
                    if (objMappingDto is null)
                    {
                        strError = $"Screen-data-store mapping file '{strMappingFilePath}' is empty or invalid.";
                        return false;
                    }

                    string strDataStoreId = (objMappingDto.DataStoreId ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(strDataStoreId))
                    {
                        strError = $"Mapping file '{Path.GetFileName(strMappingFilePath)}' is missing required property 'datastore-id'.";
                        return false;
                    }

                    if (!m_itfDataStoreRegistry.TryGetDataStore(strDataStoreId, out _, out string strDataStoreLookupError))
                    {
                        strError = $"Mapping file '{Path.GetFileName(strMappingFilePath)}' references unknown datastore-id '{strDataStoreId}'. {strDataStoreLookupError}";
                        return false;
                    }

                    IReadOnlyList<string> lstScreenIds = objMappingDto.ScreenIds is null
                        ? Array.Empty<string>()
                        : objMappingDto.ScreenIds;
                    foreach (string strScreenIdRaw in lstScreenIds)
                    {
                        string strScreenId = (strScreenIdRaw ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(strScreenId))
                        {
                            continue;
                        }

                        if (m_dictScreenIdToDataStoreId.TryGetValue(strScreenId, out string? strExistingDataStoreId))
                        {
                            strError = $"Duplicate screen mapping for screen id '{strScreenId}'. Existing datastore-id: '{strExistingDataStoreId}', new datastore-id: '{strDataStoreId}'.";
                            return false;
                        }

                        m_dictScreenIdToDataStoreId[strScreenId] = strDataStoreId;
                    }
                }

                strError = string.Empty;
                return true;
            }
            catch (JsonException objJsonException)
            {
                strError = $"Failed to parse screen-data-store mapping file. {objJsonException.Message}";
                return false;
            }
            catch (Exception objException)
            {
                strError = $"Failed to initialize screen-data-store mapping registry. {objException.Message}";
                return false;
            }
        }

        private sealed class CScreenDataStoreMappingDto
        {
            [JsonPropertyName("datastore-id")]
            [JsonRequired]
            public string DataStoreId { get; set; } = string.Empty;

            [JsonPropertyName("screen-ids")]
            public List<string>? ScreenIds { get; set; }
        }
    }
}
