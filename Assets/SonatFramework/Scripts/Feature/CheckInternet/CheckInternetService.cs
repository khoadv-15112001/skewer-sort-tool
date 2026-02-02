using System;
using System.Collections;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.CheckInternet
{
    [CreateAssetMenu(fileName = nameof(CheckInternetService), menuName = "Sonat Services/CheckInternet Service")]
    public class CheckInternetService : SonatServiceSo, IServiceWaitingSDKInitialize
    {
        [SerializeField] private float checkInterval = 30;
        [SerializeField] private bool forceInternet = false;
        [SerializeField] private string popupNoInternetName = "PopupNoInternet";
        private Coroutine checkInternetCoroutine;

        public event Action<bool> onInternetConnectionStatusChanged;
        private bool lastInternetConnectionStatus = true;
        private PopupNoInternetBase popupNoInternet;

        public void OnSonatSDKInitialize()
        {
            checkInterval = SonatSDKAdapter.GetRemoteFloat("check_internet_time_gap", checkInterval);
            forceInternet = SonatSDKAdapter.GetRemoteBool("internet_connection", forceInternet);

            checkInternetCoroutine = SonatSystem.Instance.StartCoroutine(IECheckInternetConnection());
        }

        public bool IsInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private void OnInternetConnectionStatusChanged(bool status)
        {
            lastInternetConnectionStatus = status;
            onInternetConnectionStatusChanged?.Invoke(status);

            if (!status)
            {
                if (forceInternet && popupNoInternet == null)
                {
                    popupNoInternet = PanelManager.Instance.OpenPanelByName<PopupNoInternetBase>(popupNoInternetName);
                }
            }
            else
            {
                if (popupNoInternet != null)
                {
                    popupNoInternet.Close();
                    popupNoInternet = null;
                }
            }
        }


        public void RetryCheckInternetConnection()
        {
            if (forceInternet && lastInternetConnectionStatus == false) return;
            if (popupNoInternet == null) return;
            popupNoInternet.Close();
            popupNoInternet = null;
        }

        private IEnumerator IECheckInternetConnection()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);
                if (lastInternetConnectionStatus != IsInternetConnection())
                {
                    OnInternetConnectionStatusChanged(IsInternetConnection());
                }
            }
        }
    }
}