using System;
using System.IO;

namespace GenericScreenGenUtilsLib
{
    /// <summary>
    /// Provides naming conversion helpers for screen files.
    /// </summary>
    public static class CScreenNameUtility
    {
        /// <summary>
        /// Gets the default screen name from a screen configuration file name.
        /// </summary>
        /// <param name="strScreenFileName">Screen configuration file name.</param>
        /// <returns>Screen name derived from file name without prefix and extension.</returns>
        public static string GetScreenNameFromFileName(string strScreenFileName)
        {
            string strBaseName = Path.GetFileNameWithoutExtension(strScreenFileName);

            if (strBaseName.StartsWith(CScreenGeneratorConstants.SCREEN_FILE_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                strBaseName = strBaseName.Substring(CScreenGeneratorConstants.SCREEN_FILE_PREFIX.Length);
            }

            return strBaseName.Trim();
        }

        /// <summary>
        /// Converts a screen configuration file name into a human-readable display name.
        /// </summary>
        /// <param name="strScreenFileName">Screen configuration file name.</param>
        /// <returns>Display name derived from the file name.</returns>
        public static string GetDisplayNameFromFileName(string strScreenFileName)
        {
            return GetScreenNameFromFileName(strScreenFileName).Replace('-', ' ').Trim();
        }

        /// <summary>
        /// Converts a screen configuration file name into a normalized screen identifier.
        /// </summary>
        /// <param name="strScreenFileName">Screen configuration file name.</param>
        /// <returns>Normalized screen identifier.</returns>
        public static string GetScreenIdFromFileName(string strScreenFileName)
        {
            string strDisplayName = GetDisplayNameFromFileName(strScreenFileName);
            return strDisplayName.Replace(' ', '-').ToLowerInvariant();
        }
    }
}