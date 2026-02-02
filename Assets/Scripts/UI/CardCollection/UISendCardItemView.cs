using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using TMPro;
using UnityEngine;

public class UISendCardItemView : MonoBehaviour
{
    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private TextMeshProUGUI username;

    [SerializeField] private TMPMarquee marquee;

    [SerializeField] private RectTransform usernameRect;
    [SerializeField] private RectTransform mask;

    private CardType cardType;
    private float usernameMaxWidth;
    private Member receiver;

    public void Init(CardType cardType)
    {
        this.cardType = cardType;
        usernameMaxWidth = mask.sizeDelta.x;
    }

    public void Bind(Member data)
    {
        receiver = data;

        avatar.Init(data.avatar);

        username.text = data.name;

        float width = Mathf.Min(username.preferredWidth, usernameMaxWidth);
        usernameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        if (username.preferredWidth > usernameMaxWidth)
            marquee.StartMarquee();
    }

    public void OnClickSend()
    {
        UIData data = new();
        data.Add("member", receiver);
        data.Add("cardType", cardType);
        data.Add("fromRequest", false);
        PanelManager.Instance.OpenForget<PopupConfirmSendCard>(data);
    }

    public void OnClickViewProfile()
    {
        UIData data = new();
        data.Add("userId", receiver.id);
        PanelManager.Instance.OpenForget<PopupUserProfile>(data);
    }
}
