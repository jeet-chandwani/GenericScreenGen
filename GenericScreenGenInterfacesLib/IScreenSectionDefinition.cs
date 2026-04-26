using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a screen section, including nested sections and fields.
    /// </summary>
    public interface IScreenSectionDefinition
    {
        /// <summary>
        /// Gets the section name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the layout policy for the section.
        /// </summary>
        string LayoutPolicy { get; }

        /// <summary>
        /// Gets a value indicating whether the section can be collapsed.
        /// </summary>
        bool IsCollapsible { get; }

        /// <summary>
        /// Gets the fields directly owned by this section.
        /// </summary>
        IReadOnlyList<IScreenFieldDefinition> Fields { get; }

        /// <summary>
        /// Gets the nested child sections.
        /// </summary>
        IReadOnlyList<IScreenSectionDefinition> Sections { get; }

        /// <summary>
        /// Gets the file name of a detail screen to navigate to when a row is clicked in tabular layout. Empty string means use inline edit.
        /// </summary>
        string DetailScreen { get; }
    }
}