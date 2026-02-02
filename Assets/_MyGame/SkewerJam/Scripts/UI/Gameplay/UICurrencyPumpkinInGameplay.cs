using DG.Tweening;
using SonatFramework.Scripts.UIModule.UIElements;

public class UICurrencyPumpkinInGameplay : UICurrency
{
    public override void UpdateValueView(bool doCounter = true)
    {
        int oldvalue = this.value;
        try
        {
            value = MyGame.SkewerJam.Gameplay.GameController.Instance.GameLogicHandler.Pumpkin;
        }
        catch (System.Exception)
        {
            value = 0;
        }

        if (value == oldvalue) return;
        if (gameObject.activeInHierarchy && doCounter)
            txtValue.DOCounter(oldvalue, value, counterDuration, addThousandsSeparator: false);
        else
            txtValue.text = value.ToString();

    }
}
