using System.Text.Json;
using System.Text.Json.Serialization;
using GenericScreenGenInterfacesLib;
using GenericScreenGenUtilsLib;
using MyDataStoreProviders;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Loads datastore.*.config.json entries and resolves IDataStore instances by id.
    /// </summary>
    public sealed class CDataStoreRegistry : ACanInitBase, IDataStoreRegistry
    {
        private readonly Dictionary<string, IDataStore> m_dictDataStores = new Dictionary<string, IDataStore>(StringComparer.OrdinalIgnoreCase);

        public bool TryGetDataStore(string strDataStoreId, out IDataStore? itfDataStore, out string strError)
        {
            if (string.IsNullOrWhiteSpace(strDataStoreId))
            {
                itfDataStore = null;
                strError = "Data store id cannot be empty.";
                return false;
            }

            if (!m_dictDataStores.TryGetValue(strDataStoreId.Trim(), out itfDataStore))
            {
                strError = $"Data store '{strDataStoreId}' was not found in registry.";
                return false;
            }

            strError = string.Empty;
            return true;
        }

        public IReadOnlyCollection<string> GetAllDataStoreIds()
        {
            return m_dictDataStores.Keys.ToList().AsReadOnly();
        }

        protected override bool TryInitCore(object objInputParam, out string strError)
        {
            if (objInputParam is not string strContentRootPath || string.IsNullOrWhiteSpace(strContentRootPath))
            {
                strError = "Data store registry requires a non-empty content root path.";
                return false;
            }

            string strConfigFolderPath = Path.Combine(strContentRootPath, CScreenGeneratorConstants.DATASTORE_CONFIG_FOLDER_NAME);
            if (!Directory.Exists(strConfigFolderPath))
            {
                strError = $"Data store config folder '{strConfigFolderPath}' was not found.";
                return false;
            }

            string[] arrConfigFiles = Directory.GetFiles(strConfigFolderPath, "datastore.*.config.json", SearchOption.TopDirectoryOnly);
            if (arrConfigFiles.Length == 0)
            {
                strError = "Data store registry requires at least one datastore.*.config.json file.";
                return false;
            }

            m_dictDataStores.Clear();

            try
            {
                foreach (string strConfigFilePath in arrConfigFiles)
                {
                    CDataStoreConfigDto? objConfigDto = JsonSerializer.Deserialize<CDataStoreConfigDto>(File.ReadAllText(strConfigFilePath));
                    if (objConfigDto is null)
                    {
                        strError = $"Data store config file '{strConfigFilePath}' is empty or invalid.";
                        return false;
                    }

                    string strId = (objConfigDto.Id ?? string.Empty).Trim();
                    string strName = string.IsNullOrWhiteSpace(objConfigDto.Name) ? strId : objConfigDto.Name.Trim();
                    string strProviderType = (objConfigDto.ProviderType ?? string.Empty).Trim().ToLowerInvariant();
                    Dictionary<string, string> dictParameters = objConfigDto.Parameters is null
                        ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, string>(objConfigDto.Parameters, StringComparer.OrdinalIgnoreCase);

                    if (string.IsNullOrWhiteSpace(strId))
                    {
                        strError = $"Data store config '{Path.GetFileName(strConfigFilePath)}' is missing required property 'id'.";
                        return false;
                    }

                    if (m_dictDataStores.ContainsKey(strId))
                    {
                        strError = $"Duplicate data store id '{strId}' found in config files.";
                        return false;
                    }

                    IDataStore? itfDataStore = CreateDataStore(strProviderType, dictParameters, strContentRootPath, out strError);
                    if (itfDataStore is null)
                    {
                        return false;
                    }

                    _ = new CDataStoreConfig(strId, strName, strProviderType, dictParameters);
                    m_dictDataStores[strId] = itfDataStore;
                }

                strError = string.Empty;
                return true;
            }
            catch (JsonException objJsonException)
            {
                strError = $"Failed to parse data store config. {objJsonException.Message}";
                return false;
            }
            catch (Exception objException)
            {
                strError = $"Failed to initialize data store registry. {objException.Message}";
                return false;
            }
        }

        private static IDataStore? CreateDataStore(string strProviderType, IReadOnlyDictionary<string, string> dictParameters, string strContentRootPath, out string strError)
        {
            switch (strProviderType)
            {
                case "json":
                {
                    if (!dictParameters.TryGetValue("root-folder", out string? strRootFolder) || string.IsNullOrWhiteSpace(strRootFolder))
                    {
                        strError = "JSON data store config requires 'parameters.root-folder'.";
                        return null;
                    }

                    string strResolvedFolderPath = Path.IsPathRooted(strRootFolder)
                        ? strRootFolder
                        : Path.Combine(strContentRootPath, strRootFolder);

                    strError = string.Empty;
                    return new CJsonDataStore(strResolvedFolderPath);
                }
                case "sql-server":
                {
                    if (!dictParameters.TryGetValue("connection-string", out string? strConnectionString) || string.IsNullOrWhiteSpace(strConnectionString))
                    {
                        strError = "SQL Server data store config requires 'parameters.connection-string'.";
                        return null;
                    }

                    strError = string.Empty;
                    return new CDbDataStore(strConnectionString);
                }
                default:
                    strError = $"Unsupported data store provider type '{strProviderType}'.";
                    return null;
            }
        }

        private sealed class CDataStoreConfigDto
        {
            [JsonPropertyName("id")]
            [JsonRequired]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("provider-type")]
            [JsonRequired]
            public string ProviderType { get; set; } = string.Empty;

            [JsonPropertyName("parameters")]
            public Dictionary<string, string>? Parameters { get; set; }
        }
    }
}
