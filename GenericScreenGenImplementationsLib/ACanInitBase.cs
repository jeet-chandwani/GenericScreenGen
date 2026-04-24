using GenericScreenGenInterfacesLib;

namespace GenericScreenGenImplementationsLib
{
    /// <summary>
    /// Provides a reusable template method implementation for component initialization.
    /// </summary>
    public abstract class ACanInitBase : ICanInit
    {
        private bool m_fIsInitialized;

        /// <summary>
        /// Initializes the current component.
        /// </summary>
        /// <param name="objInputParam">Initialization input parameter.</param>
        /// <param name="strError">Detailed error information when initialization fails.</param>
        /// <returns>True when initialization succeeds; otherwise false.</returns>
        public bool Init(object objInputParam, out string strError)
        {
            if (m_fIsInitialized)
            {
                strError = string.Empty;
                return true;
            }

            if (!TryInitCore(objInputParam, out strError))
            {
                return false;
            }

            if (!InitAfterLoad(out strError))
            {
                return false;
            }

            m_fIsInitialized = true;
            strError = string.Empty;
            return true;
        }

        /// <summary>
        /// Performs the component-specific initialization work.
        /// </summary>
        /// <param name="objInputParam">Initialization input parameter.</param>
        /// <param name="strError">Detailed error information when initialization fails.</param>
        /// <returns>True when initialization succeeds; otherwise false.</returns>
        protected abstract bool TryInitCore(object objInputParam, out string strError);

        /// <summary>
        /// Executes optional post-load initialization logic.
        /// </summary>
        /// <param name="strError">Detailed error information when initialization fails.</param>
        /// <returns>True when post-load initialization succeeds; otherwise false.</returns>
        protected virtual bool InitAfterLoad(out string strError)
        {
            strError = string.Empty;
            return true;
        }
    }
}