using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete UI-ready screen render model.
    /// </summary>
    public sealed class CScreenRenderModel : IScreenRenderModel
    {
        public CScreenRenderModel(string strScreenId, string strScreenFileName, string strDisplayName, IReadOnlyList<string> lstFeatures, IReadOnlyList<IScreenRenderSectionModel> lstSections)
        {
            ScreenId = strScreenId;
            ScreenFileName = strScreenFileName;
            DisplayName = strDisplayName;
            Features = lstFeatures;
            Sections = lstSections;
        }

        public string ScreenId { get; }
        public string ScreenFileName { get; }
        public string DisplayName { get; }
        public IReadOnlyList<string> Features { get; }
        public IReadOnlyList<IScreenRenderSectionModel> Sections { get; }
    }
}