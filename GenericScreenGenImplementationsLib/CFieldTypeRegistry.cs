using System.Text.Json;
using System.Text.Json.Serialization;
using GenericScreenGenInterfacesLib;
using Json.Schema;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Loads and exposes field type definitions from registry configuration.
    /// </summary>
    public sealed class CFieldTypeRegistry : ACanInitBase, IFieldTypeRegistry
    {
        private readonly Dictionary<string, IFieldTypeDefinition> m_dictFieldTypes = new Dictionary<string, IFieldTypeDefinition>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public bool TryGetFieldTypeDefinition(string strFieldTypeId, out IFieldTypeDefinition? itfFieldTypeDefinition)
        {
            if (string.IsNullOrWhiteSpace(strFieldTypeId))
            {
                itfFieldTypeDefinition = null;
                return false;
            }

            return m_dictFieldTypes.TryGetValue(strFieldTypeId.Trim().ToLowerInvariant(), out itfFieldTypeDefinition);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IFieldTypeDefinition> GetAllFieldTypes()
        {
            return m_dictFieldTypes.Values.ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public bool IsValidFieldTypeId(string strFieldTypeId)
        {
            return !string.IsNullOrWhiteSpace(strFieldTypeId) && m_dictFieldTypes.ContainsKey(strFieldTypeId.Trim().ToLowerInvariant());
        }

        /// <inheritdoc />
        protected override bool TryInitCore(object objInputParam, out string strError)
        {
            if (objInputParam is not string strContentRootPath || string.IsNullOrWhiteSpace(strContentRootPath))
            {
                strError = "Field type registry requires a non-empty content root path.";
                return false;
            }

            string strRegistryFilePath = Path.Combine(strContentRootPath, "screen", "registry-field-types.json");
            string strRegistrySchemaFilePath = Path.Combine(strContentRootPath, "Schemas", "FieldTypesRegistrySchema.json");

            if (!File.Exists(strRegistryFilePath))
            {
                strError = $"Field type registry file '{strRegistryFilePath}' was not found.";
                return false;
            }

            if (!File.Exists(strRegistrySchemaFilePath))
            {
                strError = $"Field type registry schema file '{strRegistrySchemaFilePath}' was not found.";
                return false;
            }

            try
            {
                JsonSchema objSchema = JsonSchema.FromText(File.ReadAllText(strRegistrySchemaFilePath));

                using JsonDocument objRegistryJsonDocument = JsonDocument.Parse(File.ReadAllText(strRegistryFilePath));
                EvaluationResults objEvaluationResults = objSchema.Evaluate(objRegistryJsonDocument.RootElement, new EvaluationOptions
                {
                    OutputFormat = OutputFormat.List,
                    RequireFormatValidation = true
                });

                if (!objEvaluationResults.IsValid)
                {
                    string strSchemaValidationErrors = BuildSchemaValidationError(objEvaluationResults);
                    strError = $"Field type registry validation failed. {strSchemaValidationErrors}";
                    return false;
                }

                CFieldTypesRegistryDto? objRegistryDto = JsonSerializer.Deserialize<CFieldTypesRegistryDto>(objRegistryJsonDocument.RootElement.GetRawText());
                if (objRegistryDto?.FieldTypes is null || objRegistryDto.FieldTypes.Count == 0)
                {
                    strError = "Field type registry must contain at least one field type definition.";
                    return false;
                }

                m_dictFieldTypes.Clear();
                HashSet<string> setFieldTypeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (CFieldTypeDto objFieldTypeDto in objRegistryDto.FieldTypes)
                {
                    string strFieldTypeId = objFieldTypeDto.Id.Trim().ToLowerInvariant();
                    string strFieldTypeName = objFieldTypeDto.Name.Trim();

                    if (m_dictFieldTypes.ContainsKey(strFieldTypeId))
                    {
                        strError = $"Field type registry has duplicate id '{strFieldTypeId}'.";
                        return false;
                    }

                    if (setFieldTypeNames.Contains(strFieldTypeName))
                    {
                        strError = $"Field type registry has duplicate name '{strFieldTypeName}'.";
                        return false;
                    }

                    Dictionary<string, string> dictParameters = objFieldTypeDto.Parameters is null
                        ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, string>(objFieldTypeDto.Parameters, StringComparer.OrdinalIgnoreCase);
                    List<string> lstValidators = objFieldTypeDto.Validators?.Where(strItem => !string.IsNullOrWhiteSpace(strItem)).Select(strItem => strItem.Trim()).ToList()
                        ?? new List<string>();

                    if (lstValidators.Count != 0)
                    {
                        strError = $"Field type '{strFieldTypeId}' must define validators as an empty array for current BRD-06 scope.";
                        return false;
                    }

                    m_dictFieldTypes[strFieldTypeId] = new CFieldTypeDefinition(strFieldTypeId, strFieldTypeName, dictParameters, lstValidators);
                    setFieldTypeNames.Add(strFieldTypeName);
                }

                strError = string.Empty;
                return true;
            }
            catch (JsonException objJsonException)
            {
                strError = $"Failed to parse field type registry. {objJsonException.Message}";
                return false;
            }
            catch (Exception objException)
            {
                strError = $"Failed to initialize field type registry. {objException.Message}";
                return false;
            }
        }

        private static string BuildSchemaValidationError(EvaluationResults objEvaluationResults)
        {
            List<string> lstErrors = new List<string>();
            CollectSchemaValidationErrors(objEvaluationResults, lstErrors);
            return lstErrors.Count == 0 ? "Schema validation failed with unknown errors." : string.Join(" ", lstErrors);
        }

        private static void CollectSchemaValidationErrors(EvaluationResults objEvaluationResults, ICollection<string> lstErrors)
        {
            if (objEvaluationResults.Errors is not null)
            {
                foreach (KeyValuePair<string, string> kvpError in objEvaluationResults.Errors)
                {
                    string strLocation = objEvaluationResults.InstanceLocation.ToString();
                    lstErrors.Add($"[{kvpError.Key}] at '{strLocation}': {kvpError.Value}");
                }
            }

            if (objEvaluationResults.Details is null)
            {
                return;
            }

            foreach (EvaluationResults objChildResult in objEvaluationResults.Details)
            {
                CollectSchemaValidationErrors(objChildResult, lstErrors);
            }
        }

        private sealed class CFieldTypesRegistryDto
        {
            [JsonPropertyName("field-types")]
            [JsonRequired]
            public List<CFieldTypeDto> FieldTypes { get; set; } = new List<CFieldTypeDto>();
        }

        private sealed class CFieldTypeDto
        {
            [JsonPropertyName("id")]
            [JsonRequired]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            [JsonRequired]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("parameters")]
            public Dictionary<string, string>? Parameters { get; set; }

            [JsonPropertyName("validators")]
            public List<string>? Validators { get; set; }
        }
    }
}
