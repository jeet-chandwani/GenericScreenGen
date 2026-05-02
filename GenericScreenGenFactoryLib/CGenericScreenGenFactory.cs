using GenericScreenGenImplementationsLib;
using GenericScreenGenInterfacesLib;

namespace GenericScreenGenFactoryLib
{
    /// <summary>
    /// Default factory for creating initialized screen generator components.
    /// </summary>
    public sealed class CGenericScreenGenFactory : IGenericScreenGenFactory
    {
        private string? m_strScreenFolderPath;
        private string? m_strSchemaFilePath;

        /// <inheritdoc />
        public bool Init(object objInputParam, out string strError)
        {
            if (objInputParam is not string strScreenFolderPath || string.IsNullOrWhiteSpace(strScreenFolderPath))
            {
                strError = "Factory initialization requires a non-empty screen folder path.";
                return false;
            }

            m_strScreenFolderPath = strScreenFolderPath;
            m_strSchemaFilePath = Path.Combine(Path.GetDirectoryName(strScreenFolderPath) ?? strScreenFolderPath, "Schemas", "ScreenConfigSchema.json");
            strError = string.Empty;
            return true;
        }

        /// <inheritdoc />
        public bool TryCreateScreenConfigProvider(ILayoutPolicyRegistry itfLayoutPolicyRegistry, IFieldTypeRegistry itfFieldTypeRegistry, out IScreenConfigProvider? itfScreenConfigProvider, out string strError)
        {
            if (string.IsNullOrWhiteSpace(m_strScreenFolderPath))
            {
                itfScreenConfigProvider = null;
                strError = "Factory must be initialized before creating a screen configuration provider.";
                return false;
            }

            CScreenConfigProvider objScreenConfigProvider = new CScreenConfigProvider(itfLayoutPolicyRegistry, itfFieldTypeRegistry);

            if (!objScreenConfigProvider.Init(m_strScreenFolderPath, out strError))
            {
                itfScreenConfigProvider = null;
                return false;
            }

            itfScreenConfigProvider = objScreenConfigProvider;
            strError = string.Empty;
            return true;
        }

        /// <inheritdoc />
        public bool TryCreateScreenSchemaValidator(out IScreenSchemaValidator? itfScreenSchemaValidator, out string strError)
        {
            if (string.IsNullOrWhiteSpace(m_strSchemaFilePath))
            {
                itfScreenSchemaValidator = null;
                strError = "Factory must be initialized before creating a screen schema validator.";
                return false;
            }

            CScreenSchemaValidator objScreenSchemaValidator = new CScreenSchemaValidator();

            if (!objScreenSchemaValidator.Init(m_strSchemaFilePath, out strError))
            {
                itfScreenSchemaValidator = null;
                return false;
            }

            itfScreenSchemaValidator = objScreenSchemaValidator;
            strError = string.Empty;
            return true;
        }

        /// <inheritdoc />
        public bool TryCreateScreenRenderModelFactory(out IScreenRenderModelFactory? itfScreenRenderModelFactory, out string strError)
        {
            CScreenRenderModelFactory objScreenRenderModelFactory = new CScreenRenderModelFactory();

            if (!objScreenRenderModelFactory.Init(string.Empty, out strError))
            {
                itfScreenRenderModelFactory = null;
                return false;
            }

            itfScreenRenderModelFactory = objScreenRenderModelFactory;
            strError = string.Empty;
            return true;
        }
    }
}