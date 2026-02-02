using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using Sonat.Enums;
using UnityEngine;

namespace Tool
{
    public class ToolItemSelectorPanel : SingletonSimple<ToolItemSelectorPanel>
    {
        [SerializeField] private Transform container;
        [SerializeField] private Transform selectedContainer;
        //private UIToolItem selectedItem;
        [SerializeField] private GameObject main;
        private bool initialized = false;
        private Action<int> onSelectItem;
        private Action<List<int>> onSelectItems;
        private bool selectMulti;
        private List<int> selectedItems = new List<int>();
        private List<UIToolItemSelector> allItemSelector = new List<UIToolItemSelector>();
        private List<UIToolItemSelector> itemsSelected = new List<UIToolItemSelector>();
        [SerializeField] private GameObject multiSelectObjects;

        public void Open(Action<int> callback)
        {
            onSelectItem = callback;
            main.SetActive(true);
            CreateAllItems();
            ToolManager.selectAvailable = false;
            selectMulti = false;
            multiSelectObjects.SetActive(false);
        }
        
        public void Open(List<int> selected, Action<List<int>> callback)
        {
            onSelectItems = callback;
            main.SetActive(true);
            CreateAllItems();
            ToolManager.selectAvailable = false;
            selectMulti = true;
            multiSelectObjects.SetActive(true);
            selectedItems = new List<int>(selected);
            SetPreviousSelection();
        }

        public void Close()
        {
            onSelectItem = null;
            onSelectItems = null;
            main.SetActive(false);
            ToolManager.selectAvailable = true;
        }

        private void Start()
        {
            main.SetActive(false);
        }

        private void CreateAllItems()
        {
            // if (initialized) return;
            MySonatFramework.poolingContainer.CleanContainer(container);

            var startItem = ItemId.Item_1;
            var endItem = ItemId.FINISH_NORMAL - 1;
            var levelType = UIToolPanel.Instance.GetLevelType();
            
            switch (levelType)
            {
                case LevelType.Food:
                    startItem = ItemId.Item_1;
                    endItem = ItemId.Item_1030;
                    break;
                case LevelType.Fruit:
                    startItem = ItemId.Item_Fruit_101;
                    endItem = ItemId.Item_Fruit_134_1;
                    break;
                case LevelType.Cake:
                    startItem = ItemId.Item_Cake_135;
                    endItem = ItemId.Item_Cake_177_2;
                    break;
            }
            
            allItemSelector.Clear();
            for (int i = (int)startItem; i <= (int)endItem; i++)
            {
                if (!Enum.IsDefined(typeof(ItemId), i)) continue;
                var item = MySonatFramework.poolingContainer.CreateObject<UIToolItemSelector>(container);
                item.Setup(i);
                allItemSelector.Add(item);
            }
            initialized = true;
        }

        private void SetPreviousSelection()
        {
            MySonatFramework.poolingContainer.CleanContainer(selectedContainer);
            itemsSelected.Clear();
            foreach (var itemSelector in allItemSelector)
            {
                if (selectedItems.Contains(itemSelector.Id))
                {
                    itemSelector.SetSelected(true);
                    var item = MySonatFramework.poolingContainer.CreateObject<UIToolItemSelector>(selectedContainer);
                    item.Setup(itemSelector.Id);
                    item.SetSelected(true);
                    itemsSelected.Add(item);
                }
            }
        }

        public void OnSelectItem(int id)
        {
            if (selectMulti)
            {
                if (selectedItems.Contains(id))
                {
                    selectedItems.Remove(id);
                    foreach (var itemSelector in itemsSelected.Where(itemSelector => itemSelector.Id == id))
                    {
                        itemSelector.gameObject.SetActive(false);
                        itemsSelected.Remove(itemSelector);
                        break;
                    }
                }
                else
                {
                    selectedItems.Add(id);
                    var item = MySonatFramework.poolingContainer.CreateObject<UIToolItemSelector>(selectedContainer);
                    item.Setup(id);
                    item.SetSelected(true);
                    itemsSelected.Add(item);
                }
            }
            else
            {
                onSelectItem?.Invoke(id);
                Close();
            }
        }

        public void SaveAndClose()
        {
            onSelectItems?.Invoke(selectedItems);
            Close();
        }
    }
}