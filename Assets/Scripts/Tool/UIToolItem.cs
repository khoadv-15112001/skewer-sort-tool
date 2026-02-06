using Gameplay.LevelData;
using Manager;
using SonatFramework.Scripts.UIModule.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tool
{
    public class UIToolItem : MonoBehaviour
    {
        [SerializeField] private FixedImageRatio icon;
        [SerializeField] private TMP_Text text, txtLink;
        private ItemData itemData;
        private UIToolLayer uiToolLayer;
        private int index;
        [SerializeField] private GameObject hiddenObject;
        [SerializeField] private GameObject keyObject;
        [SerializeField] private GameObject wrappedObject;
        [SerializeField] private GameObject iceObject;
        [SerializeField] private GameObject bombObject;
        [SerializeField] private GameObject keyObject2;
        [SerializeField] private GameObject keyAreaObject;
        [SerializeField] private TMP_InputField txtBombCount;

        public void Setup(int index, ItemData itemData, UIToolLayer uiToolLayer)
        {
            this.index = index;
            this.itemData = itemData;
            this.uiToolLayer = uiToolLayer;
            UpdateVisual();
        }

        public void Clear()
        {
            // Store previous data for undo
            if (itemData != null)
            {
                var previousData = new ItemData();
                previousData.id = itemData.id;
                previousData.itemType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.Delete, previousData);
            }

            this.itemData = null;
            UpdateVisual();
            uiToolLayer.OnItemDataUpdated(index, itemData);
        }

        public void SetItem(int id)
        {
            // Store previous data for undo
            ItemData previousData = null;
            if (itemData != null)
            {
                previousData = new ItemData();
                previousData.id = itemData.id;
                previousData.itemType = itemData.itemType;

                // Add undo for item switch
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.Switch, itemData.id);
            }
            else
            {
                itemData = new ItemData();
                // Add undo for item add
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.Add);
            }

            itemData.id = id;
            if (id == 1500)
            {
                itemData.itemType = ItemType.Obstacle;
            }
            UpdateVisual();
            uiToolLayer.OnItemDataUpdated(index, itemData);
        }

        private void UpdateVisual()
        {
            if (itemData != null && itemData.id > 0)
            {
                icon.gameObject.SetActive(true);
                if (itemData.id == 1500)
                {
                    Debug.LogError("(UIToolItem) ItemData is 1500");
                }
                _ = icon.SetSpriteAsync(PathManager.ItemSprite(itemData.id));
                SetVisual();
            }
            else
            {
                icon.sprite = null;
                icon.gameObject.SetActive(false);
                SetActiveObject(null);
            }
        }

        public ItemData GetItemData()
        {
            return itemData;
        }

        public void OnSelectThis()
        {
            if (Input.GetKey(KeyCode.H) && itemData is not { itemType: ItemType.Hidden })
            {
                var previousType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemData()
                {
                    id = itemData.id,
                    itemType = ItemType.Hidden
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }

            if (Input.GetKey(KeyCode.K) && itemData is not { itemType: ItemType.Key })
            {
                var previousType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemData()
                {
                    id = itemData.id,
                    itemType = ItemType.Key
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }

            if (Input.GetKey(KeyCode.P) && itemData is not { itemType: ItemType.Key2 })
            {
                var previousType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemData()
                {
                    id = itemData.id,
                    itemType = ItemType.Key2
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }

            if (Input.GetKey(KeyCode.I) && itemData is not { itemType: ItemType.Ice })
            {
                var previousType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemData()
                {
                    id = itemData.id,
                    itemType = ItemType.Ice
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }
            if (Input.GetKey(KeyCode.L) && itemData is not { isLink: true })
            {
                var previousType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemData()
                {
                    id = itemData.id,
                    itemType = itemData.itemType,
                    isLink = true
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }
            if (Input.GetKey(KeyCode.B) && itemData is not { itemType: ItemType.Bomb })
            {
                var previousType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemBombData()
                {
                    id = itemData.id,
                    itemType = ItemType.Bomb,
                    hidden = false,
                    moveLimit = 10
                };
                txtBombCount.text = "10";
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }

            if (Input.GetKey(KeyCode.A) && itemData is not { itemType: ItemType.KeyArea })
            {
                var previousType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemKeyAreaData()
                {
                    id = itemData.id,
                    itemType = ItemType.KeyArea
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }

            if (Input.GetKey(KeyCode.X))
            {
                var previousType = itemData.itemType;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);

                itemData = new ItemData()
                {
                    id = itemData.id,
                    itemType = ItemType.Normal
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }

            if (Input.GetKey(KeyCode.O))
            {
                ItemType previousType = itemData != null ? itemData.itemType : ItemType.Normal;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemData()
                {
                    id = 1500,
                    itemType = ItemType.Obstacle
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                SetItem(1500);
                return;
            }

            if (Input.GetKey(KeyCode.W))
            {
                ItemType previousType = itemData != null ? itemData.itemType : ItemType.Normal;
                ToolManager.Instance.undoController.AddItemOperation(itemData, ItemOperationType.ChangeType, previousType);
                itemData = new ItemData()
                {
                    id = itemData.id,
                    itemType = ItemType.Wrapped
                };
                SetVisual();
                uiToolLayer.OnItemDataUpdated(index, itemData);
                return;
            }

            ToolItemSelectorPanel.Instance.Open(SetItem);
        }

        public void SetVisual()
        {
            text.text = itemData.id.ToString();
            txtLink.text = itemData.isLink ? $"link" : "";
            switch (itemData.itemType)
            {
                case ItemType.Normal:
                    SetActiveObject(null);
                    break;
                case ItemType.Hidden:
                    SetActiveObject(hiddenObject);
                    break;
                case ItemType.Key:
                    SetActiveObject(keyObject);
                    break;
                case ItemType.Ice:
                    SetActiveObject(iceObject);
                    break;
                case ItemType.Bomb:
                    SetActiveObject(bombObject);
                    txtBombCount.onSubmit.RemoveAllListeners();
                    txtBombCount.text = (itemData as ItemBombData)?.moveLimit.ToString();
                    txtBombCount.onSubmit.AddListener(OnBombCountChange);
                    break;
                case ItemType.Key2:
                    SetActiveObject(keyObject2);
                    break;
                case ItemType.KeyArea:
                    SetActiveObject(keyAreaObject);
                    break;
                case ItemType.Obstacle:
                    SetActiveObject(null);
                    break;
                case ItemType.Wrapped:
                    SetActiveObject(wrappedObject);
                    break;
            }
        }

        private void SetActiveObject(GameObject obj)
        {
            hiddenObject.SetActive(obj == hiddenObject);
            keyObject.SetActive(obj == keyObject);
            iceObject.SetActive(obj == iceObject);
            bombObject.SetActive(obj == bombObject);
            keyObject2.SetActive(obj == keyObject2);
            keyAreaObject.SetActive(obj == keyAreaObject);
            wrappedObject.SetActive(obj == wrappedObject);
        }

        private void OnBombCountChange(string value)
        {
            if (int.TryParse(value, out int result))
            {
                if (itemData is ItemBombData itemBombData)
                {
                    itemBombData.moveLimit = result;
                }
                else
                {
                    itemData = new ItemBombData()
                    {
                        id = itemData.id,
                        itemType = ItemType.Bomb,
                        hidden = false,
                        moveLimit = result
                    };
                    uiToolLayer.OnItemDataUpdated(index, itemData);
                }
            }
        }
    }
}
