using System;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    public class CardParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] psByStar;
        [SerializeField] private ParticleSystem psAppear;
        [SerializeField] private float delayBeforePlayPSByStar = 0.5f;

        private int star;
        public void SetData(int star)
        {
            this.star = star;
        }

        public void PlayPSByStar()
        {
            HideAllPS();
            psByStar[star - 1].gameObject.SetActive(true);
        }

        private void HideAllPS()
        {
            foreach (var ps in psByStar)
            {
                ps.gameObject.SetActive(false);
            }
        }

        public void PlayPSAppear()
        {
            psAppear.Play();
            SonatUtils.DelayCall(delayBeforePlayPSByStar, () =>
            {
                PlayPSByStar();
            });
        }
    }
}