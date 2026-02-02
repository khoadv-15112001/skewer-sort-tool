using GrillSort.QuestEvent;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIX2Item : MonoBehaviour
{
    public GameObject txtX2;
    private readonly Service<QuestEventService> questEventService = new();

    private void Start()
    {
        QuestEventService.OnX2Item += UpdateUI;
        UpdateUI();
    }
    private void OnDestroy()
    {
        QuestEventService.OnX2Item -= UpdateUI;
    }
    private void UpdateUI()
    {
        //Debug.Log("anhnt: update x2 " + questEventService.Instance.HasX2Item());
        if (txtX2 != null)
            txtX2.SetActive(questEventService.Instance.HasX2Item());
    }
}
