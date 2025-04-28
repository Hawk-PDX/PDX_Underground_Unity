using UnityEngine;
using System;
using PDXUnderground.Models;

namespace PDXUnderground.Core.Interfaces
{
    /// <summary>
    /// Interface for the environment system that manages different areas and their states
    /// in PDX Underground (streets, tunnels, speakeasy, etc.).
    /// </summary>
    public interface IEnvironmentSystem
    {
        #region Properties
        /// <summary>
        /// Current environment type
        /// </summary>
        EnvironmentType CurrentEnvironmentType { get; }

        /// <summary>
        /// Current time of day
        /// </summary>
        TimeOfDay CurrentTimeOfDay { get; }

        /// <summary>
        /// Current weather intensity (0-1 range)
        /// </summary>
        float WeatherIntensity { get; }
        #endregion

        #region Events
        /// <summary>
        /// Event triggered when environment type changes
        /// </summary>
        event Action<EnvironmentType> OnEnvironmentChanged;

        /// <summary>
        /// Event triggered when environment type changes with additional details
        /// </summary>
        event Action<string, int> OnEnvironmentTypeChanged;

        /// <summary>
        /// Event triggered when time of day changes
        /// </summary>
        event Action<TimeOfDay> OnTimeOfDayChanged;

        /// <summary>
        /// Event triggered when weather intensity changes
        /// </summary>
        event Action<float> OnWeatherChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Change the current environment
        /// </summary>
        void SetEnvironment(EnvironmentType type);

        /// <summary>
        /// Change the current environment by index
        /// </summary>
        void ChangeEnvironment(int environmentIndex);

        /// <summary>
        /// Set the current time of day
        /// </summary>
        void SetTimeOfDay(TimeOfDay time);

        /// <summary>
        /// Set the current weather intensity
        /// </summary>
        void SetWeatherIntensity(float intensity);

        /// <summary>
        /// Get name of current environment
        /// </summary>
        string GetCurrentEnvironmentName();

        /// <summary>
        /// Get description of environment type
        /// </summary>
        string GetEnvironmentDescription(int environmentType);

        /// <summary>
        /// Get icon for environment type
        /// </summary>
        Sprite GetEnvironmentIcon(int environmentType);

        /// <summary>
        /// Get current time of day string representation
        /// </summary>
        string GetTimeOfDay();

        /// <summary>
        /// Get current weather string representation
        /// </summary>
        string GetWeatherString();
        #endregion
    }
}
