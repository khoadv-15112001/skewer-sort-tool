
using System;

namespace GrillSort.OnlineService
{
    public static class DataSyncHelper
    {
        public static Type GetType(DataSyncKey key)
        {
            switch (key)
            {
                // ----------int ------------------------
                case DataSyncKey.UserLevel_Classic:
                case DataSyncKey.sonat_sdk_classic_level:

                case DataSyncKey.LastDay:
                case DataSyncKey.SessionToday:

                case DataSyncKey.Game_Resource_Coin:
                case DataSyncKey.Game_Resource_Lives:
                case DataSyncKey.Game_Resource_PiggyPoint:
                case DataSyncKey.Game_Resource_Star:
                case DataSyncKey.Game_Resource_PreBoosterFreeze:
                case DataSyncKey.Game_Resource_PreBoosterMagnet:
                case DataSyncKey.Game_Resource_PreBoosterDoubleStar:
                case DataSyncKey.Game_Resource_BoosterFreeze:
                case DataSyncKey.Game_Resource_BoosterShuffle:
                case DataSyncKey.Game_Resource_BoosterMagnet:
                case DataSyncKey.Game_Resource_BoosterMagicKey:
                case DataSyncKey.Game_Resource_BoosterMagicWand:
                case DataSyncKey.Game_Resource_BoosterBlowTorch:

                case DataSyncKey.BoosterFreeze_DATA:
                case DataSyncKey.BoosterShuffle_DATA:
                case DataSyncKey.BoosterMagnet_DATA:
                case DataSyncKey.BoosterMagicKey_DATA:
                case DataSyncKey.BoosterMagicWand_DATA:
                case DataSyncKey.BoosterBlowTorch_DATA:

                case DataSyncKey.PreBoosterFreeze_DATA:
                case DataSyncKey.PreBoosterMagnet_DATA:
                case DataSyncKey.PreBoosterDoubleStar_DATA:

                case DataSyncKey.IsNoAds:

                case DataSyncKey.IsUnLimitedLive:
                case DataSyncKey.ReFillLivesFreeCount:
                case DataSyncKey.LIVES_SETUP_FIRST_TIME:

                case DataSyncKey.LuckySpin_Data_unlockLuckySpin:

                case DataSyncKey.DailyGift_Data_30_isUnlocked:
                case DataSyncKey.DailyGift_Data_7_isUnlocked:

                case DataSyncKey.PIGGY_BANK_DATA_isUnlocked:

                case DataSyncKey.QuestEventData_isUnlocked:

                case DataSyncKey.PROFILE_AVATAR_ID:
                case DataSyncKey.PROFILE_FRAME_ID:

                case DataSyncKey.EndlessTreasureData_isUnlocked:

                case DataSyncKey.BATTLEPASS_DATA_GENERAL_currentMilestoneIdx:
                case DataSyncKey.BATTLEPASS_DATA_GENERAL_isUnlocked:
                case DataSyncKey.BATTLEPASS_DATA_FREE_isUnlocked:
                case DataSyncKey.BATTLEPASS_DATA_PREMIUM_isUnlocked:
                case DataSyncKey.Game_Resource_BattlePass_Key:

                case DataSyncKey.WIN_STREAK_ACTIVE:
                case DataSyncKey.WIN_STREAK_LIVES:
                case DataSyncKey.WIN_STREAK_IN_AROW:

                case DataSyncKey.CARD_COLLECTION_isUnlocked:
                case DataSyncKey.CARD_COLLECTION_cardStar:
                case DataSyncKey.CARD_COLLECTION_cardStarExchangeIndex:
                case DataSyncKey.CARD_COLLECTION_isCompleteCardCollection:
                case DataSyncKey.CARD_COLLECTION_numCompleteCardCollection:
                case DataSyncKey.CARD_COLLECTION_AppearTut:

                case DataSyncKey.HLW_EVENT_DATA_isUnlocked:
                case DataSyncKey.HLW_EVENT_DATA_checkReceiveEventReward:
                case DataSyncKey.ShowPopupStart_HLW:
                case DataSyncKey.lava_quest_is_unlocked:
                case DataSyncKey.lava_quest_is_shown_popup:
                case DataSyncKey.lava_quest_is_joined:
                case DataSyncKey.lava_quest_noti_tut:
                case DataSyncKey.lava_quest_step:
                case DataSyncKey.lava_quest_save_coin:
                case DataSyncKey.lava_quest_is_expired:
                case DataSyncKey.lava_quest_start_count:
                case DataSyncKey.lava_quest_state:

                case DataSyncKey.kitchen_mission_is_unlocked:
                case DataSyncKey.kitchen_mission_is_shown_popup:
                case DataSyncKey.kitchen_mission_is_shown_tut:
                case DataSyncKey.kitchen_mission_is_event_active:
                case DataSyncKey.kitchen_mission_is_joined:
                case DataSyncKey.kitchen_mission_stage:
                case DataSyncKey.kitchen_mission_is_over:
                case DataSyncKey.kitchen_mission_is_complete_event:
                case DataSyncKey.kitchen_mission_num_use_booster:
                case DataSyncKey.kitchen_mission_num_use_revive:
                case DataSyncKey.kitchen_mission_is_expired_modify_bot:
                case DataSyncKey.kitchen_mission_is_bought_pack:
                case DataSyncKey.kitchen_mission_start_count:
                case DataSyncKey.kitchen_mission_is_log_start:

                case DataSyncKey.pinata_is_unlocked:
                case DataSyncKey.pinata_is_active:
                case DataSyncKey.pinata_win_a_row:
                case DataSyncKey.pinata_current_stage:
                case DataSyncKey.pinata_start_count:

                case DataSyncKey.Game_Resource_Avatar_Battle_Pass:
                case DataSyncKey.Game_Resource_Badge_Battle_Pass:
                case DataSyncKey.Game_Resource_Avatar_Kitchen_Mission:
                case DataSyncKey.Game_Resource_Badge_Kitchen_Mission:
                case DataSyncKey.Game_Resource_Avatar_Battle_Pass_Thanksgiving:
                case DataSyncKey.Game_Resource_Badge_Battle_Pass_Thanksgiving:

                case DataSyncKey.story_current_story_index:
                case DataSyncKey.story_max_level_story:
                case DataSyncKey.story_level_unlock_1:
                case DataSyncKey.story_level_unlock_2:
                case DataSyncKey.story_level_unlock_3:
                case DataSyncKey.story_level_unlock_4:
                case DataSyncKey.story_level_unlock_5:

                case DataSyncKey.rail_is_unlocked:
                case DataSyncKey.rail_is_active:
                case DataSyncKey.rail_is_join:
                case DataSyncKey.rail_has_ever_joined:
                case DataSyncKey.rail_is_completed:
                case DataSyncKey.rail_has_shown_first_popup:
                case DataSyncKey.rail_current_stage:
                case DataSyncKey.rail_current_milestone:
                case DataSyncKey.rail_last_token:

                    return typeof(int);

                // ----------string ------------------------
                case DataSyncKey.FirstTimeOpen:
                case DataSyncKey.LastTimeOpen:

                case DataSyncKey.item_purchased:

                case DataSyncKey.TimeFinishUnlimitedLive:
                case DataSyncKey.TimeStartCountRefill:

                case DataSyncKey.RealTime_Data:
                case DataSyncKey.LuckySpin_Data:
                case DataSyncKey.ChestRewardService_Data:
                case DataSyncKey.DailyGift_Data_30:
                case DataSyncKey.DailyGift_Data_7:
                case DataSyncKey.PIGGY_BANK_DATA:
                case DataSyncKey.QuestEventData:
                case DataSyncKey.QuestEventData_expiredTime:
                case DataSyncKey.ConsecutiveWinData:

                case DataSyncKey.PROFILE_NAME:

                case DataSyncKey.EndlessTreasureData:

                case DataSyncKey.BATTLEPASS_DATA_GENERAL_expireTime:
                case DataSyncKey.BATTLEPASS_DATA_FREE_listReceivedReward:
                case DataSyncKey.BATTLEPASS_DATA_PREMIUM_listReceivedReward:

                case DataSyncKey.WIN_STREAK_MILESTONE_CLAIMED:
                case DataSyncKey.WIN_STREAK_TIME_FINISH:
                case DataSyncKey.WIN_STREAK_NEXT_TIME:

                case DataSyncKey.CARD_COLLECTION_collectedCards:
                case DataSyncKey.CARD_COLLECTION_newCards:
                case DataSyncKey.CARD_COLLECTION_completedAlbumTypes:
                case DataSyncKey.CARD_COLLECTION_expireTime:

                case DataSyncKey.HLW_EVENT_DATA_expiredTime:
                case DataSyncKey.HLW_EVENT_DATA_timeActive:

                case DataSyncKey.lava_quest_players:
                case DataSyncKey.lava_quest_time_end:
                case DataSyncKey.lava_quest_next_time_event:

                case DataSyncKey.kitchen_mission_time_start_event:
                case DataSyncKey.kitchen_mission_time_end_event:
                case DataSyncKey.kitchen_mission_players:
                case DataSyncKey.kitchen_mission_last_time_cycle:

                case DataSyncKey.rail_last_free_token_claim_time:
                case DataSyncKey.rail_current_event_start_time:
                case DataSyncKey.rail_event_cooldown_end_time:
                case DataSyncKey.pinata_last_complete_cycle_unix:

                    return typeof(string);

            }
            return typeof(object);
        }
    }
    public enum DataSyncKey
    {
        UserLevel_Classic,
        sonat_sdk_classic_level,


        FirstTimeOpen,
        LastTimeOpen,
        LastDay,
        SessionToday,

        // Game resoure
        Game_Resource_Coin,
        Game_Resource_Lives,
        Game_Resource_PiggyPoint,
        Game_Resource_Star,

        // Pre booster
        Game_Resource_PreBoosterFreeze,// = 10,
        Game_Resource_PreBoosterMagnet,
        Game_Resource_PreBoosterDoubleStar,

        PreBoosterFreeze_DATA,
        PreBoosterMagnet_DATA,
        PreBoosterDoubleStar_DATA,

        // Booster
        Game_Resource_BoosterFreeze,
        Game_Resource_BoosterShuffle,
        Game_Resource_BoosterMagnet,
        Game_Resource_BoosterMagicKey,
        Game_Resource_BoosterMagicWand, // = 20,
        Game_Resource_BoosterBlowTorch,

        BoosterFreeze_DATA,
        BoosterShuffle_DATA,
        BoosterMagnet_DATA,
        BoosterMagicKey_DATA,
        BoosterMagicWand_DATA,
        BoosterBlowTorch_DATA,

        //
        item_purchased,

        // Is no ads
        IsNoAds,

        // Lives
        TimeFinishUnlimitedLive, // = 30,
        TimeStartCountRefill,
        IsUnLimitedLive,
        ReFillLivesFreeCount,
        LIVES_SETUP_FIRST_TIME,

        // Real time data
        RealTime_Data,

        // Lucky spin
        LuckySpin_Data,
        LuckySpin_Data_unlockLuckySpin,

        // Chest reward
        ChestRewardService_Data,

        // Daily gift
        DailyGift_Data_30,
        DailyGift_Data_30_isUnlocked,
        DailyGift_Data_7,
        DailyGift_Data_7_isUnlocked,

        // Piggy bank
        PIGGY_BANK_DATA,
        PIGGY_BANK_DATA_isUnlocked,

        // Quest event
        QuestEventData,
        QuestEventData_expiredTime,
        QuestEventData_isUnlocked,

        // Consecutive win
        ConsecutiveWinData,

        // PROFILE_AVATAR_ID
        PROFILE_AVATAR_ID,
        PROFILE_FRAME_ID,
        PROFILE_NAME,

        // Endless treasure
        EndlessTreasureData,
        EndlessTreasureData_isUnlocked,

        // Battle pass
        Game_Resource_BattlePass_Key,
        BATTLEPASS_DATA_GENERAL_currentMilestoneIdx,
        BATTLEPASS_DATA_GENERAL_expireTime,
        BATTLEPASS_DATA_GENERAL_isUnlocked,
        BATTLEPASS_DATA_FREE_isUnlocked,
        BATTLEPASS_DATA_FREE_listReceivedReward,
        BATTLEPASS_DATA_PREMIUM_isUnlocked,
        BATTLEPASS_DATA_PREMIUM_listReceivedReward,

        // Win streak
        WIN_STREAK_ACTIVE,
        WIN_STREAK_LIVES,
        WIN_STREAK_IN_AROW,
        WIN_STREAK_MILESTONE_CLAIMED,
        WIN_STREAK_TIME_FINISH,
        WIN_STREAK_NEXT_TIME,

        // Card collection
        CARD_COLLECTION_isUnlocked,
        CARD_COLLECTION_collectedCards,
        CARD_COLLECTION_newCards,
        CARD_COLLECTION_cardStar,
        CARD_COLLECTION_completedAlbumTypes,
        CARD_COLLECTION_cardStarExchangeIndex,
        CARD_COLLECTION_isCompleteCardCollection,
        CARD_COLLECTION_numCompleteCardCollection,
        CARD_COLLECTION_expireTime,
        CARD_COLLECTION_AppearTut,

        HLW_EVENT_DATA_isUnlocked,
        HLW_EVENT_DATA_expiredTime,
        HLW_EVENT_DATA_checkReceiveEventReward,
        HLW_EVENT_DATA_timeActive,
        ShowPopupStart_HLW,

        // Lava Quest
        lava_quest_is_unlocked,
        lava_quest_is_shown_popup,
        lava_quest_is_joined,
        lava_quest_noti_tut,
        lava_quest_step,
        lava_quest_save_coin,
        lava_quest_players,
        lava_quest_time_end,
        lava_quest_next_time_event,
        lava_quest_is_expired,
        lava_quest_start_count,
        lava_quest_state,

        // Kitchen Mission
        kitchen_mission_is_unlocked,
        kitchen_mission_is_shown_popup,
        kitchen_mission_is_shown_tut,
        kitchen_mission_is_event_active,
        kitchen_mission_is_joined,
        kitchen_mission_time_start_event,
        kitchen_mission_time_end_event,
        kitchen_mission_stage,
        kitchen_mission_players,
        kitchen_mission_last_time_cycle,
        kitchen_mission_is_over,
        kitchen_mission_is_complete_event,
        kitchen_mission_num_use_booster,
        kitchen_mission_num_use_revive,
        kitchen_mission_is_expired_modify_bot,
        kitchen_mission_is_bought_pack,
        kitchen_mission_start_count,
        kitchen_mission_is_log_start,

        // Pinata
        pinata_is_unlocked,
        pinata_is_active,
        pinata_win_a_row,
        pinata_current_stage,
        pinata_start_count,
        pinata_last_complete_cycle_unix,

        // Avatar
        Game_Resource_Avatar_Battle_Pass,
        Game_Resource_Badge_Battle_Pass,

        Game_Resource_Avatar_Kitchen_Mission,
        Game_Resource_Badge_Kitchen_Mission,

        Game_Resource_Avatar_Battle_Pass_Thanksgiving,
        Game_Resource_Badge_Battle_Pass_Thanksgiving,

        // Story
        story_current_story_index,
        story_max_level_story,
        story_level_unlock_1,
        story_level_unlock_2,
        story_level_unlock_3,
        story_level_unlock_4,
        story_level_unlock_5,

        // Rail Event
        rail_is_unlocked,
        rail_is_active,
        rail_is_join,
        rail_has_ever_joined,
        rail_is_completed,
        rail_has_shown_first_popup,
        rail_current_stage,
        rail_current_milestone,
        rail_last_token,
        rail_last_free_token_claim_time,
        rail_current_event_start_time,
        rail_event_cooldown_end_time
    }

    public enum DataType
    {
        None,
        String,
        Int,
        Float
    }
}