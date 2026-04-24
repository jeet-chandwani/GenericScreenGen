using GenericScreenGenInterfacesLib;

namespace GenericScreenGenFactoryLib
{
    /// <summary>
    /// Creates initialized screen generator components for the solution.
    /// </summary>
    public interface IGenericScreenGenFactory : ICanInit
    {
        /// <summary>
        /// Creates an initialized screen configuration provider.
        /// </summary>
        /// <param name="itfScreenConfigProvider">Initialized screen configuration provider.</param>
        /// <param name="strError">Detailed error information when creation fails.</param>
        /// <returns>True when the provider is created successfully; otherwise false.</returns>
        bool TryCreateScreenConfigProvider(out IScreenConfigProvider? itfScreenConfigProvider, out string strError);

        /// <summary>
        /// Creates an initialized screen schema validator.
        /// </summary>
        bool TryCreateScreenSchemaValidator(out IScreenSchemaValidator? itfScreenSchemaValidator, out string strError);

        /// <summary>
        /// Creates an initialized screen render model factory.
        /// </summary>
        bool TryCreateScreenRenderModelFactory(out IScreenRenderModelFactory? itfScreenRenderModelFactory, out string strError);
    }
}