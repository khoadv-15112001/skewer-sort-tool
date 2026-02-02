using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using UnityEngine;

namespace GrillSort.ConsecutiveWin
{
    public class UIConsecutiveWin : MonoBehaviour
    {
        [SerializeField] private UIElementConsecutiveWin[] uiElementConsecutiveWin;

        private readonly Service<ConsecutiveWinService> _consecutiveWinService = new();

        private static bool enableSlider = false;
        private static int idxElement = -1;
        private int level;
        [SerializeField] private bool isTut;

        private void OnEnable()
        {
            level = MySonatFramework.userDataService.GetLevel();
            if (_consecutiveWinService.Instance.CheckStart(level) == false)
            {
                gameObject.SetActive(false);
                return;
            }
            UpdateUI();
        }

        private void OnDisable()
        {

        }

        private void UpdateUI()
        {
            var config = _consecutiveWinService.Instance.Config;
            var maxConsecutiveWins = config.GetMaxConsecutiveWins();
            var currentConsecutiveWins = _consecutiveWinService.Instance.GetConsecutiveWins();

            if (idxElement != currentConsecutiveWins)
            {
                idxElement = currentConsecutiveWins;
                enableSlider = true;
            }

            for (int i = 1; i <= maxConsecutiveWins; i++)
            {
                uiElementConsecutiveWin[i - 1].SetData(i, config.GetTimeToAdd(i));

                var playSlider = i == currentConsecutiveWins && enableSlider == true;
                if (playSlider)
                {
                    enableSlider = false;
                }
                uiElementConsecutiveWin[i - 1].SetComplete(i <= currentConsecutiveWins, playSlider);
            }

            if (ShowTut())
            {
                SonatUtils.DelayCall(0.25f, () =>
                {
                    PanelManager.Instance.OpenPanel<PopupUnlockConsecutiveWin>(new PopupUnlockConsecutiveWin.Data(){consecutiveWinTransform = this.transform});
                }, this);
                
            }
        }

        public void OnClickNoti()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>("PopupTutConsecutiveWin");
        }

        private bool ShowTut()
        {
            //return !isTut;
            return false;
        }

    }
}
