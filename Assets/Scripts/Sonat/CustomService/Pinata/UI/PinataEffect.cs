using MyGame.Modules.CardCollection.Animation;
using SonatFramework.Scripts.UIModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Pinata
{
    public class PinataEffect : MonoBehaviour
    {
        [SerializeField] private Transform beforeParent;
        [SerializeField] private Transform afterParent;
        [SerializeField] private Transform pinataAnim;

        [SerializeField] private ParticleSystemPinata[] tapNoTransform;
        [SerializeField] private ParticleSystemPinata[] tapTransform;
        [SerializeField] private ParticleSystem[] beforeFinals;
        [SerializeField] private ParticleSystem[] finals;

        private int pigIndex => PinataController.CurrentPigIndex;

        public void PlayTapNoTransform()
        {
            if (tapNoTransform.Length == 0) return;

            var data = tapNoTransform[Mathf.Clamp(pigIndex - 1, 0, tapNoTransform.Length - 1)];

            if (data.before != null)
            {
                Instantiate(data.before, beforeParent).Play();
            }

            if (data.after != null)
            {
                Instantiate(data.after, afterParent).Play();
            }

        }

        public void PlayTapTransform()
        {
            if (tapTransform.Length == 0) return;

            var data = tapTransform[Mathf.Clamp(pigIndex - 1, 0, tapTransform.Length - 1)];

            if (data.before != null)
            {
                Instantiate(data.before, beforeParent).Play();
            }

            if (data.after != null)
            {
                Instantiate(data.after, afterParent).Play();
            }

            if (data.mask != null)
            {
                Instantiate(data.mask, pinataAnim).Play();
            }
        }

        public void PlayBeforeFinal()
        {
            if (beforeFinals.Length == 0) return;

            var data = beforeFinals[Mathf.Clamp(pigIndex - 1, 0, beforeFinals.Length - 1)];

            Instantiate(data, beforeParent).Play();
        }

        public void PlayFinal()
        {
            if (finals.Length == 0) return;

            var data = finals[Mathf.Clamp(pigIndex - 1, 0, finals.Length - 1)];

            var receivePanel = PanelManager.Instance.GetPanel<PopupReceiveCard_Pinata>();

            if (receivePanel != null)
                Instantiate(data, receivePanel.transform).Play();
        }

        [Serializable]
        public class ParticleSystemPinata
        {
            public ParticleSystem before;
            public ParticleSystem after;
            public ParticleSystem mask;
        }
    }
}