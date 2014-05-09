using Newtonsoft.Json.Linq;

namespace Probe.AutoUpdate
{
    /// <summary>
    /// Provides methods for auto update application and verify current version.
    /// </summary>
    public static class AutoUpdater
    {
        /// <summary>
        /// Verifies this instance.
        /// </summary>
        /// <returns>Returns true if current version is valid otherwise returns false.</returns>
        public static bool Verify()
        {
            return true;
        }

        /// <summary>
        /// Starts the update from URL.
        /// </summary>
        public static void CheckUpdates()
        {
        }
    }
}
