using System;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.ChestRewardProgress
{
    public abstract class ChestAnimationInterface : MonoBehaviour
    {
        public abstract void PlayIdleAnimation();
        public abstract void PlayIdleOpenAnimation();
        public abstract void PlayOpenAnimation(Action onComplete = null);
        public abstract void SetChestState(ChestState state);
        public abstract void Cleanup();
    }

    public enum ChestState: byte
    {
        None = 0,
        Locked,
        Opening,
        Opened
    }
}