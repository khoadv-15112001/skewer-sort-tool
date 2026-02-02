using Cysharp.Threading.Tasks;
using SonatFramework.Systems;

namespace SonatFramework.Scripts.Feature
{
    public abstract class SonatFeature<TConfig, TData> : IServiceInitializeAsync
    {
        public TConfig configs;
        public TData data;


        public virtual async UniTaskVoid InitializeAsync()
        {
            await LoadConfig();
            await LoadData();
        }

        protected abstract UniTask LoadConfig();
        protected abstract UniTask LoadData();

        protected abstract void SaveData();
    }
}