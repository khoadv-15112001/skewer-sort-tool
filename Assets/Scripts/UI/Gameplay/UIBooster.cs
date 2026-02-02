using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIBooster : UIBoosterBase
{
    private bool isTutorial;
    [SerializeField] private ParticleSystem effect;
    [SerializeField] private ParticleSystem suggestionEffect;
    [SerializeField] private int delayAnim = 1000;
    [SerializeField] private GameObject[] objsFree;
    [SerializeField] private GameObject[] objsBuy;
    [SerializeField] private GameObject[] objsQuantity;

    [Header("Forced to appear")] [SerializeField]
    private bool isForcedToAppear = true;

    public bool IsForcedToAppear => isForcedToAppear;

    ///[SerializeField] private float timeUseBooster = 1;
    private int MAX_USE_FREE = 1;

    private BlockPanel blockPanel;

    private EventBinding<UseBoosterEvent> eventBinding;
    private readonly Service<DataService> dataService = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        eventBinding = new EventBinding<UseBoosterEvent>(OnUseBooster);
        EventBus<UseBoosterEvent>.Register(eventBinding);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<UseBoosterEvent>.Deregister(eventBinding);
    }

    private void OnUseBooster(UseBoosterEvent eventData)
    {
        if (eventData.booster == boosterType)
        {
            var numUseFree = dataService.Instance.GetInt($"Use_Free_Booster_{boosterType}", 0);
            dataService.Instance.SetInt($"Use_Free_Booster_{boosterType}", numUseFree + 1);
        }
    }

    protected override void Init()
    {
        base.Init();
        if (effect) effect.gameObject.SetActive(false);
        if (suggestionEffect) suggestionEffect.gameObject.SetActive(false);

        // old user thì sẽ không có free booster
        // lúc unlock thì sẽ set lại = 0
        if (!dataService.Instance.HasKey($"Use_Free_Booster_{boosterType}"))
        {
            dataService.Instance.SetInt($"Use_Free_Booster_{boosterType}", MAX_USE_FREE);
        }
    }

    public override void UseBooster()
    {
        if (!isTutorial && GameplayController.instance.gameState != GameState.Playing) return;
        if (!GameplayController.instance.CanUseBooster(1.5f)) return;
        Suggest(false);

        UseBoosterAsync().Forget();
    }

    public async UniTask UseBoosterAsync()
    {
        blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent() { gameState = GameState.UsingBooster });
        base.UseBooster();

        if (CheckAutoActivateBooster(boosterType))
        {
            if (!isTutorial)
            {
                boosterService.Instance.UseBoosterSuccess(boosterType);
            }
            else
            {
                OnUseTutSuccess();
            }
        }

        if (CheckUseBoosterAnim(boosterType))
        {
            BoosterAnim boosterAnim = SonatSystem.GetService<PoolingService>().Create<BoosterAnim>($"{boosterType}Anim", PanelManager.Instance.transform);
            boosterAnim.SetData(transform.position);
            await UniTask.Delay(delayAnim);
        }

        switch (boosterType)
        {
            case GameResource.BoosterFreeze:
                await GameplayController.instance.BoosterFreeze();
                break;
            case GameResource.BoosterShuffle:
                await GameplayController.instance.BoosterShuffle();
                break;
            case GameResource.BoosterMagnet:
                // ---old---
                // await GameplayController.instance.BoosterMagnet();
                // ---new---
                await GameplayController.instance.BoosterMatch3Target();
                break;
            case GameResource.BoosterMagicWand:
                await GameplayController.instance.BoosterMagicWand();
                break;
            case GameResource.BoosterMagicKey:
                await GameplayController.instance.BoosterMagicKey();
                break;
            case GameResource.BoosterBlowTorch:
                await GameplayController.instance.BoosterBlowTorch();
                break;
        }

        isTutorial = false;
        usingBooster = false;
        blockPanel?.Close();
        blockPanel = null;
        if (GameplayController.instance.gameState != GameState.GameOver)
            EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent() { gameState = GameState.Playing });
        GameplayController.OnUseBoosterSuccess?.Invoke(boosterType);
    }

    private bool CheckAutoActivateBooster(GameResource boosterType)
    {
        switch (boosterType)
        {
            case GameResource.BoosterFreeze:
            case GameResource.BoosterShuffle:
                // case GameResource.BoosterMagnet:
                return true;
            default:
                return false;
        }
    }

    private bool CheckUseBoosterAnim(GameResource boosterType)
    {
        switch (boosterType)
        {
            case GameResource.BoosterMagnet:
            case GameResource.BoosterBlowTorch:
                return false;
            default:
                return true;
        }
    }

    private void OnUseTutSuccess()
    {
        var logSpend = new SpendResourceLogData
        {
            earnType = "tutorial",
            earnId = "_",
            source = "non_iap"
        };
        MySonatFramework.inventoryService.ReduceResource(boosterType, 1, logSpend);
        EventBus<UseBoosterEvent>.Raise(new UseBoosterEvent() { booster = boosterType });
    }

    protected override void UnlockBooster()
    {
        isTutorial = GameRemoteConfigValue.forceTutBooster;
        // base.UnlockBooster();

        // old user thì sẽ không có free booster
        // lúc unlock thì sẽ set lại = 0
        dataService.Instance.SetInt($"Use_Free_Booster_{boosterType}", 0);
        ShowPopupUnlock().Forget();
    }

    private async UniTask ShowPopupUnlock()
    {
        UIFlowController.isShowedPopupUnlockBooster = true;
        await UniTask.Delay(200);
        await UniTask.WaitUntil(() => UIFlowController.CheckConditionShowPopupUnlockBooster());
        var data = new PopupUnlockBooster.Data()
        {
            uiBooster = this
        };
        PanelManager.Instance.OpenPanelByNameAsync<PopupUnlockBooster>($"PopupUnlock{boosterType}", data).Forget();
    }

    protected override void BoosterLockFeedback()
    {
        base.BoosterLockFeedback();
        PopupToast.Cretate($"Unlock at level:", config.levelUnlock.ToString());
    }

    protected override void OnCollectResource(AddItemEvent eventData)
    {
        if (eventData.resource != this.boosterType) return;
        if (eventData.collectEffect != null)
        {
            eventData.collectEffect.Collect(this.boosterType, eventData.quantity, eventData.position, transform.position, () =>
            {
                UpdateLockVisual();
                float defaultScale = transform.localScale.x;
                transform.DOScale(defaultScale * 1.1f, 0.075f).SetLoops(2, LoopType.Yoyo);
                UpdateData();
                if (effect)
                {
                    effect.gameObject.SetActive(true);
                    effect.Play();
                }
            });
        }
        else
        {
            UpdateData();
        }
    }

    public void Suggest(bool state)
    {
        if (state)
        {
            if (!unlocked) return;
            if (suggestionEffect)
            {
                suggestionEffect.gameObject.SetActive(true);
                suggestionEffect.Play();
            }

            transform.GetChild(0).DOShakePosition(0.5f, 3f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            if (suggestionEffect)
            {
                suggestionEffect.gameObject.SetActive(false);
            }

            transform.GetChild(0).DOKill();
            transform.GetChild(0).localPosition = Vector3.zero;
        }
    }

    public override void ClickBooster()
    {
        if (usingBooster) return;
        if (boosterService.Instance.CanUseBooster(boosterType))
        {
            if (GameplayController.instance.CanUseBooster(boosterType))
            {
                UseBooster();
            }
        }
        else
        {
            if (unlocked) OnOutOfBooster();
            else
            {
                BoosterLockFeedback();
            }
        }
    }

    protected override void UpdateData()
    {
        base.UpdateData();
        TryShowFree();
    }

    private void TryShowFree()
    {
        // nếu user cũ thì thoi không có free
        if (dataService.Instance.GetInt($"Use_Free_Booster_{boosterType}", 0) < MAX_USE_FREE)
        {
            ShowFree(true);
        }
        else
        {
            ShowFree(false);
        }
    }

    private void ShowFree(bool isFree)
    {
        foreach (var obj in objsFree)
        {
            obj.SetActive(isFree);
        }

        foreach (var obj in objsQuantity)
        {
            obj.SetActive(!isFree);
        }

        foreach (var obj in objsBuy)
        {
            obj.SetActive(!isFree && boosterData.quantity <= 0);
        }
    }
}