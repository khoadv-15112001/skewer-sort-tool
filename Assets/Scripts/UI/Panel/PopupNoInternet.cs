using System.Collections;
using Sonat;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupNoInternet : Panel
{
    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        StartCoroutine(CheckInternetConnection());
    }

//     private void OnApplicationFocus(bool focus)
//     {
//         if (focus)
//         {
//             if (gameObject.activeInHierarchy && SonatSdkManager.IsInternetConnection())
//             {
//                 NoInternet.Instance.ClosePanel();
//             }
//         }
//     }
//
// #if UNITY_ANDROID
//     private void OnApplicationPause(bool pause)
//     {
//         if (!pause)
//         {
//             if (gameObject.activeInHierarchy && SonatSdkManager.IsInternetConnection())
//             {
//                 NoInternet.Instance.ClosePanel();
//             }
//         }
//     }
// #endif

    IEnumerator CheckInternetConnection()
    {
        while (gameObject.activeInHierarchy)
        {
            if (SonatSdkManager.IsInternetConnection())
            {
                NoInternet.Instance.ClosePanel();
            }
            yield return null;
        }
    }

    public void ClosePanel()
    {
        NoInternet.Instance.ClosePanel();
    }
}
