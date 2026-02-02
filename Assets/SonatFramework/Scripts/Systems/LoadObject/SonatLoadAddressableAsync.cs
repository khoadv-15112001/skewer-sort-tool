using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if using_addressable
using UnityEngine.AddressableAssets;
#endif

namespace SonatFramework.Systems.LoadObject
{
    [CreateAssetMenu(menuName = "Sonat Services/Load Service/Load Addressable Async",
        fileName = "Load Addressable Async")]
    public class SonatLoadAddressableAsync : LoadObjectServiceAsync
    {
        private readonly TimeoutController timeoutController = new();
        [SerializeField] private float timeout;
        [SerializeField] private LoadObjectServiceAsync backup;


        public override async UniTask<T> LoadAsync<T>(string assetName) where T : class
        {
#if using_addressable
            try
            {
                string fullPath = $"{path}{assetName}";
                return await Addressables.LoadAssetAsync<T>(fullPath)
                    .WithCancellation(timeoutController.Timeout(TimeSpan.FromSeconds(timeout)));
            }
            catch (OperationCanceledException e)
            {
                if (backup != null)
                {
                    return await backup.LoadAsync<T>(assetName);
                }
            }
#endif
            return null;
        }
    }
}