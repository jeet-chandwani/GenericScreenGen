using System.Collections.Generic;

namespace GenericScreenGenInterfacesLib
{
    /// <summary>
    /// Abstraction for persisted row-data storage used by generated screens.
    /// </summary>
    public interface IDataStore
    {
        /// <summary>
        /// Loads all rows for a screen key.
        /// </summary>
        /// <param name="strScreenKey">Logical screen key used to locate persisted data.</param>
        /// <param name="lstRows">Loaded rows represented as string key/value dictionaries.</param>
        /// <param name="strError">Detailed error information when load fails.</param>
        /// <returns>True when rows are loaded (or no file exists yet); otherwise false.</returns>
        bool TryLoadRows(string strScreenKey, out IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, out string strError);

        /// <summary>
        /// Persists all rows for a screen key (replaces existing content).
        /// </summary>
        /// <param name="strScreenKey">Logical screen key used to locate persisted data.</param>
        /// <param name="lstRows">Rows to persist.</param>
        /// <param name="strError">Detailed error information when save fails.</param>
        /// <returns>True when rows are persisted; otherwise false.</returns>
        bool TrySaveRows(string strScreenKey, IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, out string strError);

        /// <summary>
        /// Deletes persisted data for a screen key.
        /// </summary>
        /// <param name="strScreenKey">Logical screen key used to locate persisted data.</param>
        /// <param name="strError">Detailed error information when delete fails.</param>
        /// <returns>True when delete succeeds or file does not exist; otherwise false.</returns>
        bool TryDeleteRows(string strScreenKey, out string strError);
    }
}
