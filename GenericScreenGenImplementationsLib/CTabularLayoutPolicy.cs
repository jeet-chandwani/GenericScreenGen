using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Layout policy that renders fields in a tabular grid where each row represents a record.
    /// </summary>
    public sealed class CTabularLayoutPolicy : ILayoutPolicy
    {
        /// <inheritdoc />
        public string PolicyId => "tabular";

        /// <inheritdoc />
        public string DisplayName => "Tabular";
    }
}