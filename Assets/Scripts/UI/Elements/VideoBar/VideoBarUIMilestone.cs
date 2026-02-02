using GrillSort.Services;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoBarUIMilestone : MonoBehaviour
{
    [SerializeField] private GameObject tickObj;
    [SerializeField] private GameObject rewardObj;
    [SerializeField] private TMP_Text txtWatchCount;
    [SerializeField] private TMP_Text txtRewardValue;

    public void SetupUI(VideoBarRewardData videoBarRewardData)
    {
        txtWatchCount.text = $"{videoBarRewardData.watchCount}";

        if (videoBarRewardData.rewards.resourceDatas.Count == 1)
            txtRewardValue.text = $"x{videoBarRewardData.rewards.resourceDatas[0].quantity}";
        else
            txtRewardValue.text = "x1";
    }

    public void Completed(bool value)
    {
        rewardObj?.SetActive(!value);
        tickObj.SetActive(value);
    }
}
