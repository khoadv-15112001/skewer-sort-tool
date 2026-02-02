using System;
using Sonat.Enums;
using UnityEngine.EventSystems;

namespace SonatFramework.Systems.AudioManagement
{
    public class ButtonSoundPlayer : AudioPlayer, IPointerClickHandler
    {
        // private void OnValidate()
        // {
        //     if (audioId == AudioId.None) audioId = AudioId.ButtonClick;
        // }

        private void Awake()
        {
            //if (audioId == AudioId.None)
            //{
                audioId = AudioId.ButtonClick;
            //}
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}