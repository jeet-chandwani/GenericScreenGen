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
            string strInputType = itfFieldDefinition.Type switch
            {
                EFieldType.Integer => "number",
                EFieldType.Button => "button",
                _ => "text"
            };

            return new CScreenRenderFieldModel(
                itfFieldDefinition.Id,
                itfFieldDefinition.Name,
                itfFieldDefinition.Description,
                itfFieldDefinition.Type,
                itfFieldDefinition.Width,
                strInputType,
                itfFieldDefinition.Type == EFieldType.Button);
        }
    }
}