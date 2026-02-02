using Gameplay.Entities.Grills;

namespace Gameplay.Entities.Items
{
    public class ItemKey2 : Item
    {
        public override void OnComplete()
        {
            base.OnComplete();
            PrimaryGrillLockAndKey2.grillLock.OnCollectItemKey(this);
        }
    }
}