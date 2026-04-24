using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete UI-ready field render model.
    /// </summary>
    public sealed class CScreenRenderFieldModel : IScreenRenderFieldModel
    {
        public CScreenRenderFieldModel(string strId, string strName, string strDescription, EFieldType enuType, string strWidth, string strInputType, bool fIsActionField)
        {
            Id = strId;
            Name = strName;
            Description = strDescription;
            Type = enuType;
            Width = strWidth;
            InputType = strInputType;
            IsActionField = fIsActionField;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public EFieldType Type { get; }
        public string Width { get; }
        public string InputType { get; }
        public bool IsActionField { get; }
    }
}