using DG.Tweening;
using Gameplay.Entities.GrillScripts;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace Gameplay.Entities.Grills
{
    public class GrillVisualIce : GrillVisual
    {
        [SerializeField] private SpriteRenderer[] iceStates;
        [SerializeField] private ParticleSystem[] iceEffects;
        private int currentIceState = 0;

        protected override void SetVisual()
        {
            base.SetVisual();
            currentIceState = 0;
        }

        public void SetIceState(int state)
        {
            for (int i = 0; i < iceStates.Length; i++)
            {
                if (currentIceState <= state)
                {
                    if (i + 1 == state)
                    {
                        iceStates[i].gameObject.SetActive(true);
                        iceStates[i].SetAlpha(0);
                        iceStates[i].DOFade(1, 0.75f);
                    }
                    else
                    {
                        iceStates[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (i + 1 == state || state == 0)
                    {
                        if (state > 0)
                        {
                            iceStates[state - 1].gameObject.SetActive(true);
                            iceStates[state - 1].SetAlpha(1);
                        }

                        var iceState = iceStates[state];
                        SonatUtils.DelayCall(0.1f, () =>
                        {
                            iceState.DOFade(0, 0.3f).OnComplete(() =>
                            {
                                iceState.gameObject.SetActive(false);
                            });
                            iceEffects[state].gameObject.SetActive(true);
                            iceEffects[state].Play();
                            MySonatFramework.audioService.PlaySound(AudioId.Obstacle_Ice_break_Grill_sort_01 + (ushort)state);
                        }, this);
                    }
                    // else
                    // {
                    //     iceStates[i].gameObject.SetActive(false);
                    // }
                }
            }

            currentIceState = state;
        }

        public void ForceSetIceState(int state)
        {
            foreach (var iceState in iceStates)
            {
                iceState.gameObject.SetActive(false);
            }
            if (state - 1 >= 0)
            {
                iceStates[state - 1].gameObject.SetActive(true);
                iceStates[state - 1].SetAlpha(1);
            }
            currentIceState = state;
        }
    }
}