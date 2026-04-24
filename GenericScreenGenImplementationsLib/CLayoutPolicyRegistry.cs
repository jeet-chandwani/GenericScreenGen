using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Collects all registered <see cref="ILayoutPolicy"/> implementations and provides
    /// look-up by policy identifier.  New policies are made available by registering their
    /// <see cref="ILayoutPolicy"/> implementation with the dependency-injection container;
    /// no changes to this class are required.
    /// </summary>
    public sealed class CLayoutPolicyRegistry : ILayoutPolicyRegistry
    {
        private readonly Dictionary<string, ILayoutPolicy> m_dictPolicies;

        /// <summary>
        /// Initialises the registry with all <see cref="ILayoutPolicy"/> implementations
        /// collected from the dependency-injection container.
        /// </summary>
        /// <param name="lstPolicies">All registered layout policy implementations.</param>
        public CLayoutPolicyRegistry(IEnumerable<ILayoutPolicy> lstPolicies)
        {
            m_dictPolicies = new Dictionary<string, ILayoutPolicy>(StringComparer.OrdinalIgnoreCase);

            foreach (ILayoutPolicy itfPolicy in lstPolicies)
            {
                m_dictPolicies[itfPolicy.PolicyId] = itfPolicy;
            }
        }

        /// <inheritdoc />
        public bool TryGetPolicy(string strPolicyId, out ILayoutPolicy? itfPolicy)
        {
            return m_dictPolicies.TryGetValue(strPolicyId, out itfPolicy);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ILayoutPolicy> GetAllPolicies()
        {
            return m_dictPolicies.Values.ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public bool IsValidPolicyId(string strPolicyId)
        {
            return !string.IsNullOrWhiteSpace(strPolicyId) && m_dictPolicies.ContainsKey(strPolicyId);
        }
    }
}
