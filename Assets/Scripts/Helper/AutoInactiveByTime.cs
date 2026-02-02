using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoInactiveByTime : MonoBehaviour
{
    [SerializeField] private float timeInSeconds;
    private Coroutine coroutine;
    
    private void OnEnable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(DelayToInActive());
    }

    private void OnDisable()
    {
        coroutine = null;
    }

    IEnumerator DelayToInActive()
    {
        yield return new WaitForSeconds(timeInSeconds);
        gameObject.SetActive(false);
    }
}
