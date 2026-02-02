using SonatFramework.Scripts.UIModule;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PopupActivateBattlePass : Panel
{
    [SerializeField] private UIAvatarBase avatar;
    [SerializeField] private TMP_Text nameTxt;
    private UnityEvent _onBuyCompleted;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        // if (uiData != null && uiData.TryGet<UnityEvent>("UnityEvent", out _onBuyCompleted))
        // {

        // }

        avatar.InitSelf();
        nameTxt.text = MySonatFramework.GetService<ProfileService>().Name;
    }

    public void OnBuyCompleted()
    {
        _onBuyCompleted?.Invoke();
    }
}
