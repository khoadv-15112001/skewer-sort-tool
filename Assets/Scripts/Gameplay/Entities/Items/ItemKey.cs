using System;
using Gameplay.Entities.Grills;
using Gameplay.LevelData;
using SonatFramework.Scripts.Utils;

namespace Gameplay.Entities.Items
{
    public class ItemKey : Item
    {
        // public override void SetItemData(ItemData data, SlotBase slot)
        // {
        //     base.SetItemData(data, slot);
        //     SonatUtils.ExecuteNextFrame(() =>
        //     {
        //         PrimaryGrillLockAndKey.grillLock?.AddKeyInGame();
        //     }, 2);
        //     
        // }

        public override void OnComplete()
        {
            base.OnComplete();
            if (PrimaryGrillLockAndKey.grillLock != null)
                PrimaryGrillLockAndKey.grillLock.OnCollectItemKey(this);
        }
    }
}