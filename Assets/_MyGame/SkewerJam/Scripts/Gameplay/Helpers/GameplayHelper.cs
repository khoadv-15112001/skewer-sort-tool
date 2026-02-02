using System;
using Cysharp.Threading.Tasks;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay.Helpers
{
    public static class GameplayHelper
    {
        public static bool CheckStart()
        {
            var energy = MySonatFramework.GetService<InventoryService>().GetResource(GameResource.Energy);
            return energy > 0;
        }

        public static void GoHome()
        {
            PanelManager.Instance.OpenPanelByName<PopupLoading>("PopupLoading_SkewerJam");
            SonatUtils.DelayCall(0.25f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
        }

        public static void OnClose_ChangeGameState(GameState playing)
        {
            SonatUtils.ExecuteNextFrame(() =>
            {
                EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent() { gameState = playing });
            });
        }

        public static bool IsWin { get => PlayerPrefs.GetInt("IsWin_SkewerJam", 1) == 1; set => PlayerPrefs.SetInt("IsWin_SkewerJam", value ? 1 : 0); }
    }
}