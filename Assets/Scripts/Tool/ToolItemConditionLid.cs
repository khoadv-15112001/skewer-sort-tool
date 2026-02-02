using System;
using Gameplay.LevelData;
using Manager;
using UnityEngine;

namespace Tool
{
    public class ToolItemConditionLid: MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private GrillLidData data;
        private Sprite defaultSprite;

        private void Awake()
        {
            defaultSprite = spriteRenderer.sprite;
        }

        public void SetData(GrillLidData data)
        {
            this.data = data;
            UpdateItem();
        }

        private void UpdateItem()
        {
            if (data.itemCondition == 0)
            {
                spriteRenderer.sprite = defaultSprite;
            }
            else
            {
                spriteRenderer.SetSpriteAsync(PathManager.ItemSprite(data.itemCondition));
            }
        }

        private void SetItem(int id)
        {
            data.itemCondition = id;
            UpdateItem();
        }

        private void OnMouseUpAsButton()
        {
            ToolItemSelectorPanel.Instance.Open(SetItem);
        }
    }
}