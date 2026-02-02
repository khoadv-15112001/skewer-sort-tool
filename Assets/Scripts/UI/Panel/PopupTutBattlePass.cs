using SonatFramework.Scripts.UIModule;
using UnityEngine;
public class PopupTutBattlePass : Panel
{
    [SerializeField] private GameObject btnPlay;
    [SerializeField] private GameObject btnBattlePass;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        btnBattlePass.SetActive(false);
        btnPlay.SetActive(false);
        if (uiData != null && uiData.TryGet("ShowButtonPlay", out bool showButtonPlay))
        {
            btnPlay.SetActive(showButtonPlay);
            btnBattlePass.SetActive(!showButtonPlay);
        }
        else
        {
            btnPlay.SetActive(true);
            btnBattlePass.SetActive(false);
        }

    }
}
