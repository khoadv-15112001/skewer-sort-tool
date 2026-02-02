using Cysharp.Threading.Tasks;
using Gameplay.LevelData;
using Sonat.CustomService;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class TutorialManager : MonoBehaviour
    {
        EventBinding<LevelStartedEvent_HLW> eventBinding;

        private void OnEnable()
        {
            eventBinding = new EventBinding<LevelStartedEvent_HLW>(OnStartLevel);
        }

        private void OnDisable()
        {
            EventBus<LevelStartedEvent_HLW>.Deregister(eventBinding);
        }

        private void OnStartLevel(LevelStartedEvent_HLW eventData)
        {
            if (eventData.level == 1)
            {
                TryOpenPopupStartGameplay().Forget();
                return;
            }

            var tutorialType = CheckTutorial(eventData.level);
            if (tutorialType != TutorialType.None)
            {
                if (PlayerPrefs.HasKey($"{tutorialType.ToString()}Showed"))
                {
                    return;
                }
                CheckShowTutObstacle(tutorialType.ToString());
            }
        }

        private TutorialType CheckTutorial(int level)
        {
            var levelData = GameController.Instance.LevelGenerator.LevelData;
            if (levelData.obstacleData != null)
            {
                foreach (var obstacleData in levelData.obstacleData)
                {
                    if (obstacleData.obstacleType == ObstacleType.OctoChef)
                    {
                        return TutorialType.PopupTutOctochef_SkewerJam;
                    }
                }
            }
            return TutorialType.None;
        }

        private void CheckShowTutObstacle(string tutorial)
        {
            PlayerPrefs.SetInt($"{tutorial}Showed", 1);
            UIFlowController.isShowedTut = true;
            SonatUtils.DelayCall(1f, () => { ShowPopupTutorial<PopupTutNewMode>(tutorial).Forget(); });
        }

        private async UniTask ShowPopupTutorial<T>(string tutorial) where T : Panel
        {
            await UniTask.WaitUntil(() => UIFlowController.CheckConditionShowPopupTutNewMode());
            PanelManager.Instance.OpenPanelByName<T>(tutorial);
        }


        private async UniTask TryOpenPopupStartGameplay()
        {
            if (PlayerPrefs.GetInt("ShowPopupStartGameplay_HLW", 0) == 0)
            {
                await UniTask.Delay(1500);
                PlayerPrefs.SetInt("ShowPopupStartGameplay_HLW", 1);
                var popupStart = PanelManager.Instance.OpenPanelByName<PopupStartGameplay_HLW>("PopupStartGameplay_HLW");
                await UniTask.WaitUntil(() => popupStart == null || !popupStart.gameObject.activeInHierarchy);
                await UniTask.Delay(1500);
                var popupTut = PanelManager.Instance.OpenPanelByName<PopupTutNewMode>("PopupTutGameplayHLW");
                await UniTask.WaitUntil(() => popupTut == null || !popupTut.gameObject.activeInHierarchy);
                GameController.Instance.ChangeGameState(GameState.Playing);
            }
        }
    }

    public enum TutorialType
    {
        None,
        PopupTutOctochef_SkewerJam
    }
}