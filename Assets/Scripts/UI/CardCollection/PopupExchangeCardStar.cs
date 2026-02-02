using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyGame.Modules.CardCollection;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupExchangeCardStar : Panel
{
    [SerializeField] private TMP_Text txtStar;
    [SerializeField] private TMP_Text[] txtStarNeed;
    [SerializeField] private Button[] starbtns;
    [SerializeField] private Sprite btnGreen;
    [SerializeField] private Sprite btnGray;
    [SerializeField] private GameObject previewObj;
    [SerializeField] private UIRewardGrid uIRewardGrid;
    private readonly Service<CardCollectionService> _cardCollectionService = new();

    private int star = 0;
    public override void OnSetup()
    {
        base.OnSetup(); 

        for (int i = 0; i < txtStarNeed.Length; i++)
        {
            txtStarNeed[i].text = _cardCollectionService.Instance.starExchangeConfig.milestones[i].star.ToString();
        }
    }
    public override void Open(UIData uiData)
    {
        base.Open(uiData);
      
        UpdateUI().Forget();
    }

    private async UniTask UpdateUI()
    {
        await UniTask.Yield();

        star = _cardCollectionService.Instance.CardStar;

        txtStar.text = star.ToString();

        for (int i = 0; i < starbtns.Length; i++)
        {
            if (_cardCollectionService.Instance.CheckHasClaimChest(i))
                starbtns[i].image.sprite = btnGreen;
            else
                starbtns[i].image.sprite = btnGray;

            starbtns[i].interactable = true;
        }
    }
 
    public void OnClickExchange(int index)
    {
        if (star < _cardCollectionService.Instance.starExchangeConfig.milestones[index].star)
        {
            PopupToast.Cretate("You don't have enough stars");
            return;
        }

        if (_cardCollectionService.Instance.ExchangeCardStarToReward(index))
        {
            starbtns[index].interactable = false;

            UpdateUI().Forget();
        }

        Close();

        //HomeManager.Instance.SwitchTab(NavigationType.Home).Forget();
    }

    public void ClickPreview(int index)
    {
        //if (uIRewardGrid.gameObject.activeSelf) return;
        previewObj.SetActive(false);

        previewObj.SetActive(true);
        
        var reward = _cardCollectionService.Instance.starExchangeConfig.milestones[index].reward;

        previewObj.transform.DOMoveY(starbtns[index].transform.position.y, 0);
        uIRewardGrid.SetReward(reward);
    }
}
