using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete screen field definition.
    /// </summary>
    public sealed class CScreenFieldDefinition : IScreenFieldDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CScreenFieldDefinition"/> class.
        /// </summary>
        /// <param name="strId">Field identifier.</param>
        /// <param name="strName">Field name.</param>
        /// <param name="strDescription">Field description.</param>
        /// <param name="enuType">Field type.</param>
        /// <param name="strTypeInfo">Type-specific configuration string.</param>
        /// <param name="strWidth">Field width.</param>
        /// <param name="strMaxWidth">Optional max-width for field rendering.</param>
        public CScreenFieldDefinition(string strId, string strName, string strDescription, EFieldType enuType, string strTypeInfo, string strWidth, string strMaxWidth = "", bool fIsMandatory = false, bool fIsSearchable = false)
        {
            Id = strId;
            Name = strName;
            Description = strDescription;
            Type = enuType;
            TypeInfo = strTypeInfo;
            Width = strWidth;
            MaxWidth = strMaxWidth;
            IsMandatory = fIsMandatory;
            IsSearchable = fIsSearchable;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public EFieldType Type { get; }

        /// <inheritdoc />
        public string TypeInfo { get; }

        /// <inheritdoc />
        public string Width { get; }

        /// <inheritdoc />
        public string MaxWidth { get; }

        /// <inheritdoc />
        public bool IsMandatory { get; }

        /// <inheritdoc />
        public bool IsSearchable { get; }
    }
}
