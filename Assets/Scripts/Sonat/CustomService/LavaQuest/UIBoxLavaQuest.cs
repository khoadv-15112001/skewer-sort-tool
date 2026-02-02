using DG.Tweening;
using Sirenix.OdinInspector;
using SonatFramework.Systems.SettingsManagement.Vibation;
using SonatFramework.Systems;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.LavaQuest
{
    public class UIBoxLavaQuest : MonoBehaviour
    {
        [SerializeField] private List<Transform> placeHolders = new();

        [SerializeField] private SkeletonGraphic boxAnim;
        [SerializeField] private SkeletonGraphic waterAnim;

        [SerializeField] private CanvasGroup canvaGroup;

        [SerializeField] private List<ParticleSystem> effects;

        public Transform boxBone;

        public bool isStartPlace;

        public const string Idle = "Idle";
        public const string Shake = "Shake";
        public const string WaterDisappear = "Disappear";

        private void Start()
        {
            PlayAnim();
        }

        public void Show(bool state)
        {
            if (!state)
            {
                canvaGroup.alpha = 0;
            }
            else
            {
                canvaGroup.DOFade(1f, 0.4f);
            }
        }

        private void PlayAnim()
        {
            if (boxAnim == null) return;

            boxAnim.AnimationState.SetAnimation(0, Idle, true);
            waterAnim.AnimationState.SetAnimation(0, Idle, true);
        }

        public Transform GetPlaceHolder(int index)
        {
            return placeHolders[index];
        }

        public void PlayShake()
        {
            if (boxAnim == null) return;

            waterAnim.AnimationState.SetAnimation(0, Shake, false);
            boxAnim.AnimationState.SetAnimation(0, Shake, false);
        }

        public float offsetY;
        public float scale;
        public float duration;

        [Button("Test")]
        public void Sink()
        {
            if (boxAnim == null) return;


            foreach (var effect in effects)
            {
                effect.transform.SetParent(transform.parent);
                effect.Play();
            }

            waterAnim.transform.SetParent(transform.parent);
            waterAnim.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);

            boxAnim.transform.DOLocalMoveY(boxAnim.transform.localPosition.y - offsetY, duration);
            boxAnim.transform.DOScale(scale, duration);
            boxAnim.GetComponent<CanvasGroup>().DOFade(0f, duration);

            waterAnim.AnimationState.SetAnimation(0, WaterDisappear, false);

            float timeGap = 0.1f;

            for (int i = 0; i < 4; i++)
            {
                DOVirtual.DelayedCall(timeGap * i, () =>
                {
                    SonatSystem.GetService<VibrationService>().Vibrate(5);
                });
            }
        }
    }
}