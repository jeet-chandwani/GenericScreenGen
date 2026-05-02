using GenericScreenGenInterfacesLib;

namespace GenericScreenGenFactoryLib
{
    /// <summary>
    /// Creates initialized screen generator components for the solution.
    /// </summary>
    public interface IGenericScreenGenFactory : ICanInit
    {
        /// <summary>
        /// Creates an initialized screen configuration provider that uses the supplied layout policy
        /// registry to validate <c>layout-policy</c> values in screen config files.
        /// </summary>
        /// <param name="itfLayoutPolicyRegistry">Registry of all registered layout policies.</param>
        /// <param name="itfFieldTypeRegistry">Registry of all configured field types.</param>
        /// <param name="itfScreenConfigProvider">Initialized screen configuration provider.</param>
        /// <param name="strError">Detailed error information when creation fails.</param>
        /// <returns>True when the provider is created successfully; otherwise false.</returns>
        bool TryCreateScreenConfigProvider(ILayoutPolicyRegistry itfLayoutPolicyRegistry, IFieldTypeRegistry itfFieldTypeRegistry, out IScreenConfigProvider? itfScreenConfigProvider, out string strError);

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