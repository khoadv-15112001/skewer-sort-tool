using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.BoosteeManagement;
using Gameplay.Entities;
using Gameplay.Entities.Grills;
using Gameplay.Entities.Items;
using Gameplay.Entities.ItemScripts;
using Gameplay.Entities.Obstacle;
using Gameplay.Entities.Orders;
using Gameplay.GameplayElement;
using Gameplay.LevelData;
using Gameplay.SceneManager;
using GrillSort.ConsecutiveWin;
using GrillSort.QuestEvent;
using Helper;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.LevelManagement;
using SonatFramework.Systems.ObjectPooling;
using SonatFramework.Systems.SettingsManagement.Vibation;
using UnityEngine;
using LevelData = Gameplay.LevelData.LevelData;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    //[SerializeField] private SonatFramework.Systems.LevelManagement.LevelService levelService;
    private int level = 1;

    [SerializeField] private OrderQueue orderQueue;

    [SerializeField] private Transform queueRefPoint;
    [SerializeField] private Transform topRefPoint;
    [SerializeField] private Transform bottomRefPoint;
    [SerializeField] private Transform centerRefPoint;
    [SerializeField] private Transform centerRefPoint2;
    [SerializeField] private Transform gameplaySpace;
    [SerializeField] private Transform dropMask;
    private Bounds perfectBounds;
    public GameViewport gameViewport { get; private set; }
    private List<PrimaryGrill> primaryGrills = new();
    private List<ConveyorController> conveyors = new();
    private List<DropColumn> dropColumns = new();

    private List<ObstacleBase> obstacles = new();
    private float perfectCamSize;
    private Vector2 gridSize = new Vector2(3f, 3f);
    private LevelData levelData;
    public LevelData LevelData => levelData;
    public LevelMode LevelMode => levelData.levelMode;
    
    public int MaxOrder { get; private set; }

    public static int MAXITEMS;

    public LayerMultipleData layerMultipleData;

    //private bool moreItem;
    private Vector3 orgCameraPosition;
    private Vector3 topPosition, bottomPosition, centerPosition;
    public static LevelService levelService;
    public static int maxGrillSlot = 3;
    public static bool dropMode = false;
    private float minCamSize;
    public static float maxYGameSpace;

    public Transform GameplaySpace => gameplaySpace;
    public static ShuffleItemIds shuffleItemIds = new();
    //public Action<Item> OnClickItem;

    private void Awake()
    {
        if (levelService == null) levelService = SonatSystem.GetService<LevelRemoteService>();
        MAXITEMS = (int)ItemId.FINISH_NORMAL - 1;
    }


    // Start is called before the first frame update
    void Start()
    {
        //SonatUtils.DelayCall(0.1f, () =>
        //{

        //});
    }

    public void Init()
    {
        Canvas.ForceUpdateCanvases();
        orgCameraPosition = GameplayController.instance.mainCamera.transform.position;
        topPosition = topRefPoint.position;
        maxYGameSpace = topPosition.y + 1;
        bottomPosition = bottomRefPoint.position;
        //centerPosition = centerRefPoint.position;
        centerPosition = (topPosition + bottomPosition) / 2f; // new Vector3(0, (topPosition.y + bottomPosition.y) / 2f, 0);
        centerRefPoint.position = centerPosition;
        gameViewport = GameplayController.instance.gameViewport;
        Vector3 boundSize = gameViewport.gameViewportBounds.size;
        //boundSize.y -= ((gameViewport.maxY - topRefPoint.position.y) + (bottomPosition.y - gameViewport.minY));
        boundSize.y = topRefPoint.position.y - bottomRefPoint.position.y;
        boundSize.x -= 0.55f;
        Vector3 center = gameViewport.gameViewportBounds.center;
        center.y = (topPosition.y + bottomPosition.y) / 2;
        perfectBounds = new Bounds(center, boundSize);
        perfectCamSize = GameplayController.instance.mainCamera.orthographicSize;

        string json = Resources.Load<TextAsset>("LayerMultipleData").text;
        layerMultipleData = JsonUtility.FromJson<LayerMultipleData>(json);
        // moreItem = SonatFirebase.remote.GetRemoteBool("more_item");

        float screenRatio = Screen.width / (float)Screen.height;
        if (screenRatio < 9f / 16)
        {
            minCamSize = 12f;
        }
        else
        {
            minCamSize = 10f;
        }
    }

    public async UniTask GenerateLevel(int level, bool isReplay)
    {
        this.level = level;
        // primaryGrills.Clear();
        // conveyors.Clear();
        // obstacles.Clear();
        ClearLevel();

        int levelCategory = 0;
        if (level is >= 6 and <= 8)
        {
            int startCount = MySonatFramework.gameplayAnalyticsService.CheckStartCount(level);
            levelCategory = GameRemoteConfigValue.levelReplayData.GetCategory(level, startCount);
        }

        MySonatFramework.customTrackingService.category = levelCategory;
        levelData = levelService.GetLevelData<LevelData>(level, GameMode.Classic, category: levelCategory).Clone();
        // if (GameRemoteConfigValue.shuffleItemIds && isReplay)
        // {
        //     shuffleItemIds.SetData(levelData);
        // }

        //Debug.Log($"[ConsecutiveWinService] Generate Level {level} - time before = {levelData.time}");
        var conservativeWinService = MySonatFramework.GetService<ConsecutiveWinService>();
        var removeTime = conservativeWinService?.Config?.GetTimeToRemove(level) ?? 0;
        levelData.time = (ushort)Mathf.Max(0, levelData.time - removeTime);
        //Debug.Log($"[ConsecutiveWinService] Generate Level {level} - time after = {levelData.time}");

        ValidateLevelData(levelData);
        if (isReplay)
            ShuffleGrills(levelData);

        // if (moreItem)
        // {
        //     int loop = layerMultipleData.multiple[(level - 1) % 100];
        //     levelData.LoopLayer(loop - 1);
        // }

        GameplayController.instance.levelType = levelData.levelType;
        GameplayController.instance.timeManager.SetTimeRemaining(levelData.time);
        maxGrillSlot = 0;
        int itemCount = 0;

        foreach (var grillData in levelData.grillData)
        {
            if (grillData.isLock && grillData.grillType != GrillType.Shutter) grillData.grillType = GrillType.Lock;

#if UNITY_ANDROID
            if (!LocalizationUtils.IsJapanese() && level is >= 6 and <= 100 && grillData.grillType == GrillType.Lock)
            {
                grillData.grillType = GrillType.Normal;
            }
#endif
            PrimaryGrill grill =
                GameFactory.CreateEntity<PrimaryGrill>($"PrimaryGrill{grillData.grillType.ValidateGrillType()}_{grillData.SlotCount}", gameplaySpace);

            //grill.transform.SetParent(gameplaySpace);
            grill.SetData(grillData);
            primaryGrills.Add(grill);

            if (grillData.layer == null) continue;
            foreach (var layerData in grillData.layer)
            {
                foreach (var itemData in layerData.itemData)
                {
                    if (itemData != null && itemData.itemType != ItemType.Obstacle && itemData.id > 0)
                    {
                        itemCount++;
                    }
                }

                if (layerData.itemData != null && layerData.itemData.Length > maxGrillSlot) maxGrillSlot = layerData.itemData.Length;
            }
        }

        MaxOrder = itemCount / maxGrillSlot;
        dropMode = levelData.isDropMode || maxGrillSlot >= 5;
        if (dropMode)
        {
            //PrimaryGrillDrop5.InitializeDrop();
            CreateDropMode();
        }

        await CalculateViewport();
        CreateConveyors();
        GenerateGrillObstacles();
        CheckObstacles();
        GenerateSpecialItems();

        SonatUtils.ExecuteNextFrame(() =>
        {
            Vector3 queuePos = queueRefPoint.position;
            queuePos.z = 0;
            orderQueue.transform.position = queuePos;
            orderQueue.InitData(MaxOrder, levelData.orderData, GameplayController.instance.mainCamera.orthographicSize);
            SetupDropMode();
        });
    }

    private void GenerateSpecialItems()
    {
        QuestEventService questEventService = MySonatFramework.GetService<QuestEventService>();
        if (!questEventService.CheckEventActive() || questEventService.CheckCompleteAllQuest()) return;
        int specialItemId = questEventService.GetCurrentItemId();
        List<PrimaryGrill> grillsSelected = RandomHelper.GetRandomElemntsInList(primaryGrills, primaryGrills.Count);
        int count = 0;
        foreach (var primaryGrill in grillsSelected)
        {
            if (primaryGrill.CreateSpecialItem(specialItemId))
            {
                count++;
                if (count >= GameRemoteConfigValue.numberSpecialItemPerLevel) return;
            }
        }
    }

    private void CreateConveyors()
    {
        if (levelData.conveyorData == null) return;
        foreach (var conveyorData in levelData.conveyorData)
        {
            ConveyorType conveyorType = conveyorData.conveyorType == ConveyorType.None
                ? (conveyorData.moveType == MoveType.Horizontal ? ConveyorType.Horizontal : ConveyorType.Vertical)
                : conveyorData.conveyorType;
            var conveyor = GameFactory.CreateEntity<ConveyorController>($"Conveyor{conveyorType}", gameplaySpace);
            conveyor.SetData(conveyorData);
            //conveyor.transform.SetParent(gameplaySpace);
            conveyors.Add(conveyor);
        }
    }

    private void CreateDropMode()
    {
        foreach (var primaryGrill in primaryGrills)
        {
            if (!IsStaticGrill(primaryGrill.id)) continue;
            TryAddGrillToDropColumn(primaryGrill);
        }

        foreach (var dropColumn in dropColumns)
        {
            dropColumn.InitializeDrop();
        }
    }

    private void TryAddGrillToDropColumn(PrimaryGrill grill)
    {
        foreach (var dropColumn in dropColumns)
        {
            if (dropColumn.ValidateGrill(grill))
            {
                dropColumn.AddGrill(grill);
                return;
            }
        }

        DropColumn newDropColumn = GameFactory.CreateEntity<DropColumn>("DropColumn", gameplaySpace);
        dropColumns.Add(newDropColumn);
        newDropColumn.AddGrill(grill);
    }

    private void CheckObstacles()
    {
        LockObstacle.CheckAndStartProgress();
        PrimaryGrillIce.CheckAndStartProgress();
        PrimaryGrillIceNeighbor.CheckAndStartProgress();
    }

    private void GenerateGrillObstacles()
    {
        if (levelData.obstacleData == null) return;
        foreach (var obstacleData in levelData.obstacleData)
        {
            //var grill = primaryGrills.FirstOrDefault(e => e.id == grillObstacleData.grillId);
            var grills = primaryGrills.FindAll(e => obstacleData.grillIds.Contains(e.id));
            if (grills.Count == 0)
            {
                Debug.LogWarning($"Grill not found");
                continue;
            }

            var obstacle = GameFactory.CreateEntity<ObstacleBase>($"Obstacle{obstacleData.obstacleType}", gameplaySpace);
            List<GrillBase> grillsSelected = new List<GrillBase>(grills);
            obstacle.SetData(obstacleData);
            obstacle.SetGrill(grillsSelected);
            obstacles.Add(obstacle);
        }
    }

    private void SetupDropMode()
    {
        if (dropMode)
        {
            dropMask.gameObject.SetActive(true);
            dropMask.position = queueRefPoint.position;
        }
        else
        {
            dropMask.gameObject.SetActive(false);
        }
    }

    // private void InitLockObstacles()
    // {
    //     if (LockObstacle.lockObstacles != null && LockObstacle.lockObstacles.Count > 0)
    //     {
    //         LockObstacle.lockObstacles.Sort((a, b) => a.pr);
    //     }
    // }

    private void ValidateLevelData(LevelData levelData)
    {
        if (levelData.time < 10) levelData.time = 180;
        List<int> idsTrueInLevel = new();
        List<int> idsFalseInLevel = new();

        bool shuffleIds = GameRemoteConfigValue.shuffleItemIds;

        foreach (var grill in levelData.grillData)
        {
            if (grill.layer == null) continue;
            foreach (var layerData in grill.layer)
            {
                for (int i = 0; i < layerData.itemData.Length; i++)
                {
                    if (layerData.itemData[i] != null && layerData.itemData[i].id > 0)
                    {
                        if (shuffleIds)
                        {
                            layerData.itemData[i].id = shuffleItemIds.GetItemId(layerData.itemData[i].id);
                        }

                        if (layerData.itemData[i].id <= MAXITEMS && !idsTrueInLevel.Contains(layerData.itemData[i].id))
                            idsTrueInLevel.Add(layerData.itemData[i].id);
                        else if (layerData.itemData[i].id > MAXITEMS && layerData.itemData[i].id != 1500 && !idsFalseInLevel.Contains(layerData.itemData[i].id))
                        {
                            idsFalseInLevel.Add(layerData.itemData[i].id);
                        }

                        if (layerData.itemData[i].hidden) layerData.itemData[i].itemType = ItemType.Hidden;
                    }
                }
            }
        }

        if (idsFalseInLevel.Count == 0) return;

        List<int> otherIds = Enumerable.Range(1, MAXITEMS).Where(e => !idsTrueInLevel.Contains(e)).ToList<int>();
        Dictionary<int, int> mapNewIds = new();
        foreach (var falseId in idsFalseInLevel)
        {
            int randomId = otherIds.Count == 0 ? Random.Range(1, MAXITEMS) : otherIds[Random.Range(0, otherIds.Count)];
            mapNewIds.Add(falseId, randomId);
            if (otherIds.Count > 0) otherIds.Remove(randomId);
        }

        foreach (var grill in levelData.grillData)
        {
            foreach (var layerData in grill.layer)
            {
                foreach (var itemData in layerData.itemData)
                {
                    if (itemData != null && itemData.id > MAXITEMS && itemData.id != 1500)
                    {
                        Debug.Log($"Convert {itemData.id} to {mapNewIds[itemData.id]}");
                        itemData.id = mapNewIds[itemData.id];
                    }
                }
            }
        }
    }

    public void ShuffleGrills(LevelData levelData)
    {
        if (!GameRemoteConfigValue.shuffleGrills) return;
        Dictionary<int, List<GrillData>> grillGroup = new();
        foreach (var grillData in levelData.grillData)
        {
            // Skip IceMain and IceNeighbor grills from shuffling
            if (grillData.grillType == GrillType.IceMain || grillData.grillType == GrillType.IceNeighbor)
                continue;

            if (grillData != null && IsStaticGrill(grillData.id) && IsFreeGrill(grillData.id))
            {
                int slotCount = grillData.SlotCount;
                if (slotCount > 3) continue;
                if (HasSubGrill(grillData)) slotCount += 100;
                if (!grillGroup.ContainsKey(slotCount)) grillGroup.Add(slotCount, new List<GrillData>());
                grillGroup[slotCount].Add(grillData);
            }
        }

        foreach (var group in grillGroup.Values)
        {
            List<Vector3Data> grillPositions = new();
            foreach (var grillData in group)
            {
                grillPositions.Add(grillData.position);
            }

            grillPositions.Shuffle();
            for (int i = 0; i < group.Count; i++)
            {
                group[i].position = grillPositions[i];
            }
        }
    }


    public bool IsStaticGrill(int id)
    {
        if (levelData.conveyorData == null) return true;
        foreach (var conveyorData in levelData.conveyorData)
        {
            if (conveyorData.grillIds.Contains(id)) return false;
        }

        return true;
    }

    public bool IsFreeGrill(int id)
    {
        if (levelData.obstacleData == null) return true;
        foreach (var obstacleData in levelData.obstacleData)
        {
            if (obstacleData.obstacleType is ObstacleType.LockAreaHorizontal or ObstacleType.LockAreaVertical &&
                obstacleData.grillIds.Contains(id)) return false;
        }

        return true;
    }

    private Bounds currentBound;

    private async UniTask CalculateViewport()
    {
        currentBound = new Bounds();

        foreach (var grill in primaryGrills)
        {
            if (IsStaticGrill(grill.id) && grill.transform.position.y < 12)
            {
                currentBound.Encapsulate(grill.GetBounds());
                if (grill.HasSubGrills())
                {
                    currentBound.Encapsulate(grill.GetSubGrillPosition());
                }
                else if (dropMode)
                {
                    currentBound.Encapsulate(grill.transform.position + Vector3.down * 0.5f);
                    currentBound.Encapsulate(grill.transform.position + Vector3.up * 1.5f);
                }
            }
        }

        if (levelData.conveyorData != null)
        {
            foreach (var conveyorData in levelData.conveyorData)
            {
                Vector3 conveyorPos = conveyorData.position.ToVector3();
                Vector3 conveyorSize = new Vector3(3f, 4f, 0);
                if (conveyorData.moveType == MoveType.Vertical) conveyorSize.y = 19f;
                Bounds conveyorBounds = new Bounds(conveyorPos, conveyorSize);
                currentBound.Encapsulate(conveyorBounds);
            }
        }

        GameplayController.instance.mainCamera.transform.position = orgCameraPosition;

        float widthScale = currentBound.size.x / perfectBounds.size.x;
        float heightScale = currentBound.size.y / perfectBounds.size.y;
        float scale = Mathf.Max(widthScale, heightScale);
        GameplayController.instance.mainCamera.orthographicSize = Mathf.Clamp(perfectCamSize * scale, minCamSize, 17.5f);

        Canvas.ForceUpdateCanvases();

        await UniTask.DelayFrame(1);
        gameViewport.CalculateGameViewportBounds();

        topPosition = topRefPoint.position;
        maxYGameSpace = topPosition.y + 1;
        bottomPosition = bottomRefPoint.position;
        centerPosition = new Vector3(0, (topPosition.y + bottomPosition.y) / 2f, 0);
        centerRefPoint.position = centerPosition;

        Vector3 offset = centerPosition - currentBound.center;
        offset.z = 0;
        Vector3 newPos = orgCameraPosition - offset;
        GameplayController.instance.mainCamera.transform.position = newPos;

        Canvas.ForceUpdateCanvases();
    }

    public OrderQueue GetOrderQueue()
    {
        return orderQueue;
    }

    public void ClearLevel()
    {
        foreach (var obstacle in obstacles)
        {
            GameFactory.ReturnEntity(obstacle);
        }

        foreach (var grill in primaryGrills)
        {
            GameFactory.ReturnEntity(grill);
        }

        foreach (var conveyor in conveyors)
        {
            GameFactory.ReturnEntity(conveyor);
        }

        foreach (var dropColumn in dropColumns)
        {
            GameFactory.ReturnEntity(dropColumn);
        }


        LockObstacle.lockObstacles = null;
        PrimaryGrillIce.primaryGrillIces = null;
        primaryGrills.Clear();
        conveyors.Clear();
        dropColumns.Clear();
        obstacles.Clear();
        orderQueue.ClearOrders();
        isUseBooster = false;
        shuffleItemIds.Clear();
    }

    public PrimaryGrill GetPrimaryGrill(int id)
    {
        return primaryGrills.FirstOrDefault(e => e.id == id);
    }

    public void ReturnGrill(PrimaryGrill grill)
    {
        primaryGrills.Remove(grill);
        GameFactory.ReturnEntity(grill);
    }

    #region Boosters

    #region Shuffle

    public async UniTask Shuffle()
    {
        List<ShuffleLayerData> primaryShuffleLayerDatas = new();
        List<ShuffleLayerData> canNotShuffle = new();
        List<ShuffleLayerData> subShuffleLayerDatas = new();
        Dictionary<int, List<ItemData>> idCount = new();
        int primarySlot = 0;
        int subSlot = 0;

        foreach (var primaryGrill in primaryGrills)
        {
            var primaryShuffleLayer = primaryGrill.GetShuffleLayerData();
            if (primaryShuffleLayer != null)
            {
                if (primaryShuffleLayer.canNotShuffle)
                {
                    canNotShuffle.Add(primaryShuffleLayer);
                }
                else
                {
                    primaryShuffleLayerDatas.Add(primaryShuffleLayer);
                    primarySlot += primaryGrill.SlotCount;
                }
            }

            var subShuffleLayers = primaryGrill.GetSubsShuffleLayerData();
            if (subShuffleLayers != null)
            {
                foreach (var subShuffleLayer in subShuffleLayers)
                {
                    if (subShuffleLayer.canNotShuffle)
                    {
                        canNotShuffle.Add(subShuffleLayer);
                    }
                    else
                    {
                        subShuffleLayerDatas.Add(subShuffleLayer);
                        subSlot += primaryGrill.SlotCount;
                    }
                }
                // subShuffleLayerDatas.AddRange(subShuffleLayers);
                // int subGrillCount = primaryGrill.SubGrillsCount();
                // subSlot += (subGrillCount * primaryGrill.SlotCount);
            }
        }

        // Dictionary to track Ice items and their original positions
        Dictionary<ShuffleLayerData, Dictionary<int, ItemData>> iceItemPositions = new();
        int iceItemsInPrimary = 0;
        int iceItemsInSub = 0;

        idCount.Add(0, new List<ItemData>());
        for (int i = 0; i < 2; i++)
        {
            var data = i == 0 ? primaryShuffleLayerDatas : subShuffleLayerDatas;
            foreach (var shuffleLayerData in data)
            {
                for (int j = 0; j < shuffleLayerData.layerData.itemData.Length; j++)
                {
                    var item = shuffleLayerData.layerData.itemData[j];
                    if (item == null) item = new ItemData();

                    // Skip Ice items from shuffling - store their positions
                    if (item.itemType == ItemType.Ice)
                    {
                        if (!iceItemPositions.ContainsKey(shuffleLayerData))
                        {
                            iceItemPositions[shuffleLayerData] = new Dictionary<int, ItemData>();
                        }

                        iceItemPositions[shuffleLayerData][j] = item;

                        // Track ice items count to adjust available slots
                        if (i == 0) iceItemsInPrimary++;
                        else iceItemsInSub++;

                        continue;
                    }

                    if (!idCount.TryAdd(item.id, new List<ItemData>() { item }))
                    {
                        idCount[item.id].Add(item);
                    }
                }
            }
        }

        // Adjust slot counts to account for Ice items
        primarySlot -= iceItemsInPrimary;
        subSlot -= iceItemsInSub;

        //allItems.Shuffle();
        int typeItem = Mathf.Min(GameRemoteConfigValue.numberSpecialItemPerLevel, primaryGrills.Count, idCount.Count - 1);
        var idsShuffled = ShuffleLayerData(idCount, primarySlot, subSlot, typeItem);

        foreach (var shuffleLayerData in primaryShuffleLayerDatas)
        {
            int numSlot = shuffleLayerData.grill.SlotCount;
            LayerData layerData = new LayerData(numSlot);
            for (int i = 0; i < numSlot; i++)
            {
                // Check if there's an Ice item at this position
                if (iceItemPositions.ContainsKey(shuffleLayerData) &&
                    iceItemPositions[shuffleLayerData].ContainsKey(i))
                {
                    layerData.itemData[i] = iceItemPositions[shuffleLayerData][i];
                }
                else
                {
                    var item = new ItemData();
                    if (idsShuffled.Item1.Count > 0)
                    {
                        item = idsShuffled.Item1[0];
                        idsShuffled.Item1.RemoveAt(0);
                    }

                    layerData.itemData[i] = item;
                }
            }

            shuffleLayerData.grill.SetShuffleLayerData(layerData);
        }

        foreach (var shuffleLayerData in canNotShuffle)
        {
            shuffleLayerData.grill.SetShuffleLayerData(shuffleLayerData.layerData);
        }

        foreach (var shuffleLayerData in subShuffleLayerDatas)
        {
            int numSlot = shuffleLayerData.grill.SlotCount;
            LayerData layerData = new LayerData(numSlot);
            for (int i = 0; i < numSlot; i++)
            {
                // Check if there's an Ice item at this position
                if (iceItemPositions.ContainsKey(shuffleLayerData) &&
                    iceItemPositions[shuffleLayerData].ContainsKey(i))
                {
                    layerData.itemData[i] = iceItemPositions[shuffleLayerData][i];
                }
                else
                {
                    var item = new ItemData();
                    if (idsShuffled.Item2.Count > 0)
                    {
                        item = idsShuffled.Item2[0];
                        idsShuffled.Item2.RemoveAt(0);
                    }

                    layerData.itemData[i] = item;
                }
            }

            shuffleLayerData.grill.SetShuffleLayerData(layerData);
        }

        MySonatFramework.GetService<VibrationService>().Vibrate(50);
        await UniTask.Delay(2000);
#if UNITY_EDITOR
        CheckIdMatch3();
#endif
    }

    private (List<ItemData>, List<ItemData>) ShuffleLayerData(Dictionary<int, List<ItemData>> idCount, int primarySlot, int subSlot, int typeItem)
    {
        List<ItemData> primaryIds = new();
        List<ItemData> subIds = new();

        for (int i = 0; i < typeItem; i++)
        {
            if (idCount.Count == 0) break;
            var id = idCount.Keys.ElementAt(Random.Range(1, idCount.Count));
            for (int j = 0; j < 3; j++)
            {
                // Don't add wrapped items to primary layer
                if (idCount[id][0].itemType == ItemType.Wrapped)
                {
                    subIds.Add(idCount[id][0]);
                }
                else
                {
                    primaryIds.Add(idCount[id][0]);
                }

                idCount[id].RemoveAt(0);
                if (idCount[id].Count == 0)
                {
                    idCount.Remove(id);
                    break;
                }
            }
        }

        foreach (var ids in idCount)
        {
            for (int i = 0; i < ids.Value.Count; i++)
            {
                int r = Random.Range(0, 3);
                // Always put wrapped items in sub grills
                if (ids.Value[i].itemType == ItemType.Wrapped)
                {
                    subIds.Add(ids.Value[i]);
                }
                else if (r == 0 && primaryIds.Count < primarySlot - 3)
                {
                    primaryIds.Add(ids.Value[i]);
                }
                else
                {
                    subIds.Add(ids.Value[i]);
                }
            }
        }

        if (subSlot < subIds.Count)
        {
            int overflow = subIds.Count - subSlot;
            List<ItemData> itemsToMove = new();

            // Move non-wrapped items from subIds to primaryIds if there's overflow
            for (int i = subIds.Count - 1; i >= 0 && itemsToMove.Count < overflow; i--)
            {
                if (subIds[i].itemType != ItemType.Wrapped)
                {
                    itemsToMove.Add(subIds[i]);
                    subIds.RemoveAt(i);
                }
            }

            primaryIds.AddRange(itemsToMove);

            // If we still have overflow (all remaining items are wrapped), remove the excess wrapped items
            if (subIds.Count > subSlot)
            {
                subIds.RemoveRange(0, subIds.Count - subSlot);
            }
        }

        primaryIds.Shuffle();
        subIds.Shuffle();

        return (primaryIds, subIds);
    }

    public List<PrimaryGrill> GetPrimaryGrills()
    {
        return primaryGrills;
    }

    public bool CheckOutOfMove()
    {
        int numberSlotEmpty = 0;
        for (int i = 0; i < primaryGrills.Count; i++)
        {
            if (dropMode && !primaryGrills[i].isInGameplaySpace) continue;
            numberSlotEmpty += primaryGrills[i].NumberSlotEmpty();
            if (numberSlotEmpty >= maxGrillSlot) return false;
        }

        if (numberSlotEmpty == 0) return true;
        Dictionary<int, int> itemCount = new();

        for (int i = 0; i < primaryGrills.Count; i++)
        {
            if (primaryGrills[i].IsLock || (dropMode && !primaryGrills[i].isInGameplaySpace)) continue;
            var slots = primaryGrills[i].GetSlots();
            for (int j = 0; j < slots.Length; j++)
            {
                Item item = slots[j].GetItem();
                if (item != null && item.id > 0)
                {
                    if (!itemCount.TryAdd(item.id, 1))
                    {
                        itemCount[item.id]++;
                        if (itemCount[item.id] >= maxGrillSlot) return false;
                    }
                }
            }
        }

        return true;
    }

    public bool CheckHasMatch()
    {
        Dictionary<int, int> itemCount = new();
        for (int i = 0; i < primaryGrills.Count; i++)
        {
            if (primaryGrills[i].IsLock || (dropMode && !primaryGrills[i].isInGameplaySpace)) continue;
            var slots = primaryGrills[i].GetSlots();
            for (int j = 0; j < slots.Length; j++)
            {
                Item item = slots[j].GetItem();
                if (item != null && item.id > 0)
                {
                    if (!itemCount.TryAdd(item.id, 1))
                    {
                        itemCount[item.id]++;
                        if (itemCount[item.id] >= maxGrillSlot) return true;
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region Magnet

    public async UniTask Magnet()
    {
        List<ShuffleLayerData> primaryShuffleLayerDatas = new();

        Dictionary<int, int> idsScore = new();

        foreach (var primaryGrill in primaryGrills)
        {
            var shuffleLayerData = primaryGrill.GetMagnetLayerData();
            if (shuffleLayerData == null) continue;
            primaryShuffleLayerDatas.Add(shuffleLayerData);
            if (dropMode && !primaryGrill.isInGameplaySpace) continue;
            foreach (var item in shuffleLayerData.layerData.itemData)
            {
                if (item is not { id: > 0 and < 1000 }) continue;

                int score = 1;
                if (primaryGrill.SlotCount > 1) score += 100;
                if (!primaryGrill.IsLock) score += 1000;
                if (primaryGrill.NumberDifferentItems(item.id) > 0) score += 10;

                //int score = primaryGrill.SlotCount > 1 && !primaryGrill.IsLock ? 100 : 1;
                if (!idsScore.TryAdd(item.id, score))
                {
                    idsScore[item.id] += score;
                }
            }
        }


        var itemSelected = idsScore.OrderBy(e => e.Value).Last();

        await Match3(itemSelected.Key, primaryShuffleLayerDatas);
    }

    private async UniTask Match3(int itemId, List<ShuffleLayerData> primaryShuffleLayerDatas)
    {
        int itemSelectedCount = 0;
        Vector3[] positions = new Vector3[maxGrillSlot];

        foreach (var shuffleLayerData in primaryShuffleLayerDatas)
        {
            foreach (var item in shuffleLayerData.layerData.itemData)
            {
                if (item != null && item.id == itemId)
                {
                    positions[itemSelectedCount] = shuffleLayerData.grill.DestroyItem(itemId);
                    itemSelectedCount++;
                    if (itemSelectedCount == positions.Length) break;
                }
            }

            if (itemSelectedCount == positions.Length) break;
        }

        if (itemSelectedCount < positions.Length)
        {
            foreach (var primaryGrill in primaryGrills)
            {
                var subLayers = primaryGrill.GetSubsMagnetLayerData();
                if (subLayers != null)
                    foreach (var subLayer in subLayers)
                    {
                        foreach (var item in subLayer.layerData.itemData)
                        {
                            if (item != null && item.id == itemId)
                            {
                                positions[itemSelectedCount] = subLayer.grill.DestroyItem(itemId);
                                itemSelectedCount++;
                                if (itemSelectedCount == positions.Length) break;
                            }
                        }

                        if (itemSelectedCount == positions.Length) break;
                    }

                if (itemSelectedCount == positions.Length) break;
            }
        }

        OrderEntity orderEntity = GetOrderQueue().PopOrderEntity(itemId, null);
        var effect = SonatSystem.GetService<PoolingService>().Create<CollectEffect>("CollectEffectMagnet", Vector3.zero);
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = effect.Container.InverseTransformPoint(positions[i]);
        }

        effect.SetData(positions, itemId, null, orderEntity, 0.4f);

        MySonatFramework.audioService.PlaySound(AudioId.Items_Merge);
        MySonatFramework.GetService<VibrationService>().Vibrate(50);

        await UniTask.Delay(100);
        foreach (var primaryGrill in primaryGrills)
        {
            primaryGrill.CheckSubGrills(false);
        }

        await UniTask.Delay(50);
        foreach (var primaryGrill in primaryGrills)
        {
            primaryGrill.CheckEmpty();
        }

        GameplayController.instance.CollectItem(itemId, Vector3.zero, maxGrillSlot, false);
        await UniTask.Delay(500);
    }

    public Dictionary<int, int> GetItemsWithLayer(bool spicy)
    {
        Dictionary<int, (int, int)> idCount = new();

        foreach (var primaryGrill in primaryGrills)
        {
            int layer = 1;
            var shuffleLayerData = primaryGrill.GetShuffleLayerData();
            if (shuffleLayerData?.layerData?.itemData == null) continue;
            foreach (var itemData in shuffleLayerData.layerData.itemData)
            {
                if (itemData is not { id: > 0 }) continue;
                switch (spicy)
                {
                    case true when itemData.itemType == ItemType.Ice && primaryGrill.grillType != GrillType.Spicy:
                    case false when itemData.itemType == ItemType.Ice && primaryGrill.grillType == GrillType.Spicy:
                        continue;
                }

                if (!idCount.TryAdd(itemData.id, (1, 1)))
                {
                    int count = idCount[itemData.id].Item2;
                    count++;
                    int layerMax = Mathf.Max(idCount[itemData.id].Item1, layer);
                    idCount[itemData.id] = (layerMax, count);
                }
            }
        }

        List<List<ShuffleLayerData>> listShuffleLayerData = new List<List<ShuffleLayerData>>();
        int maxLayer = 0;
        foreach (var primaryGrill in primaryGrills)
        {
            var subLayerData = primaryGrill.GetSubsShuffleLayerData();
            listShuffleLayerData.Add(subLayerData);
            if (subLayerData?.Count > maxLayer) maxLayer = subLayerData.Count;
        }

        for (int i = 0; i < maxLayer; i++)
        {
            foreach (var subLayerData in listShuffleLayerData)
            {
                if (subLayerData == null || subLayerData.Count <= i) continue;
                var subLayer = subLayerData[i];
                if (subLayer?.layerData?.itemData == null) continue;
                int layer = i + 2;
                foreach (var itemData in subLayer.layerData.itemData)
                {
                    if (itemData is not { id: > 0 }) continue;
                    switch (spicy)
                    {
                        case true when itemData.itemType == ItemType.Ice && subLayer.grill.grillType != GrillType.Spicy:
                        case false when itemData.itemType == ItemType.Ice && subLayer.grill.grillType == GrillType.Spicy:
                            continue;
                    }

                    if (!idCount.TryAdd(itemData.id, (1, 1)))
                    {
                        int count = idCount[itemData.id].Item2;
                        count++;
                        int layerMax = count > maxGrillSlot ? idCount[itemData.id].Item1 : Mathf.Max(idCount[itemData.id].Item1, layer);
                        idCount[itemData.id] = (layerMax, count);
                    }
                }
            }
        }
        //var subLayerData = primaryGrill.GetSubsShuffleLayerData();
        // if (subLayerData?.Count == 0) continue;
        // foreach (var subLayer in subLayerData)
        // {
        //     if (subLayer?.layerData?.itemData == null) continue;
        //     layer++;
        //     foreach (var itemData in subLayer.layerData.itemData)
        //     {
        //         if (itemData is not { id: > 0 }) continue;
        //         switch (spicy)
        //         {
        //             case true when itemData.itemType == ItemType.Ice && primaryGrill.grillType != GrillType.Spicy:
        //             case false when itemData.itemType == ItemType.Ice && primaryGrill.grillType == GrillType.Spicy:
        //                 continue;
        //         }
        //         if (!idCount.TryAdd(itemData.id, (1, 1)))
        //         {
        //             int count = idCount[itemData.id].Item2;
        //             count++;
        //             int layerMax = count > maxGrillSlot ? idCount[itemData.id].Item1 : Mathf.Max(idCount[itemData.id].Item1, layer);
        //             idCount[itemData.id] = (layerMax, count);
        //         }
        //     }
        // }
        //}

        //shuffleLayer.Shuffle();
        // foreach (var primaryGrill in primaryGrills)
        // {
        //     var layerData = primaryGrill.GetSubsShuffleLayerData();
        //     if (layerData == null) continue;
        //     shuffleLayer.AddRange(layerData);
        // }
        //
        // foreach (var shuffleLayerData in shuffleLayer)
        // {
        //     if (shuffleLayerData.layerData == null) continue;
        //     foreach (var itemData in shuffleLayerData.layerData.itemData)
        //     {
        //         if (itemData is not { id: > 0 }) continue;
        //         if (idCount.TryAdd(itemData.id, 1))
        //         {
        //             idCount[itemData.id]++;
        //         }
        //     }
        // }

        var sortedIds = idCount.OrderByDescending(e => e.Value.Item2).ToDictionary(e => e.Key, e => e.Value.Item1);
        return sortedIds;
    }


    private void CheckIdMatch3()
    {
        List<ShuffleLayerData> primaryShuffleLayerDatas = new();
        List<ShuffleLayerData> subShuffleLayerDatas = new();
        Dictionary<int, int> idCount = new();
        foreach (var primaryGrill in primaryGrills)
        {
            primaryShuffleLayerDatas.Add(primaryGrill.GetMagnetLayerData());
            subShuffleLayerDatas.AddRange(primaryGrill.GetSubsMagnetLayerData());
        }

        for (int i = 0; i < 2; i++)
        {
            var data = i == 0 ? primaryShuffleLayerDatas : subShuffleLayerDatas;
            foreach (var shuffleLayerData in data)
            {
                foreach (var item in shuffleLayerData.layerData.itemData)
                {
                    if (item is not { id: > 0 }) continue;
                    if (!idCount.TryAdd(item.id, 1))
                    {
                        idCount[item.id]++;
                    }
                }
            }
        }

        foreach (var count in idCount)
        {
            if (count.Value % maxGrillSlot != 0)
            {
                Debug.LogError($"{count.Key} is out of range of {maxGrillSlot}");
            }
        }

        Debug.Log(idCount);
    }

    #endregion

    #region Booster Magic Wand

    [Header("Booster Magic Wand")] [SerializeField]
    private Transform magicWand;

    public bool CanUseBoosterMagicWand()
    {
        var CONVERTED_ITEM_COUNT = SonatSDKAdapter.GetRemoteInt("BoosterMagicWand_converted_item_count", 9);
        var count = 0;
        foreach (var primaryGrill in primaryGrills)
        {
            var shuffleLayerData = primaryGrill.GetMagnetLayerData();
            if (shuffleLayerData == null) continue;
            foreach (var item in shuffleLayerData.layerData.itemData)
            {
                if (item is not { id: > 0 and < 1000 }) continue;
                count++;
                if (count >= CONVERTED_ITEM_COUNT) return true;
            }


            var subLayers = primaryGrill.GetSubsShuffleLayerData();
            if (subLayers != null)
            {
                foreach (var subLayer in subLayers)
                {
                    foreach (var slot in subLayer.grill.GetSlots())
                    {
                        if (slot.GetItem() != null)
                        {
                            count++;
                            if (count >= CONVERTED_ITEM_COUNT) return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    // Lấy tổng cộng 9 item cừ các loại ngẫu nhiên
    // Lấy lượng tối đa có thể ở main layer, còn thiếu sẽ lấy ở sub layer
    // mỗi loại phải mất 1 lượng chia hết cho 3
    public async UniTask MagicWand()
    {
        int CONVERTED_ITEM_COUNT = SonatSDKAdapter.GetRemoteInt("BoosterMagicWand_converted_item_count", 9);
        List<SlotBase> selectedItemSlot = new();

        // Đếm số lượng các item có sẵn ở primary layer
        var idCount = GetItemsCountInPrimaryGrill();

        // Lấy id ngẫu nhiên
        var listId = idCount.Keys.ToList();
        int randId = listId[Random.Range(0, listId.Count)];
        listId.Remove(randId);

        int selectedItemId = randId;
        while (selectedItemSlot.Count < CONVERTED_ITEM_COUNT)
        {
            // Xác định số lượng item cần lấy ở mỗi loại layer
            var requestNumber = idCount[randId];
            Debug.Log($"count: {randId} {requestNumber}");
            if (selectedItemSlot.Count + requestNumber > CONVERTED_ITEM_COUNT)
            {
                requestNumber = CONVERTED_ITEM_COUNT - selectedItemSlot.Count;
            }

            var requestNumberInSubLayer = requestNumber % 3 == 0 ? 0 : 3 - requestNumber % 3;

            // Lấy item ở primary layer và sub layer[0]
            var slotsFromFirst2Layer = GetSlotsFromFirst2Layer(randId, requestNumber, requestNumberInSubLayer);
            Debug.Log($"slotsFromFirst2Layer: {slotsFromFirst2Layer.Count}");
            selectedItemSlot.AddRange(slotsFromFirst2Layer);

            // Nếu vẫn lấy thiếu ở sub layer thì lấy ở sub layer dưới
            if (slotsFromFirst2Layer.Count < requestNumber + requestNumberInSubLayer)
            {
                var count = requestNumber + requestNumberInSubLayer - slotsFromFirst2Layer.Count;
                var hiddenSlots = GetSlotsInSubHiddenLayer(randId, count);
                Debug.Log($"hiddenSlots: {hiddenSlots.Count}");
                selectedItemSlot.AddRange(hiddenSlots);
            }

            Debug.LogWarning($"selectedItemSlot: {selectedItemSlot.Count}");

            // Xác định id tiếp theo
            randId = listId[Random.Range(0, listId.Count)];
            listId.Remove(randId);
        }

        // play hiệu ứng item bay từ booster về vị trí các item đã lấy
        // thay đổi data của các item đã lấy
        await UniTask.Delay(1000);

        var effect = GameFactory.CreateEntity<CollectEffectMagicWand>("CollectEffectMagicWand", Vector3.zero);
        effect.transform.position = magicWand.position;
        effect.SetData(selectedItemSlot, selectedItemId, (slot) =>
        {
            foreach (var primaryGrill in primaryGrills)
            {
                primaryGrill.ChangeItem(slot, selectedItemId);
            }
        });

        MySonatFramework.audioService.PlaySound(AudioId.Items_Merge);
        MySonatFramework.GetService<VibrationService>().Vibrate(50);

        await UniTask.Delay(2000);
        foreach (var primaryGrill in primaryGrills)
        {
            primaryGrill.CheckComplete();
        }

        await UniTask.Delay(500);
    }

    private Dictionary<int, int> GetItemsCountInPrimaryGrill()
    {
        Dictionary<int, int> idCount = new();
        foreach (var primaryGrill in primaryGrills)
        {
            foreach (var item in primaryGrill.GetCurrentData().itemData)
            {
                if (item != null && item.id > 0 && item.id < 1000)
                {
                    if (!idCount.TryAdd(item.id, 1))
                    {
                        idCount[item.id]++;
                    }
                }
            }
        }

        return idCount;
    }


    private List<SlotBase> GetSlotsFromFirst2Layer(int selectedItemId, int requestNumber, int requestNumberInSubLayer)
    {
        Debug.Log($"Change GetItemsFromFirst2Layer: {selectedItemId} {requestNumber} {requestNumberInSubLayer}");
        var slots = new List<SlotBase>();
        var countInPrimary = 0;
        var countInSub = 0;
        foreach (var primaryGrill in primaryGrills)
        {
            // lấy item ở primary layer
            if (countInPrimary < requestNumber)
            {
                foreach (var slot in primaryGrill.GetSlots())
                {
                    if (slot.GetItem() != null && slot.GetItem().id == selectedItemId)
                    {
                        slots.Add(slot);
                        countInPrimary++;
                        if (countInPrimary == requestNumber) break;
                    }
                }
            }

            // lấy item ở sub layer
            if (countInSub < requestNumberInSubLayer)
            {
                var subGrills = primaryGrill.GetSubGrills();
                if (subGrills != null && subGrills.Count > 0)
                {
                    // Ưu tiên lấy ở sub layer trên cùng
                    foreach (var slot in subGrills[0].GetSlots())
                    {
                        if (slot.GetItem() != null && slot.GetItem().id == selectedItemId)
                        {
                            slots.Add(slot);
                            countInSub++;
                            if (countInSub == requestNumberInSubLayer) break;
                        }
                    }
                }
            }
        }

        return slots;
    }

    private List<SlotBase> GetSlotsInSubHiddenLayer(int selectedItemId, int count)
    {
        Debug.Log($"Change GetSlotsInSubHiddenLayer: {selectedItemId} {count}");
        var slots = new List<SlotBase>();
        foreach (var primaryGrill in primaryGrills)
        {
            var subGrills = primaryGrill.GetSubGrills();
            if (subGrills == null) continue;

            for (int i = 1; i < subGrills.Count; i++)
            {
                int index = 0;
                foreach (var itemData in subGrills[i].GetCurrentData().itemData)
                {
                    if (itemData != null && itemData.id == selectedItemId)
                    {
                        slots.Add(subGrills[i].GetSlots()[index]);
                        count--;
                        if (count == 0) return slots;
                    }

                    index++;
                }
            }
        }

        return slots;
    }

    #endregion

    private bool isUseBooster = false;

    #region Booster Magic Key

    public async UniTask MagicKey()
    {
        isUseBooster = false;
        //GrillInteractionLayer.OnSelectGrill += BoosterMagicKeyByGrill;
        //PopupGuideMagicKey.onClose += CloseBoosterMagicKey;
        GameplayController.instance.SelectItem(null);
        // foreach (var primaryGrill in primaryGrills)
        // {
        //     if (IsLockGrill(primaryGrill))
        //     {
        //         primaryGrill.EnableClick();
        //     }
        // }
        //
        // UIData uiData = new();
        // uiData.Add("onClose", (Action)CloseBoosterMagicKey);
        // uiData.Add("onSelectEntity", (Action<PrimaryGrill>)BoosterMagicKeyByGrill);
        // PopupGuideMagicKey popupGuideMagicKey = PanelManager.Instance.OpenPanel<PopupGuideMagicKey>(uiData);

        HighlightGrill(true, CheckMagicKeyHighlightGrill);
        HighlightObstacle(true, CheckMagicKeyHighlightObstacle);

        // open popup highlight item
        var uiData = new UIData();
        uiData.Add("onClose", (Action)(() =>
        {
            HighlightGrill(false, null);
            HighlightObstacle(false, null);
        }));
        uiData.Add("onSelectEntity", (Action<List<EntityBase>>)OnSelectEntity_MagicKey);
        PopupGuideMagicKey popupGuideMagicKey = PanelManager.Instance.OpenPanel<PopupGuideMagicKey>(uiData);

        await UniTask.WaitUntil(() => popupGuideMagicKey == null || !popupGuideMagicKey.gameObject.activeInHierarchy);
    }

    private bool CheckMagicKeyHighlightGrill(PrimaryGrill grill)
    {
        return IsLockGrill(grill);
    }

    private bool CheckMagicKeyHighlightObstacle(ObstacleBase obstacle)
    {
        return obstacle.ObstacleType is ObstacleType.LockAreaHorizontal or ObstacleType.LockAreaVertical && obstacle.active;
    }

    private bool IsLockGrill(PrimaryGrill grill)
    {
        switch (grill.grillType)
        {
            case GrillType.Lock:
            case GrillType.LockAds:
            case GrillType.LockAndKey:
            case GrillType.LockAndKey2:
            case GrillType.Shutter:
            case GrillType.Lid:
                //case GrillType.Vending:
                return grill.LockState == 1;
        }

        return false;
    }

    private void OnSelectEntity_MagicKey(List<EntityBase> entities)
    {
        foreach (var entity in entities)
        {
            switch (entity)
            {
                case PrimaryGrill grill:
                    BoosterMagicKeyByGrill(grill);
                    return;
                case ObstacleBase obstacleBase:
                    BoosterMagicKeyByObstacle(obstacleBase);
                    break;
            }
        }
    }

    public void BoosterMagicKeyByGrill(PrimaryGrill primaryGrill)
    {
        if (isUseBooster || !IsLockGrill(primaryGrill)) return;

        isUseBooster = true;
        MySonatFramework.GetService<BoosterService>().UseBoosterSuccess(GameResource.BoosterMagicKey);

        PanelManager.Instance.ClosePanel<PopupGuideMagicKey>(); // đã disable tất cả
        primaryGrill.EnableClick(); // chặn click

        var effect = MySonatFramework.poolingService.Create<UIBoosterMagicKeyEffect>("UIBoosterMagicKeyEffect", Vector3.zero, PanelManager.Instance.transform);
        effect.SetData(primaryGrill.transform.position, () =>
        {
            primaryGrill.Unlock();
            primaryGrill.DisableClick();
            isUseBooster = false;
        });
    }

    public void BoosterMagicKeyByObstacle(ObstacleBase obstacle)
    {
        if (isUseBooster || !obstacle.active) return;
        if (obstacle.ObstacleType != ObstacleType.LockAreaHorizontal && obstacle.ObstacleType != ObstacleType.LockAreaVertical) return;
        isUseBooster = true;
        MySonatFramework.GetService<BoosterService>().UseBoosterSuccess(GameResource.BoosterMagicKey);

        PanelManager.Instance.ClosePanel<PopupGuideMagicKey>(); // đã disable tất cả

        var effect = MySonatFramework.poolingService.Create<UIBoosterMagicKeyEffect>("UIBoosterMagicKeyEffect", Vector3.zero, PanelManager.Instance.transform);

        if (obstacle is LockAreaObstacle lockAreaObstacle)
        {
            effect.SetData(lockAreaObstacle.LockPosition.position, () =>
            {
                lockAreaObstacle.UnlockByBooster();
                isUseBooster = false;
            });
        }
    }

    public void CloseBoosterMagicKey()
    {
        //GrillInteractionLayer.OnSelectGrill -= BoosterMagicKeyByGrill;
        //PopupGuideMagicKey.onClose -= CloseBoosterMagicKey;

        foreach (var primaryGrill in primaryGrills)
        {
            primaryGrill.DisableClick();
        }
    }

    public bool CanUseBoosterMagicKey()
    {
        foreach (var primaryGrill in primaryGrills)
        {
            if (primaryGrill.grillType == GrillType.Lock
                || primaryGrill.grillType == GrillType.LockAds
                || primaryGrill.grillType == GrillType.LockAndKey
                || primaryGrill.grillType == GrillType.LockAndKey2
                || primaryGrill.grillType == GrillType.Lid
                || primaryGrill.grillType == GrillType.Shutter
               //|| primaryGrill.grillType == GrillType.Vending
               )
            {
                if (primaryGrill.IsLock == true)
                {
                    return true;
                }
            }
        }

        if (obstacles != null)
            foreach (var obstacle in obstacles)
            {
                if (obstacle.ObstacleType is ObstacleType.LockAreaHorizontal or ObstacleType.LockAreaVertical && obstacle.active) return true;
            }

        return false;
    }

    #endregion

    #region Booster Magic Match3 target

    public async UniTask Match3Target()
    {
        isUseBooster = false;
        GameplayController.instance.SelectItem(null);
        HighlightItems(true, CheckMatch3ItemHighlight);
        //OnClickItem += OnClickItem_Match3Target;

        // open popup highlight item
        var uiData = new UIData();
        uiData.Add("content", "Select any item to match");
        uiData.Add("onClose", (Action)(() => { HighlightItems(false); }));
        uiData.Add("onSelectEntity", (Action<List<EntityBase>>)OnClickItem_Match3Target);
        PopupHightlightItem popupHighlightItem = PanelManager.Instance.OpenPanel<PopupHightlightItem>(uiData);
        await UniTask.WaitUntil(() => popupHighlightItem == null || !popupHighlightItem.gameObject.activeInHierarchy);
        await UniTask.WaitUntil(() => !isUseBooster);
    }

    private bool CheckMatch3ItemHighlight(Item item)
    {
        if (item.id == 0 || item.id >= 1000) return false;
        if (item.Slot.GetGrill().IsLock) return false;
        if (item.IsLocked) return false;
        if (item.itemType == ItemType.Bomb && item is ItemBombMove { Exploded: false }) return false;
        return true;
    }

    private void OnClickItem_Match3Target(List<EntityBase> entities)
    {
        if (isUseBooster) return;
        foreach (var entity in entities)
        {
            if (entity is not Item item || item.id >= 1000) continue;
            if (!CheckMatch3ItemHighlight(item)) return;

            isUseBooster = true;

            MySonatFramework.GetService<BoosterService>().UseBoosterSuccess(GameResource.BoosterMagnet);

            // play anim
            BoosterAnim boosterAnim = SonatSystem.GetService<PoolingService>()
                .Create<BoosterAnim>($"{GameResource.BoosterMagnet}Anim", PanelManager.Instance.transform);
            boosterAnim.SetData(transform.position);

            // compute logic
            List<ShuffleLayerData> primaryShuffleLayerDatas = new();

            foreach (var primaryGrill in primaryGrills)
            {
                var shuffleLayerData = primaryGrill.GetMagnetLayerData();
                if (shuffleLayerData == null) continue;
                primaryShuffleLayerDatas.Add(shuffleLayerData);
            }

            // active
            SonatUtils.DelayCall(2f, () =>
            {
                Match3(item.id, primaryShuffleLayerDatas).Forget();
                PanelManager.Instance.ClosePanel<PopupHightlightItem>();
                SonatUtils.DelayCall(1, () => { isUseBooster = false; }, this);
            }, this);
            return;
        }
    }

    public bool CanUseBoosterMagicMatch3Target()
    {
        return true;
    }

    #endregion


    #region Booster BlowTorch

    public async UniTask BlowTorch()
    {
        isUseBooster = false;
        GameplayController.instance.SelectItem(null);
        HighlightItems(true, CheckBlowTorchItemHighlight);
        HighlightGrill(true, CheckBlowTorchGrillHighlight);
        HighlightObstacle(true, CheckBlowTorchObstacleHighlight);

        // open popup highlight item
        var uiData = new UIData();
        uiData.Add("onClose", (Action)(() =>
        {
            HighlightItems(false);
            HighlightGrill(false, null);
            HighlightObstacle(false, CheckBlowTorchObstacleHighlight);
        }));
        uiData.Add("onSelectEntity", (Action<List<EntityBase>>)OnSelectEntity_BlowTorch);
        PopupHightlightItem popupHighlightItem = PanelManager.Instance.OpenPanel<PopupHightlightItem>(uiData);
        await UniTask.WaitUntil(() => popupHighlightItem == null || !popupHighlightItem.gameObject.activeInHierarchy);
    }

    private bool CheckBlowTorchItemHighlight(Item item)
    {
        return item.itemType == ItemType.Ice && item.IsLocked && !item.Slot.GetGrill().IsLock;
    }

    private bool CheckBlowTorchGrillHighlight(PrimaryGrill grill)
    {
        return grill.grillType is GrillType.Ice && grill.LockState == 1;
    }

    private bool CheckBlowTorchObstacleHighlight(ObstacleBase obstacle)
    {
        return obstacle.ObstacleType is ObstacleType.OctoChef;
    }


    private void OnSelectEntity_BlowTorch(List<EntityBase> entities)
    {
        foreach (var entity in entities)
        {
            switch (entity)
            {
                case Item item when CheckBlowTorchItemHighlight(item):
                    OnClickItem_BlowTorch(item);
                    return;
                case PrimaryGrill { grillType: GrillType.Ice, LockState: 1 } grill:
                    OnSelectGrill_BlowTorch(grill);
                    return;
                case ObstacleBase obstacleBase:
                    OnSelectObstacle_BlowTorch(obstacleBase);
                    break;
            }
        }
    }

    private void OnClickItem_BlowTorch(Item item)
    {
        if (isUseBooster) return;

        if (item.Slot.GetGrill().IsLock) return;

        BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<GameStateChangeEvent>.Raise(new() { gameState = GameState.UsingBooster });
        isUseBooster = true;
        MySonatFramework.GetService<BoosterService>().UseBoosterSuccess(GameResource.BoosterBlowTorch);

        EffectPoolBase boosterAnim = SonatSystem.GetService<PoolingService>()
            .Create<EffectPoolBase>($"BoosterBlowTorchAnim", PanelManager.Instance.transform);
        boosterAnim.transform.position = item.transform.position;
        SonatUtils.DelayCall(1.5f, () =>
        {
            (item as ItemIce)?.Unlock();
            isUseBooster = false;
            if (GameplayController.instance.gameState != GameState.GameOver)
                EventBus<GameStateChangeEvent>.Raise(new() { gameState = GameState.Playing });
            blockPanel?.Close();
        }, this);
    }

    private void OnSelectGrill_BlowTorch(PrimaryGrill primaryGrill)
    {
        if (isUseBooster) return;

        BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<GameStateChangeEvent>.Raise(new() { gameState = GameState.UsingBooster });
        isUseBooster = true;
        MySonatFramework.GetService<BoosterService>().UseBoosterSuccess(GameResource.BoosterBlowTorch);
        EffectPoolBase boosterAnim = SonatSystem.GetService<PoolingService>()
            .Create<EffectPoolBase>($"BoosterBlowTorchAnim", PanelManager.Instance.transform);
        boosterAnim.transform.position = primaryGrill.transform.position;
        SonatUtils.DelayCall(1.5f, () =>
        {
            primaryGrill.Unlock();
            primaryGrill.DisableClick();
            isUseBooster = false;
            if (GameplayController.instance.gameState != GameState.GameOver)
                EventBus<GameStateChangeEvent>.Raise(new() { gameState = GameState.Playing });
            blockPanel?.Close();
        }, this);
    }

    private void OnSelectObstacle_BlowTorch(ObstacleBase obstacle)
    {
        if (isUseBooster) return;

        if (obstacle.ObstacleType is not ObstacleType.OctoChef) return;

        BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<GameStateChangeEvent>.Raise(new() { gameState = GameState.UsingBooster });
        isUseBooster = true;
        MySonatFramework.GetService<BoosterService>().UseBoosterSuccess(GameResource.BoosterBlowTorch);
        EffectPoolBase boosterAnim = SonatSystem.GetService<PoolingService>()
            .Create<EffectPoolBase>($"BoosterBlowTorchAnim", PanelManager.Instance.transform);
        boosterAnim.transform.position = obstacle.transform.position;
        SonatUtils.DelayCall(1.5f, () =>
        {
            if (obstacle is OctoChefObstacle octoChefObstacle)
            {
                octoChefObstacle.BlowTorch();
            }

            isUseBooster = false;
            if (GameplayController.instance.gameState != GameState.GameOver)
                EventBus<GameStateChangeEvent>.Raise(new() { gameState = GameState.Playing });
            blockPanel?.Close();
        }, this);
    }

    public bool CanUseBoosterBlowTorch()
    {
        foreach (var primaryGrill in primaryGrills)
        {
            switch (primaryGrill.grillType)
            {
                case GrillType.Ice:
                    if (primaryGrill.IsLock == true)
                    {
                        return true;
                    }

                    break;
            }

            // var listBaseGrill = new List<GrillBase>();
            // listBaseGrill.Add(primaryGrill);
            // if (primaryGrill.HasSubGrills())
            // {
            //     listBaseGrill.AddRange(primaryGrill.GetSubGrills());
            // }

            //foreach (var grill in listBaseGrill)
            //{
            if (primaryGrill.IsLock) continue;
            foreach (var slot in primaryGrill.GetSlots())
            {
                Item item = slot.GetItem();
                if (item != null && item.itemType == ItemType.Ice && item.IsLocked)
                {
                    return true;
                }
            }
            // }
        }

        if (obstacles != null)
        {
            foreach (var obstacle in obstacles)
            {
                if (obstacle.ObstacleType == ObstacleType.OctoChef && obstacle.active) return true;
            }
        }

        return false;
    }

    private void HighlightItems(bool highlight, Func<Item, bool> CheckFunc = null) // highlight item tất cả hoặc theo loại item
    {
        foreach (var primaryGrill in primaryGrills)
        {
            foreach (var slot in primaryGrill.GetSlots())
            {
                var item = slot.GetItem();
                if (item == null) continue;
                if (CheckFunc != null && !CheckFunc(item)) continue;
                if (highlight)
                {
                    item.Highlight();
                }
                else
                {
                    item.UnHighlight();
                }
            }
        }
    }

    private void HighlightGrill(bool highlight, Func<PrimaryGrill, bool> CheckFunc)
    {
        foreach (var primaryGrill in primaryGrills)
        {
            if (CheckFunc == null || CheckFunc(primaryGrill))
            {
                if (highlight)
                {
                    primaryGrill.EnableClick();
                }
                else
                {
                    primaryGrill.DisableClick();
                }
            }
        }
    }

    private void HighlightObstacle(bool highlight, Func<ObstacleBase, bool> CheckObstacleFunc = null)
    {
        if (obstacles == null) return;
        foreach (var obstacle in obstacles)
        {
            if (CheckObstacleFunc == null || CheckObstacleFunc.Invoke(obstacle))
            {
                if (highlight)
                {
                    obstacle.Highlight(true);
                }
                else
                {
                    obstacle.Highlight(false);
                }
            }
        }
    }

    public void ForceUnhighlight()
    {
        HighlightGrill(false, null);
        HighlightItems(false, null);
        HighlightObstacle(false, null);
    }

    #endregion

    #endregion

    public void OnGameOver()
    {
        ForceUnhighlight();
    }

    #region Check suggest

    public List<Item> GetSuggestItems()
    {
        List<ShuffleLayerData> primaryShuffleLayerDatas = new();
        int slotEmpty = 0;
        Dictionary<int, List<Item>> idCount = new();
        foreach (var primaryGrill in primaryGrills)
        {
            if (primaryGrill.IsLock) continue;
            var shuffleLayerData = primaryGrill.GetMagnetLayerData();
            if (shuffleLayerData == null) continue;
            primaryShuffleLayerDatas.Add(shuffleLayerData);
            if (dropMode && !primaryGrill.isInGameplaySpace) continue;
            slotEmpty += primaryGrill.NumberSlotEmpty();
            foreach (var slot in shuffleLayerData.grill.GetSlots())
            {
                var item = slot.GetItem();
                if (item == null || item.id <= 0 || item.id >= 1000) continue;

                if (idCount.ContainsKey(item.id) == false)
                {
                    idCount[item.id] = new List<Item>();
                }

                idCount[item.id].Add(item);
            }
        }

        if (slotEmpty == 0) return null;

        foreach (var id in idCount.Keys)
        {
            if (idCount[id].Count >= maxGrillSlot)
            {
                return idCount[id].GetRange(0, maxGrillSlot);
            }
        }

        return null;
    }

    public PrimaryGrill GetLockAds()
    {
        foreach (var primaryGrill in primaryGrills)
        {
            if (primaryGrill.grillType == GrillType.LockAds && primaryGrill.IsLock)
            {
                return primaryGrill;
            }
        }

        return null;
    }

    public int GetPrimaryItemRemain()
    {
        int count = 0;
        if (primaryGrills == null || primaryGrills.Count == 0) return 0;

        for (int i = 0; i < primaryGrills.Count; i++)
        {
            var slots = primaryGrills[i].GetSlots();
            for (int j = 0; j < slots.Length; j++)
            {
                Item item = slots[j].GetItem();
                if (item != null && item.id > 0)
                {
                    count++;
                }
            }
        }

        return count;
    }

    #endregion

    public bool HasSubGrill(GrillData grillData)
    {
        if (grillData.grillType == GrillType.Vending) return false;
        return grillData.layer is { Count: > 1 };
    }

    public bool CheckItemById(int id)
    {
        if (primaryGrills == null || primaryGrills.Count == 0) return false;

        foreach (var primaryGrill in primaryGrills)
        {
            if (primaryGrill.CheckItemWithId(id)) return true;

            if (primaryGrill.HasSubGrills())
            {
                foreach (var subGrill in primaryGrill.GetSubGrills())
                {
                    if (subGrill.CheckItemWithId(id)) return true;
                }
            }
        }

        return false;
    }

    public OrderList CheckOrderRemain()
    {
        return orderQueue.CheckOrderRemain();
    }

    public bool IsWinLevel()
    {
        foreach (var primaryGrill in primaryGrills)
        {
            foreach (var slot in primaryGrill.GetSlots())
            {
                if (slot != null && !slot.isEmpty()) return false;
            }

            var subGrills = primaryGrill.GetSubGrills();
            if (subGrills != null)
                foreach (var subGrill in subGrills)
                {
                    var current = subGrill.GetCurrentData();
                    if (current is { itemData: not null })
                        foreach (var itemData in current.itemData)
                        {
                            if (itemData is { id: > 0 }) return false;
                        }
                }
        }

        return true;
    }

    public bool AnyConveyors()
    {
        return conveyors != null && conveyors.Count > 0;
    }
}