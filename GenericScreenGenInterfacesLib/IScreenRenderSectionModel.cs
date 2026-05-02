using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a UI-ready section model for dynamic screen rendering.
    /// </summary>
    public interface IScreenRenderSectionModel
    {
        string Name { get; }
        string LayoutPolicy { get; }
        bool IsCollapsible { get; }
        bool ShowBorder { get; }
        IReadOnlyList<IScreenRenderFieldModel> Fields { get; }
        IReadOnlyList<IScreenRenderSectionModel> Sections { get; }
        IReadOnlyList<IScreenRenderSelectionActionModel> SelectionActions { get; }
    }
}