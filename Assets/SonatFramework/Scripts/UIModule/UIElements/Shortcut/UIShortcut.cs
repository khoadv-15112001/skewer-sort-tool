using SonatFramework.Systems.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Scripts.UIModule.UIElements.Shortcut
{
    [RequireComponent(typeof(Button))]
    public class UIShortcut : MonoBehaviour
    {
        [SerializeField] private string panelOpenName; 
        [SerializeField] private string tracking;
        private Button button;

        // Start is called before the first frame update
        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClickShortcut);
        }

        private void OnClickShortcut()
        {
            EventBus<ClickShortcutEvent>.Raise(new ClickShortcutEvent(){shortcut = tracking});
            OpenPanel();
        }

        private void OpenPanel()
        {
            PanelManager.Instance.OpenPanelByName<Panel>(panelOpenName);
        }
    }
}