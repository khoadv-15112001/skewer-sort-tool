using System.Linq;
using Cysharp.Threading.Tasks;
using MyGame.SkewerJam.Objects.Entities;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class ItemManager : MonoBehaviour
    {
        private int totalItems = 0;
        private int currentItems = 0;

        public int TotalItems => totalItems;
        public int CurrentItems => currentItems;


        public void OnStartCollectItem(OrderEntity orderEntity)
        {
            currentItems -= orderEntity.MaxItems;
            Debug.Log($"OnStartCollectItem: {currentItems}");
        }

        public async UniTask Init()
        {
            // await UniTask.WaitUntil(() => GameController.Instance.LevelGenerator.LevelData != null);

            var levelData = GameController.Instance.LevelGenerator.LevelData;
            totalItems = 0;
            currentItems = 0;
            foreach (var grillData in levelData.grillData)
            {
                if (grillData.layer != null)
                {
                    foreach (var layerData in grillData.layer)
                    {
                        if (layerData.itemData != null)
                        {
                            foreach (var itemData in layerData.itemData)
                            {
                                if (itemData != null && itemData.id > 0)
                                {
                                    totalItems++;
                                }
                            }
                        }
                    }
                }
            }

            currentItems = totalItems;
            Debug.Log($"Init: {currentItems}");
            GameController.Instance.GameLogicHandler.OnStartCollectItem += OnStartCollectItem;
        }

        public void Clear()
        {
            GameController.Instance.GameLogicHandler.OnStartCollectItem -= OnStartCollectItem;
            totalItems = 0;
            currentItems = 0;
        }
    }
}