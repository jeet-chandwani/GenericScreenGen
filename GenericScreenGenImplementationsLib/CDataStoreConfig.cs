using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Default IDataStoreConfig implementation.
    /// </summary>
    public sealed class CDataStoreConfig : IDataStoreConfig
    {
        public CDataStoreConfig(string strId, string strName, string strProviderType, IReadOnlyDictionary<string, string> dictParameters)
        {
            Id = strId;
            Name = strName;
            ProviderType = strProviderType;
            Parameters = dictParameters;
        }

        public string Id { get; }

        public string Name { get; }

        public string ProviderType { get; }

        public IReadOnlyDictionary<string, string> Parameters { get; }
    }
}
