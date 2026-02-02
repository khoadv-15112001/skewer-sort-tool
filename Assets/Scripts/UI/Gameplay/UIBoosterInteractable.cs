using Gameplay.Entities;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.BoosterManagement;
using UnityEngine;

public class UIBoosterInteractable : MonoBehaviour
{
    [SerializeField] private GameResource boosterType;
    [SerializeField] private GameObject objDisable;

    [SerializeField] private bool subcribeEvent_OnItemMoveSlot = true;
    [SerializeField] private bool subcribeEvent_OnCollectItem = true;
    [SerializeField] private bool subcribeEvent_OnActionUnlockGrill = true;
    [SerializeField] private bool subcribeEvent_OnActionLockGrill = true;

    private void OnEnable()
    {
        GameplayController.OnLoadLevel += OnLoadLevel;

        if (subcribeEvent_OnItemMoveSlot)
        {
            GameplayController.OnItemMoveSlot += OnItemMoveSlot;
        }

        if (subcribeEvent_OnCollectItem)

            GameplayController.OnCollectItem += OnCollectItem;

        if (subcribeEvent_OnActionUnlockGrill)
        {
            GameplayController.OnActionUnlockGrill += OnActionUnlockGrill;
        }

        if (subcribeEvent_OnActionLockGrill)
        {
            GameplayController.OnActionLockGrill += OnActionLockGrill;
        }

        GameplayController.OnUseBoosterSuccess += OnUseBoosterSuccess;

        // trường hợp: khi load level -> enable --> check xem có thể hiện booster không
        try
        {
            OnCheckBooster(0);
        }
        catch (System.Exception)
        {
        }
    }

    private void OnLoadLevel(int level)
    {
        OnCheckBooster(1);
    }

    private void OnItemMoveSlot(Item item, SlotBase slot)
    {
        OnCheckBooster(1);
    }

    private void OnCollectItem(int itemId)
    {
        OnCheckBooster(1);
    }


    private void OnActionUnlockGrill(PrimaryGrill grill)
    {
        OnCheckBooster(1);
    }

    private void OnActionLockGrill(PrimaryGrill grill)
    {
        OnCheckBooster(1);
    }

    private void OnDisable()
    {
        GameplayController.OnItemMoveSlot -= OnItemMoveSlot;
        GameplayController.OnCollectItem -= OnCollectItem;
        GameplayController.OnActionLockGrill -= OnActionLockGrill;
        GameplayController.OnActionUnlockGrill -= OnActionUnlockGrill;
        GameplayController.OnUseBoosterSuccess -= OnUseBoosterSuccess;
        GameplayController.OnLoadLevel -= OnLoadLevel;
    }

    private void OnUseBoosterSuccess(GameResource resource)
    {
        OnCheckBooster(2.5f);
    }

    private void OnCheckBooster(float delay)
    {
        SonatUtils.DelayCall(delay, () =>
        {
            if (GameplayController.instance.CanUseBooster(boosterType))
            {
                EnableBooster();
            }
            else
            {
                DisableBooster();
            }
        }, this);
        
    }

    private void EnableBooster()
    {
        if (objDisable != null)
        {
            objDisable.SetActive(false);
        }
    }

    private void DisableBooster()
    {
        if (objDisable != null)
        {
            var boosterData = MySonatFramework.GetService<BoosterService>().GetBoosterData(boosterType);
            objDisable.SetActive(true);
        }
    }
}