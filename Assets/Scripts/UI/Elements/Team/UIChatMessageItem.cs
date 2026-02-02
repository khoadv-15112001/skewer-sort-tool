using Helper;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChatMessageItem : MonoBehaviour
{
    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private TextMeshProUGUI timeTxt;

    [SerializeField] private RectTransform bubbleChat;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private RectTransform rect;

    [SerializeField] private bool isSelf;

    private float constHeight;
    private string userId;

    private readonly Service<ProfileService> profileService = new();

    public void Init()
    {
        constHeight = username.GetComponent<RectTransform>().sizeDelta.y + timeTxt.GetComponent<RectTransform>().sizeDelta.y;
    }

    public void Bind(TeamMessageData data)
    {
        userId = data.sender.id;

        username.text = data.sender.name;
        content.text = data.content;
        timeTxt.text = TimeHelper.GetLocalTimeFromISO(data.created_at).ToString();

        if (isSelf)
        {
            avatar.InitSelf();
        }
        else
        {
            avatar.Init(data.sender.avatar);
        }

        content.ForceMeshUpdate();
        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, content.preferredHeight);

        bubbleChat.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentRect.sizeDelta.y + constHeight);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bubbleChat.sizeDelta.y);
    }

    public void OnClick()
    {
        UIData data = new();
        data.Add("userId", userId);
        PanelManager.Instance.OpenForget<PopupUserProfile>(data);
    }
}
