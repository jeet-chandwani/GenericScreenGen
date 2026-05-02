namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Provides the screen-id to datastore-id association.
    /// </summary>
    public interface IScreenDataStoreMappingRegistry
    {
        bool TryGetDataStoreIdForScreen(string strScreenId, out string strDataStoreId, out string strError);
    }
}
