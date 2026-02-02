using System.Collections.Generic;
using Newtonsoft.Json;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using UnityEngine;

namespace Manager
{
    public static class GameRemoteConfigValue
    {
        public static bool showNativeAds;
        public static int voiceInterval;
        public static int levelForceHome;
        public static int levelAppearLuckySpin;
        public static int levelBeginReceivePiggyPoint;
        public static AudioId bgrMusic;
        public static bool forceTutBooster;
        public static int numberSpecialItemPerLevel;
        public static int iceGrillStep = 4;
        public static int iceGrillStep_SkewerJam = 6;
        public static int itemBombLimit;
        public static bool popupUnlockTray;
        public static int numberItemCanMergeAfterShuffle = 5;
        public static int levelShowInterLose;
        public static int levelShowInterWin;
        public static int levelShowInterReplay;
        public static bool localizeJapan;
        public static int countLoseToShowOffer;
        public static bool noCharacter;
        public static int timeRevive;
        public static bool noOrder;
        public static int maxPopupMO;
        public static int coinRevive;

        public static bool shuffleGrills;

        //public static bool noRewarded;
        public static bool swapItem;
        public static bool bonusTime;
        public static bool shuffleItemIds;
        public static LevelReplayData levelReplayData;
        public static int starRate;

        public static void LoadData()
        {
            voiceInterval = SonatSDKAdapter.GetRemoteInt("voice_interval", 5);
            levelForceHome = SonatSDKAdapter.GetRemoteInt("levelForceHome", 6);
            levelAppearLuckySpin = SonatSDKAdapter.GetRemoteInt("levelAppearLuckySpin", 11);
            levelBeginReceivePiggyPoint = SonatSDKAdapter.GetRemoteInt("levelBeginReceivePiggyPoint", 2);
            bgrMusic = SonatSDKAdapter.GetRemoteConfig<AudioId>("bgr_music", AudioId.BGM_Ingame_Summer);
            forceTutBooster = SonatSDKAdapter.GetRemoteBool("force_tut_booster", false);
            numberSpecialItemPerLevel = SonatSDKAdapter.GetRemoteInt("number_special_item_per_level", 3);
            iceGrillStep = SonatSDKAdapter.GetRemoteInt("ice_grill_step", 4);
            iceGrillStep_SkewerJam = SonatSDKAdapter.GetRemoteInt("ice_grill_step_skewer_jam", 6);
            itemBombLimit = SonatSDKAdapter.GetRemoteInt("item_bomb_move_limit", 10);
            popupUnlockTray = SonatSDKAdapter.GetRemoteBool("popup_unlock_tray", true);
            numberItemCanMergeAfterShuffle = SonatSDKAdapter.GetRemoteInt("number_item_can_merge_after_shuffle", 5);
            levelShowInterLose = SonatSDKAdapter.GetRemoteInt("level_show_inter_lose", 0);
            levelShowInterWin = SonatSDKAdapter.GetRemoteInt("level_show_inter_win", 0);
            levelShowInterReplay = SonatSDKAdapter.GetRemoteInt("level_show_inter_replay", 0);
            localizeJapan = SonatSDKAdapter.GetRemoteBool("localize_japan", false);
            PlayerPrefs.SetInt("localize_japan", localizeJapan ? 1 : 0);
            Debug.Log($"anhnt: haskey localize_japan={PlayerPrefs.HasKey("localize_japan")}; remote localize_japan = {localizeJapan} ");

            //PlayerPrefs.SetInt("localize_japan", localizeJapan ? 1 : 0);
            countLoseToShowOffer = SonatSDKAdapter.GetRemoteInt("count_lose_to_show_offer", 0);
            noCharacter = SonatSDKAdapter.GetRemoteBool("no_character", true);
            timeRevive = SonatSDKAdapter.GetRemoteInt("time_revive", 45);
            noOrder = SonatSDKAdapter.GetRemoteBool("no_order", false);
            maxPopupMO = SonatSDKAdapter.GetRemoteInt("max_popup_mo", 1);
            coinRevive = SonatSDKAdapter.GetRemoteInt("coin_revive", 150);
            shuffleGrills = SonatSDKAdapter.GetRemoteBool("shuffle_grills", true);
            //noRewarded = SonatSDKAdapter.GetRemoteBool("no_rewarded", false);
            swapItem = SonatSDKAdapter.GetRemoteBool("swap_item", false);
            if (PlayerPrefs.HasKey("cheat_swap_item"))
                swapItem = PlayerPrefs.GetInt("cheat_swap_item") == 1 ? true : false;

            bonusTime = SonatSDKAdapter.GetRemoteBool("bonus_time", false);
            shuffleItemIds = SonatSDKAdapter.GetRemoteBool("shuffle_item_ids", false);
            starRate = SonatSDKAdapter.GetRemoteInt("star_rate", 4);

            LevelReplayData levelReplayDataDefault = new LevelReplayData();
            levelReplayDataDefault.levelReplayData.Add(6, new Dictionary<int, int>() { { 1, 0 }, { 3, 1 }, { 5, 2 } });
            levelReplayDataDefault.levelReplayData.Add(7, new Dictionary<int, int>() { { 1, 0 }, { 3, 1 }, { 5, 2 } });
            levelReplayDataDefault.levelReplayData.Add(8, new Dictionary<int, int>() { { 1, 0 }, { 3, 1 }, { 5, 2 } });
            Debug.Log("Level Replay Data: " + JsonConvert.SerializeObject(levelReplayDataDefault));
            levelReplayData = SonatSDKAdapter.GetRemoteConfig("level_replay_data", levelReplayDataDefault);
        }
    }

    public class LevelReplayData
    {
        public Dictionary<int, Dictionary<int, int>> levelReplayData = new Dictionary<int, Dictionary<int, int>>();

        public int GetCategory(int level, int startCount)
        {
            int category = 0;
            if (levelReplayData.TryGetValue(level, out Dictionary<int, int> replayData))
            {
                foreach (var data in replayData)
                {
                    if (data.Key > startCount) break;
                    category = data.Value;
                }
            }

            return category;
        }
    }
}