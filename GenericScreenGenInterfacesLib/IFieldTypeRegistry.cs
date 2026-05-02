using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Provides look-up access to field type definitions loaded from registry configuration.
    /// </summary>
    public interface IFieldTypeRegistry : ICanInit
    {
        /// <summary>
        /// Tries to get a field type definition by field type identifier.
        /// </summary>
        bool TryGetFieldTypeDefinition(string strFieldTypeId, out IFieldTypeDefinition? itfFieldTypeDefinition);

        /// <summary>
        /// Gets all field type definitions currently loaded in the registry.
        /// </summary>
        IReadOnlyCollection<IFieldTypeDefinition> GetAllFieldTypes();

        /// <summary>
        /// Gets a value indicating whether a field type id exists in the registry.
        /// </summary>
        bool IsValidFieldTypeId(string strFieldTypeId);
    }
}
