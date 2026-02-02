using System;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace Gameplay.Effect
{
    public class KeyEffect : EffectPoolBase
    {
        private Quaternion rotation;
        [SerializeField] private Transform key;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Ease ease;
        [SerializeField] private Animator animator;

        public override void Setup()
        {
            base.Setup();
            rotation = key.transform.rotation;
        }

        public void SetData(Vector3 keyPos, Vector3 targetPos, Action callback)
        {
            key.transform.rotation = rotation;
            this.transform.position = keyPos;
            transform.DOMove(targetPos, duration).SetEase(ease).OnComplete(() => UnlockAnim(callback));
            key.transform.DORotate(Vector3.zero, duration);
        }

        private void UnlockAnim(Action callback)
        {
            MySonatFramework.audioService.PlaySound(AudioId.Obstacle_Locknkey_Turning_Grill_sort);
            SonatUtils.DelayCall(0.75f, callback, this);
            animator.Play("Unlock");
        }

        public override void OnReturnObj()
        {
            transform.DOKill();
            key.transform.DOKill();
            base.OnReturnObj();
        }
    }
}