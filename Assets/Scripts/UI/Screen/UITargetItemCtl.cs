using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using UnityEngine;

public class UITargetItemCtl : MonoBehaviour
{
    public UIItemGoal itemGoalPrefab;

    public Transform itemGoalContainer;

    private Dictionary<int, UIItemGoal> itemGoalDict = new Dictionary<int, UIItemGoal>();

    public Dictionary<int, UIItemGoal> ItemGoalDict => itemGoalDict;

    private void Awake()
    {
        GameplayController.OnLoadLevel += OnLevelLoaded;
        GameplayController.OnTargetUpdate += OnTargetUpdate;
    }

    private void OnDestroy()
    {
        GameplayController.OnLoadLevel -= OnLevelLoaded;
        GameplayController.OnTargetUpdate -= OnTargetUpdate;
    }

    private void OnLevelLoaded(int level)
    {
        SetData(GameplayController.instance.TargetData);
    }

    private void OnTargetUpdate(int itemId, int remainingQuantity)
    {
        if (itemGoalDict.TryGetValue(itemId, out UIItemGoal itemGoal))
        {
            if (remainingQuantity <= 0)
            {
                itemGoal.OnComplete();
            }
            else
            {
                itemGoal.OnUpdateProgress(remainingQuantity);
            }
        }
    }

    public void SetData(List<TargetData> targetData)
    {
        Clear();
        foreach (var item in targetData)
        {
            UIItemGoal itemGoal = Instantiate(itemGoalPrefab, itemGoalContainer);
            itemGoal.SetData(item.id, item.quantity);
            itemGoalDict[item.id] = itemGoal;
        }
    }

    public void Clear()
    {
        foreach (Transform child in itemGoalContainer)
        {
            Destroy(child.gameObject);
        }
        itemGoalDict.Clear();
    }
}
