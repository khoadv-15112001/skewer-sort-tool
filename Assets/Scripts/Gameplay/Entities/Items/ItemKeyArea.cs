using Gameplay.Entities.Grills;
using Gameplay.Entities.Obstacle;
using Gameplay.LevelData;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace Gameplay.Entities.Items
{
    public class ItemKeyArea : Item
    {
        [SerializeField] private Transform keyObj;
        private bool active = false;

        public override void SetItemData(ItemData data, SlotBase slot)
        {
            base.SetItemData(data, slot);
            SonatUtils.ExecuteNextFrame(() => { LockAreaObstacle.instance.SetKey(this); }, 2);
            keyObj.gameObject.SetActive(true);
            active = true;
        }

        public override void OnComplete()
        {
            base.OnComplete();
            if (active && LockAreaObstacle.instance != null)
                LockAreaObstacle.instance.OnCollectItemKey(this);
            active = false;
        }

        public void OnUnlockByBooster()
        {
            keyObj.gameObject.SetActive(false);
            active = false;
        }

        public override ItemData GetCurrentItemData()
        {
            if (active)
            {
                return base.GetCurrentItemData();
            }
            else
            {
                return new ItemData()
                {
                    itemType = ItemType.Normal,
                    id = this.id,
                };
            }
        }
    }
}