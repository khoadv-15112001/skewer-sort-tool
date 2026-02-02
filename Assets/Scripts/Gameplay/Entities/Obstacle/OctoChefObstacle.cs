using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gameplay.Entities.Grills;
using Gameplay.Entities.Obstacle.Visual;
using Gameplay.LevelData;
using MyGame.SkewerJam.Scripts.SO.Behavior;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.Utils;
using UnityEngine;
using Random = System.Random;

namespace Gameplay.Entities.Obstacle
{
    public class OctoChefObstacle : ObstacleBase
    {
        public override ObstacleType ObstacleType => ObstacleType.OctoChef;

        //[SerializeField, ReadOnly] private int id;
        [SerializeField, Range(0, 1), Tooltip("Tỉ lệ số item đã hoàn thành")]
        private float itemThreshold = 2f / 3;

        [SerializeField, Range(0, 1), Tooltip("Tỉ lệ số grill trống còn lại")]
        private float grillThreshold = 1f / 2;

        [SerializeField] private OctoChefVisual visual;
        [SerializeField] private int slotCount = 3;

        [Header("Behavior")] [SerializeField] private OctoChefBehaviorSO octoChefBehaviorSO;


        private Random _rng;

        private List<PrimaryGrill> _validGrills;

        //private List<int> _validGrillIds;
        private PrimaryGrill _currentGrill;
        private static List<int> _currentGrillIds = new();
        private int _maxItemCount;
        private int _currentItemCount;
        private const int maxState = 2;
        private int state = maxState;

        public int SlotCount => slotCount;


        void OnDisable()
        {
            octoChefBehaviorSO.UnregisterEvents_OnCollectItem(OnCollectItem);
            transform.DOKill();
            transform.localScale = Vector3.one;
            _currentGrillIds.Clear();
        }

        public override void OnComplete()
        {
            transform.DOKill();
            //transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(Remove);
            active = false;
            visual.Out();
            SonatUtils.DelayCall(0.75f, Remove, this);
        }

        public override void SetGrill(List<GrillBase> grills)
        {
            var allGrills = octoChefBehaviorSO.GetPrimaryGrills();
            _validGrills = allGrills; //.Where(e => e.grillType != GrillType.Lock).ToList();
            //var _validGrillIds = _validGrills.Select(e => e.id).ToList();

            var levelData = octoChefBehaviorSO.GetLevelData();
            var items = levelData.grillData.Where(e => e != null && e.layer != null)
                .SelectMany(e => e.layer.Where(e => e != null && e.itemData != null)
                    .SelectMany(e => e.itemData.Where(e => e != null && e.id > 0 && e.id <= 1000)))
                .ToList();
            _maxItemCount = items.Count;
            _currentItemCount = _maxItemCount;
            state = maxState;
            visual.SetState(1);

            var level = MySonatFramework.userDataService.GetLevel();
            var seed = level + obstacleData.id;
            _rng = new Random(level);

            octoChefBehaviorSO.RegisterEvents_OnCollectItem(OnCollectItem);

            octoChefBehaviorSO.SetStartGrill(this, grills);
            active = true;
        }

        public int GetCurrentGrill()
        {
            return _currentGrill != null ? _currentGrill.id : 0;
        }

        public void SetCurrentGrill(PrimaryGrill grill)
        {
            _currentGrill = grill;
            _currentGrill.AddLockState();
            transform.SetParent(_currentGrill.transform);
            transform.localPosition = Vector3.zero;
            //transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
            PlayAppearAnimation();

            _currentGrillIds.Add(grill.id);

            // case duy nhất: lock
            if (grill.grillType == GrillType.Lock)
            {
                (grill as PrimaryGrillLock)?.SetInteractable(false);
            }
        }

        private void OnCollectItem(int id)
        {
            _currentItemCount -= 3;
            SonatUtils.DelayCall(0.1f, MoveToNewGrill, this);
        }

        private void MoveToNewGrill()
        {
            var lastGrill = _currentGrill;
            UnlockCurrentGrill();

            if (CheckComplete())
            {
                OnComplete();
            }
            else
            {
                _currentGrill = octoChefBehaviorSO.GetRandomGrill(_validGrills, slotCount, lastGrill, _rng);

                SonatUtils.DelayCall(0.75f, () =>
                {
                    lastGrill.UnlockState();
                    if (lastGrill.grillType == GrillType.Lock)
                    {
                        (lastGrill as PrimaryGrillLock).SetInteractable(true);
                    }

                    SetCurrentGrill(_currentGrill);
                }, this);
            }
        }

        public void UnlockCurrentGrill()
        {
            if (_currentGrill == null) return;
            _currentGrill.UnlockState();

            _currentGrillIds.Remove(_currentGrill.id);

            // case duy nhất: lock
            if (_currentGrill.grillType == GrillType.Lock)
            {
                (_currentGrill as PrimaryGrillLock).SetInteractable(true);
            }

            _currentGrill = null;
            visual.Out();
        }

        private void PlayAppearAnimation()
        {
            //transform.localScale = Vector3.one;
            //transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack).SetLoops(2, LoopType.Yoyo);
            visual.Appear();
        }

        private void Remove()
        {
            octoChefBehaviorSO.gameFactorySO.ReturnEntity(this);
        }

        private bool CheckComplete()
        {
            if (_currentItemCount <= _maxItemCount * (1 - GetItemThreshold()))
            {
                return true;
            }

            // tính số grill trống
            var emptyGrillCount = _validGrills.Count(e => e.GetMagnetLayerData().layerData.itemData.All(e => (e.id <= 0 || e.id > 1000)));
            if (emptyGrillCount >= _validGrills.Count * GetGrillThreshold())
            {
                return true;
            }

            return false;
        }

        private float GetItemThreshold()
        {
            var threshold = octoChefBehaviorSO.GetItemThreshold();
            return threshold == -1f ? itemThreshold : threshold;
        }

        private float GetGrillThreshold()
        {
            var threshold = octoChefBehaviorSO.GetGrillThreshold();
            return threshold == -1f ? grillThreshold : threshold;
        }

        public void BlowTorch()
        {
            if (state <= 0) return;
            state--;

            visual.BlowTorch();
            if (state == 0)
            {
                UnlockCurrentGrill();
                OnComplete();
                return;
            }
            else
            {
                visual.SetState(maxState - state + 1);
            }
        }

        public override void Highlight(bool highlight)
        {
            base.Highlight(highlight);
            _currentGrill?.SetHighlight(highlight);
            visual.Highlight(highlight);
        }

        public override void Setup()
        {
        }

        public override void OnCreateObj(params object[] args)
        {
            visual.OnCreate();
        }

        public override void OnReturnObj()
        {
        }

        #region Interact

        private void Update()
        {
            octoChefBehaviorSO.CustomUpdate(this);
        }

        #endregion
    }
}