namespace Sonat
{
    public enum RemoteConfigKey
    {
        None = 0,
        level_start_show_interstitial,
        level_start_show_native_banner,

        priority_mediation,

        time_gap_interstitial,
        time_gap_interstitial_reward,
        time_gap_interstitial_onfocus,

        inter_ads_cache,
        reward_ads_cache,

        auto_show_banner,
        auto_show_app_open_ads,
        force_mediation_prioritize,

        mediation_placement,
        log_ads_state,

        complete_level_logs,
        complete_rw_ads_logs,
        paid_ad_impression_logs,
        levels_log_iaa_iap,
        
        turn_off_focus_ads,
        seconds_to_dispose_ads,
        min_seconds_out_focus,
        max_seconds_out_focus,
        
        time_show_loading_ads,
        level_start_show_banner,
        remote_level_data
    }
}