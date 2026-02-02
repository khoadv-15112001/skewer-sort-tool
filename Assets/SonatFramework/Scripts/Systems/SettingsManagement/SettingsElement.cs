using System;
using Sirenix.OdinInspector;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.SettingsManagement.Vibation;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Systems.SettingsManagement
{
    public class SettingsElement : MonoBehaviour
    {
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle vibrateToggle;
        [SerializeField] private Toggle hintToggle;

        [SerializeField] private Service<AudioService> audioService = new();
        [SerializeField] private Service<VibrationService> vibrationService = new();

        public static bool HintIsOn { get => PlayerPrefs.GetInt("Hint_isOn", 1) == 1; set => PlayerPrefs.SetInt("Hint_isOn", value ? 1 : 0); }
        public static event Action<bool> OnHintChangedEvent;
        public void Start()
        {
            Setup();
        }

        private void Setup()
        {
            musicToggle.isOn = audioService.Instance.GetVolume(AudioTracks.Music) != 0;
            soundToggle.isOn = audioService.Instance.GetVolume(AudioTracks.Sound) != 0;
            vibrateToggle.isOn = vibrationService.Instance.GetVibrationState();
            if (hintToggle != null) hintToggle.isOn = HintIsOn;

            musicToggle.onValueChanged.AddListener(OnMusicChanged);
            soundToggle.onValueChanged.AddListener(isOn => audioService.Instance.SetVolume(AudioTracks.Sound, isOn ? 1 : 0));
            vibrateToggle.onValueChanged.AddListener(isOn => vibrationService.Instance.SetVibrationState(isOn));
            if (hintToggle != null) hintToggle.onValueChanged.AddListener(OnHintChanged);
        }

        private void OnMusicChanged(bool isOn)
        {
            audioService.Instance.SetVolume(AudioTracks.Music, isOn ? 1 : 0);
            if (!isOn)
            {
                audioService.Instance.StopMusic();
            }
            else
            {
                audioService.Instance.ResumeMusic();
            }
        }

        private void OnHintChanged(bool isOn)
        {
            HintIsOn = isOn;
            OnHintChangedEvent?.Invoke(isOn);
        }
    }
}