using System;
using Cysharp.Threading.Tasks;
using Gameplay.LevelData;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Tool
{
    public class ToolItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TMP_Text text, txtLink;
        [SerializeField] private GameObject hiddenObject;
        [SerializeField] private GameObject keyObject;
        [SerializeField] private GameObject iceObject;
        [SerializeField] private GameObject bombObject;
        [SerializeField] private GameObject keyObject2;
        [SerializeField] private GameObject keyAreaObject;
        [SerializeField] private GameObject wrappedItem;
        private ItemData itemData;

        public void SetItemData(ItemData data)
        {
            if (data == null || data.id == 0)
            {
                spriteRenderer.gameObject.SetActive(false);
                SetActiveObject(null);
            }
            else
            {
                this.itemData = data;
                spriteRenderer.gameObject.SetActive(true);

                SetVisual();
            }
        }

        public void SetVisual()
        {
            text.text = itemData.id.ToString();
            txtLink.text = itemData.isLink ? $"link" : "";
            _ = spriteRenderer.SetSpriteAsync(PathManager.ItemSprite(itemData.id));
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
                    SetActiveObject(wrappedItem);
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
            keyAreaObject?.SetActive(obj == keyAreaObject);
            wrappedItem.SetActive(obj == wrappedItem);
        }
    }
}