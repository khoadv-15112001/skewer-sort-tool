using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GrillSort.Rail
{
    public class UIRailCharacter : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic anim;

        private State lastIdleState = State.Idle1;
        private Tween idleTween;

        public void PlayAnim(State state, bool isLoop)
        {
            // Dừng random idle tự động khi play animation khác
            StopRandomIdle();

            anim.AnimationState.SetAnimation(0, state.ToString(), isLoop);
        }

        public float GetTimeAnim(State state)
        {
            return anim.SkeletonData.FindAnimation(state.ToString()).Duration;
        }

        public void AddAnimation(State state, bool isLoop)
        {
            anim.AnimationState.AddAnimation(0, state.ToString(), isLoop, 0);
        }

        public void PlayAnimWithCallback(State state, bool isLoop, Action onComplete)
        {
            // Dừng random idle tự động khi play animation khác
            StopRandomIdle();

            var trackEntry = anim.AnimationState.SetAnimation(0, state.ToString(), isLoop);

            if (onComplete != null && trackEntry != null)
            {
                trackEntry.Complete += (entry) => onComplete?.Invoke();
            }
        }

        public void PlayRandomIdle(bool isLoop = true)
        {
            // Random theo tỉ lệ:
            // Idle1: 50%, Idle2: 20%, Idle3_Wave/Idle4/Idle5: mỗi cái 10%
            int randomValue = UnityEngine.Random.Range(0, 100);
            State randomIdle;

            if (randomValue < 50) // 0-49 = 50%
            {
                randomIdle = State.Idle1;
            }
            else if (randomValue < 70) // 50-69 = 20%
            {
                randomIdle = State.Idle2;
            }
            else if (randomValue < 80) // 70-79 = 10%
            {
                randomIdle = State.Idle3_Wave;
            }
            else if (randomValue < 90) // 80-89 = 10%
            {
                randomIdle = State.Idle4;
            }
            else // 90-99 = 10%
            {
                randomIdle = State.Idle5;
            }

            lastIdleState = randomIdle;

            // Dừng tween cũ nếu có
            StopRandomIdle();

            // Chơi animation không loop, sau khi xong sẽ tự động random idle khác
            anim.AnimationState.SetAnimation(0, randomIdle.ToString(), false);

            // Lấy thời gian của animation để biết khi nào kết thúc
            var trackEntry = anim.AnimationState.GetCurrent(0);
            if (trackEntry != null)
            {
                float animDuration = trackEntry.Animation.Duration;
                idleTween = DOVirtual.DelayedCall(animDuration, () => PlayRandomIdle(false));
            }
        }

        public void StopRandomIdle()
        {
            if (idleTween != null)
            {
                idleTween.Kill();
                idleTween = null;
            }
        }

        public void WalkInFromLeft(Action onComplete = null)
        {
            // Hiện character
            gameObject.SetActive(true);

            // Đặt character ở ngoài màn hình bên trái
            var characterRect = GetComponent<RectTransform>();
            var canvas = characterRect.GetComponentInParent<Canvas>();
            var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
            var startX = -canvasWidth / 2f - 200f;
            var centerX = 0f;

            characterRect.anchoredPosition = new Vector2(startX, characterRect.anchoredPosition.y);

            // Play animation Walk
            PlayAnim(State.Walk, true);

            // Di chuyển từ trái vào giữa
            characterRect.DOAnchorPosX(centerX, 1.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Sau khi vào giữa, chơi animation Wave
                    lastIdleState = State.Idle3_Wave; // Set để tránh random lại wave
                    PlayAnim(State.Idle3_Wave, false);

                    DOVirtual.DelayedCall(GetTimeAnim(State.Idle3_Wave), () =>
                    {
                        PlayRandomIdle();
                    });

                    // Sau khi wave xong
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        onComplete?.Invoke();
                    });
                });
        }

        public enum State
        {
            AfterOpen_Forward_Idle,
            Idle1,
            Idle2,
            Idle3_Wave,
            Idle4,
            Idle5,
            OpenBox_Blue,
            OpenBox_Red,
            OpenBox_Purple,
            OpenBox_Blue1,
            OpenBox_Blue2,
            OpenBox_Blue3,
            OpenBox_Blue_Idle,
            OpenBox_Red1,
            OpenBox_Red2,
            OpenBox_Red3,
            OpenBox_Red_Idle,
            OpenBox_Purple1,
            OpenBox_Purple2,
            OpenBox_Purple3,
            OpenBox_Purple_Idle,
            Walk
        }
    }
}