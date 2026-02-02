using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using SonatFramework.Systems.EventBus;
using UnityEngine;

public class BIGameConfigService : SingletonSimple<BIGameConfigService>
{
    public bool inited;
    public BIGameConfig BIGameConfig { get; private set; }
    [SerializeField] private float timeout = 10;

    private OnlineService _onlineService;
    EventBinding<OnlineService.BIGetOrSignUpEvent> getOrSignUpEvent;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Initialize(OnlineService onlineService)
    {
        this._onlineService = onlineService;
        getOrSignUpEvent = new EventBinding<OnlineService.BIGetOrSignUpEvent>(OnBIGetOrSignUpEvent);
        StartCoroutine(CheckTimeout());
    }

    private void OnDestroy()
    {
        EventBus<OnlineService.BIGetOrSignUpEvent>.Deregister(getOrSignUpEvent);
    }

    private void OnBIGetOrSignUpEvent(OnlineService.BIGetOrSignUpEvent eventData)
    {
        GetGlobalConfig().Forget();
    }
    
    private IEnumerator CheckTimeout()
    {
        yield return new WaitForSeconds(timeout);
        inited = true;
    }

    private async UniTask GetGlobalConfig()
    {
        BIGameConfig = await _onlineService.Get<BIGameConfig>("system-config/game/global");

        //Debug.Log($"[BIGameConfigService] BIGameConfig: {Newtonsoft.Json.JsonConvert.SerializeObject(BIGameConfig)}");
        inited = true;
    }

    public BIRequestConfig GetDefaultRequestConfig()
    {
        return new(BIGameConfig.maxItemsPerHelpRequestQuantity, BIGameConfig.expireSecondsOfHelpRequest,
            BIGameConfig.coolDownSecondsBetweenHelpRequest, BIGameConfig.enableHelperResourceDeduction,
            BIGameConfig.hideOldHelpRequestAfterCoolDownReset);
    }

    public BIRequestConfig GetResourceRequestConfig(string resoure)
    {
        if (BIGameConfig.helpRequestConfigByResource == null)
            return GetDefaultRequestConfig();

        if (resoure == OnlineResourceType.Lives)
        {
            if (BIGameConfig.helpRequestConfigByResource.lives != null)
                return BIGameConfig.helpRequestConfigByResource.lives;
        }
        else if (resoure == OnlineResourceType.Card)
        {
            if (BIGameConfig.helpRequestConfigByResource.card != null)
                return BIGameConfig.helpRequestConfigByResource.card;
        }

        return GetDefaultRequestConfig();
    }
}
