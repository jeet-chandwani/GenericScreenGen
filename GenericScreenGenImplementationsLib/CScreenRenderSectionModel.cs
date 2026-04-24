using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete UI-ready section render model.
    /// </summary>
    public sealed class CScreenRenderSectionModel : IScreenRenderSectionModel
    {
        public CScreenRenderSectionModel(string strName, string strLayoutPolicy, bool fIsCollapsible, bool fShowBorder, IReadOnlyList<IScreenRenderFieldModel> lstFields, IReadOnlyList<IScreenRenderSectionModel> lstSections)
        {
            Name = strName;
            LayoutPolicy = strLayoutPolicy;
            IsCollapsible = fIsCollapsible;
            ShowBorder = fShowBorder;
            Fields = lstFields;
            Sections = lstSections;
        }

        public string Name { get; }
        public string LayoutPolicy { get; }
        public bool IsCollapsible { get; }
        public bool ShowBorder { get; }
        public IReadOnlyList<IScreenRenderFieldModel> Fields { get; }
        public IReadOnlyList<IScreenRenderSectionModel> Sections { get; }
    }
}