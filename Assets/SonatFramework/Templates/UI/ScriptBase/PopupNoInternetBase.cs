using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.Feature.CheckInternet;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.NetworkManagement;
using UnityEngine;

public class PopupNoInternetBase : Panel
{
    [SerializeField] private bool autoCheckRetry = true;

    [ShowIf("@autoCheckRetry == true")] [SerializeField]
    private float checkingTimeRate = 0.1f;

    private Service<CheckInternetService> checkInternetService;

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        if (autoCheckRetry)
            StartCoroutine(CheckInternet());
    }

    public virtual void RetryClick()
    {
        checkInternetService.Instance.RetryCheckInternetConnection();
    }

    IEnumerator CheckInternet()
    {
        while (gameObject.activeInHierarchy)
        {
            if (checkInternetService.Instance.IsInternetConnection())
            {
                RetryClick();
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(checkingTimeRate);
            }
        }
    }
}