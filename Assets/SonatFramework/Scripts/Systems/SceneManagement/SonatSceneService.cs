using System;
using Cysharp.Threading.Tasks;
using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SonatFramework.Systems.SceneManagement
{
    [CreateAssetMenu(menuName = "Sonat Services/Scene Service", fileName = "SonatSceneService")]
    public class SonatSceneService : SceneService, IServiceInitialize
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
            LoadSceneAsync(newPlacement, callback).Forget();
        }
        
        protected async UniTask LoadSceneAsync(GamePlacement newPlacement, Action callback = null)
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