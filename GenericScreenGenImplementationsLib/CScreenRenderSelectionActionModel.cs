using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Concrete UI-ready selection action model.
    /// </summary>
    public sealed class CScreenRenderSelectionActionModel : IScreenRenderSelectionActionModel
    {
        public CScreenRenderSelectionActionModel(string strEvent, string strTargetScreenId, bool fIncludeRecordId)
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