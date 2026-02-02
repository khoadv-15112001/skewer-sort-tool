using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Gameplay.Entities.Grills;
using Gameplay.Entities.Items;
using Gameplay.Entities.Obstacle;
using Gameplay.LevelData;
using Helper;
using MyGame.SkewerJam.Level;
using MyGame.SkewerJam.Objects;
using Sonat.Enums;
using SonatFramework.Systems.LevelManagement;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private LevelServiceAsync levelService;
        private LevelData_SkewerJam _levelData = null;

        public LevelData_SkewerJam LevelData => _levelData;

        private int _level;

        public void Init()
        {
        }

        public void Clear()
        {
            _levelData = null;
        }

        public async UniTask GenerateLevel(int level, bool backup = false)
        {
            _level = level;
            Debug.Log("<color=green>[LevelGenerator]</color> GenerateLevel: " + level);
            _levelData = await levelService.GetLevelData<LevelData_SkewerJam>(level, GameMode.SkewerJam);
            _levelData = ValidateLevelData(_levelData);

            Debug.Log("<color=green>[LevelGenerator]</color> GenerateLevel: " + level);
            await GameController.Instance.GameLogicHandler.ItemManager.Init();

            if (backup == false)
            {
                var listWaitingGrillIds = Enumerable.Repeat(0, _levelData.numberOfWaitingGrill).ToList();
                var listOrderData = new List<(int maxNumber, int itemId, int number)>();
                await LoadGameObjects(_levelData.grillData, _levelData.conveyorData, listOrderData, _levelData.obstacleData, listWaitingGrillIds);
            }
            else
            {
                var gameplayState = await GameplayStateSaver.Instance.GetGameplayState();
                if (gameplayState.grillData == null || gameplayState.grillData.Count == 0 || CheckEmptyLevel(gameplayState.grillData))
                {
                    var listWaitingGrillIds = Enumerable.Repeat(0, _levelData.numberOfWaitingGrill).ToList();
                    var listOrderData = new List<(int maxNumber, int itemId, int number)>();
                    await LoadGameObjects(_levelData.grillData, _levelData.conveyorData, listOrderData, _levelData.obstacleData, listWaitingGrillIds);
                }
                else
                {
                    GameController.Instance.SetPumpkin(gameplayState.currentPumpkin);
                    await LoadGameObjects(
                        gameplayState.grillData,
                        gameplayState.conveyorData,
                        gameplayState.orderInfos,
                        gameplayState.listObstacleData,
                        gameplayState.waitingGrillIds,
                        gameplayState.itemStateDatas
                    );


                    var grillManager = GameController.Instance.GameLogicHandler.GrillManager;
                    grillManager.SetIceState(gameplayState.primaryGrillIces);

                    var obstacleManager = GameController.Instance.GameLogicHandler.ObstacleManager;
                    obstacleManager.SetObstacleStateData(gameplayState.obstacleStateData);
                }
            }
        }

        private bool CheckEmptyLevel(List<GrillData> grillData)
        {
            foreach (var data in grillData)
            {
                if (data.layer != null && data.layer.Count > 0)
                {
                    foreach (var layerData in data.layer)
                    {
                        if (layerData is { itemData: { Length: > 0 } })
                        {
                            foreach (var itemData in layerData.itemData)
                            {
                                if (itemData != null && itemData.id > 0) return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public async UniTask LoadGameObjects(
            List<GrillData> grillData,
            List<ConveyorData> listConveyorData,
            List<(int maxNumber, int itemId, int number)> orderData,
            List<ObstacleData> listObstacleData,
            List<int> listWaitingGrillIds,
            List<ItemStateData> itemStateDatas = null)
        {
            var gameLogicHandler = GameController.Instance.GameLogicHandler;

            // create grill
            var grillManager = gameLogicHandler.GrillManager;
            await CreateGrill(grillData, grillManager);
            await UniTask.DelayFrame(1);
            if (itemStateDatas != null)
            {
                SetItemStateDatas(itemStateDatas, grillManager);
            }

            // create waiting grill
            var waitingGrillManager = gameLogicHandler.WaitingGrillManager;
            await CreateWaitingGrill(listWaitingGrillIds, itemStateDatas, waitingGrillManager);

            await UniTask.DelayFrame(1);
            // create order
            var orderManager = gameLogicHandler.OrderManager;
            await CreateOrder(orderData, orderManager);

            await UniTask.DelayFrame(1);
            await GameController.Instance.GameViewport.CalculateViewport();

            await UniTask.DelayFrame(1);
            // create conveyor
            // tính toán viewport mới create conveyor
            var conveyorManager = gameLogicHandler.ConveyorManager;
            await CreateConveyors(listConveyorData, conveyorManager);

            await UniTask.DelayFrame(1);
            // create obstacle
            var obstacleManager = gameLogicHandler.ObstacleManager;
            await GenerateObstacles(listObstacleData, obstacleManager);
        }

        private void SetItemStateDatas(List<ItemStateData> itemStateDatas, GrillManager grillManager)
        {
            foreach (var itemStateData in itemStateDatas)
            {
                var grill = grillManager.GetGrill(itemStateData.grillId);
                if (grill != null)
                {
                    if (itemStateData.slotIndex >= grill.GetSlots().Length) continue;

                    var slot = grill.GetSlot(itemStateData.slotIndex);
                    if (slot != null)
                    {
                        var item = slot.GetItem();
                        if (item != null && item is ItemBombMove itemBombMove)
                        {
                            if (itemStateData.active == false)
                            {
                                itemBombMove.SkipBomb();
                            }
                            else
                            {
                                itemBombMove.SetBombCount(itemStateData.bombCount);
                            }
                        }
                    }
                }
            }
        }

        private LevelData_SkewerJam ValidateLevelData(LevelData_SkewerJam levelData)
        {
            var levelDataSkewerJam = levelData.CloneSkewerJam();
            levelDataSkewerJam.numberOfWaitingGrill = 5;

            // 
            // switch (levelDataSkewerJam.difficulty)
            // {
            //     case LevelDifficulty.Easy:
            //         levelDataSkewerJam.rescueCondition = new RescueCondition();
            //         levelDataSkewerJam.rescueCondition.maxNumberRescues = 5;
            //         levelDataSkewerJam.rescueCondition.maxRescueGap = 2;
            //         levelDataSkewerJam.rescueCondition.remainingWaitingGrill = 2;

            //         levelDataSkewerJam.logicOrderConfigs = new List<LogicOrderConfig>();
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 1f, minNumberSteps = 0 });
            //         break;
            //     case LevelDifficulty.Hard:
            //         levelDataSkewerJam.rescueCondition = new RescueCondition();
            //         levelDataSkewerJam.rescueCondition.maxNumberRescues = 3;
            //         levelDataSkewerJam.rescueCondition.maxRescueGap = 2;
            //         levelDataSkewerJam.rescueCondition.remainingWaitingGrill = 2;

            //         levelDataSkewerJam.logicOrderConfigs = new List<LogicOrderConfig>();
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 0.5f, minNumberSteps = 1 });
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 0.8f, minNumberSteps = 2 });
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 1f, minNumberSteps = 0 });
            //         break;
            //     case LevelDifficulty.SuperHard:
            //         levelDataSkewerJam.rescueCondition = new RescueCondition();
            //         levelDataSkewerJam.rescueCondition.maxNumberRescues = 3;
            //         levelDataSkewerJam.rescueCondition.maxRescueGap = 2;
            //         levelDataSkewerJam.rescueCondition.remainingWaitingGrill = 1;

            //         levelDataSkewerJam.logicOrderConfigs = new List<LogicOrderConfig>();
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 0.2f, minNumberSteps = 0 });
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 0.4f, minNumberSteps = 1 });
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 0.6f, minNumberSteps = 0 });
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 0.8f, minNumberSteps = 2 });
            //         levelDataSkewerJam.logicOrderConfigs.Add(new LogicOrderConfig() { region = 1f, minNumberSteps = 0 });
            //         break;
            // }

            // shuffle item
            if (_level > 40)
            {
                var itemIds = new HashSet<int>();
                foreach (var grillData in levelDataSkewerJam.grillData)
                {
                    if (grillData.layer == null) continue;
                    foreach (var layerData in grillData.layer)
                    {
                        if (layerData.itemData == null) continue;
                        foreach (var itemData in layerData.itemData)
                        {
                            if (itemData != null && itemData.id > 0)
                            {
                                itemIds.Add(itemData.id);
                            }
                        }
                    }
                }

                // shuffle item
                var dictConvertItemId = new Dictionary<int, int>();
                var shuffleItemIds = new List<int>(itemIds.ToList());
                shuffleItemIds.Shuffle();
                for (int i = 0; i < itemIds.Count; i++)
                {
                    dictConvertItemId.Add(itemIds.ToList()[i], shuffleItemIds[i]);
                }

                // set laij item id
                foreach (var grillData in levelDataSkewerJam.grillData)
                {
                    if (grillData.layer == null) continue;
                    foreach (var layerData in grillData.layer)
                    {
                        if (layerData.itemData == null) continue;
                        foreach (var itemData in layerData.itemData)
                        {
                            if (itemData != null && itemData.id > 0)
                            {
                                itemData.id = dictConvertItemId[itemData.id];
                            }
                        }
                    }
                }
            }

            return levelDataSkewerJam;
        }

        #region Create core

        private async UniTask CreateGrill(List<GrillData> listGrillData, GrillManager grillManager)
        {
            foreach (var grillData in listGrillData)
            {
                if (grillData.isLock && grillData.grillType != GrillType.Shutter) grillData.grillType = GrillType.Lock;

                var grillType = grillData.grillType.ValidateGrillType();
                var slotCount = grillData.SlotCount;

                var (validateGrillType, validateSlotCount) = ValidateGrillType(grillType, slotCount);
                if (validateGrillType == null) continue;

                PrimaryGrill grill = await GameFactory.Instance.CreateEntityAsync<PrimaryGrill>(
                    $"PrimaryGrill{validateGrillType}_{validateSlotCount}",
                    grillManager.transform
                );

                //grill.transform.SetParent(gameplaySpace);
                await grill.SetData(grillData);
                grillManager.AddGrill(grill);
            }

            CheckObstacles();
        }

        private (GrillType? validateGrillType, int validateSlotCount) ValidateGrillType(GrillType grillType, int slotCount)
        {
            switch (grillType)
            {
                case GrillType.LockAds:
                    return (null, 0);
                case GrillType.Vending:
                    return (GrillType.Normal, 1);
            }

            return (grillType, slotCount);
        }

        private async UniTask CreateWaitingGrill(List<int> listWaitingGrillIds, List<ItemStateData> itemStateDataInWaitingGrill,
            WaitingGrillManager waitingSlotManager)
        {
            await waitingSlotManager.SetData(listWaitingGrillIds, itemStateDataInWaitingGrill);
        }

        private async UniTask CreateOrder(List<(int maxNumber, int itemId, int number)> listOrderData, OrderManager orderManager)
        {
            await orderManager.SetData(listOrderData);
        }

        #endregion


        private void CheckObstacles()
        {
            LockObstacle.CheckAndStartProgress();
            PrimaryGrillIce.CheckAndStartProgress();
        }

        private async UniTask CreateConveyors(List<ConveyorData> listConveyorData, ConveyorManager conveyorManager)
        {
            if (listConveyorData == null) return;

            foreach (var conveyorData in listConveyorData)
            {
                ConveyorType conveyorType = conveyorData.conveyorType == ConveyorType.None
                    ? (conveyorData.moveType == MoveType.Horizontal ? ConveyorType.Horizontal : ConveyorType.Vertical)
                    : conveyorData.conveyorType;
                var conveyor = await GameFactory.Instance.CreateEntityAsync<ConveyorController>($"Conveyor{conveyorType}", conveyorManager.transform);
                conveyor.SetData(conveyorData);
                //conveyor.transform.SetParent(gameplaySpace);
                conveyorManager.AddConveyor(conveyor);
            }
        }

        public bool IsStaticGrill(int id)
        {
            if (_levelData.conveyorData == null) return true;
            foreach (var conveyorData in _levelData.conveyorData)
            {
                if (conveyorData.grillIds.Contains(id)) return false;
            }

            return true;
        }

        private async UniTask GenerateObstacles(List<ObstacleData> listObstacleData, ObstacleManager obstacleManager)
        {
            if (listObstacleData == null) return;
            foreach (var obstacleData in listObstacleData)
            {
                //var grill = primaryGrills.FirstOrDefault(e => e.id == grillObstacleData.grillId);
                var grillManager = GameController.Instance.GameLogicHandler.GrillManager;
                var listGrills = grillManager.ListGrills;
                var grills = listGrills.FindAll(e => obstacleData.grillIds.Contains(e.id));
                if (grills.Count == 0)
                {
                    Debug.LogWarning($"Grill not found");
                    continue;
                }

                var obstacle = await GameFactory.Instance.CreateEntityAsync<ObstacleBase>($"Obstacle{obstacleData.obstacleType}", obstacleManager.transform);
                List<GrillBase> grillsSelected = new List<GrillBase>(grills);
                obstacle.SetData(obstacleData);
                obstacle.SetGrill(grillsSelected);
                obstacleManager.AddObstacle(obstacle);
            }
        }
    }
}