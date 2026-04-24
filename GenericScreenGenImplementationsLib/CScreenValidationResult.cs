using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete screen schema validation result.
    /// </summary>
    public sealed class CScreenValidationResult : IScreenValidationResult
    {
        public CScreenValidationResult(string strScreenFileName, bool fIsValid, IReadOnlyList<IScreenValidationIssue> lstIssues)
        {
            ScreenFileName = strScreenFileName;
            IsValid = fIsValid;
            Issues = lstIssues;
        }

        public string ScreenFileName { get; }
        public bool IsValid { get; }
        public IReadOnlyList<IScreenValidationIssue> Issues { get; }
    }
}