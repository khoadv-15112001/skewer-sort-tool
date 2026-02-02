namespace Gameplay.Entities
{
    public class PrimarySlot : SlotBase
    {
        private PrimaryGrill primaryGrill;

        public Item Item => item;
        public PrimaryGrill PrimaryGrill => primaryGrill;
        
        private void Awake()
        {
            primaryGrill = grill as PrimaryGrill;
        }

        #region Interactive

        protected void OnMouseDown()
        {
            slotBaseBehaviorSO.OnMouseDown(this);
        }

        #endregion

        public override void AddItem(Item item)
        {
            base.AddItem(item);
            primaryGrill.CheckCompleteLogic();
        }
    }
}