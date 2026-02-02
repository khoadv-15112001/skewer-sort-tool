using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Gameplay;
using GrillSort;
using GrillSort.BlackFriday;
using GrillSort.XmasEvent;
using GrillSort.ConsecutiveWin;
using GrillSort.OnlineService;
using GrillSort.Pinata;
using GrillSort.QuestEvent;
using GrillSort.Story;
using GrillSort.Winstreak;
using MyGame.Modules.CardCollection;
using Sonat;
using Sonat.Data;
using Sonat.DebugViewModule;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.SceneManagement;
using SonatFramework.Systems.TimeManagement;
using Spine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class CheatPanel : Panel
{
    public GameObject panel;
    public TMP_Dropdown cheatOptsDropdown;
    public TMP_Dropdown[] cheatDropdowns;
    public TMP_InputField[] inputValues;

    public TMP_InputField inputPassword;

    //public InputField ipAdressInput;
    public GameObject lockObj;
    public TMP_InputField timeNow;
    public Toggle localizeToggle;
    public Toggle showDebugConsoleToggle, forceLocalizeJPToggle;
    public GameObject fpsLog;
    public GameObject downloadProcess;
    private readonly Service<DataService> dataService = new Service<DataService>();
    private readonly Service<WinStreakManager> _winstreakManager = new Service<WinStreakManager>();
    private readonly Service<CardCollectionService> _cardCollectionService = new Service<CardCollectionService>();
    private readonly Service<UserGroupService> userGroupService = new Service<UserGroupService>();

    private PlayerPrefString ipAdress;
    [SerializeField] private GameObject debugLogConsolePrefab;
    private static GameObject debugLogConsole;

    public static bool IsCheating = false;

    public override void OnSetup()
    {
        base.OnSetup();
        InitOpts();
        OnDropdown(0);
        OnInputField(1);
        localizeToggle.isOn = dataService.Instance.GetInt("USING_LOCAL_TIME", 0) == 1;
        localizeToggle.onValueChanged.AddListener(SwitchLocalTime);
#if UNITY_EDITOR
        IsCheating = true;
        lockObj.SetActive(false);
#else
		if(PlayerPrefs.HasKey("SONAT_CHEATED"))
		{
            IsCheating = true;
			lockObj.SetActive(false);
		}
		else
		{
			lockObj.SetActive(true);
		}
#endif
        ipAdress = new PlayerPrefString("IP_ADDRESS", "");
        //ipAdressInput.text = ipAdress.Value;
        ShowDebugConsole(showDebugConsoleToggle.isOn);
        showDebugConsoleToggle.onValueChanged.AddListener(ShowDebugConsole);

        forceLocalizeJPToggle.isOn = LocalizationUtils.ForceJapan;
        forceLocalizeJPToggle.onValueChanged.AddListener((value) => { LocalizationUtils.ForceJapan = value; });
    }

    private void CheatStarCard(string str)
    {
        int.TryParse(str, out int star);
        _cardCollectionService.Instance.AddCardStar(star);
    }
    private void CheatRewardCollection(GameResource gameResource)
    {
        Debug.Log($"anhnt: gameResource={gameResource}");
        if (GameResourceHelper.ResourceType(gameResource) != GameResourceType.Card) return;
        RewardData rewardData = new();
        rewardData.AddReward(new ResourceData(gameResource, 1));

        MySonatFramework.inventoryService.AddReward(rewardData);
        Debug.Log($"anhnt: rewardData={rewardData.resourceDatas.Count}");

        UIData uiData = new UIData();
        uiData.Add("Title", "Cheat Card!");
        uiData.Add("Reward", rewardData);
        uiData.Add("x2", false);
        PanelManager.Instance.OpenPanel<PopupReward>(uiData);
    }

    public void OpenCard(string str)
    {
        if (Enum.TryParse<GameResource>(str, out var card))
        {
            CheatRewardCollection(card);
        }
    }

    private void ChangeWinstreakDayUnlock(string str)
    {
        int.TryParse(str, out var dayUnlock);

        _winstreakManager.Instance.config.dayUnlock = dayUnlock;
        panel.SetActive(false);

        Debug.Log($"anhnt: [Winstreak] dayUnlock = " + dayUnlock);
    }

    public void Unlock()
    {
        if (inputPassword.text.Equals("Sonat@111"))
        {
            IsCheating = true;
            lockObj.SetActive(false);
            PlayerPrefs.SetInt("SONAT_CHEATED", 1);
        }
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        panel.SetActive(true);
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        // DOVirtual.DelayedCall(0.2f, () =>
        // {
        // 	PanelManager.Instance.MoveBackPanel(this);
        // });
    }

    public override void OnFocus()
    {
    }

    public override void OnFocusLost()
    {
    }

    public override void Close()
    {
        base.Close();
    }

    protected override void OnCloseCompleted()
    {
        base.OnCloseCompleted();
    }

    private CheatOption GetCheatOpt()
    {
        CheatOption cheatOption = cheatOptsDropdown.options[cheatOptsDropdown.value].text.ToEnum<CheatOption>();
        return cheatOption;
    }

    private void OnCheatOptChange(int value)
    {
        OnDropdown(0);
        OnInputField(0);
        CheatOption cheatOption = GetCheatOpt();
        switch (cheatOption)
        {
            case CheatOption.Level:
                OnDropdown(0);
                OnInputField(1);
                break;
            case CheatOption.Win:
            case CheatOption.Lose:
            case CheatOption.WinState:
                OnDropdown(0);
                OnInputField(0);
                break;
            case CheatOption.Resource:
                OnDropdown(1);
                OnInputField(1);
                InitResourceDropdown();
                break;
            case CheatOption.RemoteConfig:
            case CheatOption.PlayerPrefs:
                OnDropdown(0);
                OnInputField(2);
                break;
            case CheatOption.GDLevel:
                OnDropdown(0);
                OnInputField(0);
                //InitGDLevelDropdown();
                break;
            case CheatOption.StarChest:
                OnInputField(2);
                break;
            case CheatOption.LevelChest:
                OnInputFieldLevelChest(2);
                break;
            case CheatOption.TransportTracking:
                OnDropdown(0);
                OnInputField(1);
                inputValues[0].text = ipAdress.Value;
                break;
            case CheatOption.NoAds:
                OnDropdown(0);
                OnInputField(0);
                break;
            case CheatOption.DownloadLevelDriveZip:
                OnDropdown(0);
                OnInputField(1);
                inputValues[0].text = "2";
                break;
            case CheatOption.QuestEvent:
                OnDropdown(1);
                OnInputFieldQuestEvent(1);
                InitQuestEventDropdown();
                break;
            case CheatOption.DownloadLevelDriveFolder:
                OnDropdown(1);
                OnInputField(1);
                InitDriveFolderDropdown();
                break;
            case CheatOption.Server:
                OnDropdown(2);
                OnInputField(1);
                InitDataSyncKeyDropdown(0);
                InitDataTypeDropdown(1);
                break;
            case CheatOption.ServerDeleteUserData:
                OnDropdown(0);
                OnInputField(0);
                break;
            case CheatOption.UserCampaignSegment:
                OnInputField(1);
                break;
            case CheatOption.Leaderboard:
                OnDropdown(1);
                OnInputField(1);
                InitLeaderboardDropdown(0);
                break;
            case CheatOption.Team:
                OnDropdown(1);
                OnInputField(1);
                InitTeamDropdown(0);
                break;
            case CheatOption.CardCollection:
                OnDropdown(1);
                InitCardCollectionDropdown();
                break;
            case CheatOption.Winstreak:
                OnInputField(1);
                inputValues[0].text = _winstreakManager.Instance.config.dayUnlock.ToString();
                break;
            case CheatOption.PlayStoryVideo:
                OnDropdown(1);
                OnInputField(0);
                InitStoryDropdown();
                break;
            case CheatOption.Pinata_Winstreak:
                OnDropdown(0);
                OnInputField(1);
                break;
            case CheatOption.Pinata_Auto_Gold:
                break;
            case CheatOption.TeamOffer_Reset:
                OnInputField(0);
                OnDropdown(0);
                break;
            case CheatOption.BlackFriday:
                OnInputField(1);
                OnDropdown(0);
                // Display current LTV (CheatLTV nếu > 0, không thì real LTV)
                if (UserGroupService.CheatLTV > 0)
                {
                    inputValues[0].text = UserGroupService.CheatLTV.ToString("F2");
                }
                else
                {
                    inputValues[0].text = Sonat.IapModule.SonatIap.sn_ltv_iap.ToString("F2");
                }
                break;
            case CheatOption.ResetXmasEventPack:
                OnInputField(0);
                OnDropdown(0);
                break;
            case CheatOption.TradeCard:
                OnDropdown(1);
                InitTradeCardDropdown();
                break;
        }
    }

    private CheatLevelSource GetCheatLevelSource()
    {
        CheatLevelSource levelSource = cheatDropdowns[0].options[cheatDropdowns[0].value].text.ToEnum<CheatLevelSource>();
        return levelSource;
    }

    private void OnCheatGDLevelChange(int value)
    {
        CheatLevelSource levelSource = GetCheatLevelSource();

        switch (levelSource)
        {
            case CheatLevelSource.Resources:
                OnInputField(0);
                break;
            case CheatLevelSource.Drive:
                OnInputField(1);
                break;
        }
    }


    public void InitOpts()
    {
        List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();
        for (CheatOption i = CheatOption.Level; i < CheatOption.MAX; i++)
        {
            if (!Enum.IsDefined(typeof(CheatOption), i)) continue;
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }

        cheatOptsDropdown.ClearOptions();
        cheatOptsDropdown.options = opts;
        cheatOptsDropdown.onValueChanged.RemoveAllListeners();
        cheatOptsDropdown.onValueChanged.AddListener(OnCheatOptChange);
        OnCheatOptChange(0);
    }

    public void InitQuestEventDropdown()
    {
        List<TMP_Dropdown.OptionData> opts = new()
        {
            new()
            {
                text = "Item"
            },
            new()
            {
                text = "X2 Item"
            }
        };

        cheatDropdowns[0].onValueChanged.RemoveAllListeners();
        cheatDropdowns[0].ClearOptions();
        cheatDropdowns[0].options = opts;
    }
    public void InitResourceDropdown()
    {
        List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();

        for (GameResource i = GameResource.Coin; i < GameResource.END; i++)
        {
            if (!Enum.IsDefined(typeof(GameResource), i)) continue;
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }

        cheatDropdowns[0].onValueChanged.RemoveAllListeners();
        cheatDropdowns[0].ClearOptions();
        cheatDropdowns[0].options = opts;
    }

    public void InitDriveFolderDropdown()
    {
        CheatManager.DownloadLevelFolderData((subFolders) =>
        {
            List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();

            foreach (var subFolder in subFolders)
            {
                TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
                {
                    text = subFolder
                };
                opts.Add(data);
            }

            cheatDropdowns[0].onValueChanged.RemoveAllListeners();
            cheatDropdowns[0].ClearOptions();
            cheatDropdowns[0].options = opts;
        });
    }

    public void InitGDLevelDropdown()
    {
        int cheatLevelSource = PlayerPrefs.GetInt("SONAT_CHEATED_LEVELSOURCE", 0);
        List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();

        for (CheatLevelSource i = CheatLevelSource.Resources; i < CheatLevelSource.MAX; i++)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }

        cheatDropdowns[0].onValueChanged.RemoveAllListeners();
        cheatDropdowns[0].onValueChanged.AddListener(OnCheatGDLevelChange);
        cheatDropdowns[0].ClearOptions();
        cheatDropdowns[0].options = opts;
        cheatDropdowns[0].value = cheatLevelSource;
    }

    public void InitStoryDropdown()
    {
        List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();
        for (StoryBase.EStory i = StoryBase.EStory.Story_1; i < StoryBase.EStory.MAX; i++)
        {
            if (!Enum.IsDefined(typeof(StoryBase.EStory), i)) continue;
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }
        cheatDropdowns[0].onValueChanged.RemoveAllListeners();
        cheatDropdowns[0].ClearOptions();
        cheatDropdowns[0].options = opts;
    }

    public void OnCheatClick()
    {
        Cheat();
    }

    private void Cheat()
    {
        CheatOption cheatOption = GetCheatOpt();
        switch (cheatOption)
        {
            case CheatOption.Win:
                panel.SetActive(false);
                CheatManager.CheatWin();
                break;
            case CheatOption.WinState:
                panel.SetActive(false);
                CheatManager.CheatWinState();
                break;
            case CheatOption.Lose:
                Close();
                CheatManager.CheatLose();
                break;
            case CheatOption.Level:
                panel.SetActive(false);
                if (int.TryParse(inputValues[0].text, out var level))
                {
                    CheatManager.CheatLevel(level);
                }

                break;
            case CheatOption.Resource:
                if (int.TryParse(inputValues[0].text, out var value))
                {
                    GameResource resource = cheatDropdowns[0].options[cheatDropdowns[0].value].text.ToEnum<GameResource>();
                    CheatManager.CheatResource(resource, value);
                }

                break;
            case CheatOption.RemoteConfig:
                CheatManager.CheatRemoteConfig(inputValues[0].text, inputValues[1].text);
                break;
            case CheatOption.PlayerPrefs:
                CheatManager.CheatPlayerPrefs(inputValues[0].text, inputValues[1].text);
                break;
            case CheatOption.TransportTracking:
                StartTransportDebugView();
                break;
            case CheatOption.NoAds:
                CheatManager.CheatNoAds();
                break;
            case CheatOption.DownloadLevelDriveZip:
                int linkRow = 2;
                int.TryParse(inputValues[0].text, out linkRow);
                CheatManager.CheatLevelDrive(linkRow);
                break;
            case CheatOption.DownloadLevelDriveFolder:
                string folder = cheatDropdowns[0].options[cheatDropdowns[0].value].text;
                int levelFile = int.Parse(inputValues[0].text);
                CheatManager.CheatLevelDriveFolder(folder, levelFile, () =>
                {
                    panel.SetActive(false);
                    CheatManager.CheatLevel(levelFile);
                });
                break;
            // case CheatOption.StarChest:
            // 	if (int.TryParse(inputValues[0].text, out int chestID) && int.TryParse(inputValues[1].text, out int curStar))
            // 	{
            //                  panel.SetActive(false);
            //                  CheatManager.CheatStarChest(chestID, curStar);
            //              }
            //              break;
            case CheatOption.LevelChest:
                if (int.TryParse(inputValues[0].text, out int chestId) && int.TryParse(inputValues[1].text, out int curLevel))
                {
                    panel.SetActive(false);
                    CheatManager.CheatLevelChest(chestId - 1, curLevel);
                }
                break;
            case CheatOption.QuestEvent:

                var text = inputValues[0].text.Trim();
                var parts = text.Split(new[] { ',', ' ', ';' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (cheatDropdowns[0].value == 0) // item
                {
                    if (parts.Length >= 2 &&
                        int.TryParse(parts[0], out var milestone) &&
                        int.TryParse(parts[1], out var numItem))
                    {
                        CheatManager.CheatQuestEvent(milestone - 1, numItem);
                    }
                    else
                    {
                        Debug.LogWarning("[Cheat] Sai dinh dang");
                    }
                }
                else if (cheatDropdowns[0].value == 1) // x2 item
                {
                    if (int.TryParse(parts[0], out var numItem))
                    {
                        CheatManager.CheatX2QuestEvent(numItem);
                    }
                    else
                    {
                        Debug.LogWarning("[Cheat] Sai dinh dang");
                    }
                }
                panel.SetActive(false);

                break;
            case CheatOption.GDLevel:
                Screen.SetResolution(1920, 1080, false);
                MySonatFramework.GetService<SonatSceneService>().SwitchScene(GamePlacement.Tool, true);
                break;
            case CheatOption.Server:
                panel.SetActive(false);
                DataSyncKey dataSyncKey = cheatDropdowns[0].options[cheatDropdowns[0].value].text.ToEnum<DataSyncKey>();
                DataType dataType = cheatDropdowns[1].options[cheatDropdowns[1].value].text.ToEnum<DataType>();
                CheatManager.CheatServer(dataSyncKey, dataType, inputValues[0].text);
                break;
            case CheatOption.ServerDeleteUserData:
                panel.SetActive(false);
                ServerHelper.DeleteUser();
                break;
            case CheatOption.UserCampaignSegment:
                UserData.UserCampaignSegment.Value = inputValues[0].text;

                var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
                consecutiveWinService.LoadRemote();

                Debug.Log("[UserCampaignSegment] = " + UserData.UserCampaignSegment.Value);

                break;
            case CheatOption.Leaderboard:
                string curOpt = cheatDropdowns[0].options[cheatDropdowns[0].value].text;

                if (curOpt == "Force unlock")
                {
                    if (inputValues[0].text.ToLower() == "false")
                        LeaderboardService.forceUnlock = false;
                    else if (inputValues[0].text.ToLower() == "true")
                        LeaderboardService.forceUnlock = true;
                }
                else if (curOpt == "Limit top")
                {
                    LeaderboardService.limitTopLeaderboard = int.Parse(inputValues[0].text);
                }
                break;
            case CheatOption.Team:
                string s = cheatDropdowns[0].options[cheatDropdowns[0].value].text;

                if (s == "Force unlock")
                {
                    if (inputValues[0].text.ToLower() == "false")
                        TeamService.forceUnlock = false;
                    else if (inputValues[0].text.ToLower() == "true")
                        TeamService.forceUnlock = true;
                }

                break;
            case CheatOption.CardCollection:
                //CardType cardType = cheatDropdowns[0].options[cheatDropdowns[0].value].text.ToEnum<CardType>();
                //CheatManager.CheatCardCollection(cardType);
                OpenCard(cheatDropdowns[0].options[cheatDropdowns[0].value].text);

                break;

            case CheatOption.Winstreak:
                ChangeWinstreakDayUnlock(inputValues[0].text.ToLower());
                break;
            case CheatOption.PlayStoryVideo:
                OnPlayVideoStory();
                break;
            case CheatOption.Pinata_Winstreak:
                if (int.TryParse(inputValues[0].text, out var v))
                {
                    PinataService.Instance.CheatWinARow(v);
                }
                break;
            case CheatOption.Pinata_Auto_Gold:
                PinataService.CheatAutoGold = true;
                break;
            case CheatOption.TeamOffer_Reset:
                MySonatFramework.GetService<TeamOfferService>().ResetPurchase();
                break;  
            case CheatOption.BlackFriday:
                if (float.TryParse(inputValues[0].text, out float ltv))
                {
                    // Set CheatLTV trong UserGroupService
                    userGroupService.Instance.SetCheatLTV(ltv);
                    
                    // Recalculate user group cho BlackFriday event
                    MySonatFramework.GetService<BlackFridayService>().RecalculateUserGroup();

                    panel.SetActive(false);
                }
                break;
            case CheatOption.ResetXmasEventPack:
                // Reset XmasEvent data
                MySonatFramework.GetService<XmasEventService>().CheatResetEventData();
                panel.SetActive(false);
                Debug.Log("[Cheat] XmasEvent data reset");
                break;
            case CheatOption.TradeCard:
                string opt = cheatDropdowns[0].options[cheatDropdowns[0].value].text;
                if (opt.Equals("Reset Send Limit"))
                    MySonatFramework.GetService<CardCollectionService>().ResetLimitSendCard();
                else if (opt.Equals("Reset Request Card Gap"))
                    MySonatFramework.GetService<CardCollectionService>().ResetTimeRequestCard();
                break;
        }
    }

    private void OnPlayVideoStory()
    {
        MySonatFramework.GetService<StoryService>().PlayVideoByID_Cheating((StoryBase.EStory)cheatDropdowns[0].value + 1);
    }

    private void OnDropdown(int number)
    {
        for (int i = 0; i < cheatDropdowns.Length; i++)
        {
            cheatDropdowns[i].gameObject.SetActive(i < number);
        }
    }
    private void OnInputField(int number)
    {
        for (int i = 0; i < inputValues.Length; i++)
        {
            inputValues[i].gameObject.SetActive(i < number);
            inputValues[i].text = "";
        }
    }
    private void OnInputFieldQuestEvent(int number)
    {
        Service<QuestEventService> service = new();

        for (int i = 0; i < inputValues.Length; i++)
        {
            inputValues[i].gameObject.SetActive(i < number);

            if (i == 0)
                inputValues[i].text = $"{service.Instance.Data.currentQuestIdx + 1};{service.Instance.Data.numItem}";
            else if (i == 1)
                inputValues[i].text = $"";
            else
                inputValues[i].text = "";
        }
    }
    private void OnInputFieldLevelChest(int number)
    {
        Service<ChestRewardService> _chestRewardService = new();

        for (int i = 0; i < inputValues.Length; i++)
        {
            inputValues[i].gameObject.SetActive(i < number);

            if (i == 0)
                inputValues[i].text = $"{_chestRewardService.Instance.data.currentChestIndex + 1}";
            else if (i == 1)
                inputValues[i].text = $"{_chestRewardService.Instance.data.currentProgress}";
            else
                inputValues[i].text = "";
        }
    }

    private void SwitchLocalTime(bool state)
    {
        SonatSystem.GetService<SonatTimeService>().SetUsingLocalTime(state);
    }

    public void OnOffCheat()
    {
        panel.SetActive(!panel.activeInHierarchy);
        // if (!panel.activeInHierarchy)
        // {
        //     EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent() { gameState = GameState.Playing });
        // }
        // else
        // {
        //     EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent() { gameState = GameState.Paused });
        // }
    }

    public void ShowFPSLog()
    {
        fpsLog.gameObject.SetActive(true);
    }

    public void ShowDebugLog()
    {
        SonatDebugView.OpenDebugLogScreen();
    }

    public void CheckSdkInspector()
    {
        SonatSdkInspector.GetInstance().ShowInfo();
    }

    public void StartTransportDebugView()
    {
        ipAdress.Value = inputValues[0].text;
        SonatDebugView.StartTransportDebugView(ipAdress.Value);
        Close();
    }

    public void StopTransportDebugView()
    {
        SonatDebugView.StopTransportDebugView();
    }

    private void ShowDebugConsole(bool state)
    {
        if (state)
        {
            if (debugLogConsole == null)
            {
                debugLogConsole = Instantiate(debugLogConsolePrefab);
            }
        }
        else
        {
            if (debugLogConsole != null)
            {
                Destroy(debugLogConsole);
            }
        }
    }

    public void InitDataSyncKeyDropdown(int number)
    {
        List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();

        foreach (DataSyncKey i in Enum.GetValues(typeof(DataSyncKey)))
        {
            if (!Enum.IsDefined(typeof(DataSyncKey), i)) continue;
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }

        cheatDropdowns[number].onValueChanged.RemoveAllListeners();
        cheatDropdowns[number].ClearOptions();
        cheatDropdowns[number].options = opts;
    }

    public void InitDataTypeDropdown(int number)
    {
        List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();

        foreach (DataType i in Enum.GetValues(typeof(DataType)))
        {
            if (!Enum.IsDefined(typeof(DataType), i)) continue;
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }

        cheatDropdowns[number].onValueChanged.RemoveAllListeners();
        cheatDropdowns[number].ClearOptions();
        cheatDropdowns[number].options = opts;
    }

    public void InitLeaderboardDropdown(int number)
    {
        List<TMP_Dropdown.OptionData> opts = new()
        {
            new() { text = "Force unlock" },
            new() { text = "Limit top" },
        };

        cheatDropdowns[number].onValueChanged.RemoveAllListeners();
        cheatDropdowns[number].ClearOptions();
        cheatDropdowns[number].options = opts;
    }

    public void InitTeamDropdown(int number)
    {
        List<TMP_Dropdown.OptionData> opts = new()
        {
            new() { text = "Force unlock" },
        };

        cheatDropdowns[number].onValueChanged.RemoveAllListeners();
        cheatDropdowns[number].ClearOptions();
        cheatDropdowns[number].options = opts;
    }

    public void InitCardCollectionDropdown()
    {
        //List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();
        //foreach (CardType i in Enum.GetValues(typeof(CardType)))
        //{
        //    opts.Add(new TMP_Dropdown.OptionData() { text = i.ToString() });
        //}

        //cheatDropdowns[0].onValueChanged.RemoveAllListeners();
        //cheatDropdowns[0].ClearOptions();
        //cheatDropdowns[0].options = opts;

        List<string> cards = new();

        for (int i = ((int)GameResource.Card_Randomx1); i <= ((int)GameResource.Card_Randomx6); i++)
        {
            cards.Add(((GameResource)i).ToString());
        }
        cheatDropdowns[0].options.Clear();

        cheatDropdowns[0].AddOptions(cards);
    }

    public void InitTradeCardDropdown()
    {
        List<TMP_Dropdown.OptionData> opts = new()
        {
            new() { text = "Reset Send Limit" },
            new() { text = "Reset Request Card Gap" },
        };

        cheatDropdowns[0].onValueChanged.RemoveAllListeners();
        cheatDropdowns[0].ClearOptions();
        cheatDropdowns[0].options = opts;
    }


    // private void OnEnable()
    // {
    // 	StartCoroutine(CountTime());
    // }
    //
    // IEnumerator CountTime()
    // {
    // 	long now = SonatSystemManager.serviceManager.Get<ITimeService>().GetUnixTimeSeconds();
    // 	while (true)
    // 	{
    // 		timeNow.text = now.ToString();
    // 		yield return new WaitForSeconds(1);
    // 		now++;
    // 	}
    // }

    public void ResetTutorial()
    {
        foreach (TutorialType tutorial in Enum.GetValues(typeof(TutorialType)))
        {
            PlayerPrefs.SetInt($"{tutorial}Showed", 0);
        }
    }
}