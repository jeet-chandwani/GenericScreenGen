using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Represents a configured data store entry loaded from datastore.*.config.json.
    /// </summary>
    public interface IDataStoreConfig
    {
        string Id { get; }

        string Name { get; }

        string ProviderType { get; }

        IReadOnlyDictionary<string, string> Parameters { get; }
    }
}
