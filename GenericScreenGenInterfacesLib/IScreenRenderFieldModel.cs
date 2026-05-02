namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Describes a UI-ready field model for dynamic screen rendering.
    /// </summary>
    public interface IScreenRenderFieldModel
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        EFieldType Type { get; }
        string TypeInfo { get; }
        string Width { get; }
        string MaxWidth { get; }
        string ControlType { get; }
        string InputType { get; }
        int MinChars { get; }
        int MaxChars { get; }
        int Lines { get; }
        IReadOnlyList<string> LookupValues { get; }
        /// <summary>Per-option description lines matching the same index as <see cref="LookupValues"/>. Empty string when not provided.</summary>
        IReadOnlyList<string> LookupOptionDescriptions { get; }
        /// <summary>Per-option image URLs matching the same index as <see cref="LookupValues"/>. Empty string when not provided.</summary>
        IReadOnlyList<string> LookupOptionImages { get; }
        bool IsMandatory { get; }
        bool IsMultiple { get; }
        bool IsActionField { get; }
        /// <summary>When true the lookup field control shows a search/filter input.</summary>
        bool IsSearchable { get; }
    }
}