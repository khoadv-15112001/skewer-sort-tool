using System;
using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UITimeCountdown : MonoBehaviour
{
    [SerializeField] private TMP_Text textTimeCounter;
    [SerializeField] private UnityEvent onFinish;
    [SerializeField] private long timeFinish;
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
        long timeRemaining = timeFinish - timeService.Instance.GetUnixTimeSeconds();
        while (true)
        {
            if (timeRemaining <= 0)
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
                textTimeCounter.text = SonatUtils.GetTimeByFormat(timeRemaining, TxtTimeFormat.ShortDay_FullTime);
            }

            yield return new WaitForSeconds(1f);
            timeRemaining -= 1;
        }
    }
}