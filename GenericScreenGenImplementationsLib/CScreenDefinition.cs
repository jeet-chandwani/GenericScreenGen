using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Represents a concrete parsed screen definition.
    /// </summary>
    public sealed class CScreenDefinition : IScreenDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CScreenDefinition"/> class.
        /// </summary>
        /// <param name="strScreenId">Normalized screen identifier.</param>
        /// <param name="strScreenFileName">Backing screen file name.</param>
        /// <param name="strDisplayName">Display name.</param>
        /// <param name="lstSections">Screen sections.</param>
        /// <param name="lstFeatures">Enabled screen-level features.</param>
        public CScreenDefinition(
            string strScreenId,
            string strScreenFileName,
            string strDisplayName,
            IReadOnlyList<IScreenSectionDefinition> lstSections,
            IReadOnlyList<string> lstFeatures)
        {
            ScreenId = strScreenId;
            ScreenFileName = strScreenFileName;
            DisplayName = strDisplayName;
            Sections = lstSections;
            Features = lstFeatures;
        }

        /// <inheritdoc />
        public string ScreenId { get; }

        /// <inheritdoc />
        public string ScreenFileName { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public IReadOnlyList<IScreenSectionDefinition> Sections { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> Features { get; }
    }
}