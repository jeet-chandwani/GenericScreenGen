using GenericScreenGenInterfacesLib;
using GenericScreenGenUtilsLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Layout policy that renders each field on its own line in a vertical stack.
    /// This is the default layout policy for screen sections.
    /// </summary>
    public sealed class CPerLineLayoutPolicy : ILayoutPolicy
    {
        /// <inheritdoc />
        public string PolicyId => CScreenGeneratorConstants.DEFAULT_LAYOUT_POLICY;

        /// <inheritdoc />
        public string DisplayName => "Per Line";
    }
}
