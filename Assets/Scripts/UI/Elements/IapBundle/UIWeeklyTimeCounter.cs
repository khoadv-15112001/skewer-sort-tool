using System;
using System.Collections;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIWeeklyTimeCounter : MonoBehaviour
{
    [SerializeField] private DayOfWeek endDayOfWeek;
    [SerializeField] private TMP_Text textTimeCounter;
    [SerializeField] private UnityEvent onFinish;
    private readonly Service<TimeService> timeService = new Service<TimeService>();

    private Coroutine countdownCoroutine;

    private void OnEnable()
    {
        StartCountTime();
    }

    private void OnDisable()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }

    private void StartCountTime()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        while (true)
        {
            TimeSpan remainingTime = GetTimeUntilEndDayOfWeek();

            if (remainingTime.TotalSeconds <= 0)
            {
                // Countdown finished
                if (textTimeCounter != null)
                {
                    textTimeCounter.text = "00:00:00";
                }
                onFinish?.Invoke();
                yield break;
            }

            // Update UI text
            if (textTimeCounter != null)
            {
                textTimeCounter.text = SonatUtils.GetTimeByFormat((long)remainingTime.TotalSeconds, TxtTimeFormat.SmartFull);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private TimeSpan GetTimeUntilEndDayOfWeek()
    {
        DateTime now = timeService.Instance.GetCurrentTime();
        DateTime endTime = GetNextOccurrenceOfDayOfWeek(endDayOfWeek);

        return endTime - now;
    }

    private DateTime GetNextOccurrenceOfDayOfWeek(DayOfWeek targetDay)
    {
        DateTime now = timeService.Instance.GetCurrentTime();
        int daysUntilTarget = ((int)targetDay - (int)now.DayOfWeek + 7) % 7;

        // // If today is the target day, count to end of current day (23:59:59)
        // if (daysUntilTarget == 0)
        // {
        //     return now.Date.AddDays(1).AddSeconds(-1); // End of current day
        // }

        return now.Date.AddDays(daysUntilTarget);
    }
}
