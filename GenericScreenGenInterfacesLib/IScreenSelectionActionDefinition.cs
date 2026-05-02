namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a section-level selection action mapping for tabular row interactions.
    /// </summary>
    public interface IScreenSelectionActionDefinition
    {
        /// <summary>
        /// Gets the triggering selection event name (for example: click, double-click).
        /// </summary>
        string Event { get; }

        /// <summary>
        /// Gets the target child screen file name to navigate for this action.
        /// </summary>
        string TargetScreen { get; }

        /// <summary>
        /// Gets a value indicating whether record-id must be propagated for this action.
        /// </summary>
        bool IncludeRecordId { get; }
    }
}