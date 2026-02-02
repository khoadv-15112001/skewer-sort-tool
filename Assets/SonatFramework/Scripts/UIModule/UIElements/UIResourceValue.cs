using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using TMPro;
using UnityEngine;

public class UIResourceValue : MonoBehaviour
{
    [SerializeField] private GameResource resource;
    [SerializeField] private TMP_Text txtValue;
    private readonly Service<InventoryService> inventoryService = new();
    protected virtual void OnDisable()
    {
        inventoryService.Instance.OnResourceUpdate -= OnResourceUpdate;
    }

    public virtual void OnEnable()
    {
        OnResourceUpdate(resource);
        inventoryService.Instance.OnResourceUpdate += OnResourceUpdate;
    }

    protected virtual void OnResourceUpdate(GameResource res)
    {
        if (res != resource && res != GameResource.MAX) return;
        var value = inventoryService.Instance.GetResource(resource);
        txtValue.text = value.ToString();
    }

}
