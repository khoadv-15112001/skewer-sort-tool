using SonatFramework.Scripts.Utils;
using UnityEngine;
using Sonat.Enums;
using System;
namespace MyGame.Modules.CardCollection.Sound
{
    public class PlaySounds : MonoBehaviour
    {
        [SerializeField] private AudioList[] audioList;

        void OnEnable()
        {
            if(audioList == null || audioList.Length == 0) return;

            foreach (var sound in audioList)
            {
                SonatUtils.DelayCall(sound.delay, () =>
                {
                    MySonatFramework.audioService.PlaySound(sound.audioId);
                }, this);
            }

        }
    }

    [Serializable]
    public class AudioList
    {
        public AudioId audioId;
        public float delay;
    }
}