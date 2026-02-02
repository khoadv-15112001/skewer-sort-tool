using System;
using System.Collections;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.ObjectPooling;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Entities.Orders
{
    public class OrderVisual : MonoBehaviour, IPoolingObject
    {
        public enum Character : byte
        {
            Male,
            Female,
        }

        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private Character character;
        private bool isCompleted = false;
        private static int voiceCounter = 0;
        

        public void OnComplete()
        {
            isCompleted = true;
            PlayAnimation("Win", false, () => { PlayAnimation("Idle", true); });

            // var currentCombo = MySonatFramework.GetService<ComboService>().Combo;
            //
            // if (GameplayController.BgMusic == AudioId.BGM_Ingame_Japan_Grill_sort)
            // {
            //     if (currentCombo % 3 == 1)
            //     {
            //         PlayVoice(GameplayController.BgMusic);
            //     }
            // }
            // else
            // {
            //     if (currentCombo == 5 || currentCombo >= 10)
            //     {
            //         PlayVoice(GameplayController.BgMusic);
            //     }
            // }

            transform.SetLocalPositionZ(0.1f);
        }

        private void PlayVoice(AudioId bgMusic)
        {
            var currentCombo = MySonatFramework.GetService<ComboService>().Combo;

            switch (bgMusic)
            {
                case AudioId.BGM_Ingame_Japan_Grill_sort:
                    var audioIdMan = (AudioId)((int)AudioId.Voice_Char_man_JP_Finish_Arigatou + voiceCounter);
                    var audioIdWoman = (AudioId)((int)AudioId.Voice_Char_woman_JP_Finish_Arigatou + voiceCounter);
                    var range = (int)AudioId.Voice_Char_woman_JP_Finish_Arigatou - (int)AudioId.Voice_Char_man_JP_Finish_Arigatou;

                    MySonatFramework.audioService.PlaySound(character == Character.Male ? audioIdMan : audioIdWoman);

                    voiceCounter++;
                    if (voiceCounter >= range)
                    {
                        voiceCounter = 0;
                    }

                    break;

                default:
                    AudioId baseAudioId;

                    if (currentCombo == 5)
                    {
                        baseAudioId = character == Character.Male ? AudioId.Voice_Char_man_Combo_Good : AudioId.Voice_Char_woman_Combo_Good;
                    }
                    else if (currentCombo >= 10)
                    {
                        var cyclePosition = (currentCombo - 10) % 4;

                        switch (cyclePosition)
                        {
                            case 0: // Mốc 10, 14, 18, 22... -> Wow
                                baseAudioId = character == Character.Male ? AudioId.Voice_Char_man_Combo_Wow : AudioId.Voice_Char_woman_Combo_Wow;
                                break;
                            case 1: // Mốc 11, 15, 19, 23... -> Excellent  
                                baseAudioId = character == Character.Male ? AudioId.Voice_Char_man_Combo_Excellent : AudioId.Voice_Char_woman_Combo_Excellent;
                                break;
                            case 2: // Mốc 12, 16, 20, 24... -> Amazing
                                baseAudioId = character == Character.Male ? AudioId.Voice_Char_man_Combo_Amazing : AudioId.Voice_Char_woman_Combo_Amazing;
                                break;
                            case 3: // Mốc 13, 17, 21, 25... -> Incredible
                                baseAudioId = character == Character.Male ? AudioId.Voice_Char_man_Combo_Incredible : AudioId.Voice_Char_woman_Combo_Incredible;
                                break;
                            default:
                                return;
                        }
                    }
                    else
                    {
                        return;
                    }

                    MySonatFramework.audioService.PlaySound(baseAudioId);
                    break;
            }
        }

        private void PlayAnimation(string animationName, bool loop, Action onComplete = null)
        {
            if (skeletonAnimation == null) return;
            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.Initialize(true);
            if (loop)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
            }
            else
            {
                skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop).Complete += (track) => { onComplete?.Invoke(); };
            }
        }

        private void IdleAnimation()
        {
            if (isCompleted || !gameObject.activeInHierarchy) return;
            int r = Random.Range(0, 5);
            string animName = "Idle";
            if (r == 1) animName = "Idle2";
            PlayAnimation(animName, false, IdleAnimation);
        }
        public void PlayVoice(string voice)
        {
            if (LocalizationUtils.IsJapanese())
            {
                MySonatFramework.audioService.PlaySound($"Order/Voice/JapanVoice_{character}_{voice}");

            }
            else
            {
                MySonatFramework.audioService.PlaySound($"Order/Voice/Voice_{character}_{voice}");
            }
        }
        public void Setup()
        {
        }

        public void OnCreateObj(params object[] args)
        {
            IdleAnimation();
        }

        public void OnReturnObj()
        {
            isCompleted = false;
        }
    }
}