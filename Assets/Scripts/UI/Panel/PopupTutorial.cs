using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Entities;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PopupTutorial : Panel
{
    [SerializeField] private Transform hand;
    private LevelGenerator levelGenerator;
    private List<PrimaryGrill> primaryGrills;
    private Vector3 startPosition;
    private Vector3 endPosition;
    

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        levelGenerator = GameplayController.instance.levelGenerator;
        primaryGrills = levelGenerator.GetPrimaryGrills();
        hand.gameObject.SetActive(false);
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        SonatUtils.DelayCall(1f, StartTutorial, this);
        GameplayController.OnCollectItem += OnCollectItem;
        GameplayController.OnSelectItem += OnSelectItem;
        GameplayController.OnLevelEnd += OnLevelEnd;
    }

    private void OnLevelEnd(bool isWin)
    {
        Close();
    }

    public override void Close()
    {
        base.Close();
        GameplayController.OnCollectItem -= OnCollectItem;
        GameplayController.OnSelectItem -= OnSelectItem;
        GameplayController.OnLevelEnd -= OnLevelEnd;
    }

    private void OnSelectItem(Item item)
    {
        hand.gameObject.SetActive(false);
    }

    private void OnCollectItem(int id)
    {
        hand.gameObject.SetActive(false);
        SonatUtils.DelayCall(1f, StartTutorial, this);
    }

    private void StartTutorial()
    {
        Item item = GetSingleItem();
        if (item != null)
        {
            PrimarySlot slot = GetPrimarySlot(item.id);
            if (slot != null)
            {
                hand.gameObject.SetActive(true);
                //hand.transform.position = item.transform.position;
                hand.DOMove(slot.transform.position, 1.35f).From(item.transform.position).SetLoops(-1, LoopType.Restart).SetEase(Ease.InOutSine);
            }
        }
    }

    private Item GetSingleItem()
    {
        foreach (var primaryGrill in primaryGrills)
        {
            var slots = primaryGrill.GetSlots();
            int count = 0;
            List<Item> items = new List<Item>();
            Item item = null;
            byte sameId = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                var _item = slots[i].GetItem();
                if (_item != null && _item.gameObject.activeInHierarchy && _item.id > 0)
                {
                    int index = items.FindIndex(e => e.id == _item.id);
                    if (index >= 0)
                    {
                        items.RemoveAt(index);
                    }
                    else
                    {
                        items.Add(_item);
                    }
                }
            }

            if (items.Count > 0) return items[0];
        }

        return null;
    }

    private PrimarySlot GetPrimarySlot(int id)
    {
        foreach (var primaryGrill in primaryGrills)
        {
            var slots = primaryGrill.GetSlots();
            int count = 0;
            PrimarySlot slot = null;
            for (int i = 0; i < slots.Length; i++)
            {
                var _item = slots[i].GetItem();
                if (_item != null && _item.id == id)
                {
                    count++;
                }
                else
                {
                    slot = slots[i] as PrimarySlot;
                }
            }

            if (count == 2) return slot;
        }

        return null;
    }
}