using UnityEngine;
using System;

namespace PDXUnderground.Core.Interfaces
{
    public interface IEnvironmentController
    {
        void TransitionToArea(string areaName);
        EnvironmentType GetCurrentEnvironmentType();
        void SetEnvironmentType(EnvironmentType type);
        void InitializeEnvironment();
        event Action<string> OnAreaTransition;
        event Action<EnvironmentType> OnEnvironmentTypeChanged;
    }

    public enum EnvironmentType
    {
        Streets,
        Tunnels,
        Speakeasy
    }
}

