using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes the schema validation result for a screen configuration file.
    /// </summary>
    public interface IScreenValidationResult
    {
        /// <summary>
        /// Gets the validated screen file name.
        /// </summary>
        string ScreenFileName { get; }

        /// <summary>
        /// Gets a value indicating whether the screen file is schema-valid.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Gets the collected validation issues.
        /// </summary>
        IReadOnlyList<IScreenValidationIssue> Issues { get; }
    }
}