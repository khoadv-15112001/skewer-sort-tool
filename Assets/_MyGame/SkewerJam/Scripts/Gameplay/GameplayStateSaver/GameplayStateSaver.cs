using System;
using System.Collections.Generic;
using System.IO;
using Base.Singleton;
using Cysharp.Threading.Tasks;
using Gameplay.LevelData;
using SonatFramework.Systems.LevelManagement;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class GameplayStateSaver : Singleton<GameplayStateSaver>
    {
        // [BoxGroup("SERVICES")]
        // [Required]
        // [SerializeField]
        // private Service<LoadObjectServiceAsync> loadObjectServiceAsync = new();

        // [BoxGroup("SERVICES")]
        // [Required]
        // [SerializeField]
        // private Service<SaveObjectServiceAsync> saveObjectServiceAsync = new();

        private const string DATA_KEY = "GameplayStateSaver";

        public int Status
        {
            get => PlayerPrefs.GetInt($"{DATA_KEY}_Status", 0);
            set => PlayerPrefs.SetInt($"{DATA_KEY}_Status", value);
        }

        protected override void OnAwake()
        {
            // throw new System.NotImplementedException();
        }

        public bool CheckBackup()
        {
            return Status == 1;
        }


        public GameplayState GetCurrentState()
        {
            var state = new GameplayState();
            state.currentPumpkin = GameController.Instance.GameLogicHandler.Pumpkin;
            state.grillData = GameController.Instance.GameLogicHandler.GrillManager.GetGrillData();
            state.conveyorData = GameController.Instance.GameLogicHandler.ConveyorManager.GetConveyorData();
            state.waitingGrillIds = GameController.Instance.GameLogicHandler.WaitingGrillManager.GetWaitingGrillIds();
            state.orderInfos = GameController.Instance.GameLogicHandler.OrderManager.GetOrderItemDatas();
            state.listObstacleData = GameController.Instance.GameLogicHandler.ObstacleManager.GetObstacleData();
            state.primaryGrillIces = GameController.Instance.GameLogicHandler.GrillManager.GetIceState();
            state.obstacleStateData = GameController.Instance.GameLogicHandler.ObstacleManager.GetObstacleStateData();
            state.itemStateDatas = GameController.Instance.GameLogicHandler.GetItemStateDatas();
            return state;

        }

        public async UniTask<GameplayState> GetGameplayState()
        {
            string saveFilePath = Path.Combine(Application.persistentDataPath, $"{DATA_KEY}_CurrentState.json");
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                GameplayState data = Newtonsoft.Json.JsonConvert.DeserializeObject<GameplayState>(json, LevelService.Settings);
                Debug.Log($"Game loaded from: {saveFilePath}");
                return data;
            }
            else
            {
                Debug.LogWarning("No save file found, creating new data.");
                return new GameplayState();
            }
        }

        public async UniTask Save()
        {
            var state = GetCurrentState();
            await SaveGameplayState(state);
            Status = (int)GameplayStatus.Saved;

        }

        public async UniTask SaveGameplayState(GameplayState state)
        {
            string saveFilePath = Path.Combine(Application.persistentDataPath, $"{DATA_KEY}_CurrentState.json");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(state, Newtonsoft.Json.Formatting.Indented, LevelService.Settings);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game saved to: {saveFilePath}");
            // await saveObjectServiceAsync.Instance.SaveObject(state, $"{DATA_KEY}_CurrentState.json");
        }

        public void SetStatus(GameplayStatus saved = GameplayStatus.None)
        {
            Status = (int)saved;
        }
    }

    public enum GameplayStatus
    {
        None = 0,
        Saved = 1
    }

    public class GameplayState
    {
        public int currentPumpkin;
        // lưu trạng thái grill
        public List<GrillData> grillData;
        public List<ConveyorData> conveyorData;

        // lưu trạng thái của waiting grill
        public List<int> waitingGrillIds;

        // lưu thông tin order
        public List<(int maxNumber, int itemId, int number)> orderInfos;

        // lưu trạng thái của obstacle
        public List<ObstacleData> listObstacleData;

        // lưu trạng thái của primary grill ice
        public List<PrimaryGrillIceStateData> primaryGrillIces;

        // lưu trạng thái của obstacle
        public List<ObstacleStateData> obstacleStateData;

        // lưu trạng thái của item trong waiting grill
        public List<ItemStateData> itemStateDatas;

    }

    public class PrimaryGrillIceStateData
    {
        public int id;
        public int currentState;
        public int numStep;
    }

    public class ObstacleStateData
    {
        public int id;
        public bool active;
        public int grillId;
    }

    public class ItemStateData
    {
        public int grillId;
        public int slotIndex;
        public int id;
        public int bombCount;
        public bool active;
    }
}