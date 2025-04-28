using UnityEngine;

namespace PDXUnderground.Core.Interfaces
{
    /// <summary>
    /// Interface for environment setup and management
    /// </summary>
    public interface IEnvironmentSetup
    {
        /// <summary>
        /// Gets the nearest spawn point to a given position
        /// </summary>
        /// <param name="position">Position to find spawn point near</param>
        /// <returns>The nearest spawn point position</returns>
        Vector3 GetNearestSpawnPoint(Vector3 position);
    }
}

