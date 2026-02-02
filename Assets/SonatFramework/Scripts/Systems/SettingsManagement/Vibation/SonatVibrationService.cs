using SonatFramework.Systems.GameDataManagement;
using UnityEngine;

namespace SonatFramework.Systems.SettingsManagement.Vibation
{
    [CreateAssetMenu(fileName = nameof(SonatVibrationService), menuName = "Sonat Services/Vibration Service")]
    public class SonatVibrationService : VibrationService
    {
        private const string VibrationKey = "SonatVibrationKey";
        [SerializeField] private Service<DataService> dataService;


        public override void SetVibrationState(bool state)
        {
            dataService.Instance.SetBool(VibrationKey, state);
        }

        public override bool GetVibrationState()
        {
            return dataService.Instance.GetBool(VibrationKey, true);
        }

        public override void Vibrate(long milliseconds)
        {
        }
    }
}