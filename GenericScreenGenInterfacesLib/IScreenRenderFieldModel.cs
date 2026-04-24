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
        string Width { get; }
        string InputType { get; }
        bool IsActionField { get; }
    }
}