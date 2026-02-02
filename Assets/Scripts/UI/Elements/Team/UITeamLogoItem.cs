using Manager;
using SonatFramework.Systems.EventBus;
using UnityEngine;
using UnityEngine.UI;

public class UITeamLogoItem : MonoBehaviour
{
    [SerializeField] private Image logo;

    private int id;

    public void Bind(int id)
    {
        this.id = id;

        _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(id));
    }

    public void OnClick()
    {
        EventBus<ChooseTeamLogoEvent>.Raise(new ChooseTeamLogoEvent() { id = id });
    }

    public struct ChooseTeamLogoEvent : IEvent
    {
        public int id;
    }
}
