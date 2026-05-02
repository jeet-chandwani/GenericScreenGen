using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a dynamic screen loaded from a screen configuration file.
    /// </summary>
    public interface IScreenDefinition
    {
        /// <summary>
        /// Gets the normalized screen identifier.
        /// </summary>
        string ScreenId { get; }

        /// <summary>
        /// Gets the backing configuration file name.
        /// </summary>
        string ScreenFileName { get; }

        /// <summary>
        /// Gets the display name derived from the screen file name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the top-level sections for this screen.
        /// </summary>
        IReadOnlyList<IScreenSectionDefinition> Sections { get; }

        /// <summary>
        /// Gets the list of field identifiers that together define the screen record key.
        /// </summary>
        IReadOnlyList<string> Key { get; }

        /// <summary>
        /// Gets the enabled screen-level features.
        /// </summary>
        IReadOnlyList<string> Features { get; }

        /// <summary>
        /// Gets the optional UI theme identifier for this screen.
        /// </summary>
        string Theme { get; }
    }
}