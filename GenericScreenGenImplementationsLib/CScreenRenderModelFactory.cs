using GenericScreenGenInterfacesLib;
using GenericScreenGenUtilsLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Creates UI-ready render models from parsed screen definitions.
    /// </summary>
    public sealed class CScreenRenderModelFactory : IScreenRenderModelFactory
    {
        public bool Init(object objInputParam, out string strError)
        {
            strError = string.Empty;
            return true;
        }

        public bool TryCreateRenderModel(IScreenDefinition itfScreenDefinition, out IScreenRenderModel? itfScreenRenderModel, out string strError)
        {
            if (itfScreenDefinition is null)
            {
                itfScreenRenderModel = null;
                strError = "Screen definition is required.";
                return false;
            }

            List<IScreenRenderSectionModel> lstSections = itfScreenDefinition.Sections
                .Select(CreateSectionRenderModel)
                .Cast<IScreenRenderSectionModel>()
                .ToList();

            itfScreenRenderModel = new CScreenRenderModel(itfScreenDefinition.ScreenId, itfScreenDefinition.ScreenFileName, itfScreenDefinition.DisplayName, lstSections);
            strError = string.Empty;
            return true;
        }

        private static CScreenRenderSectionModel CreateSectionRenderModel(IScreenSectionDefinition itfSectionDefinition)
        {
            List<IScreenRenderFieldModel> lstFields = itfSectionDefinition.Fields
                .Select(CreateFieldRenderModel)
                .Cast<IScreenRenderFieldModel>()
                .ToList();

            List<IScreenRenderSectionModel> lstSections = itfSectionDefinition.Sections
                .Select(CreateSectionRenderModel)
                .Cast<IScreenRenderSectionModel>()
                .ToList();

            return new CScreenRenderSectionModel(
                itfSectionDefinition.Name,
                itfSectionDefinition.LayoutPolicy,
                itfSectionDefinition.IsCollapsible,
                !string.Equals(itfSectionDefinition.Name, CScreenGeneratorConstants.DEFAULT_SECTION_NAME, StringComparison.OrdinalIgnoreCase),
                lstFields,
                lstSections);
        }

        private static CScreenRenderFieldModel CreateFieldRenderModel(IScreenFieldDefinition itfFieldDefinition)
        {
            string strControlType;
            string strInputType = itfFieldDefinition.Type switch
            {
                EFieldType.Integer => "number",
                EFieldType.Button => "button",
                EFieldType.Date => "date",
                EFieldType.DateTime => "datetime-local",
                EFieldType.Lookup => "select",
                _ => "text"
            };

            int iMinChars = 0;
            int iMaxChars = 0;
            int iLines = 1;
            IReadOnlyList<string> lstLookupValues = Array.Empty<string>();

            switch (itfFieldDefinition.Type)
            {
                case EFieldType.Text:
                    ParseTextTypeInfo(itfFieldDefinition.TypeInfo, out iMinChars, out iMaxChars, out iLines);
                    strControlType = iLines > 1 ? "textarea" : "input";
                    break;
                case EFieldType.Lookup:
                    lstLookupValues = ParseLookupTypeInfo(itfFieldDefinition.TypeInfo);
                    strControlType = "select";
                    break;
                case EFieldType.Button:
                    strControlType = "button";
                    break;
                default:
                    strControlType = "input";
                    break;
            }

            return new CScreenRenderFieldModel(
                itfFieldDefinition.Id,
                itfFieldDefinition.Name,
                itfFieldDefinition.Description,
                itfFieldDefinition.Type,
                itfFieldDefinition.TypeInfo,
                itfFieldDefinition.Width,
                strControlType,
                strInputType,
                iMinChars,
                iMaxChars,
                iLines,
                lstLookupValues,
                itfFieldDefinition.Type == EFieldType.Button);
        }

        private static void ParseTextTypeInfo(string strTypeInfo, out int iMinChars, out int iMaxChars, out int iLines)
        {
            iMinChars = 0;
            iMaxChars = 10;
            iLines = 1;

            if (string.IsNullOrWhiteSpace(strTypeInfo))
            {
                return;
            }

            string strNormalizedTypeInfo = strTypeInfo.Trim().Trim('{', '}');
            string[] arrTypeInfoParts = strNormalizedTypeInfo.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (string strTypeInfoPart in arrTypeInfoParts)
            {
                string[] arrKeyValue = strTypeInfoPart.Split('=', 2, StringSplitOptions.TrimEntries);

                if (arrKeyValue.Length != 2)
                {
                    continue;
                }

                string strKey = arrKeyValue[0].Trim().ToLowerInvariant();
                string strValue = arrKeyValue[1].Trim();

                if (!int.TryParse(strValue, out int iParsedValue))
                {
                    continue;
                }

                switch (strKey)
                {
                    case "min-chars":
                        iMinChars = Math.Max(0, iParsedValue);
                        break;
                    case "max-chars":
                        iMaxChars = Math.Max(0, iParsedValue);
                        break;
                    case "lines":
                        iLines = Math.Max(1, iParsedValue);
                        break;
                }
            }

            if (iMaxChars > 0 && iMaxChars < iMinChars)
            {
                iMaxChars = iMinChars;
            }
        }

        private static IReadOnlyList<string> ParseLookupTypeInfo(string strTypeInfo)
        {
            if (string.IsNullOrWhiteSpace(strTypeInfo))
            {
                return Array.Empty<string>();
            }

            string strNormalizedTypeInfo = strTypeInfo.Trim().Trim('{', '}');

            return strNormalizedTypeInfo
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(strItem => !string.IsNullOrWhiteSpace(strItem))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}