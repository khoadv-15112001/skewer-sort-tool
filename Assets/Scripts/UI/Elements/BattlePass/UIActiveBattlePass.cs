using System.Collections;
using System.Collections.Generic;
using GrillSort.BattlePass;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class UIActiveBattlePass : MonoBehaviour
{
    [SerializeField] private GameObject activeObj;
    [SerializeField] private GameObject inactiveObj;
    [SerializeField] private UnityEvent OnDisablePack;

    [SerializeField] private UnityEvent onClick;
    private readonly Service<BattlePassService> battlePassService = new Service<BattlePassService>();

    private void OnEnable()
    {
        UpdateUI();
        battlePassService.Instance.OnUpdateUI += UpdateUI;
    }

    private void OnDisable()
    {
        battlePassService.Instance.OnUpdateUI -= UpdateUI;
    }

    private void UpdateUI(int idx = -1)
    {
        if (battlePassService.Instance.IsUnlocked() == true && battlePassService.Instance.CheckActivate() == false)
        {
            if (activeObj != null)
                activeObj.SetActive(true);
            if (inactiveObj != null)
                inactiveObj.SetActive(false);
        }
        else
        {
            if (activeObj != null)
                activeObj.SetActive(false);
            if (inactiveObj != null)
                inactiveObj.SetActive(true);

            OnDisablePack?.Invoke();
        }
    }

    public void OnClick()
    {
        var pack = battlePassService.Instance.GetCurrentThemeData().activateRewards;

        PanelManager.Instance.OpenPanel<PopupReward>(new UIData().Add("Reward", pack));
        battlePassService.Instance.ActivateBattlePass();
        onClick?.Invoke();
    }
}