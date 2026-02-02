namespace Gameplay.Entities.Items
{
    public class ItemHidden: Item
    {
        protected override void OnItemBeginDrag()
        {
            slot.OnItemDrag();
            visual.OnBeginDrag();
        }
    }
}