#if CUSTOM_ENUM
namespace Sonat.Enums
{
    public enum AudioId : ushort
    {
        None = 0,
        ButtonClick = 1,
        Win_Music,
        Lose_Music,
        BGM_Ingame_Summer,
        Items_Merge,
        Items_Pick,
        Items_Put,
        Coin_Received,
        Obstacle_Chain_01,
        Obstacle_Chain_02,
        Obstacle_Chain_03,
        Items_Fly_Whoosh,
        Stars_Fill,
        Items_Collected,
        Chest_Level_Appear,
        Chest_Level_Idle,
        Chest_Level_Open,
        Time_Warning,
        Time_Count,


        Items_Pick_0 = 20,
        Items_Pick_1 = 21,
        Items_Pick_2 = 22,
        Items_Put_0 = 30,
        Items_Put_1 = 31,
        Items_Put_2 = 32,

        Items_Direct_Pick_Grill_sort,
        Items_Direct_Put_Grill_sort,
        Items_Glass_Put_Grill_sort,
        Items_Tray_Put_Grill_sort,

        Items_Merge_combo_1 = 41,
        Items_Merge_combo_2,
        Items_Merge_combo_3,
        Items_Merge_combo_4,
        Items_Merge_combo_5,
        Items_Merge_combo_6,
        Items_Merge_combo_7,
        Items_Merge_combo_8,
        Items_Merge_combo_9,
        Items_Merge_combo_10,
        Items_Merge_combo_11,
        Items_Merge_combo_12,
        Items_Merge_combo_13,

        Items_Special_Merge_Grill_sort,

        Obstacle_Ice_break_Grill_sort_01 = 60,
        Obstacle_Ice_break_Grill_sort_02 = 61,
        Obstacle_Ice_break_Grill_sort_03 = 62,

        Obstacle_Bomb_counting_Grill_sort = 65,
        Obstacle_Bomb_explosion_Grill_sort,
        Items_Merge_SMode_Drop_line_Grill_sort,
        Shelf_Close_SMode_Drop_line_Grill_sort,

        Obstacle_Locknkey_Open_Grill_sort,
        Obstacle_Locknkey_Turning_Grill_sort,


        VOICE = 100,
        Voice_Char_woman_Finish_01,
        Voice_Char_woman_Finish_02,
        Voice_Char_woman_Finish_03,
        Voice_Char_woman_Finish_04,

        Voice_Char_man_Finish_01,
        Voice_Char_man_Finish_02,
        Voice_Char_man_Finish_03,
        Voice_Char_man_Finish_04,
        Voice_Char_man_Finish_05,
        Voice_Char_man_Finish_06,

        Voice_Char_man_Combo_Good = 120,
        Voice_Char_man_Combo_Wow,
        Voice_Char_man_Combo_Excellent,
        Voice_Char_man_Combo_Amazing,
        Voice_Char_man_Combo_Incredible,

        Voice_Char_woman_Combo_Good = 125,
        Voice_Char_woman_Combo_Wow,
        Voice_Char_woman_Combo_Excellent,
        Voice_Char_woman_Combo_Amazing,
        Voice_Char_woman_Combo_Incredible,

        Voice_Char_man_JP_Finish_Arigatou = 130,
        Voice_Char_man_JP_Finish_Osu,
        Voice_Char_man_JP_Finish_Yosh,

        Voice_Char_woman_JP_Finish_Arigatou,
        Voice_Char_woman_JP_Finish_Aha,
        Voice_Char_woman_JP_Finish_Hai,
        
        // Additional Combo Text Voices (for future use) - placed after Japanese voices
        Voice_Char_woman_Combo_Tasty = 136,       // Combo 6
        Voice_Char_woman_Combo_Awesome,           // Combo 8
        Voice_Char_woman_Combo_Wonderful,         // Combo 12
        Voice_Char_woman_Combo_Perfect,           // Combo 14
        Voice_Char_woman_Combo_Unbelievable,      // Combo 18
        Voice_Char_woman_Combo_SmokinHot,         // Combo 20

        Piggy_Coins_Appear = 200,
        Piggy_Coins_Drop,
        Piggy_Pop_up_Pig_appear_sfx_Grill_sort,
        Piggy_Pop_up_Pig_full_sfx_Grill_sort,

        Booster_Spatula_Appear_Grill_sort = 300,
        Booster_Shuffle_Appear_Grill_sort,
        Booster_Freeze_Appear_Grill_sort,
        Booster_Key_Appear_Grill_sort,
        Booster_Out_Grill_sort,
        Booster_Received_Grill_sort,

        Home_Transition_Logo_appear_sfx_ingame_Grill_sort = 325,
        Home_Transition_Logo_out_sfx_ingame_Grill_sort,

        Spin_Rolling_Grill_sort,
        Spin_Win_prize_Grill_sort,

        // Black Friday Event
        Friday_sale_Box_Appear_Grill_sort,
        Friday_sale_Music_Grill_sort,
        Friday_sale_Spin_Rolling_Grill_sort,
        Friday_sale_Discount_60_Grill_sort,
        Friday_sale_Discount_40_Grill_sort,
        Friday_sale_Discount_30_Grill_sort,
        Friday_sale_Discount_20_Grill_sort,

        Items_Merge_SMode_HLW_Grill_sort,
        Items_Pick_SMode_HLW_Grill_sort,

        PreWin_Music,
        Lose_HLW_Music_Grill_sort,
        Lose_HLW_Panel_Outofmove_Appear_Grill_sort,
        Lose_HLW_Panel_Outofmove_Out_Grill_sort,
        Pre_win_HLW_sound_Grill_sort,
        Win_HLW_Music_fireworks_Grill_sort,
        Win_HLW_Particle_loop_Grill_sort,
        Win_HLW_Pumkin_Received_Grill_sort,







        BGM_Ingame_Summer_old = 1000,
        BGM_Ingame_Japan_Grill_sort,
        BGM_Home_summer_Grill_sort,
        BGM_Home_summer_Grill_sort_01,

        BGM_Ingame_Halloween_Grill_sort,
        BGM_Ingame_Halloween_01_Grill_sort,

        BGM_Home_Japan_01_Grill_sort,
        BGM_Home_Japan_02_Grill_sort,

        BGM_Ingame_Japan_01_Grill_sort,

        // card collection
        Card_collection_Appear_open_Grill_sort = 1010,
        Card_Appear_Grill_sort,
        Card_Disappear_Grill_sort,
        Stars_Fill_Grill_sort,
        // item quest event
        Items_Special_Collected_Grill_sort,
        // winstreak
        WinStreak_Chest_Appear_Grill_sort,
        WinStreak_Chest_Open_Grill_sort,
        WinStreak_Lose1_Grill_sort,
        WinStreak_Lose2_Grill_sort,
        WinStreak_Lose3_Grill_sort,
        WinStreak_Progress_Move_up_Grill_sort,
        WinStreak_Progress_Unlock_prize_Grill_sort,
        // drop mode
        Items_Put_SMode_Drop_line_Grill_sort,
        //unlock booster
        Booster_Unlock_Grill_sort,
        //IAP purchase
        IAP_Purchase_completed_Grill_sort,
        //pre booster level
        Pre_level_Items_Appear_Grill_sort,
        Pre_level_Box_Fly_out_Grill_sort,
        Pre_level_Box_Fly_in_Grill_sort,

        Items_Collected_Grill_sort,
        Collection_Full_single_album_Grill_sort,
        Collection_Unlock_rewards_Album_Grill_sort,
        SpinTick,
        SpinStop,

        SantaHaul_Board_open_Grill_sort,
        SantaHaul_Price_Unlock_Grill_sort,
        SantaHaul_Board_Music_Grill_sort
    }
}
#endif