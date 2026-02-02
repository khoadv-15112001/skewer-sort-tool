using GrillSort.LavaQuest;
using GrillSort.Pinata;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPinataGroupStageLock : MonoBehaviour
{
    [SerializeField] private Button btn;
    [SerializeField] private TMP_Text timeTxt;

    private void Start()
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            PopupToast.Cretate("Available in value", $"{PinataService.Instance.GetRemainTime()}");
        });
    }

    private void OnEnable()
    {
        Tick();
        PinataService.OnTick += Tick;
    }

    private void OnDisable()
    {
        PinataService.OnTick -= Tick;
    }

    private void Tick()
    {
        timeTxt.text = PinataService.Instance.GetRemainTime();
    }
}
