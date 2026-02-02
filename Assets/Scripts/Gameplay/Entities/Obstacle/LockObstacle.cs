using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using Spine.Unity;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Entities.Obstacle
{
    public class LockObstacle : MonoBehaviour
    {
        public ObstacleType ObstacleType => ObstacleType.Lock;
        private int progress = 4;
        [SerializeField] private TMP_Text txtProgress;
        private PrimaryGrill primaryGrill;

        public static List<LockObstacle> lockObstacles;
        [SerializeField] private ParticleSystem breakChanceParticle;
        [SerializeField] private ParticleSystem unlockParticle;
        [SerializeField] private Transform[] chancePos;

        //[SerializeField] private GameObject[] chances;
        //[SerializeField] private GameObject lockObj;
        [SerializeField] private SkeletonAnimation animation;
        private bool started = false;
        [SerializeField] private GameObject progressObj;
        private int priority;
        private bool blockClick = false;

        public void StartProgress()
        {
            if (started) return;
            progress = 4;
            started = true;
            var grillBehavior = primaryGrill.GrillBaseBehaviorSO;
            grillBehavior.eventSystemSO.RegisterEvents_OnCollectItem(OnCollectAItem);
        }

        public void SetData(GrillBase grill, int priority)
        {
            this.priority = priority;

            if (animation == null)
                animation = GetComponentInChildren<SkeletonAnimation>();

            gameObject.SetActive(true);
            primaryGrill = grill as PrimaryGrill;
            primaryGrill.SetLockItems(true);
            //primaryGrill.AddObstacle(this);
            transform.SetParent(primaryGrill.transform);
            transform.localPosition = Vector3.back;
            animation.gameObject.SetActive(true);
            breakChanceParticle.gameObject.SetActive(false);
            unlockParticle.gameObject.SetActive(false);
            started = false;
            UpdateVisual();
            UpdateAnim("Idle", true);

            lockObstacles ??= new();
            lockObstacles.Add(this);
            blockClick = false;

            isClicked = false;
        }

        public static void CheckAndStartProgress()
        {
            if (lockObstacles is not { Count: > 0 }) return;
            lockObstacles.Sort((a, b) => a.priority - b.priority);
            lockObstacles[0].StartProgress();
        }

        public void NextProgress()
        {
            var grillBehavior = primaryGrill.GrillBaseBehaviorSO;
            grillBehavior.eventSystemSO.UnregisterEvents_OnCollectItem(OnCollectAItem);
            lockObstacles.Remove(this);
            if (lockObstacles.Count > 0)
            {
                lockObstacles[0].StartProgress();
            }
        }

        public void OnComplete()
        {
            primaryGrill.Unlock();
            // //primaryGrill.RemoveObstacle(ObstacleType);
            gameObject.SetActive(false);
            // //SonatUtils.DelayCall(0.5f, () => GameFactory.ReturnEntity(this), this);
        }

        public void OnCollectAItem(int id)
        {
            if (primaryGrill.LockState > 1) return;
            progress--;
            SonatUtils.DelayCall(0.5f, () =>
            {
                breakChanceParticle.transform.position = chancePos[progress].position;
                breakChanceParticle.gameObject.SetActive(true);
                breakChanceParticle.Play();
                AudioId breakAudio = AudioId.Obstacle_Chain_01 + (ushort)Random.Range(0, 3);
                MySonatFramework.audioService.PlaySound(breakAudio);
            }, this);

            if (progress == 0)
            {
                NextProgress();
            }

            UpdateAnim("Unlock", false, () =>
            {
                if (progress == 0)
                {
                    unlockParticle.gameObject.SetActive(true);
                    unlockParticle.Play();
                    animation.gameObject.SetActive(false);
                    SonatUtils.DelayCall(0.5f, OnComplete, this);
                    progress = 4;
                }
                else
                {
                    UpdateVisual();
                }
            });
        }

        private void UpdateVisual()
        {
            txtProgress.text = progress.ToString();
            UpdateSkin(progress.ToString());
        }

        private void UpdateAnim(string animName, bool loop, Action onComplete = null)
        {
            animation.AnimationState.ClearTracks();
            animation.Initialize(true);
            animation.AnimationState.SetAnimation(0, animName, loop).Complete += (track) => { onComplete?.Invoke(); };
        }

        private void UpdateSkin(string skinName)
        {
            if (animation == null)
            {
                Debug.LogError($"[LockObstacle] Missing SkeletonAnimation on {name}");
                return;
            }

            animation.AnimationState.ClearTracks();
            animation.initialSkinName = skinName;
            animation.Initialize(true);
            UpdateAnim("Idle", true);
        }


        private bool isClicked = false;

        private void OnMouseUpAsButton()
        {
            var gameState = primaryGrill.GrillBaseBehaviorSO.GetGameState();
            if (GameplayController.instance.gameState != GameState.Playing) return;
            if (isClicked || blockClick) return;
            if (GameRemoteConfigValue.popupUnlockTray)
            {
                PanelManager.Instance.OpenPanelByName<PopupUnlockGrill>(
                    "PopupUnlockLockGrill",
                    new UIData().Add("Callback", (Action)ForceUnlock).Add("PrimaryGrill", primaryGrill)
                );
            }
            else
            {
                MySonatFramework.ShowRewardAds(ForceUnlock, "booster", "unlock");
            }
        }

        public void ForceUnlock()
        {
            isClicked = true;
            progress = 0;
            SonatUtils.DelayCall(0.5f, () =>
            {
                AudioId breakAudio = AudioId.Obstacle_Chain_01 + (ushort)Random.Range(0, 3);
                MySonatFramework.audioService.PlaySound(breakAudio);
            }, this);

            NextProgress();

            UpdateAnim("Unlock", false, () =>
            {
                unlockParticle.gameObject.SetActive(true);
                unlockParticle.Play();
                animation.gameObject.SetActive(false);
                SonatUtils.DelayCall(0.5f, OnComplete, this);
            });
        }

        public void SetVisibility(bool visible)
        {
            //progressObj.SetActive(!visible);
        }

        public void SetBlockClick(bool value)
        {
            blockClick = value;
        }

        public void OnDisable()
        {
            progress = 4;
            started = false;
            var grillBehavior = primaryGrill.GrillBaseBehaviorSO;
            grillBehavior.eventSystemSO.UnregisterEvents_OnCollectItem(OnCollectAItem);
        }

        private void OnDestroy()
        {
            progress = 4;
            started = false;
            var grillBehavior = primaryGrill.GrillBaseBehaviorSO;
            grillBehavior.eventSystemSO.UnregisterEvents_OnCollectItem(OnCollectAItem);
        }
    }
}