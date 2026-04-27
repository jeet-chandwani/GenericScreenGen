using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete screen section definition.
    /// </summary>
    public sealed class CScreenSectionDefinition : IScreenSectionDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CScreenSectionDefinition"/> class.
        /// </summary>
        /// <param name="strName">Section name.</param>
        /// <param name="strLayoutPolicy">Layout policy.</param>
        /// <param name="fIsCollapsible">Indicates whether the section can collapse.</param>
        /// <param name="lstFields">Section fields.</param>
        /// <param name="lstSections">Nested sections.</param>
        public CScreenSectionDefinition(
            string strName,
            string strLayoutPolicy,
            bool fIsCollapsible,
            IReadOnlyList<IScreenFieldDefinition> lstFields,
            IReadOnlyList<IScreenSectionDefinition> lstSections,
            string strDetailScreen = "")
        {
            Name = strName;
            LayoutPolicy = strLayoutPolicy;
            IsCollapsible = fIsCollapsible;
            Fields = lstFields;
            Sections = lstSections;
            DetailScreen = strDetailScreen;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string LayoutPolicy { get; }

        /// <inheritdoc />
        public bool IsCollapsible { get; }

        /// <inheritdoc />
        public IReadOnlyList<IScreenFieldDefinition> Fields { get; }

        /// <inheritdoc />
        public IReadOnlyList<IScreenSectionDefinition> Sections { get; }

        /// <inheritdoc />
        public string DetailScreen { get; }
    }
}