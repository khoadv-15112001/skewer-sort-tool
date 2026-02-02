using SonatFramework.Systems;
using SonatFramework.Systems.SettingsManagement.Vibation;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(MyVibrationService), menuName = "My Services/Vibration Service")]
public class MyVibrationService : SonatVibrationService, IServiceInitialize
{
    private bool vibrationStatte;
    public void Initialize()
    {
        vibrationStatte = base.GetVibrationState();
#if UNITY_ANDROID || UNITY_IOS
        Vibration.Init();
#endif
    }

    public override bool GetVibrationState()
    {
        return vibrationStatte;
    }

    public override void SetVibrationState(bool state)
    {
        base.SetVibrationState(state);
        vibrationStatte = state;
    }

    public override void Vibrate(long milliseconds)
    {
        if(!vibrationStatte) return;
#if UNITY_ANDROID
        Vibration.VibrateAndroid(milliseconds);
#elif UNITY_IOS
        if (milliseconds < 75)
        {
            Vibration.VibrateIOS(ImpactFeedbackStyle.Medium);
        }
        else
        {
            Vibration.VibrateIOS(ImpactFeedbackStyle.Heavy);
        }
#endif
    }
}