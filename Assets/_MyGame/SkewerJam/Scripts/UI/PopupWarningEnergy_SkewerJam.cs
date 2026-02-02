using MyGame.SkewerJam.Gameplay;
using MyGame.SkewerJam.Gameplay.Helpers;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement;
using UnityEngine;
using UnityEngine.UI;

public class PopupWarningEnergy_SkewerJam : Panel
{
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnPlayMain;

    // [SerializeField] private GameObject currencyToView;
    [SerializeField] private GameObject[] continueTags;

    private GamePlacement gamePlacement;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        if (uiData.TryGet<GamePlacement>("GamePlacement", out GamePlacement gamePlacement))
        {
            this.gamePlacement = gamePlacement;
        }
        else
        {
            this.gamePlacement = GamePlacement.Gameplay_SkewerJam;
        }

        btnPlayMain.gameObject.SetActive(true);
        btnContinue.gameObject.SetActive(false);
        // currencyToView.SetActive(gamePlacement == GamePlacement.Gameplay_SkewerJam);

        // show tag
        foreach (var tag in continueTags)
        {
            tag.SetActive(gamePlacement == GamePlacement.Gameplay_SkewerJam);
        }
        continueTags[0].SetActive(gamePlacement == GamePlacement.Gameplay_SkewerJam);


        if (gamePlacement == GamePlacement.Gameplay_SkewerJam)
        {
            GameplayStateSaver.Instance.Save();
        }
    }

    public void OnClickContinue()
    {
        var energy = MySonatFramework.GetService<InventoryService>().GetResource(GameResource.Energy);
        if (energy > 0)
        {
            base.Close();
            GameController.Instance.Continue();
        }
        else
        {
            PopupToast.Cretate("Not enough Energies!");
        }
    }

    public void OnClose()
    {
        base.Close();

        if (gamePlacement == GamePlacement.Gameplay_SkewerJam)
        {
            GameplayHelper.GoHome();
        }
        else if (gamePlacement == GamePlacement.Home)
        {
            PanelManager.Instance.ClosePanel<PanelLeaderboard_SkewerJam>();
        }
    }

    public void OnCompletePack()
    {
        base.Close();

        switch (gamePlacement)
        {
            case GamePlacement.Gameplay_SkewerJam:
                GameController.Instance.Continue();
                break;
        }
    }

}
