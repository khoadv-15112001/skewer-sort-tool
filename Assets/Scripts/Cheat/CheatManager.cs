using Base.Singleton;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.LevelManagement;
using SonatFramework.Systems.UserData;
using GrillSort.QuestEvent;
using Sonat.Data;
using LevelData = Gameplay.LevelData.LevelData;
using GrillSort.OnlineService;
using MyGame.Modules.CardCollection;
using SonatFramework.Systems.SceneManagement;
using MyGame.SkewerJam.Gameplay;

public class CheatManager : Singleton<CheatManager>
{
    [SerializeField] private LevelDriveService levelDriveService;
    [SerializeField] private LevelDriveServiceFolder levelDriveServiceFolder;

    protected override void OnAwake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CheatPanel cheatPanel = PanelManager.Instance.GetPanel<CheatPanel>();
            if (cheatPanel == null)
            {
                PanelManager.Instance.OpenForget<CheatPanel>();
            }
            else
            {
                cheatPanel.OnOffCheat();
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                CheatWinState();
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                CheatWin();
                return;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                CheatLose();
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                PanelManager.Instance.OpenPanelByName<PopupPack>("PopupWeekendDeal");
                return;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                PanelManager.Instance.OpenPanelByName<PopupPack>("PopupStarterPack");
                return;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                PanelManager.Instance.OpenPanelByName<BasePanel>("LuckySpinPanel");
                return;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                return;
            }
        }
    }
#endif

    public static void CheatWinState()
    {
        //GamePlayController.Instance.CheatWinState();
    }

    public static void CheatWin()
    {
        if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay)
        {
            _ = GameplayController.instance.Win();
        }
        else if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay_SkewerJam)
        {
            _ = GameController.Instance.Win();
        }
    }

    public static void CheatTime(int time)
    {
        int timeRemaining = (int)GameplayController.instance.timeManager.GetTimeRemaining();
        GameplayController.instance.timeManager.AddTime(time - timeRemaining);
    }

    public static void CheatLose()
    {
        if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay)
        {
            float timeRemaining = GameplayController.instance.timeManager.GetTimeRemaining();
            GameplayController.instance.timeManager.AddTime(1 - (int)timeRemaining);
        }
        else if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay_SkewerJam)
        {
            _ = GameController.Instance.Stuck(StuckType.SkewerJam_OutOfSpace);
        }
        //GameplayController.instance.Stuck();
    }

    public static void CheatLevel(int level)
    {
        if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay)
        {
            SonatSystem.GetService<UserDataService>().SaveLevel(level);
#if sonat_sdk_v2
            UserData.SetLevel(level);
#endif
            GameplayController.level = level;
            _ = GameplayController.instance.PlayLevel(level);
            return;
        }
        else if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay_SkewerJam)
        {
            SonatSystem.GetService<UserDataService>().SaveLevel(level, GameMode.SkewerJam);
#if sonat_sdk_v2
            UserData.SetLevel(level, GameMode.SkewerJam.ToString());
#endif
            _ = GameController.Instance.PlayLevel(level, force: true);
            return;
        }
        //PlayLevel().Forget();
    }

    private static async UniTaskVoid PlayLevel()
    {
    }

    public static void CheatResource(GameResource resource, int value)
    {
        var inventoryService = SonatSystem.GetService<InventoryService>();
        inventoryService.SetResource(resource, value);
        inventoryService.NotiUpdateResource(resource);
    }

    public static void CheatRemoteConfig(string key, string value)
    {
        if (int.TryParse(value, out var intValue))
        {
            PlayerPrefs.SetInt($"remote_value_{key}", intValue);
        }
        else if (bool.TryParse(value, out var booValue))
        {
            PlayerPrefs.SetInt($"remote_value_{key}", booValue ? 1 : 0);
        }
        else
        {
            PlayerPrefs.SetString($"remote_value_{key}", value);
        }
    }

    public static void CheatPlayerPrefs(string key, string value)
    {
        if (value.Length < 8 && int.TryParse(value, out var intValue))
        {
            PlayerPrefs.SetInt($"{key}", intValue);
        }
        else if (bool.TryParse(value, out var booValue))
        {
            PlayerPrefs.SetInt($"{key}", booValue ? 1 : 0);
        }
        else
        {
            PlayerPrefs.SetString($"{key}", value);
        }
    }

    public static void CheatNoAds()
    {
        SonatSDKAdapter.SetNoAds(true);
    }

    public static bool IsOpenCheat()
    {
        return PlayerPrefs.GetInt("SONAT_CHEATED", 0) == 1 || Application.isEditor;
    }

    public static CheatLevelSource GetLevelSource()
    {
        CheatLevelSource cheatLevelSource = (CheatLevelSource)PlayerPrefs.GetInt("SONAT_CHEATED_LEVELSOURCE", 0);

        if (cheatLevelSource == CheatLevelSource.Drive && !IsOpenCheat())
        {
            cheatLevelSource = CheatLevelSource.Resources;
        }

        return cheatLevelSource;
    }

    public static void CheatLevelDrive(int linkRow)
    {
        var levelService = Instantiate(Instance.levelDriveService);
        var popup = PanelManager.Instance.OpenPanel<PopupLoadingIap>();

        levelService.DownloadData(linkRow, (success) =>
        {
            LevelGenerator.levelService = levelService;
            popup.Close();
            OnDownloadLevelDriveCompleted(success);
        }).Forget();
    }

    private static LevelDriveServiceFolder _levelDriveServiceFolder;


    public static void DownloadLevelFolderData(Action<List<string>> callback)
    {
        if (_levelDriveServiceFolder == null)
        {
            _levelDriveServiceFolder = Instantiate(Instance.levelDriveServiceFolder);
        }

        if (_levelDriveServiceFolder.GetAvailableSubfolders() != null)
        {
            callback.Invoke(_levelDriveServiceFolder.GetAvailableSubfolders());
        }

        var popup = PanelManager.Instance.OpenPanel<PopupLoadingIap>();
        _levelDriveServiceFolder.RefreshFolderContents((success) =>
        {
            popup.Close();
            OnDownloadLevelDriveCompleted(success);
            callback.Invoke(_levelDriveServiceFolder.GetAvailableSubfolders());
        }).Forget();
    }

    public static async UniTaskVoid CheatLevelDriveFolder(string folderName, int level, Action callback)
    {
        if (_levelDriveServiceFolder == null || !_levelDriveServiceFolder.IsReady()) return;
        var popup = PanelManager.Instance.OpenPanel<PopupLoadingIap>();
        bool success = await _levelDriveServiceFolder.GetLevelInSubfolder<LevelData>(folderName, level, GameMode.Classic);
        if (!success)
        {
            popup.Close();
            PopupToast.Cretate($"Not found level {level} in folder {folderName}");
            return;
        }

        LevelGenerator.levelService = _levelDriveServiceFolder;
        callback?.Invoke();
        popup.Close();
    }

    private static void OnDownloadLevelDriveCompleted(bool success)
    {
        PopupToast.Cretate($"{success}");
    }

    public static Action<int> CollectSpecialItem;

    public static void CheatQuestEvent(int milestone, int numItem)
    {
        var questEventService = SonatSystem.GetService<QuestEventService>();

        questEventService.CheatMilestone(milestone, numItem);

        CollectSpecialItem?.Invoke(numItem);
    }

    public static void CheatX2QuestEvent(int numItem)
    {
        var questEventService = SonatSystem.GetService<QuestEventService>();
        if (numItem > 0)
        {
            MySonatFramework.GetService<InventoryService>().AddResource(GameResource.X2ItemQuestEvent, 1);
            questEventService.CheckActiveX2Item();
        }
        else
        {
            questEventService.ResetX2Item();
        }
        QuestEventService.OnX2Item?.Invoke();
    }

    public static void CheatServer(DataSyncKey dataSyncKey, DataType dataType, string text)
    {
        if (dataType == DataType.None)
        {
            string[] paths = new string[] { dataSyncKey.ToString() };
            ServerHelper.GetValueFromServer(paths).Forget();
        }
        else
        {
            _ = ServerHelper.SetValueToServer(dataSyncKey.ToString(), dataType, text);
        }
    }

    public static void CheatCardCollection(CardType cardType)
    {
        var cardCollectionService = SonatSystem.GetService<CardCollectionService>();
        cardCollectionService.AddCard(cardType, 1);
    }

    internal static void CheatLevelChest(int chestId, int curLevel)
    {
        var chestRewardService = SonatSystem.GetService<ChestRewardService>();

        chestRewardService.UpdateProgress(chestId, curLevel);
    }
}


public enum CheatOption
{
    Level,
    Win,
    WinState,
    Lose,
    Resource,
    RemoteConfig,
    PlayerPrefs,
    GDLevel,
    StarChest,
    LevelChest,
    TransportTracking,
    NoAds,
    DownloadLevelDriveZip,
    QuestEvent,
    DownloadLevelDriveFolder,
    Server,
    ServerDeleteUserData,
    UserCampaignSegment,
    Leaderboard,
    CardCollection,
    Team,
    Winstreak,
    X2SausageGold,
    PlayStoryVideo,
    Pinata_Winstreak,
    Pinata_Auto_Gold,
    TeamOffer_Reset,
    BlackFriday,
    TradeCard,
    ResetXmasEventPack,
    MAX,
}

public enum CheatLevelSource
{
    Resources,
    Drive,
    MAX,
}