using System;
using System.Linq;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using UnityEngine;

[CreateAssetMenu(fileName = "SalePackConfig", menuName = "Sonat Configs Custom/SalePackConfig")]
public class SalePackConfig : ConfigSo
{
    public SalePack[] salePacks;

    public SalePack GetPack(int saleSessionId)
    {
        return salePacks.FirstOrDefault(pack => pack.SaleSessionId == saleSessionId);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        int id = 0;
        foreach (var pack in salePacks)
        {
            pack.SaleSessionId = id++;
        }
    }
#endif
}

[Serializable]
public class SalePack
{
    [GUIColor(0, 1, 0, 1)]
    [SerializeField] private int _saleSessionId;
    public int SaleSessionId { get => _saleSessionId; set => _saleSessionId = value; }

    public ShopItemKey shopItemKey;

    [Header("Sale")]
    public ShopItemKey saleItemKey;
    public int saleDuration;

    [Header("Condition")]
    public bool startFromAppearance = true;
}
