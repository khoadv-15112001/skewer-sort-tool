using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Entities.Obstacle;
using Gameplay.LevelData;
using MyGame.SkewerJam.Gameplay;
using MyGame.SkewerJam.Gameplay.Helpers;
using MyGame.SkewerJam.Scripts.SO.SkewerJam.Gameplay;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using UnityEngine;
using static PopupUnlock_SkewerJam;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "OctoChefBehaviorSO_SkewerJam", menuName = "MyGame/SkewerJam/OctoChefBehaviorSO_SkewerJam")]
    public class OctoChefBehaviorSO_SkewerJam : OctoChefBehaviorSO
    {
        [Space(10)]
        [Header("Config")]
        [SerializeField] private OctoChefConfigSO_SkewerJam octoChefConfigSO_SkewerJam;

        public override LevelData GetLevelData()
        {
            return GameController.Instance.LevelGenerator.LevelData;
        }

        public override List<PrimaryGrill> GetPrimaryGrills()
        {
            return GameController.Instance.GameLogicHandler.GrillManager.ListGrills;
        }

        public override float GetItemThreshold()
        {
            var difficulty = GameController.Instance.LevelGenerator.LevelData.difficulty;
            return octoChefConfigSO_SkewerJam.GetItemThreshold(difficulty);
        }

        public override float GetGrillThreshold()
        {
            var difficulty = GameController.Instance.LevelGenerator.LevelData.difficulty;
            return octoChefConfigSO_SkewerJam.GetGrillThreshold(difficulty);
        }

        public override void SetStartGrill(OctoChefObstacle octoChefObstacle, List<GrillBase> grills)
        {
            var listGrills = GameController.Instance.GameLogicHandler.GrillManager.ListGrills;
            var randomGrill = GetRandomGrillByOrderItems(listGrills, octoChefObstacle.SlotCount, null, new System.Random(), true);
            octoChefObstacle.SetCurrentGrill(randomGrill);
        }

        public override PrimaryGrill GetRandomGrill(List<PrimaryGrill> validGrills, int slotCount, PrimaryGrill lastGrill, System.Random rng)
        {
            // theo levelDifficulty: easy, normal, hard
            // 
            return GetRandomGrillByOrderItems(validGrills, slotCount, lastGrill, rng, false);

        }

        private PrimaryGrill GetRandomGrillByOrderItems(List<PrimaryGrill> validGrills, int slotCount, PrimaryGrill lastGrill, System.Random rng, bool force)
        {

            var difficulty = GameController.Instance.LevelGenerator.LevelData.difficulty;
            var (rate0, rate1, rate2) = octoChefConfigSO_SkewerJam.GetRate(difficulty);

            var dictGrillCountOrderItems = GrillHelper.GetGrillCountOrderItems();

            var tempList = validGrills.Where(e => (e.IsLock == false && e.SlotCount == slotCount)).Select(e => e.id).ToList();

            var listGrill0 = tempList.Where(e => dictGrillCountOrderItems[e] == 0).ToList();
            var listGrill1 = tempList.Where(e => dictGrillCountOrderItems[e] == 1).ToList();
            var listGrill2 = tempList.Where(e => dictGrillCountOrderItems[e] == 2).ToList();


            var selectedList = listGrill0;
            if (force == false)
            {
                var randomList = UnityEngine.Random.Range(0, 1.0f);
                Debug.Log("<color=green>OctoChefBehaviorSO_SkewerJam:</color> randomList: " + randomList);
                if (randomList < rate0)
                {
                    Debug.Log("<color=green>OctoChefBehaviorSO_SkewerJam:</color> randomList < rate0");
                    selectedList = listGrill0;
                }
                else if (randomList < rate0 + rate1)
                {
                    Debug.Log("<color=green>OctoChefBehaviorSO_SkewerJam:</color> randomList < rate0 + rate1");
                    selectedList = listGrill1;
                }
                else
                {
                    Debug.Log("<color=green>OctoChefBehaviorSO_SkewerJam:</color> randomList < rate0 + rate1 + rate2");
                    selectedList = listGrill2;
                }
            }

            if (selectedList.Count == 0)
            {
                Debug.Log("<color=red>OctoChefBehaviorSO_SkewerJam:</color> selectedList.Count == 0");
                selectedList = tempList;
            }

            var randId = UnityEngine.Random.Range(0, selectedList.Count);
            return validGrills.First(e => e.id == selectedList[randId]);
        }

        public override void RegisterEvents_OnCollectItem(Action<int> onCollectItem)
        {
            var gameLogicHandler = GameController.Instance.GameLogicHandler;
            gameLogicHandler.OnAppearNextOrderItem += onCollectItem;
        }

        public override void UnregisterEvents_OnCollectItem(Action<int> onCollectItem)
        {
            var gameLogicHandler = GameController.Instance.GameLogicHandler;
            gameLogicHandler.OnAppearNextOrderItem -= onCollectItem;
        }

        public override void CustomUpdate(OctoChefObstacle octoChefObstacle)
        {
            // Khi nhả chuột trái
            //  var popup = PanelManager.Instance.GetPanel<PopupUnlock_SkewerJam>();
            if (GameController.Instance.GameState == GameState.Playing && Input.GetMouseButtonUp(0))
            {
                var hits = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                foreach (var hit in hits)
                {
                    if (hit.gameObject == octoChefObstacle.gameObject)
                    {
                        OpenPopupUnlock(()=>{
                            octoChefObstacle.UnlockCurrentGrill();
                            octoChefObstacle.OnComplete();
                        });
                        break;
                    }
                }
            }
        }

        private void OpenPopupUnlock(Action onComplete)
        {
            var uiData = new UIData();
            uiData.Add("SelectedObjectType", SelectedObjectType.OctoChef);
            uiData.Add("Price", GameController.Instance.GameConfig.unlockOctoChefPrice);
            uiData.Add("OnSuccess", onComplete);
            PanelManager.Instance.OpenPanelByName<PopupUnlock_SkewerJam>("PopupUnlockOctochef_SkewerJam", uiData);
        }
    }
}