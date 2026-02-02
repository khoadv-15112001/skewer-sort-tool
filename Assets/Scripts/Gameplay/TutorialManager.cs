using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.LevelData;
using Manager;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.EventBus;
using UnityEngine;

namespace Gameplay
{
    public class TutorialManager : MonoBehaviour
    {
        EventBinding<LevelStartedEvent> eventBinding;

        private void Start()
        {
            eventBinding = new EventBinding<LevelStartedEvent>(OnStartLevel);

            if (GameRemoteConfigValue.swapItem && !PlayerPrefs.HasKey("PopupTutSwapItemsShowed"))
            {
                int level = MySonatFramework.userDataService.GetLevel();
                if (level <= 1)
                {
                    PlayerPrefs.SetInt("PopupTutSwapItemsShowed", 1);
                }
            }
        }

        private void OnDisable()
        {
            EventBus<LevelStartedEvent>.Deregister(eventBinding);
        }

        private void OnStartLevel(LevelStartedEvent eventData)
        {
            if (eventData.level > 500)
            {
                return;
            }

            CheckTutorial(eventData.level);
        }


        private void CheckTutorial(int level)
        {
            if (level == 1)
            {
                PanelManager.Instance.OpenPanel<PopupTutorial>();
                return;
            }

            var listTutorial = GetListTutorialTypes();

            foreach (var tutorial in listTutorial)
            {
                if (PlayerPrefs.HasKey($"{tutorial.ToString()}Showed"))
                {
                    if (PlayerPrefs.GetInt($"{tutorial.ToString()}Showed") == 1)
                    {
                        continue;
                    }
                }

                CheckShowTutObstacle(tutorial.ToString());
                return;
            }


            if (GameRemoteConfigValue.swapItem && !PlayerPrefs.HasKey("PopupTutSwapItemsShowed"))
            {
                PlayerPrefs.SetInt("PopupTutSwapItemsShowed", 1);
                CheckShowTutObstacle("PopupTutSwapItems");
            }

            // switch (level)
            // {
            //     case 1:
            //         PanelManager.Instance.OpenPanel<PopupTutorial>();
            //         break;
            //     case 3:
            //         CheckShowTutObstacle("PopupTutSingleGrill");
            //         break;
            //     case 5:
            //         CheckShowTutObstacle("PopupTutLockTray");
            //         break;
            //     // case 5:
            //     //     CheckShowTutObstacle("PopupTutLockObstacle");
            //     //     break;
            //     case 7:
            //         CheckShowTutObstacle("PopupTutEmptyTray");
            //         break;
            //     case 8:
            //         CheckShowTutObstacle("PopupTutOrder");
            //         break;
            //     case 9:
            //         CheckShowTutObstacle("PopupTutLiddedTray");
            //         break;
            //     case 10:
            //         CheckShowTutObstacle("PopupTutConveyor");
            //         break;
            //     case 12:
            //         CheckShowTutObstacle("PopupTutDropLine");
            //         break;
            //     case 18:
            //         CheckShowTutObstacle("PopupTutVendingTray");
            //         break;
            //     case 24:
            //         CheckShowTutObstacle("PopupTutHiddenItem");
            //         break;
            //     case 46:
            //         CheckShowTutObstacle("PopupTutLockAndKey");
            //         break;
            //     case 66:
            //         CheckShowTutObstacle("PopupTutFreeze");
            //         break;
            //     case 77:
            //         CheckShowTutObstacle("PopupTutIceLock");
            //         break;
            //     case 91:
            //         CheckShowTutObstacle("PopupTutBomb");
            //         break;
            //     default:
            //         break;
            //         // case 30:
            //         //     CheckShowTutObstacle("PopupTutDropLine");
            //         //     break;
            // }
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

        public List<TutorialType> GetListTutorialTypes()
        {
            var list = new HashSet<TutorialType>();

            var levelData = GameplayController.instance.levelGenerator.LevelData;

            // check grill
            foreach (var grillData in levelData.grillData)
            {
                switch (grillData.grillType)
                {
                    case GrillType.Normal:
                        if (grillData.SlotCount == 1)
                        {
                            list.Add(TutorialType.PopupTutSingleGrill);
                        }

                        if (grillData.SlotCount == 7)
                        {
                            list.Add(TutorialType.PopupTutDropLine);
                        }

                        break;
                    case GrillType.Ice:
                        list.Add(TutorialType.PopupTutIceLock);
                        break;
                    case GrillType.Lid:
                        list.Add(TutorialType.PopupTutLiddedTray);
                        break;
                    case GrillType.Vending:
                        list.Add(TutorialType.PopupTutVendingTray);
                        break;
                    case GrillType.LockAndKey:
                    case GrillType.LockAndKey2:
                        list.Add(TutorialType.PopupTutLockAndKey);
                        break;
                    case GrillType.Lock:
                        list.Add(TutorialType.PopupTutLockTray);
                        break;
                    case GrillType.LockAds:
                        list.Add(TutorialType.PopupTutLockAds);
                        break;
                    case GrillType.Spicy:
                        list.Add(TutorialType.PopupTutSpicyTray);
                        break;
                    case GrillType.Broken:
                        list.Add(TutorialType.PopupTutBrokenTray);
                        break;
                    case GrillType.Overcooked:
                        list.Add(TutorialType.PopupTutOvercooked);
                        break;
                }

                // if (item.layer == null || item.layer.Count == 0)
                // {
                //     list.Add(TutorialType.PopupTutEmptyTray);
                //     continue;
                // }

                // check layer/ item
                if (grillData.layer != null)
                    foreach (var layer in grillData.layer)
                    {
                        foreach (var itemData in layer.itemData)
                        {
                            if (itemData == null) continue;
                            switch (itemData.itemType)
                            {
                                case ItemType.Hidden:
                                    list.Add(TutorialType.PopupTutHiddenItem);
                                    break;
                                case ItemType.Bomb:
                                    list.Add(TutorialType.PopupTutBomb);
                                    break;
                                case ItemType.Ice:
                                    list.Add(TutorialType.PopupTutFreeze);
                                    break;
                                case ItemType.Obstacle:
                                    list.Add(TutorialType.PopupTutStarbar);
                                    break;
                                case ItemType.Key:
                                case ItemType.Key2:
                                    break;
                            }
                        }
                    }

                if (levelData.obstacleData != null)
                {
                    foreach (var obstacleData in levelData.obstacleData)
                    {
                        switch (obstacleData.obstacleType)
                        {
                            case ObstacleType.OctoChef:
                                list.Add(TutorialType.PopupTutOctoChef);
                                break;
                            case ObstacleType.LockAreaHorizontal or ObstacleType.LockAreaVertical:
                                list.Add(TutorialType.PopupTutLockArea);
                                break;
                        }
                    }
                }
            }

            // check conveyor
            if (levelData.conveyorData != null && levelData.conveyorData.Count > 0)
            {
                list.Add(TutorialType.PopupTutConveyor);
            }

            // check order
            if (levelData.orderData != null && levelData.orderData.Count > 0)
            {
                list.Add(TutorialType.PopupTutOrder);
            }

            return list.ToList();
        }

        [Button]
        public void ShowTut(TutorialType tutorialType)
        {
            CheckShowTutObstacle(tutorialType.ToString());
        }
    }

    public enum TutorialType
    {
        PopupTutSingleGrill,
        PopupTutHiddenItem,
        PopupTutOrder,
        PopupTutConveyor,
        PopupTutLockTray,
        PopupTutVendingTray,
        PopupTutFreeze,
        PopupTutIceLock,
        PopupTutBomb,
        PopupTutLockAndKey,
        PopupTutLiddedTray,

        //PopupTutEmptyTray,
        PopupTutDropLine,
        PopupTutLockAds,
        PopupTutSpicyTray,
        PopupTutBrokenTray,
        PopupTutLockArea,
        PopupTutOctoChef,
        PopupTutOvercooked,
        PopupTutStarbar
    }
}