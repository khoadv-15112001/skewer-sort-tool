using Cysharp.Threading.Tasks;
using I2.Loc;
using MyGame.SkewerJam.Gameplay;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;

public class PopupBackHome_SkewerJam : Panel
{
    [SerializeField] private LocalizationParamsManager localize;
    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        var pumpkinInGame = GameController.Instance.GameLogicHandler.Pumpkin;
        localize.SetParameterValue("VALUE", pumpkinInGame.ToString());
    }

    public void OnClickContinue()
    {
        base.Close();
        GameController.Instance.Continue();
    }

    public void OnClickHome()
    {
        base.Close();

        SaveAndGoHome();

        // GameplayHelper.GoHome();
    }

    public async UniTask SaveAndGoHome()
    {
        PanelManager.Instance.OpenPanelByName<PopupLoading>("PopupLoading_SkewerJam", new UIData().Add("Time", 1f));
        await GameplayStateSaver.Instance.Save();
        SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
    }
}