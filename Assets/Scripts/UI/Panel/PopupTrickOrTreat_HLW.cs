using MyGame.SkewerJam.Gameplay.Helpers;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
public class PopupTrickOrTreat_HLW : Panel
{
    public void CloseAndTryOpenPopupWarningEnergy()
    {
        Close();

        var gamePlacement = MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement();
        switch (gamePlacement)
        {
            case GamePlacement.Home:
                var energy = MySonatFramework.GetService<InventoryService>().GetResource(GameResource.Energy);
                var popup = PanelManager.Instance.GetPanel<PanelLeaderboard_SkewerJam>();
                if (energy <= 0 && popup != null && popup.gameObject.activeInHierarchy == true)
                {
                    var uiData = new UIData();
                    uiData.Add("GamePlacement", GamePlacement.Home);
                    PanelManager.Instance.OpenPanelByName<PopupWarningEnergy_SkewerJam>("PopupWarningEnergy_SkewerJam", uiData);
                }

                break;
            case GamePlacement.Gameplay_SkewerJam:
                break;
        }
    }

    public override void Close()
    {
        base.Close();

        var gamePlacement = MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement();
        switch (gamePlacement)
        {
            case GamePlacement.Home:
                break;
            case GamePlacement.Gameplay_SkewerJam:
                GameplayHelper.OnClose_ChangeGameState(GameState.Playing);
                break;
        }
    }
}
