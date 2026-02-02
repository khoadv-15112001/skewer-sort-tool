using GrillSort.BattlePass;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBattlePassPack : MonoBehaviour
{
    [SerializeField] private GameObject packDefault;
    [SerializeField] private GameObject packThanksgiving;
    [SerializeField] private GameObject packXmas;

    private BattlePassService battlePassService => SonatSystem.GetService<BattlePassService>();

    private void OnEnable()
    {
        CheckTheme();
        UIShopPackContent.OnExpand += CheckTheme;
    }

    private void OnDisable()
    {
        UIShopPackContent.OnExpand -= CheckTheme;
    }

    private void CheckTheme()
    {
        var theme = battlePassService.GetCurrentThemeData().theme;

        packDefault.SetActive(theme == BattlePassGeneralConfig.Theme.Default);
        packThanksgiving.SetActive(theme == BattlePassGeneralConfig.Theme.Thanksgiving);
        packXmas.SetActive(theme == BattlePassGeneralConfig.Theme.Xmas);
    }
}
