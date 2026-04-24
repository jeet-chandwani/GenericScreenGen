using System;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Defines a standard initialization contract for framework components.
    /// </summary>
    public interface ICanInit
    {
        /// <summary>
        /// Initializes the component with the provided input parameter.
        /// </summary>
        /// <param name="objInputParam">Initialization input parameter.</param>
        /// <param name="strError">Detailed error information when initialization fails.</param>
        /// <returns>True when initialization succeeds; otherwise false.</returns>
        bool Init(object objInputParam, out string strError);
    }
}