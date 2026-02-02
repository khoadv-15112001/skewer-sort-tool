using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using TMPro;
using UnityEngine;

public class UITime : MonoBehaviour
{
    [SerializeField] private TimeType timeType = TimeType.Dayly;
    [SerializeField] private TMP_Text txtTime;

    private Coroutine countTimeCoroutine;

    public void OnEnable()
    {
        if (timeType == TimeType.Dayly)
        {
            SetTime(GetRemainingTimeInDay());
        }
    }

    public void OnDisable()
    {
        if (countTimeCoroutine != null)
        {
            StopCoroutine(countTimeCoroutine);
        }
    }

    public void SetTime(int timeRemaining)
    {
        StartCountTime(timeRemaining);
    }

    private void StartCountTime(int timeRemaining)
    {
        if (countTimeCoroutine != null)
        {
            StopCoroutine(countTimeCoroutine);
        }

        countTimeCoroutine = StartCoroutine(CountTime(timeRemaining));
    }

    private IEnumerator CountTime(int timeRemaining)
    {
        txtTime.text = FormatRewardSO.FormatTime(timeRemaining);
        yield return new WaitForSeconds(1);
        while (timeRemaining > 0)
        {
            timeRemaining--;
            txtTime.text = FormatRewardSO.FormatTime(timeRemaining);
            yield return new WaitForSeconds(1);
        }
        txtTime.text = FormatRewardSO.FormatTime(0);
        countTimeCoroutine = null;
    }

    public static int GetRemainingTimeInDay()
    {
        var timeService = MySonatFramework.GetService<TimeService>();
        var currentTime = timeService.GetCurrentTime();
        var nextResetTime = currentTime.Date.AddDays(1);
        return Mathf.FloorToInt((float)(nextResetTime - currentTime).TotalSeconds);
    }

    public enum TimeType
    {
        Custom,
        Dayly
    }
}
