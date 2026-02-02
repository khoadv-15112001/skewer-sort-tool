using System;

namespace SonatFramework.Systems.SettingsManagement.Vibation
{
    public abstract class VibrationService : SonatServiceSo
    {
        public Action onVibrateUpdate;
        public abstract void SetVibrationState(bool state);
        public abstract bool GetVibrationState();

        public abstract void Vibrate(long milliseconds);
    }
}