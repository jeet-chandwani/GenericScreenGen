using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a UI-ready screen render model.
    /// </summary>
    public interface IScreenRenderModel
    {
        string ScreenId { get; }
        string ScreenFileName { get; }
        string DisplayName { get; }
        string Theme { get; }
        IReadOnlyList<string> Features { get; }
        IReadOnlyList<IScreenRenderSectionModel> Sections { get; }
    }
}