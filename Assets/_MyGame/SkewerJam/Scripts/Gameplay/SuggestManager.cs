using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Manager;
using Sonat.Enums;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class SuggestManager : MonoBehaviour
    {
        [SerializeField] private float waitSuggestTime = 10f;

        private Item suggestItem;

        public void Init()
        {
            // StartCoroutine(StartWaitSuggests());

            // var gameLogicHandler = GameController.Instance.GameLogicHandler;
            // gameLogicHandler.OnItemMoveSlot += OnItemMoveSlot;
        }

        // private void OnItemMoveSlot(Item item, SlotBase slot)
        // {
        //     ClearSuggestItem();
        // }

        // public IEnumerator StartWaitSuggests()
        // {
        //     yield return new WaitForSeconds(waitSuggestTime);
        //     yield return new WaitUntil(() => GameController.Instance.GameState == GameState.Playing && suggestItem == null);
        //     var item = GetSuggestItem();
        //     if (item != null)
        //     {
        //         suggestItem = item;
        //         item.SetSuggest(true);
        //     }
        // }

        // public Item GetSuggestItem()
        // {
        //     var gameLogicHandler = GameController.Instance.GameLogicHandler;

        //     var orderItemsDict = gameLogicHandler.OrderManager.GetOrderItemsDict();
        //     var listItemsInGrillManager = gameLogicHandler.GrillManager.GetItemsWithLayer(1);

        //     foreach (var (itemId, (maxItems, num)) in orderItemsDict)
        //     {
        //         var item = listItemsInGrillManager.Find(e => (ItemId)e.id == itemId);
        //         if (item != null)
        //         {
        //             return item;
        //         }
        //     }
        //     return null;
        // }

        // public void ClearSuggestItem()
        // {
        //     Clear();
        //     StartCoroutine(StartWaitSuggests());
        // }

        public void Clear()
        {
            // StopAllCoroutines();
            // if (suggestItem != null)
            // {
            //     suggestItem.SetSuggest(false);
            //     suggestItem = null;
            // }
        }
    }
}