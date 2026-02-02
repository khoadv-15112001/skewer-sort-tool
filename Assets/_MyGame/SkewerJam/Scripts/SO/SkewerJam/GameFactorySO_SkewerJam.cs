using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "GameFactorySO_SkewerJam", menuName = "MyGame/SkewerJam/GameFactorySO_SkewerJam")]
    public class GameFactorySO_SkewerJam : GameFactorySO
    {
        public override async UniTask<T> CreateItem<T>(string name, Transform parent = null)
        {
            return await MyGame.SkewerJam.Gameplay.GameFactory.Instance.CreateEntityAsync<T>(name, parent);
        }

        public override async UniTask<T> CreateItem<T>(string name, Vector3 position, Transform parent = null)
        {
            return await MyGame.SkewerJam.Gameplay.GameFactory.Instance.CreateEntityAsync<T>(name, position, parent);
        }

        public override void ReturnEntity<T>(T subGrill)
        {
            MyGame.SkewerJam.Gameplay.GameFactory.Instance.ReturnEntity(subGrill);
        }
    }
}