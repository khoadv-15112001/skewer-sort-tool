using System;
using Cysharp.Threading.Tasks;
using Manager;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sonat.CustomService
{
    [CreateAssetMenu(menuName = "Custom Services/Scene Service", fileName = "CustomSceneService")]
    public class CustomSceneService : SceneService, IServiceInitialize
    {
        private GamePlacement currentPlacement = GamePlacement.Loading;

        public override GamePlacement GetCurrentGamePlacement()
        {
            return currentPlacement;
        }

        public override void SwitchScene(GamePlacement newPlacement, bool force = false, Action callback = null)
        {
            if (newPlacement == currentPlacement && !force) return;
            EventBus<SwitchPlacementEvent>.Raise(
                new SwitchPlacementEvent { from = currentPlacement, to = newPlacement });

            //string sceneName = newPlacement.ToString();
            // if (newPlacement == GamePlacement.Gameplay && GameRemoteConfigValue.noCharacter)
            // {
            //     sceneName = "Gameplay_NoCharacter";
            // }
            LoadSceneAsync(newPlacement, callback).Forget();
        }

        private async UniTask LoadSceneAsync(GamePlacement newPlacement, Action callback = null)
        {
            string sceneName = newPlacement.ToString();
            await SceneManager.LoadSceneAsync(sceneName);
            currentPlacement = newPlacement;
            callback?.Invoke();
        }

        public void Initialize()
        {
            currentPlacement = GamePlacement.Loading;
        }
    }
}