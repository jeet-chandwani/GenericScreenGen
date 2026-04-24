using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete UI-ready screen render model.
    /// </summary>
    public sealed class CScreenRenderModel : IScreenRenderModel
    {
        public CScreenRenderModel(string strScreenId, string strScreenFileName, string strDisplayName, IReadOnlyList<IScreenRenderSectionModel> lstSections)
        {
            ScreenId = strScreenId;
            ScreenFileName = strScreenFileName;
            DisplayName = strDisplayName;
            Sections = lstSections;
        }

        public string ScreenId { get; }
        public string ScreenFileName { get; }
        public string DisplayName { get; }
        public IReadOnlyList<IScreenRenderSectionModel> Sections { get; }
    }
}