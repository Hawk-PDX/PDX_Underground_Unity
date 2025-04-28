using UnityEngine;
using PDXUnderground.Core;

namespace PDXUnderground.Core.Utils
{
    public static class TimeUtils
    {
        public static float TimeOfDayToFloat(TimeOfDay time)
        {
            switch (time)
            {
                case TimeOfDay.Midnight: return 0.0f;
                case TimeOfDay.Dawn: return 0.25f;
                case TimeOfDay.Morning: return 0.35f;
                case TimeOfDay.Day: return 0.5f;
                case TimeOfDay.Noon: return 0.5f;
                case TimeOfDay.Afternoon: return 0.65f;
                case TimeOfDay.Dusk: return 0.75f;
                case TimeOfDay.Night: return 0.9f;
                default: return 0.5f;
            }
        }

        public static TimeOfDay FloatToTimeOfDay(float timeValue)
        {
            if (timeValue < 0.15f) return TimeOfDay.Midnight;
            if (timeValue < 0.25f) return TimeOfDay.Dawn;
            if (timeValue < 0.35f) return TimeOfDay.Morning;
            if (timeValue < 0.45f) return TimeOfDay.Day;
            if (timeValue < 0.55f) return TimeOfDay.Noon;
            if (timeValue < 0.65f) return TimeOfDay.Afternoon;
            if (timeValue < 0.75f) return TimeOfDay.Dusk;
            if (timeValue < 0.85f) return TimeOfDay.Night;
            return TimeOfDay.Midnight;
        }
    }
}

