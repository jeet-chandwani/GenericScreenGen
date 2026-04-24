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
        string ControlType { get; }
        string InputType { get; }
        int MinChars { get; }
        int MaxChars { get; }
        int Lines { get; }
        IReadOnlyList<string> LookupValues { get; }
        bool IsActionField { get; }
    }
}