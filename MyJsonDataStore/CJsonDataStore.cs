using System.Text.Json;
using GenericScreenGenInterfacesLib;

namespace MyJsonDataStore
{
    /// <summary>
    /// JSON-backed implementation of IDataStore.
    /// Persists each screen key as an individual JSON file under a configured folder.
    /// </summary>
    public sealed class CJsonDataStore : IDataStore
    {
        private readonly string m_strRootFolderPath;

        public CJsonDataStore(string strRootFolderPath)
        {
            m_strRootFolderPath = strRootFolderPath;
        }

        public bool TryLoadRows(string strScreenKey, out IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, out string strError)
        {
            lstRows = Array.Empty<IReadOnlyDictionary<string, string>>();
            strError = string.Empty;

            if (!TryEnsureRootFolder(out strError))
            {
                return false;
            }

            string strDataFilePath = BuildScreenFilePath(strScreenKey);
            if (!File.Exists(strDataFilePath))
            {
                return true;
            }

            try
            {
                string strJson = File.ReadAllText(strDataFilePath);
                List<Dictionary<string, string>>? lstParsedRows = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(strJson);
                lstRows = (lstParsedRows ?? new List<Dictionary<string, string>>()).Select(dictRow => (IReadOnlyDictionary<string, string>)dictRow).ToList();
                return true;
            }
            catch (Exception ex)
            {
                strError = "Failed to load data for screen key '" + strScreenKey + "'. " + ex.Message;
                return false;
            }
        }

        public bool TrySaveRows(string strScreenKey, IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, out string strError)
        {
            strError = string.Empty;

            if (!TryEnsureRootFolder(out strError))
            {
                return false;
            }

            string strDataFilePath = BuildScreenFilePath(strScreenKey);

            try
            {
                List<Dictionary<string, string>> lstWritableRows = lstRows
                    .Select(dictRow => dictRow.ToDictionary(objKvp => objKvp.Key, objKvp => objKvp.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                string strJson = JsonSerializer.Serialize(lstWritableRows, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(strDataFilePath, strJson);
                return true;
            }
            catch (Exception ex)
            {
                strError = "Failed to save data for screen key '" + strScreenKey + "'. " + ex.Message;
                return false;
            }
        }

        public bool TryDeleteRows(string strScreenKey, out string strError)
        {
            strError = string.Empty;

            if (!TryEnsureRootFolder(out strError))
            {
                return false;
            }

            string strDataFilePath = BuildScreenFilePath(strScreenKey);
            if (!File.Exists(strDataFilePath))
            {
                return true;
            }

            try
            {
                File.Delete(strDataFilePath);
                return true;
            }
            catch (Exception ex)
            {
                strError = "Failed to delete data for screen key '" + strScreenKey + "'. " + ex.Message;
                return false;
            }
        }

        private string BuildScreenFilePath(string strScreenKey)
        {
            string strSafeKey = string.Concat(strScreenKey.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_'));
            if (string.IsNullOrWhiteSpace(strSafeKey))
            {
                strSafeKey = "default";
            }

            return Path.Combine(m_strRootFolderPath, strSafeKey + ".rows.json");
        }

        private bool TryEnsureRootFolder(out string strError)
        {
            strError = string.Empty;

            try
            {
                Directory.CreateDirectory(m_strRootFolderPath);
                return true;
            }
            catch (Exception ex)
            {
                strError = "Failed to initialize data-store folder '" + m_strRootFolderPath + "'. " + ex.Message;
                return false;
            }
        }
    }
}
