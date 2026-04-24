namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a single schema validation issue for a screen configuration file.
    /// </summary>
    public interface IScreenValidationIssue
    {
        /// <summary>
        /// Gets the validation issue code.
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Gets the JSON path where the issue was found.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the validation issue message.
        /// </summary>
        string Message { get; }
    }
}