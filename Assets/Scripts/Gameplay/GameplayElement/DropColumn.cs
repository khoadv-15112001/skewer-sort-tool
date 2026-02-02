using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Entities;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace Gameplay.GameplayElement
{
    public class DropColumn: MonoBehaviour, IPoolingObject
    {
        private List<PrimaryGrill> allGrill;
        private List<Vector3> dropPoints;
        private float posXBase;
        private float epsilon = 1.5f;
        [SerializeField] private float dropDuration = 0.45f;
        //[SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private AnimationCurve dropCurve;

        public bool ValidateGrill(PrimaryGrill grill)
        {
            if (grill == null)
            {
                return true;
            }
            if(grill.SlotCount >= 5) return true;
            return Mathf.Abs(grill.transform.position.x - posXBase) <= epsilon;
        }

        public void AddGrill(PrimaryGrill grill)
        {
            if (allGrill == null)
            {
                allGrill = new List<PrimaryGrill>();
                posXBase = grill.transform.position.x;
            }
            allGrill.Add(grill);
            grill.onMainLayerEmpty += OnGrillEmpty;
            grill.onMainLayerComplete += OnGrillComplete;
            grill.SetMaskVisible(true);
        }
        
        public void InitializeDrop()
        {
            dropPoints = new List<Vector3>();
            allGrill.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
            foreach (var grill in allGrill)
            {
                dropPoints.Add(grill.transform.position);
            }
        }

        private void OnGrillEmpty(PrimaryGrill grill)
        {
            if(grill.SubGrillsCount() > 0 || !CanRevokeGrill(false)) return;
            FinishGrill(grill);
        }

        private void OnGrillComplete(PrimaryGrill grill)
        {
            if(grill.SubGrillsCount() > 0 || !CanRevokeGrill(true)) return;
            //MySonatFramework.audioService.PlaySound(AudioId.Items_Merge_SMode_Drop_line_Grill_sort);
            FinishGrill(grill, GameDefine.timeDelaySubGrill);
        }
        
        private void FinishGrill(PrimaryGrill grill, float delayRevoke = 0)
        {
            grill.available = false;
            if (allGrill.Contains(grill)) allGrill.Remove(grill);
            RevokeGrill(grill, delayRevoke);
        }

        private void RevokeGrill(PrimaryGrill grill, float delay = 0)
        {
            CheckInGameBoard();
            SonatUtils.DelayCall(delay, () =>
            {
                grill.transform.DOScale(0, dropDuration).OnComplete(() => GameplayController.instance.levelGenerator.ReturnGrill(grill));
                SonatUtils.DelayCall(0.25f, () =>
                {
                    DropGrills();

                    SonatUtils.DelayCall(0.25f, () => { MySonatFramework.audioService.PlaySound(AudioId.Shelf_Close_SMode_Drop_line_Grill_sort); }, this);
                }, this);
            });
        }
        
        private void CheckInGameBoard()
        {
            for (int i = 0; i < allGrill.Count; i++)
            {
                if (!allGrill[i].isInGameplaySpace)
                    allGrill[i].isInGameplaySpace = dropPoints[i].y < LevelGenerator.maxYGameSpace;
            }
        }
        
        private void DropGrills()
        {
            for (int i = 0; i < allGrill.Count; i++)
            {
                Drop(allGrill[i], dropPoints[i]);
            }
        }
        
        public void Drop(PrimaryGrill grill, Vector3 position)
        {
            grill.transform.DOKill();
            grill.transform.DOMove(position, dropDuration).SetEase(dropCurve).SetDelay(0.1f);
        }

        public void Setup()
        {
            
        }

        public void OnCreateObj(params object[] args)
        {
            
        }

        public void OnReturnObj()
        {
            allGrill = null;
            dropPoints = null;
            posXBase = 0;
        }
        
        public bool CanRevokeGrill(bool complete)
        {
            if (allGrill.Count > 1) return true;
            int itemRemain = GameplayController.instance.levelGenerator.GetPrimaryItemRemain();
            if (complete)
            {
                return itemRemain <= LevelGenerator.maxGrillSlot;
            }
            else
            {
                return itemRemain == 0;
            }
        }
    }
}