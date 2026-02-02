using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.BoosteeManagement;
using Gameplay.LevelData;
using Manager;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace Gameplay.Entities.Grills
{
    public class PrimaryGrillIce : PrimaryGrill
    {
        [SerializeField] private int iceState = 3;
        [SerializeField] private GrillVisualIce visualIce;
        private int currentState = 0;
        private int numStep = 0;
        private bool success = false;
        public static List<PrimaryGrillIce> primaryGrillIces;
        private bool started;
        [HideInInspector] public int priority;

        public int CurrentState => currentState;
        public int NumStep => numStep;

        public override async UniTask SetData(GrillData grillData)
        {
            currentState = iceState;
            lockState = 1;
            numStep = 0;
            started = false;
            success = false;
            base.SetData(grillData);
            visualIce.SetIceState(currentState);
            SetLockItems(true);

            primaryGrillIces ??= new List<PrimaryGrillIce>();
            primaryGrillIces.Add(this);
            if (grillData is GrillIceData grillIceData)
            {
                priority = grillIceData.priority;
            }
        }

        public static void CheckAndStartProgress()
        {
            if (primaryGrillIces is not { Count: > 0 }) return;
            primaryGrillIces.Sort((a, b) => a.GetPriority().CompareTo(b.GetPriority()));
            primaryGrillIces[0].StartProgress();
        }

        public int GetPriority()
        {
            return lockState > 1 ? 100 + priority : priority;
        }

        public void StartProgress()
        {
            if (started) return;
            started = true;

            grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnCollectItem(OnCollectItem);
            grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnDropItem(OnItemDropped);
        }

        public void NextProgress()
        {
            grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnCollectItem(OnCollectItem);
            grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
            primaryGrillIces.Remove(this);
            if (primaryGrillIces.Count > 0)
            {
                primaryGrillIces[0].StartProgress();
            }
        }

        private void OnDisable()
        {
            grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnCollectItem(OnCollectItem);
            grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
        }

        private void OnCollectItem(int itemId)
        {
            if (!IsLock || LockState > 1) return;
            success = true;
            DownState();
            numStep = 0;
        }

        private void OnItemDropped(Item item, bool changed)
        {
            // if (!changed || !isLock) return;
            if (!changed || lockState != 1) return;
            numStep++;
            success = false;
            if (numStep >= GetIceGrillStep() && currentState < iceState)
            {
                SonatUtils.DelayCall(0.35f, () =>
                {
                    if (!success) UpState();
                }, this);
                numStep = 0;
            }
        }

        private int GetIceGrillStep()
        {
            return grillBaseBehaviorSO.GetIceGrillStep();
        }

        public override ShuffleLayerData GetShuffleLayerData()
        {
            if (IsLock)
                return null;
            else
            {
                return base.GetShuffleLayerData();
            }
        }

        public override List<ShuffleLayerData> GetSubsShuffleLayerData()
        {
            if (IsLock)
                return null;
            return base.GetSubsShuffleLayerData();
        }

        public void UpState()
        {
            if (currentState >= iceState) return;
            this.currentState++;
            visualIce.SetIceState(currentState);
        }

        public void DownState()
        {
            if (currentState <= 0) return;
            this.currentState--;
            visualIce.SetIceState(currentState);

            if (currentState == 0)
            {
                UnlockIce();
            }
        }

        private void UnlockIce()
        {
            NextProgress();
            SetLockItems(false);
        }


        public override void EnableClick()
        {
            base.EnableClick();
            SetSlotCollider(false);
        }

        public override void Unlock()
        {
            StartCoroutine(PlayUnlockIce());
        }

        private IEnumerator PlayUnlockIce()
        {
            for (int i = currentState; i > 0; i--)
            {
                DownState();
                yield return new WaitForSeconds(0.3f);
            }

            base.Unlock();
            SetSlotCollider(true);
        }

        public void SetIceState(int currentState, int numStep)
        {
            this.currentState = currentState;
            this.numStep = numStep;
            visualIce.ForceSetIceState(currentState);

            if (currentState == 0)
            {
                UnlockIce();
            }
        }
    }
}