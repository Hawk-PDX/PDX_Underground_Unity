using System;
using UnityEngine;

namespace PDXUnderground.Core.Interfaces
{
    /// <summary>
    /// Interface for managing game environments and transitions
    /// </summary>
    public interface IEnvironmentSystem
    {
        /// <summary>
        /// Event triggered when the environment changes
        /// </summary>
        /// <param name="environmentType">Type ID of the new environment</param>
        /// <param name="environmentName">Name of the new environment</param>
        event Action<int, string> OnEnvironmentChanged;

        /// <summary>
        /// Current environment type name
        /// </summary>
        string CurrentEnvironmentType { get; }

        /// <summary>
        /// Current environment ID
        /// </summary>
        int CurrentEnvironmentId { get; }

        /// <summary>
        /// Whether a transition is currently in progress
        /// </summary>
        bool IsTransitioning { get; }

        /// <summary>
        /// Transition to a new environment
        /// </summary>
        /// <param name="environmentId">ID of the target environment</param>
        /// <returns>True if transition started successfully</returns>
        bool TransitionTo(int environmentId);

        /// <summary>
        /// Get the name of an environment by ID
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>Environment name</returns>
        string GetEnvironmentName(int environmentId);
    }
}

