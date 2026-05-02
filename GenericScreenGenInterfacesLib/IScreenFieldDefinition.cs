namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a single field inside a generated screen section.
    /// </summary>
    public interface IScreenFieldDefinition
    {
        /// <summary>
        /// Gets the unique field identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the field display name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the context-sensitive field description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        EFieldType Type { get; }

        /// <summary>
        /// Gets type-specific configuration data for the field.
        /// </summary>
        string TypeInfo { get; }

        /// <summary>
        /// Gets the CSS width value for the field.
        /// </summary>
        string Width { get; }

        /// <summary>
        /// Gets the optional CSS max-width value for the field.
        /// </summary>
        string MaxWidth { get; }

        /// <summary>
        /// Gets a value indicating whether the field is mandatory, regardless of field type.
        /// </summary>
        bool IsMandatory { get; }

        /// <summary>
        /// Gets a value indicating whether the lookup field supports a search/filter input.
        /// Defaults to <see langword="false"/> when not specified in the screen config JSON.
        /// </summary>
        bool IsSearchable { get; }
    }
}