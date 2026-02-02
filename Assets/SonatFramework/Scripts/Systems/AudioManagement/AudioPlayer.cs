using System;
using Sirenix.OdinInspector;
using Sonat.Enums;
using UnityEngine;

namespace SonatFramework.Systems.AudioManagement
{
    public class AudioPlayer : MonoBehaviour
    {
        [EnumToggleButtons] [SerializeField] private AudioTracks audioTracks = AudioTracks.Sound;

        [SerializeField] protected AudioId audioId;

        [Range(0f, 1f)] [SerializeField] private float volume = 1;

        [SerializeField] private Service<AudioService> audioService = new();


        public virtual void PlayAudio()
        {
            switch (audioTracks)
            {
                case AudioTracks.Sound:
                    audioService.Instance.PlaySound(audioId, volume);
                    break;
                case AudioTracks.Music:
                    audioService.Instance.PlayMusic(audioId, true, volume);
                    break;
            }
        }
    }
}