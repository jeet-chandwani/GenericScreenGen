using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete UI-ready field render model.
    /// </summary>
    public sealed class CScreenRenderFieldModel : IScreenRenderFieldModel
    {
        public CScreenRenderFieldModel(
            string strId,
            string strName,
            string strDescription,
            EFieldType enuType,
            string strTypeInfo,
            string strWidth,
            string strControlType,
            string strInputType,
            int iMinChars,
            int iMaxChars,
            int iLines,
            IReadOnlyList<string> lstLookupValues,
            bool fIsActionField)
        {
            Id = strId;
            Name = strName;
            Description = strDescription;
            Type = enuType;
            TypeInfo = strTypeInfo;
            Width = strWidth;
            ControlType = strControlType;
            InputType = strInputType;
            MinChars = iMinChars;
            MaxChars = iMaxChars;
            Lines = iLines;
            LookupValues = lstLookupValues;
            IsActionField = fIsActionField;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public EFieldType Type { get; }
        public string TypeInfo { get; }
        public string Width { get; }
        public string ControlType { get; }
        public string InputType { get; }
        public int MinChars { get; }
        public int MaxChars { get; }
        public int Lines { get; }
        public IReadOnlyList<string> LookupValues { get; }
        public bool IsActionField { get; }
    }
}