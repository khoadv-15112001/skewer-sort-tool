using Gameplay.Entities;
using Gameplay.LevelData;
using UnityEngine;

public class ItemWrapped : Item
{
    [SerializeField] private SpriteRenderer wrappedSprite;
    [SerializeField] private SpriteRenderer itemSprite;

    public override void SetItemData(ItemData data, SlotBase slot)
    {
        base.SetItemData(data, slot);
        
        // If item is on primary grill, unwrap it immediately
        if (isPrimary)
        {
            UnwrappedItem();
        }
        else
        {
            wrappedSprite.gameObject.SetActive(true);
            itemSprite.gameObject.SetActive(false);
        }
    }

    public override void MoveToPrimary(SlotBase slot, int index)
    {
        UnwrappedItem();
        base.MoveToPrimary(slot, index);
    }

    public void UnwrappedItem()
    {
        wrappedSprite.gameObject.SetActive(false);
        itemSprite.gameObject.SetActive(true);
    }
}
