using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete schema validation issue.
    /// </summary>
    public sealed class CScreenValidationIssue : IScreenValidationIssue
    {
        public CScreenValidationIssue(string strCode, string strPath, string strMessage)
        {
            Code = strCode;
            Path = strPath;
            Message = strMessage;
        }

        public string Code { get; }
        public string Path { get; }
        public string Message { get; }
    }
}