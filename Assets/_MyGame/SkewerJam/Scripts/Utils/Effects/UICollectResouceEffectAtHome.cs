using System;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace SkewerJam.Utils.Effects
{
    public class UICollectResouceEffectAtHome : UICollectEffectItem
    {
        [Header("Config for UICollectResouceEffectAtHome")]
        [SerializeField] private float delayJump = 1.5f;

        protected override void DOEffect()
        {
            SonatUtils.DelayCall(delayJump, () =>
            {
                base.DOEffect();
            }, this);
        }
    }
}
