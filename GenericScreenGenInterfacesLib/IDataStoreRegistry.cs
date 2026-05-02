using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Resolves IDataStore instances by configured data store id.
    /// </summary>
    public interface IDataStoreRegistry
    {
        bool TryGetDataStore(string strDataStoreId, out IDataStore? itfDataStore, out string strError);

        IReadOnlyCollection<string> GetAllDataStoreIds();
    }
}
