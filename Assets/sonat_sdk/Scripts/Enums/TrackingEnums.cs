namespace Sonat
{
    public enum sonat_log_enum
    {
        cmp_impression,
        cmp_user_consent,
        cmp_user_do_not_consent ,
    }

    public enum EventNameEnum
    {
        app_loading,
        game_scene_level,
        buy_iap,
        show_video,
        video_rewarded,
        show_interstitial,
        level_start,
        remove_ads_success,
        ctr_impression,
        paid_ad_click,
        level_end,
        level_up,
        use_booster,
        post_score,
        app_spent_time,
        tutorial_begin,
        tutorial_complete,
        screen_view,
        earn_virtual_currency,
        spend_virtual_currency,
        show_rate,
        click_icon_shortcut,
        af_purchase,
        click_shop,
        paid_ad_impression,
        cross_pro,
        buy_shop_item,
        view_shop_item,
        cancel_shop_item,
        open_shop,
        show_ui,
        buy_shop_item_iap,
        att_status,
        ad_impression,
        gdpr_status,
        download_level_data,
        level_start_feature,
        level_end_feature,
        ads_state,
        ad_duration,
        level_suspend,
        level_resume,
        app_suspend,
        app_resume
        
    }

    public enum EventNameEnumForAf
    {
        af_ad_view,
        video_rewarded,
        show_interstitial,
        show_video,
    }

    public enum ParameterEnumForAf
    {
        af_adrev_ad_type,
        af_revenue,
        af_quantity,
        af_content_type,
        af_content_id,
        af_order_id,
        af_receipt_id,
        af_currency
    }

    public enum UserPropertyName
    {
        last_screen,
        level,
        mode,
        continue_with,
        iap_count,
        rwd_count
    }

    public enum ParameterValue
    {
        IntersAds,
        RewardedAds,
        ad_break,
    }

    public enum ParameterEnum
    {
        placement,
        af_adrev_ad_type, // for af
        log_level,
        value,
        name,
        level,
        product_click_buy,
        in_app_key,
        store_product_id,
        price,
        ad_format,
        ad_value,
        placement_name,
        fb_instance_id,
        mode,
        use_booster_count,
        play_time,
        move_count,
        score,
        highest_score,
        success,
        lose_cause,
        is_first_play,
        character,
        time_msec,
        screen_name,
        screen_class,
        item_type,
        item_id,
        open_by,
        action,
        shortcut,
        virtual_currency_name,
        ad_placement,
        ad_source,
        ad_platform,
        sn_ltv_iaa,
        sn_ltv_iap,
        ad_id,
        step,
        currency,
        is_first_buy,
        spent,
        virtual_currency_type,
        amount,
        phase,
        continue_with,
        continue_times,
        source,
        value_in_usd,
        millisecond,
        buy_count,
        start_count,
        prev_use_booster_count,
        type,
        status,
        map_id,
        location,
        screen,
        value_game_currency,
        remain_time,
        remain_move,
        remain_item,
    }

    public enum AdsPlatform
    {
        applovinmax,
        googleadmob,
        ironsource
    }

    public enum AdTypeLogStep
    {
        admob_ad_request,
    }

    public enum AdTypeLog
    {
        undefined,
        banner,
        interstitial,
        native,
        video,
        rewarded_video,
        rewarded,
        mraid,
        mrec,
        offer_wall,
        playable,
        more_apps,
        video_interstitial,
        medium,
        custom,
        banner_interstitial,
        app_open,
        other,
        native_banner,
        collapsible_banner,
    }

    public enum network_connect_type
    {
        none = 0,
        wifi = 1,
        mobile = 2,
        other = 3,
    }

    public enum FireBaseDefaultEvent
    {
        my_first_open,
        my_first_game_play,
        my_first_action,
        my_first_game_win,
    }

    public enum StartLevelType
    {
        New,
        Revive,
        NextPhase,
    }
}