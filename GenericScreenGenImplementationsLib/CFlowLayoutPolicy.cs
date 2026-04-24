using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Layout policy that places fields left-to-right and wraps to the next line when needed.
    /// </summary>
    public sealed class CFlowLayoutPolicy : ILayoutPolicy
    {
        /// <inheritdoc />
        public string PolicyId => "flow";

        /// <inheritdoc />
        public string DisplayName => "Flow";
    }
}