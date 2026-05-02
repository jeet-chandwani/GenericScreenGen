namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a UI-ready selection action model for tabular row interactions.
    /// </summary>
    public interface IScreenRenderSelectionActionModel
    {
        string Event { get; }
        string TargetScreen { get; }
        bool IncludeRecordId { get; }
    }
}