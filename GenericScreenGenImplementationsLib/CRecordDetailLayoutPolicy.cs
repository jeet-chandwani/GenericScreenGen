using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Layout policy that renders a single record for detailed viewing and editing.
    /// Fields are displayed one-per-line (similar to per-line policy) with save/cancel actions
    /// and an optional side-by-side original-values comparison.
    /// </summary>
    public sealed class CRecordDetailLayoutPolicy : ILayoutPolicy
    {
        /// <inheritdoc />
        public string PolicyId => "record-detail";

        /// <inheritdoc />
        public string DisplayName => "Record Detail";
    }
}
