namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Maintains the set of registered <see cref="ILayoutPolicy"/> implementations and provides
    /// look-up by policy identifier.  Register new layout policies by adding implementations of
    /// <see cref="ILayoutPolicy"/> to the dependency-injection container; the registry will
    /// collect them automatically.
    /// </summary>
    public interface ILayoutPolicyRegistry
    {
        /// <summary>
        /// Attempts to retrieve the layout policy with the specified identifier.
        /// </summary>
        /// <param name="strPolicyId">Kebab-case policy identifier (e.g. "per-line").</param>
        /// <param name="itfPolicy">The matching policy when found; otherwise <c>null</c>.</param>
        /// <returns>True when the policy is registered; otherwise false.</returns>
        bool TryGetPolicy(string strPolicyId, out ILayoutPolicy? itfPolicy);

        /// <summary>
        /// Returns all registered layout policies.
        /// </summary>
        IReadOnlyCollection<ILayoutPolicy> GetAllPolicies();

        /// <summary>
        /// Indicates whether the specified policy identifier is registered.
        /// </summary>
        /// <param name="strPolicyId">Kebab-case policy identifier.</param>
        /// <returns>True when the policy is registered; otherwise false.</returns>
        bool IsValidPolicyId(string strPolicyId);
    }
}
