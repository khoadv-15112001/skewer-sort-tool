using DG.Tweening;
using GrillSort.PiggyBank;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupPiggyBank : Panel
{
    [SerializeField] private UIPiggyBankProgress piggyBankProgress;
    [SerializeField] private Button btnClaim;
    [SerializeField] private Button btnPlay;
    [SerializeField] private TMP_Text txtPrice;


    [Header("Visual")] [SerializeField] private TMP_Text txtCoinReward;
    [SerializeField] private GameObject pCoinReward;
    [SerializeField] private GameObject pFull;
    [SerializeField] private GameObject imgFlare;

    private readonly Service<PiggyBankService> _piggyBankService = new();
    private readonly Service<ShopService> _shopService = new();
    private ShopItemKey _key = ShopItemKey.PiggyBankPack1;

    public override void OnSetup()
    {
        base.OnSetup();
        _piggyBankService.Instance.OnDataChanged += UpdateUI;
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        UpdateUI();
        imgFlare.transform.DORotate(new Vector3(0, 0, -360), 4, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

    public override void Close()
    {
        imgFlare.transform.DOKill();
        base.Close();
    }

    protected void OnDestroy()
    {
        _piggyBankService.Instance.OnDataChanged -= UpdateUI;
        
    }

    private void UpdateUI()
    {
        var data = _piggyBankService.Instance.PiggyBankData;
        var config = _piggyBankService.Instance.PiggyBankConfig;
        var configTier = config.GetPiggyTierConfig(data.piggyTier);
        piggyBankProgress.Setup(configTier);
        piggyBankProgress.SetData(data);

        if (_piggyBankService.Instance.CanClaimReward())
        {
            btnClaim.gameObject.SetActive(true);
            btnPlay.gameObject.SetActive(false);
        }
        else
        {
            btnClaim.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);
        }

        // update price
        _key = _piggyBankService.Instance.GetCurrentRewardShopKey();
        txtPrice.text = $"{SonatSDKAdapter.GetProductPrice(_key)}";

        // update visual
        var isFull = _piggyBankService.Instance.CanUpdatePiggyTier();
        pCoinReward.gameObject.SetActive(!isFull);
        pFull.gameObject.SetActive(isFull);

        // set coin reward
        if (isFull == false)
        {
            //txtCoinReward.text = $"{_piggyBankService.Instance.GetCurrentRewardQuantity()}";
            txtCoinReward.text = $"{MySonatFramework.inventoryService.GetResource(GameResource.PiggyPoint)}";
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtCoinReward.GetComponent<RectTransform>());
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            var pointToAdd = _piggyBankService.Instance.PiggyBankConfig.reward;
            _piggyBankService.Instance.AddPiggyPoints(5);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _piggyBankService.Instance.ResetData();
        }
    }
#endif

    public void OnClickClaim()
    {
        if (!MySonatFramework.IsNetworkAvailable())
        {
            NoInternet.Instance.ForceShowPopup();
            return;
        }
        _shopService.Instance.BuyPack(_key);
    }

    public void OnClickPlay()
    {
        if (MySonatFramework.livesService.CanPlay())
        {
            LoadingScreenInstance.Instance.Show(1.5f);
            SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Gameplay); });
        }
        else
        {
            PanelManager.Instance.OpenPanel<PopupRefillLives>();
            PopupToast.Cretate("No more lives left!");
        }
    }


    private void OnEnable()
    {
        _shopService.Instance.OnBuySuccess += OnBuySuccess;
    }

    private void OnDisable()
    {
        _shopService.Instance.OnBuySuccess -= OnBuySuccess;
    }

    private void OnBuySuccess(ShopItemKey key)
    {
        if (!_shopService.Instance.VerifyPack(key))
        {
            gameObject.SetActive(false);
            return;
        }

        if (key != _key) return;
        BuyComplete();
    }

    private void BuyComplete()
    {
        var canUpdateTier = _piggyBankService.Instance.CanUpdatePiggyTier();

        // nhận thưởng mốc hiện tại
        _piggyBankService.Instance.ClaimReward();

        // nếu nhận và full thì update tier
        if (canUpdateTier)
        {
            _piggyBankService.Instance.UpdatePiggyTier();
        }
    }
}