using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a field type definition loaded from the field type registry.
    /// </summary>
    public interface IFieldTypeDefinition
    {
        /// <summary>
        /// Gets the unique field type identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the display name of the field type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets default parameter values keyed by parameter name.
        /// </summary>
        IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>
        /// Gets validator identifiers configured for the field type.
        /// </summary>
        IReadOnlyList<string> Validators { get; }
    }
}
