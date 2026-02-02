using MyGame.SkewerJam.Gameplay;
using MyGame.SkewerJam.Gameplay.Helpers;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;

public class PopupLose_SkewerJam : Panel
{
    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        MySonatFramework.audioService.StopMusic();
        MySonatFramework.audioService.PlaySound(AudioId.Lose_HLW_Music_Grill_sort);
    }

    public void OnClickRetry()
    {
        base.Close();
        GameController.Instance.Replay();
    }

    public void OnClickHome()
    {
        base.Close();
        GameplayHelper.GoHome();
    }
}
