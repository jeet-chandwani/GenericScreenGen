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
            IReadOnlyList<string> lstLookupOptionDescriptions,
            IReadOnlyList<string> lstLookupOptionImages,
            bool fIsMandatory,
            bool fIsMultiple,
            bool fIsActionField,
            bool fIsSearchable = false)
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
            LookupOptionDescriptions = lstLookupOptionDescriptions;
            LookupOptionImages = lstLookupOptionImages;
            IsMandatory = fIsMandatory;
            IsMultiple = fIsMultiple;
            IsActionField = fIsActionField;
            IsSearchable = fIsSearchable;
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
        public IReadOnlyList<string> LookupOptionDescriptions { get; }
        public IReadOnlyList<string> LookupOptionImages { get; }
        public bool IsMandatory { get; }
        public bool IsMultiple { get; }
        public bool IsActionField { get; }
        public bool IsSearchable { get; }
    }
}
