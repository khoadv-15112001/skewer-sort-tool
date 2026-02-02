using System;
using System.Collections.Generic;
using Sonat.Data;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using Sonat.IapModule;
using UnityEngine;

namespace Sonat.TrackingModule
{
    [Serializable]
    public abstract class SonatLogBasic : SonatLogBase
    {
        public int level = UserData.GetLevel();
        public int phase = int.MinValue;

        public virtual List<LogParameter> BaseLogs()
        {
            var parameters = new List<LogParameter>();
            parameters.Add(new LogParameter(ParameterEnum.level.ToString(), level.ToString()));
            if (phase >= 0)
                parameters.Add(new LogParameter(ParameterEnum.phase.ToString(), phase));
            return parameters;
        }
    }

    [Serializable]
    public abstract class SonatLogBasicMode : SonatLogBasic
    {
        public string mode = UserData.GetMode();

        public override List<LogParameter> BaseLogs()
        {
            var parameters = base.BaseLogs();
            if (!string.IsNullOrEmpty(mode))
                parameters.Add(new LogParameter(ParameterEnum.mode.ToString(), mode));
            return parameters;
        }
    }

    // [Serializable]
    // public abstract class SonatLogLevelStartEndBase : SonatLogBasicMode
    // {
    //     public string continue_with;
    //     public int continue_times;
    //     public string map_id;
    //
    //     public override List<LogParameter> BaseLogs()
    //     {
    //         var parameters = base.BaseLogs();
    //         if (!string.IsNullOrEmpty(continue_with))
    //             parameters.Add(new LogParameter(ParameterEnum.continue_with.ToString(), continue_with));
    //         if (continue_times > 0)
    //             parameters.Add(new LogParameter(ParameterEnum.continue_times.ToString(), continue_times));
    //         if (!string.IsNullOrEmpty(map_id))
    //         {
    //             parameters.Add(new LogParameter(ParameterEnum.map_id.ToString(), map_id));
    //         }
    //
    //         return parameters;
    //     }
    // }

    [Serializable]
    public class SonatCompleteSceneTime : SonatLogBasicMode
    {
        public string placement;
        public int millisecond;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.millisecond.ToString(), millisecond));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));

            return parameters;
        }

        public override string EventName => "complete_scene_time";
    }

    [Serializable]
    public class SonatAppSpentTime : SonatLogBasicMode
    {
        public string placement;
        public int millisecond;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.millisecond.ToString(), millisecond));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));

            return parameters;
        }

        public override string EventName => "app_spent_time";
    }

    [Serializable]
    public class SonatLogLevelStart : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.level_start.ToString();

        public bool is_first_play;
        public int start_count;
        public string continue_with;
        public int continue_times;

        public bool setUserProperty = true;

        protected override List<LogParameter> GetParameters()
        {
            if (setUserProperty)
            {
                SonatFirebase.analytic.SetUserProperty(UserPropertyName.level.ToString(), level.ToString());
                SonatFirebase.analytic.SetUserProperty(UserPropertyName.mode.ToString(), mode);

                UserData.SetMode(mode);
                UserData.SetLevel(level, mode);
            }

            List<LogParameter> parameters = BaseLogs();
            if (!string.IsNullOrEmpty(continue_with))
                parameters.Add(new LogParameter(ParameterEnum.continue_with.ToString(), continue_with));
            if (continue_times > 0)
                parameters.Add(new LogParameter(ParameterEnum.continue_times.ToString(), continue_times));

            parameters.Add(new LogParameter(ParameterEnum.is_first_play.ToString(), is_first_play));
            parameters.Add(new LogParameter(ParameterEnum.start_count.ToString(), start_count));


            if (mode == "classic")
                if (SonatLogRecursive.NeedLogIaaIap(level))
                {
                    parameters.Add(new LogParameter(ParameterEnum.sn_ltv_iaa.ToString(),
                        SonatAnalyticTracker.sn_ltv_iaa));
                    parameters.Add(new LogParameter(ParameterEnum.sn_ltv_iap.ToString(),
                        SonatIap.sn_ltv_iap));
                }

            return parameters;
        }

        //        public override void Post(bool logAf = false)
        //        {
        //            base.Post(true);
        //        }

        //        protected override void LogAf(List<LogParameter> listParameters)
        //        {
        //            var temp = listParameters.ToList();
        //            for (var i = temp.Count - 1; i >= 0; i--)
        //                if(temp[i].stringKey == ParameterEnum.sn_ltv_iaa.ToString() || temp[i].stringKey == ParameterEnum.sn_ltv_iap.ToString())
        //                    temp.RemoveAt(i);
        //            base.LogAf(temp);
        //        }
    }

    [Serializable]
    public class SonatLogLevelEnd : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.level_end.ToString();

        public string continue_with;
        public int continue_times;
        public int use_booster_count;
        public int play_time;
        public int move_count;
        public bool is_first_play;
        public int start_count;
        public bool success;
        public string lose_cause;

        public int remain_time = -1;
        public int remain_move = -1;
        public int remain_item = -1;
        

        public SonatLogLevelEnd()
        {
        }

        public SonatLogLevelEnd(int level, int phase, int play_time, bool success, bool is_first_play, string mode = "classic")
        {
            this.mode = mode;
            this.level = level;
            this.phase = phase;
            this.play_time = play_time;
            this.success = success;
            this.is_first_play = is_first_play;

#if UNITY_EDITOR
            if (play_time <= 0)
                SonatDebugType.Tracking.LogWarning("Warning playtime zero");
#endif
        }

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = BaseLogs();

            if (!string.IsNullOrEmpty(continue_with))
                parameters.Add(new LogParameter(ParameterEnum.continue_with.ToString(), continue_with));
            if (continue_times > 0)
                parameters.Add(new LogParameter(ParameterEnum.continue_times.ToString(), continue_times));

            parameters.Add(new LogParameter(ParameterEnum.start_count.ToString(), start_count));
            parameters.Add(new LogParameter(ParameterEnum.use_booster_count.ToString(), use_booster_count));
            parameters.Add(new LogParameter(ParameterEnum.play_time.ToString(), play_time));
            parameters.Add(new LogParameter(ParameterEnum.move_count.ToString(), move_count));
            parameters.Add(new LogParameter(ParameterEnum.success.ToString(), success));
            parameters.Add(new LogParameter(ParameterEnum.is_first_play.ToString(), is_first_play));
            
            if(remain_time >= 0) parameters.Add(new LogParameter(ParameterEnum.remain_time.ToString(), remain_time));
            if(remain_move >= 0) parameters.Add(new LogParameter(ParameterEnum.remain_time.ToString(), remain_move));
            if(remain_item >= 0) parameters.Add(new LogParameter(ParameterEnum.remain_time.ToString(), remain_item));
            
            if (!success)
                parameters.Add(new LogParameter(ParameterEnum.lose_cause.ToString(), lose_cause));
            return parameters;
        }

        public override void Post(bool logAf = false)
        {
            base.Post(logAf);
            if (success)
            {
                SonatLogRecursive.CheckLogCompleteLevelUA(level);
            }
        }
    }


    [Serializable]
    public class SonatLogLevelUp : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.level_up.ToString();

        public string character;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            if (!string.IsNullOrEmpty(character))
                parameters.Add(new LogParameter(ParameterEnum.character.ToString(), character));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogUseBooster : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.use_booster.ToString();

        public string name;
        public string placement;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.name.ToString(), name));
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }
    }

    // [Serializable]
    // public class SonatLogPostScore : SonatLogBasicMode
    // {
    //     public override string EventName => EventNameEnum.post_score.ToString();
    //
    //     public int score;
    //
    //     protected override List<LogParameter> GetParameters()
    //     {
    //         List<LogParameter> parameters = base.BaseLogs();
    //         parameters.Add(new LogParameter(ParameterEnum.score.ToString(), score));
    //         parameters.Add(new LogParameter(ParameterEnum.level.ToString(), level));
    //         if (!string.IsNullOrEmpty(mode))
    //             parameters.Add(new LogParameter(ParameterEnum.mode.ToString(), mode));
    //         return parameters;
    //     }
    // }

    [Serializable]
    public class SonatLogSpentTime : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.app_spent_time.ToString();

        public string type;
        public string name;
        public string placement;
        public long time_msec;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.type.ToString(), type));
            parameters.Add(new LogParameter(ParameterEnum.name.ToString(), name));
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.time_msec.ToString(), time_msec));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogTutorialBegin : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.tutorial_begin.ToString();

        public string placement;
        public int step;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.step.ToString(), step));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }

        public SonatLogTutorialBegin(string placementValue, int stepValue)
        {
            placement = placementValue;
            step = stepValue;
        }
    }

    [Serializable]
    public class SonatLogTutorialComplete : SonatLogBasic
    {
        public override string EventName => EventNameEnum.tutorial_complete.ToString();
        public string placement;
        public int step;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.step.ToString(), step));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }

        public SonatLogTutorialComplete(string placementValue, int stepValue)
        {
            placement = placementValue;
            step = stepValue;
        }
    }

    /// <summary>
    /// "Nhằm để thống kê được thời gian user ở từng giao diện ( screen class nếu ko đặt thì dùng luôn giá trị của screen_name )"
    /// </summary>
    [Serializable]
    public class SonatLogScreenView : SonatLogBasic
    {
        public override string EventName => EventNameEnum.screen_view.ToString();

        /// <summary>
        /// "Tên của screen trong game. Tuân theo quy tắc viết hoa mỗi  chữ cái của mỗi từ"
        /// </summary>
        public string screen_name;

        /// <summary>
        /// "Tên class của Screen  trong code(có thể dùng chính screen_name)"
        /// </summary>
        public string screen_class;

        public bool saveLastScreen = true;

        private string LastScreen
        {
            get => PlayerPrefs.GetString("last_screen");
            set => PlayerPrefs.SetString("last_screen", value);
        }

        private string LastScreenClass
        {
            get => PlayerPrefs.GetString("last_screen_class");
            set => PlayerPrefs.SetString("last_screen_class", value);
        }

        protected override List<LogParameter> GetParameters()
        {
            if (saveLastScreen)
            {
                LastScreen = screen_name;
                LastScreenClass = screen_class;
            }

            SonatFirebase.analytic.SetUserProperty(UserPropertyName.last_screen.ToString(), screen_name);
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.screen_name.ToString(), screen_name));
            if (!string.IsNullOrEmpty(screen_class))
                parameters.Add(new LogParameter(ParameterEnum.screen_class.ToString(), screen_class));
            return parameters;
        }
    }

    public class SonatLogLastScreenView : SonatLogBase
    {
        public override string EventName => EventNameEnum.screen_view.ToString();

        private string LastScreen
        {
            get => PlayerPrefs.GetString("last_screen");
        }

        private string LastScreenClass
        {
            get => PlayerPrefs.GetString("last_screen_class");
        }

        protected override List<LogParameter> GetParameters()
        {
            SonatFirebase.analytic.SetUserProperty(UserPropertyName.last_screen.ToString(), LastScreen);
            List<LogParameter> parameters = new List<LogParameter>();
            parameters.Add(new LogParameter(ParameterEnum.screen_name.ToString(), LastScreen));
            if (!string.IsNullOrEmpty(LastScreenClass))
                parameters.Add(new LogParameter(ParameterEnum.screen_class.ToString(), LastScreenClass));
            return parameters;
        }
    }


    [Serializable]
    public class SonatLogEarnVirtualCurrency : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.earn_virtual_currency.ToString();

        public string virtual_currency_name;
        public string virtual_currency_type;
        public int value;
        public string source;
        public string placement;
        public string spend_item_type;
        public string spend_item_id;
        public bool is_first_buy;
        public string location;
        public string screen;
        public float valueGameCurrency;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.virtual_currency_name.ToString(), virtual_currency_name));
            parameters.Add(new LogParameter(ParameterEnum.virtual_currency_type.ToString(), virtual_currency_type));
            parameters.Add(new LogParameter(ParameterEnum.value.ToString(), value));
            parameters.Add(new LogParameter(ParameterEnum.source.ToString(), source));
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.item_type.ToString(), spend_item_type));
            parameters.Add(new LogParameter(ParameterEnum.item_id.ToString(), spend_item_id));
            parameters.Add(new LogParameter(ParameterEnum.is_first_buy.ToString(), is_first_buy));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            parameters.Add(new LogParameter(ParameterEnum.value_game_currency.ToString(), valueGameCurrency));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogSpendVirtualCurrency : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.spend_virtual_currency.ToString();

        public string virtual_currency_name;
        public string virtual_currency_type;
        public int value;
        public string placement;
        public string earn_item_type;
        public string earn_item_id;
        public string location;
        public string screen;
        public float valueGameCurrency;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.virtual_currency_name.ToString(), virtual_currency_name));
            parameters.Add(new LogParameter(ParameterEnum.virtual_currency_type.ToString(), virtual_currency_type));
            parameters.Add(new LogParameter(ParameterEnum.value.ToString(), value));
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.item_type.ToString(), earn_item_type));
            parameters.Add(new LogParameter(ParameterEnum.item_id.ToString(), earn_item_id));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            parameters.Add(new LogParameter(ParameterEnum.value_game_currency.ToString(), valueGameCurrency));
            return parameters;
        }
    }

    public class SonatLogCrossPromotion : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.cross_pro.ToString();

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogShowInterstitial : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.show_interstitial.ToString();

        public string placement;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogVideoRewarded : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.video_rewarded.ToString();

        public string placement;
        public string item_type;
        public string item_id;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            PlayerPrefInt rwdCount = new PlayerPrefInt("SONAT_RWD_COUNT", 0);
            rwdCount.Value++;
            SonatFirebase.analytic.SetUserProperty(UserPropertyName.rwd_count.ToString(), rwdCount.Value.ToString());
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            if (!string.IsNullOrEmpty(item_type))
                parameters.Add(new LogParameter(ParameterEnum.item_type.ToString(), item_type));
            if (!string.IsNullOrEmpty(item_id))
                parameters.Add(new LogParameter(ParameterEnum.item_id.ToString(), item_id));
            //   parameters.Add(new LogParameter(ParameterEnum.value.ToString(), value));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }

        public override void Post(bool logAf = false)
        {
            base.Post(logAf);
            //SonatLogRecursive.CheckLogCompleteRewardedAdsUA();
        }
    }


    [Serializable]
    public class SonatPaidAdClick : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.paid_ad_click.ToString();
        public AdTypeLog ad_format;
        public string ad_placement;
        public string fb_instance_id;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.ad_format.ToString(), ad_format.ToString()));
            parameters.Add(new LogParameter(ParameterEnum.ad_placement.ToString(), ad_placement));
            parameters.Add(new LogParameter(ParameterEnum.fb_instance_id.ToString(), fb_instance_id));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogClickShop : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.click_shop.ToString();
        public string action;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.action.ToString(), action.ToString()));
            return parameters;
        }
    }


    [Serializable]
    public class SonatLogShowRate : SonatLogBasicMode
    {
        public enum ShowRateOpenBy
        {
            user,
            auto,
        }

        public enum ShowRateAction
        {
            open,
            star_0,
            rate_now,
            other_star,
            star_5,
            star_4,
            close
        }

        private static readonly string[] convert =
        {
            "open",
            "0_star",
            "rate_now",
            "other_star",
            "5_star",
            "4_star",
            "close",
        };

        public override string EventName => EventNameEnum.show_rate.ToString();

        public ShowRateOpenBy open_by;
        public ShowRateAction action;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.open_by.ToString(), open_by.ToString()));
            parameters.Add(new LogParameter(ParameterEnum.action.ToString(), convert[(int)action]));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogClickIconShortcut : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.click_icon_shortcut.ToString();
        public string shortcut;
        public string placement;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.shortcut.ToString(), shortcut));
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }

        public SonatLogClickIconShortcut()
        {
        }

        public SonatLogClickIconShortcut(string shortcut)
        {
            this.shortcut = shortcut;
        }
    }

    [Serializable]
    public class SonatLogShowUi : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.show_ui.ToString();
        public string placement;
        public string action;
        public string open_by;
        public string name;
        public string location;
        public string screen;
        public string ui_name;
        public string ui_type;
        public string ui_class;


        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.action.ToString(), action));
            parameters.Add(new LogParameter(ParameterEnum.open_by.ToString(), open_by));
            parameters.Add(new LogParameter(ParameterEnum.name.ToString(), name));
            //parameters.Add(new LogParameter(ParameterEnum.level.ToString(), level));
            //parameters.Add(new LogParameter(ParameterEnum.mode.ToString(), mode));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            parameters.Add(new LogParameter("ui_name", ui_name));
            parameters.Add(new LogParameter("ui_type", ui_type));
            parameters.Add(new LogParameter("ui_class", ui_class));
            return parameters;
        }
    }


    [Serializable]
    public class SonatLogOpenShop : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.open_shop.ToString();
        public string placement;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            //parameters.Add(new LogParameter(ParameterEnum.level.ToString(), level));
            //parameters.Add(new LogParameter(ParameterEnum.mode.ToString(), mode));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogCancelShopItem : SonatLogBasic
    {
        public override string EventName => EventNameEnum.cancel_shop_item.ToString();
        public string item_id;
        public string placement;
        public string item_type;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.item_id.ToString(), item_id));
            parameters.Add(new LogParameter(ParameterEnum.item_type.ToString(), item_type));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogViewShopItem : SonatLogBasic
    {
        public override string EventName => EventNameEnum.view_shop_item.ToString();
        public string item_id;
        public string placement;
        public string item_type;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.item_id.ToString(), item_id));
            parameters.Add(new LogParameter(ParameterEnum.item_type.ToString(), item_type));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogBuyShopItem : SonatLogBasicMode
    {
        public override string EventName => EventNameEnum.buy_shop_item.ToString();
        public string item_id;
        public string placement;
        public string item_type;
        public float value;
        public string currency;
        public bool is_first_buy;
        public string location;
        public string screen;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.item_id.ToString(), item_id));
            parameters.Add(new LogParameter(ParameterEnum.placement.ToString(), placement));
            parameters.Add(new LogParameter(ParameterEnum.item_type.ToString(), item_type));
            parameters.Add(new LogParameter(ParameterEnum.value.ToString(), value));
            parameters.Add(new LogParameter(ParameterEnum.currency.ToString(), currency));
            parameters.Add(new LogParameter(ParameterEnum.is_first_buy.ToString(), is_first_buy));
            parameters.Add(new LogParameter(ParameterEnum.location.ToString(), location));
            parameters.Add(new LogParameter(ParameterEnum.screen.ToString(), screen));
            return parameters;
        }
    }

    [Serializable]
    public class SonatBuyLogData
    {
        public string placement;
        public string item_type;
        public int phase;
        public string location;
        public string screen;

        public SonatBuyLogData()
        {
        }

        public SonatBuyLogData(string placement, string itemType, int phase)
        {
            this.placement = placement;
            this.item_type = itemType;
            this.phase = phase;
        }
    }

    [Serializable]
    public class SonatLogBuyShopItemIap : SonatLogBuyShopItem
    {
        public override string EventName => EventNameEnum.buy_shop_item_iap.ToString();
        public float value_in_usd;
        public int buy_count;

        protected override List<LogParameter> GetParameters()
        {
            SonatFirebase.analytic.SetUserProperty(UserPropertyName.iap_count.ToString(), buy_count.ToString());
            List<LogParameter> parameters = base.GetParameters();
            parameters.Add(new LogParameter(ParameterEnum.value_in_usd.ToString(), value_in_usd));
            parameters.Add(new LogParameter(ParameterEnum.buy_count.ToString(), buy_count));
            return parameters;
        }

        public SonatLogBuyShopItemIap(SonatBuyLogData input)
        {
            placement = input.placement;
            item_type = input.item_type;
            phase = input.phase;
            location = input.location;
            screen = input.screen;
        }
    }

    [Serializable]
    public class SonatLogInAppPurchaseAppsFlyer : SonatLogAppflyerBase
    {
        public double af_revenue;
        public int af_quantity = 1;
        public string af_content_type;
        public string af_content_id;
        public string af_order_id;
        public string af_receipt_id;
        public string af_currency;
        public override string EventName => EventNameEnum.af_purchase.ToString();

        protected override Dictionary<string, string> GetParameters()
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add(ParameterEnumForAf.af_revenue.ToString(), af_revenue.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(",", "."));
            parameters.Add(ParameterEnumForAf.af_quantity.ToString(), af_quantity.ToString());
            parameters.Add(ParameterEnumForAf.af_content_type.ToString(), af_content_type);
            parameters.Add(ParameterEnumForAf.af_content_id.ToString(), af_content_id);
            parameters.Add(ParameterEnumForAf.af_order_id.ToString(), af_order_id);
            parameters.Add(ParameterEnumForAf.af_receipt_id.ToString(), af_receipt_id);
            parameters.Add(ParameterEnumForAf.af_currency.ToString(), af_currency);
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogATT : SonatLogBase
    {
        public string status;
        private long timeSinceStart;
        public override string EventName => EventNameEnum.att_status.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = new List<LogParameter>();
            parameters.Add(new LogParameter(ParameterEnum.status.ToString(), status));
            parameters.Add(new LogParameter("time_since_start", (Time.realtimeSinceStartup * 1000).ToString()));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogGDPR : SonatLogBase
    {
        public string status;
        public override string EventName => EventNameEnum.gdpr_status.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = new List<LogParameter>();
            parameters.Add(new LogParameter(ParameterEnum.status.ToString(), status));
            parameters.Add(new LogParameter("time_since_start", (Time.realtimeSinceStartup * 1000).ToString()));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogDownloadLevelData : SonatLogBase
    {
        public string phase;
        public string map_id;
        public override string EventName => EventNameEnum.download_level_data.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = new List<LogParameter>();
            parameters.Add(new LogParameter("phase", phase));
            parameters.Add(new LogParameter("map_id", map_id));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogStartFeature : SonatLogBasicMode
    {
        public int start_count;
        public override string EventName => EventNameEnum.level_start_feature.ToString();

        protected override List<LogParameter> GetParameters()
        {
            //
            var parameters = base.BaseLogs();
            //parameters.Add(new LogParameter(ParameterEnum.level.ToString(), level));
            parameters.Add(new LogParameter(ParameterEnum.start_count.ToString(), start_count));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogEndFeature : SonatLogBasicMode
    {
        public bool success;
        public int start_count;
        public override string EventName => EventNameEnum.level_end_feature.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = base.BaseLogs();
            //parameters.Add(new LogParameter(ParameterEnum.level.ToString(), level));
            parameters.Add(new LogParameter(ParameterEnum.start_count.ToString(), start_count));
            parameters.Add(new LogParameter(ParameterEnum.success.ToString(), success.ToString()));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogProgressFeature : SonatLogBasicMode
    {
        public bool success;
        public int start_count;
        public override string EventName => EventNameEnum.level_end_feature.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = base.BaseLogs();
            //parameters.Add(new LogParameter(ParameterEnum.level.ToString(), level));
            parameters.Add(new LogParameter(ParameterEnum.start_count.ToString(), start_count));
            parameters.Add(new LogParameter(ParameterEnum.success.ToString(), success.ToString()));
            return parameters;
        }
    }

    [Serializable]
    public class SonatLogAdsState : SonatLogBase
    {
        public string adsState;
        public string adsType;
        public override string EventName => EventNameEnum.ads_state.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = new List<LogParameter>();
            parameters.Add(new LogParameter("ads_state", adsState));
            parameters.Add(new LogParameter("ads_type", adsType));
            return parameters;
        }

        public SonatLogAdsState()
        {
        }

        public SonatLogAdsState(string adsType, string adsState)
        {
            this.adsType = adsType;
            this.adsState = adsState;
        }

        public override void Post(bool logAf = false)
        {
            if (RemoteConfigKey.log_ads_state.GetBool())
                base.Post(logAf);
        }
    }

    [Serializable]
    public class SonatLogAdDuration : SonatLogBase
    {
        string format;
        string adSource;
        string adValue;
        int adDuration;
        int adClickCount;
        int? adClick;
        int? adBounceback;
        string medationPlatform;

        public override string EventName => EventNameEnum.ad_duration.ToString();


        public SonatLogAdDuration()
        {
        }

        public SonatLogAdDuration(string mediation, string format, string adSource, string adValue, AdDurationTracker.AdMetrics adMetrics)
        {
            this.format = format;
            this.adSource = adSource; // Derive Ad source 
            this.adValue = adValue; // Derive Ad Value 
            this.adDuration = adMetrics.AdDuration;
            this.adClickCount = adMetrics.AdClickCount;
            this.adClick = adMetrics.AdClickTime;
            this.adBounceback = adMetrics.AdBounceBackTime;
            this.medationPlatform = mediation;
        }


        protected override List<LogParameter> GetParameters()
        {
            var parameters = new List<LogParameter>();
            parameters.Add(new LogParameter("ad_source", adSource));
            parameters.Add(new LogParameter("ad_value", adValue));
            parameters.Add(new LogParameter("format", format));
            parameters.Add(new LogParameter("ad_duration", adDuration));
            parameters.Add(new LogParameter("ad_click_count", adClickCount));
            if (adClick.HasValue)
                parameters.Add(new LogParameter("ad_click", adClick.Value));
            if (adBounceback.HasValue)
                parameters.Add(new LogParameter("ad_bounce_back", adBounceback.Value));
            parameters.Add(new LogParameter("mediationPlatform", medationPlatform));
            return parameters;
        }
    }
    
    [Serializable]
    public class SonatLogAppSuspend : SonatLogBasicMode
    {
        public string firebase_screen;
        public override string EventName => EventNameEnum.app_suspend.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = base.BaseLogs();
            parameters.Add(new LogParameter("firebase_screen", firebase_screen));
            return parameters;
        }
    }
    
    [Serializable]
    public class SonatLogAppResume : SonatLogBasicMode
    {
        public string firebase_screen;
        public override string EventName => EventNameEnum.app_resume.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = base.BaseLogs();
            parameters.Add(new LogParameter("firebase_screen", firebase_screen));
            return parameters;
        }
    }
    
    [Serializable]
    public class SonatLogLevelSuspend : SonatLogBasicMode
    {
        public string firebase_screen;
        public override string EventName => EventNameEnum.level_suspend.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = base.BaseLogs();
            parameters.Add(new LogParameter("firebase_screen", firebase_screen));
            return parameters;
        }
    }
    
    [Serializable]
    public class SonatLogLevelResume : SonatLogBasicMode
    {
        public string firebase_screen;
        public override string EventName => EventNameEnum.level_resume.ToString();

        protected override List<LogParameter> GetParameters()
        {
            var parameters = base.BaseLogs();
            parameters.Add(new LogParameter("firebase_screen", firebase_screen));
            return parameters;
        }
    }
    
}