using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Concrete field type definition loaded from registry configuration.
    /// </summary>
    public sealed class CFieldTypeDefinition : IFieldTypeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CFieldTypeDefinition"/> class.
        /// </summary>
        public CFieldTypeDefinition(string strId, string strName, IReadOnlyDictionary<string, string> dictParameters, IReadOnlyList<string> lstValidators)
        {
            Id = strId;
            Name = strName;
            Parameters = dictParameters;
            Validators = lstValidators;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> Validators { get; }
    }
}
