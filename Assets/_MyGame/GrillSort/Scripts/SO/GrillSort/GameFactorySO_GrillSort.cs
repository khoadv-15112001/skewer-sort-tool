using Cysharp.Threading.Tasks;
using Gameplay;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "GameFactorySO_GrillSort", menuName = "MyGame/GrillSort/GameFactorySO_GrillSort")]
    public class GameFactorySO_GrillSort : GameFactorySO
    {
        public override async UniTask<T> CreateItem<T>(string name, Transform parent = null)
        {
            var item = GameFactory.CreateEntity<T>(name, parent);
            return item;
        }

        public override async UniTask<T> CreateItem<T>(string name, Vector3 position, Transform parent = null)
        {
            var item = GameFactory.CreateEntity<T>(name, position, parent);
            return item;
        }

        public override void ReturnEntity<T>(T subGrill)
        {
            GameFactory.ReturnEntity(subGrill);
        }
    }
}