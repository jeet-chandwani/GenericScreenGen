using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Provides access to available screen configuration files and parsed screen definitions.
    /// </summary>
    public interface IScreenConfigProvider : ICanInit
    {
        /// <summary>
        /// Returns the available screen configuration file names.
        /// </summary>
        /// <param name="lstScreenFileNames">Resolved screen configuration file names.</param>
        /// <param name="strError">Detailed error information when retrieval fails.</param>
        /// <returns>True when screen file names are available; otherwise false.</returns>
        bool TryGetAvailableScreens(out IReadOnlyList<string> lstScreenFileNames, out string strError);

        /// <summary>
        /// Returns the parsed screen definition for a specific screen file.
        /// </summary>
        /// <param name="strScreenFileName">Screen configuration file name.</param>
        /// <param name="itfScreenDefinition">Parsed screen definition.</param>
        /// <param name="strError">Detailed error information when retrieval fails.</param>
        /// <returns>True when the screen definition is found; otherwise false.</returns>
        bool TryGetScreenDefinition(string strScreenFileName, out IScreenDefinition? itfScreenDefinition, out string strError);
    }
}