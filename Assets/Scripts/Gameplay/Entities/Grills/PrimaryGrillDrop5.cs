using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.LevelData;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace Gameplay.Entities.Grills
{
    public class PrimaryGrillDrop5 : PrimaryGrill
    {
        //private bool available;
        [SerializeField] private float dropDuration = 0.35f;
        [SerializeField] private Ease ease = Ease.InQuad;
        //public bool Available => available;
        private static List<Vector3> dropPoints;
        private static List<PrimaryGrillDrop5> allGrill;

        public override async UniTask SetData(GrillData grillData)
        {
            SetMaskVisible(true);
            base.SetData(grillData);
            isInGameplaySpace = grillData.position.y < LevelGenerator.maxYGameSpace;
        }

        // public static void InitializeDrop()
        // {
        //     dropPoints = new List<Vector3>();
        //     allGrill = GameplayController.instance.levelGenerator.GetPrimaryGrills().OfType<PrimaryGrillDrop5>()
        //         .Where(e => GameplayController.instance.levelGenerator.IsFreeGrill(e)).ToList();
        //     allGrill.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
        //     foreach (var grill in allGrill)
        //     {
        //         dropPoints.Add(grill.transform.position);
        //     }
        // }

        private void DropGrills()
        {
            for (int i = 0; i < allGrill.Count; i++)
            {
                allGrill[i].Drop(dropPoints[i]);
            }
        }

        private void CheckInGameSpace()
        {
            for (int i = 0; i < allGrill.Count; i++)
            {
                if (!allGrill[i].isInGameplaySpace)
                    allGrill[i].isInGameplaySpace = dropPoints[i].y < LevelGenerator.maxYGameSpace;
            }
        }

        protected override void OnComplete()
        {
            if (!CanRevokeGrill(true))
            {
                base.OnComplete();
                isProcess = false;
                return;
            }
            available = false;
            if (allGrill.Contains(this)) allGrill.Remove(this);
            base.OnComplete();
            Finish(GameDefine.timeDelaySubGrill);
            MySonatFramework.audioService.PlaySound(AudioId.Items_Merge_SMode_Drop_line_Grill_sort);
        }

        public override void CheckEmpty()
        {
            foreach (var slot in slots)
            {
                if (!slot.isEmpty()) return;
            }

            if (!CanRevokeGrill(false))
            {
                return;
            }

            Finish();
        }


        protected override void UpdateSubGrills()
        {
        }

        private void Finish(float delayRevoke = 0)
        {
            available = false;
            if (allGrill.Contains(this)) allGrill.Remove(this);
            RevokeGrill(delayRevoke);
        }

        private void RevokeGrill(float delay = 0)
        {
            CheckInGameSpace();
            SonatUtils.DelayCall(delay, () =>
            {
                transform.DOScale(0, dropDuration).OnComplete(() => GameplayController.instance.levelGenerator.ReturnGrill(this));
                SonatUtils.DelayCall(0.25f, () =>
                {
                    DropGrills();
                    SonatUtils.DelayCall(0.25f, () => { MySonatFramework.audioService.PlaySound(AudioId.Shelf_Close_SMode_Drop_line_Grill_sort); }, this);
                }, this);
            });
        }

        public void Drop(Vector3 position)
        {
            transform.DOKill();
            transform.DOMove(position, dropDuration).SetEase(ease).SetDelay(0.1f);
        }

        public override SlotBase GetNearestSlot(Vector3 position)
        {
            if (!available) return null;
            return base.GetNearestSlot(position);
        }

        public override int NumberSlotEmpty()
        {
            if (!available) return 0;
            return base.NumberSlotEmpty();
        }

        public override int NumberDifferentItems(int id)
        {
            if (!available) return 0;
            return base.NumberDifferentItems(id);
        }

        public override void OnCreateObj(params object[] args)
        {
            base.OnCreateObj(args);
            available = true;
        }

        public override void OnReturnObj()
        {
            base.OnReturnObj();
            transform.localScale = Vector3.one;
        }

        // public bool CanRevokeGrill(bool complete)
        // {
        //     if (allGrill.Count > 1) return true;
        //     int itemRemain = GameplayController.instance.levelGenerator.GetPrimaryItemRemain();
        //     if (complete)
        //     {
        //         return itemRemain <= SlotCount;
        //     }
        //     else
        //     {
        //         return itemRemain == 0;
        //     }
        // }
    }
}