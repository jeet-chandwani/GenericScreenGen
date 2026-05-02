using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Concrete UI-ready selection action model.
    /// </summary>
    public sealed class CScreenRenderSelectionActionModel : IScreenRenderSelectionActionModel
    {
        public CScreenRenderSelectionActionModel(string strEvent, string strTargetScreen, bool fIncludeRecordId)
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