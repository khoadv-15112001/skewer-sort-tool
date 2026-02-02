using Cysharp.Threading.Tasks;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems;
using Spine.Unity;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;

namespace GrillSort.Pinata
{
    public class PinataAnimHandle : MonoBehaviour
    {
        [SerializeField] private PinataEffect pinataEffect;

        [SerializeField] private SkeletonGraphic logoAnim;
        [SerializeField] private SkeletonGraphic pigAnim;
        [SerializeField] private SkeletonGraphic ropeAnim;
        [SerializeField] private DOTweenAnimation scaleAnimation;

        [SerializeField] private float timeDelayAfterLogo;
        [SerializeField] private float timeDelayAfterAppear;

        [SerializeField] private float timeGapTap = 0.1f;
        [SerializeField] private float tapThresholdStrong = 0.5f;
        [SerializeField] private float timeGapChance = 0.1f;
        [SerializeField] private float timeDelayAfterExplore = 0.9f;
        [SerializeField] private float timeDelayAfterExplore_Gold = 0.95f;

        [SerializeField] private AudioClip appearAudio;
        [SerializeField] private AudioClip[] tap1Audios;
        [SerializeField] private AudioClip tap2Audio;
        [SerializeField] private AudioClip explosionAudio;

        public static Action OnTapPinataSuccessful;
        public static Action OnExplosion;

        private bool _isReady;
        public bool IsReady => _isReady;

        private float _lastTapTime = -999f;

        private PinataConfig.Stage.UpgradeStarRate _upgradeStarData;

        private SonatAudioService audioService => SonatSystem.GetService<SonatAudioService>();

        public void Appear()
        {
            _isReady = false;

            logoAnim.gameObject.SetActive(false);
            pigAnim.gameObject.SetActive(false);
            ropeAnim.gameObject.SetActive(false);

            AppearAsync().Forget();

            PinataController.OnTapPinata += OnTapPinata;
            PinataController.OnFullMilestone += OnFullMilestone;
        }

        private void OnDestroy()
        {
            PinataController.OnTapPinata -= OnTapPinata;
            PinataController.OnFullMilestone -= OnFullMilestone;
        }

        private async UniTask AppearAsync()
        {
            audioService.PlayAudio("", appearAudio);
            var logoAppear = logoAnim.AnimationState.SetAnimation(0, LogoState.Logo_Appear2.ToString(), false);
            logoAnim.gameObject.SetActive(true);

            await UniTask.WaitForSeconds(timeDelayAfterLogo);

            pigAnim.gameObject.SetActive(true);
            ropeAnim.gameObject.SetActive(true);

            var pigAppear = pigAnim.AnimationState.SetAnimation(0, GetPigAnimation(PigState.Appear), false);
            ropeAnim.AnimationState.SetAnimation(0, RopeState.Appear.ToString(), false);

            await UniTask.WaitForSeconds(timeDelayAfterAppear);

            _isReady = true;
        }

        private void OnTapPinata()
        {
            float currentTime = Time.time;

            if (currentTime - _lastTapTime <= timeGapTap)
            {
                return;
            }

            if (currentTime - _lastTapTime <= timeGapChance)
            {
                OnTap(true);
                PlayScaleAnimation();
                return;
            }

            bool canBonusCard = CanBonusCard();

            if (canBonusCard)
                UIPinataMilestoneChanceController.OnBonusCard?.Invoke();

            if (CanUpgradeStar() && !canBonusCard)
            {
                if (_upgradeStarData.IsUpgrade)
                {
                    OnChangeTo();
                    pinataEffect.PlayTapTransform();
                    PinataController.CurrentPigIndex++;
                }
                else
                {
                    OnTap();
                }

                PinataController.CurrentStar++;
                PinataController.OnUpdateStar?.Invoke();
            }
            else
            {
                OnTap();
            }

            PlayScaleAnimation();

            _lastTapTime = currentTime;

            PinataController.StarCollected.Add(PinataController.CurrentStar);

            OnTapPinataSuccessful?.Invoke();
        }

        private void OnTap(bool forceWeak = false)
        {
            float currentTime = Time.time;

            float delta = currentTime - _lastTapTime;

            if (delta <= tapThresholdStrong && !forceWeak)
            {
                Tap_2();
            }
            else
            {
                Tap_1();
            }

            pinataEffect.PlayTapNoTransform();
        }

        public bool CanUpgradeStar()
        {
            var stageData = PinataController.StageData;
            var currentStar = PinataController.CurrentStar;

            if (currentStar >= 3) return false;

            _upgradeStarData = stageData.UpgradeStarRates[Mathf.Clamp(currentStar - 1, 0, stageData.UpgradeStarRates.Length - 1)];
            var upgradeStarRate = _upgradeStarData.Rate;

            if (PinataService.CheatAutoGold) return true;

            return UnityEngine.Random.value <= upgradeStarRate;
        }

        public bool CanBonusCard()
        {
            if (PinataService.CheatAutoGold) return false;

            if (UIPinataMilestoneChanceController.Milestones.Count >= 5) return false;

            return UnityEngine.Random.value <= PinataController.StageData.BonusCardRate;
        }

        private void PlayScaleAnimation()
        {
            scaleAnimation.transform.DOKill();
            scaleAnimation.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.1f, 1, 1);
        }

        private void Tap_1()
        {
            pigAnim.AnimationState.SetAnimation(0, GetPigAnimation(PigState.Tap1), false);
            ropeAnim.AnimationState.SetAnimation(0, RopeState.Tap1.ToString(), false);
            //logoAnim.AnimationState.SetAnimation(0, LogoState.Logo_Attack.ToString(), false);
            audioService.PlayAudio("", tap1Audios[UnityEngine.Random.Range(0, tap1Audios.Length)]);
        }

        private void Tap_2()
        {
            pigAnim.AnimationState.SetAnimation(0, GetPigAnimation(PigState.Tap2), false);
            ropeAnim.AnimationState.SetAnimation(0, RopeState.Tap2.ToString(), false);
            //logoAnim.AnimationState.SetAnimation(0, LogoState.Logo_Attack.ToString(), false);
            audioService.PlayAudio("", tap2Audio);
        }

        private void OnChangeTo()
        {
            logoAnim.AnimationState.SetAnimation(0, LogoState.Logo_Attack.ToString(), false);
            ropeAnim.AnimationState.SetAnimation(0, RopeState.Changeto.ToString(), false);
            pigAnim.AnimationState.SetAnimation(0, GetPigAnimation(PigState.Changeto), false);
            audioService.PlayAudio("", tap2Audio);
        }

        private void OnFullMilestone()
        {
            Explose().Forget();
            //logoAnim.AnimationState.SetAnimation(0, LogoState.Logo_Explored.ToString(), false);
            //ropeAnim.AnimationState.SetAnimation(0, RopeState.Explore.ToString(), false);
            //pigAnim.AnimationState.SetAnimation(0, GetPigAnimation(PigState.Explore), false);

            //audioService.PlayAudio("", explosionAudio);
        }

        private async UniTaskVoid Explose()
        {
            await UniTask.WaitForSeconds(0.2f);

            pinataEffect.PlayBeforeFinal();

            var anim = pigAnim.AnimationState.SetAnimation(0, GetPigAnimation(PigState.Explore), false);
            ropeAnim.AnimationState.SetAnimation(0, RopeState.Explore.ToString(), false);
            audioService.PlayAudio("", explosionAudio);

            var timeAnim = anim.AnimationTime;

            await UniTask.WaitForSeconds(PinataController.CurrentPigIndex == 4 ? timeDelayAfterExplore_Gold : timeDelayAfterExplore);

            PlayScaleAnimation();

            //logoAnim.AnimationState.SetAnimation(0, LogoState.Logo_Explored.ToString(), false);

            OnExplosion?.Invoke();

            pinataEffect.PlayFinal();
        }

        public string GetPigAnimation(PigState state)
        {
            string anim = $"{PinataController.CurrentPigIndex}_{state}";

            if (state == PigState.Changeto)
            {
                anim += PinataController.CurrentPigIndex + 1;
            }

            return anim;
        }

        public enum RopeState
        {
            Appear,
            Changeto,
            Explore,
            Idle,
            Tap1,
            Tap2
        }

        public enum LogoState
        {
            Logo_Appear2,
            Logo_Attack,
            Logo_Explored
        }

        public enum PigState
        {
            Appear,
            Changeto,
            Idle,
            Tap1,
            Tap2,
            Explore
        }
    }
}
