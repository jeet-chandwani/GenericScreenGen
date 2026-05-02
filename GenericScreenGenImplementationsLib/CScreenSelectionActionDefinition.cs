using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Concrete selection action definition for tabular row interactions.
    /// </summary>
    public sealed class CScreenSelectionActionDefinition : IScreenSelectionActionDefinition
    {
        public CScreenSelectionActionDefinition(string strEvent, string strTargetScreenId, bool fIncludeRecordId)
        {
            Event = strEvent;
            TargetScreenId = strTargetScreenId;
            IncludeRecordId = fIncludeRecordId;
        }

        public string Event { get; }

        public string TargetScreenId { get; }

        public bool IncludeRecordId { get; }
    }
}