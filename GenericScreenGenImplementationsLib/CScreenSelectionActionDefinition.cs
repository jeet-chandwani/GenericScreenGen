using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Concrete selection action definition for tabular row interactions.
    /// </summary>
    public sealed class CScreenSelectionActionDefinition : IScreenSelectionActionDefinition
    {
        public CScreenSelectionActionDefinition(string strEvent, string strTargetScreen, bool fIncludeRecordId)
        {
            Event = strEvent;
            TargetScreen = strTargetScreen;
            IncludeRecordId = fIncludeRecordId;
        }

        public string Event { get; }

        public string TargetScreen { get; }

        public bool IncludeRecordId { get; }
    }
}