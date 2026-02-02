using System;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Gameplay.Entities.Obstacle.Visual
{
    public class OctoChefVisual: MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation[] octoAnims;
        //[SerializeField] private SortingGroup sortingGroup;
        private int idleCount = 0;
        private int idleRandom;
        private int state;
        [SerializeField] private GameObject smoke;

        public void OnCreate()
        {
            Highlight(false);
            smoke?.SetActive(false);
        }

        public void SetState(int state)
        {
            this.state = state;
        }

        public void Appear()
        {
            idleRandom = Random.Range(2, 4);
            PlayAnimation($"Stage{state}_Appear", false, Idle);
            MySonatFramework.audioService.PlaySound("Obstacle_Octochef_Appear");
        }

        public void Idle()
        {
            if (idleCount >= idleRandom)
            {
                idleCount = 0;
                PlayAnimation($"Stage{state}_Idle2", false, Idle);
            }
            else
            {
                PlayAnimation($"Stage{state}_Idle", false, Idle);
                idleCount++;
            }
        }

        public void Out()
        {
            PlayAnimation($"Stage{state}_Out", false, null);
            MySonatFramework.audioService.PlaySound("Obstacle_Octochef_Dissappear");
        }

        public void BlowTorch()
        {
            smoke?.SetActive(true);
            PlayAnimation("Destroy", false, Idle);
        }


        private void PlayAnimation(string animationName, bool loop, Action _callback)
        {
            Action callback = _callback;
            foreach (var octoAnim in octoAnims)
            {
                octoAnim.AnimationState.ClearTracks();
                octoAnim.Initialize(true);
                if (loop)
                {
                    octoAnim.AnimationState.SetAnimation(0, animationName, true);
                }
                else
                {
                    octoAnim.AnimationState.SetAnimation(0, animationName, false).Complete += (track) =>
                    {
                        callback?.Invoke();
                        callback = null;
                    };
                }
            }
        }

        public void Highlight(bool highlight)
        {
            //sortingGroup.enabled = highlight;
        }
    }
}