namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Creates UI-ready render models from parsed screen definitions.
    /// </summary>
    public interface IScreenRenderModelFactory : ICanInit
    {
        /// <summary>
        /// Creates a render model for a parsed screen definition.
        /// </summary>
        bool TryCreateRenderModel(IScreenDefinition itfScreenDefinition, out IScreenRenderModel? itfScreenRenderModel, out string strError);
    }
}