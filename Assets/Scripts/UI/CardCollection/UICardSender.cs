using GrillSort.OnlineService;
using TMPro;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    public class UICardSender : MonoBehaviour
    {
        [SerializeField] private UIAvatarBase avatar;
        [SerializeField] private TextMeshProUGUI username;

        public void Bind(UserProfile sender)
        {
            if (sender == null)
            {
                gameObject.SetActive(false);
                return;
            }

            username.text = sender.name;
            avatar.Init(sender.avatar);
        }
    }
}
