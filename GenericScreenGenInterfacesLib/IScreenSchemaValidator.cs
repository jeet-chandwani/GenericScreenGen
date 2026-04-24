namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Validates screen configuration files against the solution JSON schema.
    /// </summary>
    public interface IScreenSchemaValidator : ICanInit
    {
        /// <summary>
        /// Validates a screen configuration file.
        /// </summary>
        /// <param name="strScreenFilePath">Screen configuration file path.</param>
        /// <param name="itfValidationResult">Schema validation result.</param>
        /// <param name="strError">Detailed error information when validation fails unexpectedly.</param>
        /// <returns>True when validation completes; otherwise false.</returns>
        bool TryValidateScreen(string strScreenFilePath, out IScreenValidationResult? itfValidationResult, out string strError);
    }
}