using System.Data;
using System.Text.Json;
using GenericScreenGenInterfacesLib;
using Microsoft.Data.SqlClient;

namespace MyDataStoreProviders
{
    /// <summary>
    /// SQL Server-backed implementation of IDataStore.
    /// Uses a single table with one row per screen key + record id.
    /// </summary>
    public sealed class CDbDataStore : IDataStore
    {
        private const string RECORD_ID_FIELD_NAME = "__record-id";
        private readonly string m_strConnectionString;

        public CDbDataStore(string strConnectionString)
        {
            m_strConnectionString = strConnectionString;
        }

        public bool TryLoadRows(string strScreenKey, out IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, out string strError)
        {
            lstRows = Array.Empty<IReadOnlyDictionary<string, string>>();
            strError = string.Empty;

            if (string.IsNullOrWhiteSpace(strScreenKey))
            {
                strError = "Screen key cannot be empty.";
                return false;
            }

            try
            {
                using SqlConnection objConnection = new SqlConnection(m_strConnectionString);
                objConnection.Open();

                if (!TryEnsureTableExists(objConnection, out strError))
                {
                    return false;
                }

                using SqlCommand objCommand = new SqlCommand(@"
SELECT RecordId, RowData
FROM dbo.ScreenData
WHERE ScreenKey = @screenKey;", objConnection);
                objCommand.Parameters.Add(new SqlParameter("@screenKey", SqlDbType.NVarChar, 200) { Value = strScreenKey });

                using SqlDataReader objReader = objCommand.ExecuteReader();
                List<IReadOnlyDictionary<string, string>> lstResult = new List<IReadOnlyDictionary<string, string>>();

                while (objReader.Read())
                {
                    string strRecordId = objReader.GetString(0);
                    string strRowData = objReader.GetString(1);

                    Dictionary<string, string>? dictRow = JsonSerializer.Deserialize<Dictionary<string, string>>(strRowData);
                    Dictionary<string, string> dictSafeRow = dictRow is null
                        ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, string>(dictRow, StringComparer.OrdinalIgnoreCase);

                    dictSafeRow[RECORD_ID_FIELD_NAME] = strRecordId;
                    lstResult.Add(dictSafeRow);
                }

                lstRows = lstResult;
                return true;
            }
            catch (Exception objException)
            {
                strError = $"Failed to load rows from SQL data store for screen '{strScreenKey}'. {objException.Message}";
                return false;
            }
        }

        public bool TrySaveRows(string strScreenKey, IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, out string strError)
        {
            strError = string.Empty;

            if (string.IsNullOrWhiteSpace(strScreenKey))
            {
                strError = "Screen key cannot be empty.";
                return false;
            }

            try
            {
                using SqlConnection objConnection = new SqlConnection(m_strConnectionString);
                objConnection.Open();

                if (!TryEnsureTableExists(objConnection, out strError))
                {
                    return false;
                }

                using SqlTransaction objTransaction = objConnection.BeginTransaction();

                using (SqlCommand objDeleteCommand = new SqlCommand("DELETE FROM dbo.ScreenData WHERE ScreenKey = @screenKey;", objConnection, objTransaction))
                {
                    objDeleteCommand.Parameters.Add(new SqlParameter("@screenKey", SqlDbType.NVarChar, 200) { Value = strScreenKey });
                    objDeleteCommand.ExecuteNonQuery();
                }

                foreach (IReadOnlyDictionary<string, string> itfRow in lstRows)
                {
                    if (!itfRow.TryGetValue(RECORD_ID_FIELD_NAME, out string? strRecordId) || string.IsNullOrWhiteSpace(strRecordId))
                    {
                        objTransaction.Rollback();
                        strError = $"Row for screen '{strScreenKey}' is missing required field '{RECORD_ID_FIELD_NAME}'.";
                        return false;
                    }

                    Dictionary<string, string> dictPayload = itfRow
                        .Where(objKvp => !string.Equals(objKvp.Key, RECORD_ID_FIELD_NAME, StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(objKvp => objKvp.Key, objKvp => objKvp.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

                    string strRowData = JsonSerializer.Serialize(dictPayload);

                    using SqlCommand objInsertCommand = new SqlCommand(@"
INSERT INTO dbo.ScreenData (ScreenKey, RecordId, RowData)
VALUES (@screenKey, @recordId, @rowData);", objConnection, objTransaction);
                    objInsertCommand.Parameters.Add(new SqlParameter("@screenKey", SqlDbType.NVarChar, 200) { Value = strScreenKey });
                    objInsertCommand.Parameters.Add(new SqlParameter("@recordId", SqlDbType.NVarChar, 100) { Value = strRecordId });
                    objInsertCommand.Parameters.Add(new SqlParameter("@rowData", SqlDbType.NVarChar) { Value = strRowData });
                    objInsertCommand.ExecuteNonQuery();
                }

                objTransaction.Commit();
                return true;
            }
            catch (Exception objException)
            {
                strError = $"Failed to save rows to SQL data store for screen '{strScreenKey}'. {objException.Message}";
                return false;
            }
        }

        public bool TryDeleteRows(string strScreenKey, out string strError)
        {
            strError = string.Empty;

            if (string.IsNullOrWhiteSpace(strScreenKey))
            {
                strError = "Screen key cannot be empty.";
                return false;
            }

            try
            {
                using SqlConnection objConnection = new SqlConnection(m_strConnectionString);
                objConnection.Open();

                if (!TryEnsureTableExists(objConnection, out strError))
                {
                    return false;
                }

                using SqlCommand objCommand = new SqlCommand("DELETE FROM dbo.ScreenData WHERE ScreenKey = @screenKey;", objConnection);
                objCommand.Parameters.Add(new SqlParameter("@screenKey", SqlDbType.NVarChar, 200) { Value = strScreenKey });
                objCommand.ExecuteNonQuery();

                return true;
            }
            catch (Exception objException)
            {
                strError = $"Failed to delete rows in SQL data store for screen '{strScreenKey}'. {objException.Message}";
                return false;
            }
        }

        private static bool TryEnsureTableExists(SqlConnection objConnection, out string strError)
        {
            strError = string.Empty;

            try
            {
                using SqlCommand objCommand = new SqlCommand(@"
IF OBJECT_ID('dbo.ScreenData', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScreenData
    (
        ScreenKey NVARCHAR(200) NOT NULL,
        RecordId NVARCHAR(100) NOT NULL,
        RowData NVARCHAR(MAX) NOT NULL,
        CONSTRAINT PK_ScreenData PRIMARY KEY (ScreenKey, RecordId)
    );
END;", objConnection);

                objCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception objException)
            {
                strError = "Failed to ensure SQL data table exists. " + objException.Message;
                return false;
            }
        }
    }
}
