namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Represents a named layout policy that governs how fields within a screen section are arranged.
    /// New policies can be added by implementing this interface and registering the implementation
    /// in the <see cref="ILayoutPolicyRegistry"/>.
    /// </summary>
    public interface ILayoutPolicy
    {
        /// <summary>
        /// Unique identifier for this layout policy in kebab-case format (e.g. "per-line").
        /// This value is matched against the <c>layout-policy</c> property in screen config files.
        /// </summary>
        string PolicyId { get; }

        /// <summary>
        /// Human-readable name for this layout policy (e.g. "Per Line").
        /// </summary>
        string DisplayName { get; }
    }
}
