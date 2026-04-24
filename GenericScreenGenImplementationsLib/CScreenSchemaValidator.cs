using System.Text.Json;
using GenericScreenGenInterfacesLib;
using Json.Schema;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Validates screen configuration files against a JSON schema file.
    /// </summary>
    public sealed class CScreenSchemaValidator : ACanInitBase, IScreenSchemaValidator
    {
        private JsonSchema? m_objSchema;

        public bool TryValidateScreen(string strScreenFilePath, out IScreenValidationResult? itfValidationResult, out string strError)
        {
            if (m_objSchema is null)
            {
                itfValidationResult = null;
                strError = "Schema validator is not initialized.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(strScreenFilePath) || !File.Exists(strScreenFilePath))
            {
                itfValidationResult = null;
                strError = $"Screen file '{strScreenFilePath}' was not found.";
                return false;
            }

            try
            {
                using JsonDocument objJsonDocument = JsonDocument.Parse(File.ReadAllText(strScreenFilePath));
                EvaluationResults objEvaluationResults = m_objSchema.Evaluate(objJsonDocument.RootElement, new EvaluationOptions
                {
                    OutputFormat = OutputFormat.List,
                    RequireFormatValidation = true
                });

                List<IScreenValidationIssue> lstIssues = new List<IScreenValidationIssue>();
                CollectIssues(objEvaluationResults, lstIssues);
                itfValidationResult = new CScreenValidationResult(Path.GetFileName(strScreenFilePath), objEvaluationResults.IsValid, lstIssues);
                strError = string.Empty;
                return true;
            }
            catch (JsonException objJsonException)
            {
                itfValidationResult = new CScreenValidationResult(
                    Path.GetFileName(strScreenFilePath),
                    false,
                    new List<IScreenValidationIssue>
                    {
                        new CScreenValidationIssue("json.parse", "$", objJsonException.Message)
                    });
                strError = string.Empty;
                return true;
            }
            catch (Exception objException)
            {
                itfValidationResult = null;
                strError = $"Failed to validate screen file '{Path.GetFileName(strScreenFilePath)}'. {objException.Message}";
                return false;
            }
        }

        protected override bool TryInitCore(object objInputParam, out string strError)
        {
            if (objInputParam is not string strSchemaFilePath || string.IsNullOrWhiteSpace(strSchemaFilePath))
            {
                strError = "Schema validator requires a non-empty schema file path.";
                return false;
            }

            if (!File.Exists(strSchemaFilePath))
            {
                strError = $"Schema file '{strSchemaFilePath}' was not found.";
                return false;
            }

            try
            {
                m_objSchema = JsonSchema.FromText(File.ReadAllText(strSchemaFilePath));
                strError = string.Empty;
                return true;
            }
            catch (Exception objException)
            {
                strError = $"Failed to load schema file '{strSchemaFilePath}'. {objException.Message}";
                return false;
            }
        }

        private static void CollectIssues(EvaluationResults objEvaluationResults, ICollection<IScreenValidationIssue> lstIssues)
        {
            if (objEvaluationResults.Errors is not null)
            {
                foreach (KeyValuePair<string, string> kvpError in objEvaluationResults.Errors)
                {
                    lstIssues.Add(new CScreenValidationIssue(kvpError.Key, objEvaluationResults.InstanceLocation.ToString(), kvpError.Value));
                }
            }

            if (objEvaluationResults.Details is null)
            {
                return;
            }

            foreach (EvaluationResults objChildResult in objEvaluationResults.Details)
            {
                CollectIssues(objChildResult, lstIssues);
            }
        }
    }
}