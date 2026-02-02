using MyGame.SkewerJam.Gameplay.Helpers;
using MyGame.SkewerJam.Scripts.Service;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SkewerJam.UI.Elements
{
    public class UIButtonPlay : MonoBehaviour
    {
        [SerializeField] private Image imageGreen;
        [SerializeField] private Image imageGray;

        [SerializeField] private bool forcePlay = false;

        private Service<HLWEventService> hlwEventService = new();
        private EventBinding<AddItemEvent> addItemEvent;
        private void OnEnable()
        {
            if (hlwEventService.Instance.GetRemainTime() <= 0)
            {
                gameObject.SetActive(false);
                return;
            }

            UpdateUI();
            addItemEvent = new EventBinding<AddItemEvent>(UpdateUI);
        }

        private void OnDisable()
        {
            EventBus<AddItemEvent>.Deregister(addItemEvent);
        }

        private void UpdateUI()
        {
            if (MySonatFramework.GetService<InventoryService>().GetResource(GameResource.Energy) > 0 || PlayerPrefs.GetInt("FirstPlayHLW", 0) == 0)
            {
                if (imageGreen != null) imageGreen.gameObject.SetActive(true);
                if (imageGray != null) imageGray.gameObject.SetActive(false);
            }
            else
            {
                if (imageGreen != null) imageGreen.gameObject.SetActive(false);
                if (imageGray != null) imageGray.gameObject.SetActive(true);
            }
        }

        public void PlayClick()
        {
            if (GameplayHelper.CheckStart() || forcePlay || PlayerPrefs.GetInt("FirstPlayHLW", 0) == 0)
            {
                PlayerPrefs.SetInt("FirstPlayHLW", 1);
                MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Gameplay_SkewerJam);
            }
            else
            {
                // PanelManager.Instance.OpenPanel<PopupWarningEnergy_SkewerJam>(new UIData().Add("GamePlacement", GamePlacement.Home));
                PanelManager.Instance.OpenPanel<PopupTrickOrTreat_HLW>();
            }
        }
    }
}
