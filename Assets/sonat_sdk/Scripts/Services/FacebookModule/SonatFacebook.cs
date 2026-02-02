using System;
using System.Collections;
using System.Collections.Generic;
#if using_facebook
using Facebook.Unity;
#endif
using Sonat.Debugger;
using UnityEngine;
#if using_facebook
#endif

namespace Sonat.FacebookModule
{
    [CreateAssetMenu(menuName = "SonatSDK/Services/Facebook Service", fileName = nameof(SonatFacebook))]
    public class SonatFacebook : SonatService
    {
        public override SonatServiceType ServiceType => SonatServiceType.FacebookService;
        public override bool Ready { get; set; }

        [SerializeField] private bool login;

        // Include Facebook namespace
        public Action<string> loggedReadyCallback;
        public Action<string> onLoggingCallback;
        public Action loggedOutCallback;

        public override void Initialize(Action<ISonatService> onInitialized)
        {
            base.Initialize(onInitialized);
#if using_facebook
            if (!FB.IsInitialized)
            {
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                FB.ActivateApp();
                Ready = true;
                OnInitialized?.Invoke(this);
                if (login)
                    CheckPlayFab();
            }
#endif
        }


        public void CheckPlayFab()
        {
#if using_facebook
            if (FB.IsLoggedIn)
                loggedReadyCallback.Invoke(AccessToken.CurrentAccessToken.TokenString);
            else
            if(loggedReadyCallback != null)
                loggedReadyCallback.Invoke(string.Empty);
#endif
        }


        private void InitCallback()
        {
#if using_facebook
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
                Ready = true;
                OnInitialized?.Invoke(this);
                CheckPlayFab();
            }
            else
            {
                if(loggedReadyCallback != null)
                    loggedReadyCallback.Invoke(string.Empty);
                SonatDebugType.Common.LogError("Failed to Initialize the Facebook SDK");
            }
#endif
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        public Action<bool> loggedEvent;

        public void LoginWithFacebook()
        {
            SonatDebugType.Common.Log(nameof(LoginWithFacebook));
            var perms = new List<string>() { "public_profile" };
#if using_facebook
            FB.LogInWithReadPermissions(perms, AuthCallback);
#endif
        }

        public void LogOut()
        {
            SonatDebugType.Common.Log(nameof(LogOut));
#if using_facebook
            if (FB.IsLoggedIn)
            {
                FB.LogOut();
                StopCoroutine("CheckForSuccussfulLogout");
                StartCoroutine("CheckForSuccussfulLogout");
            }
#endif
        }

#if using_facebook
        IEnumerator CheckForSuccussfulLogout()
        {
        
            while (FB.IsLoggedIn)
            {
                yield return new WaitForSeconds(0.1f);
            }

            loggedEvent?.Invoke(false);
            loggedOutCallback.Invoke();
        }
#endif

#if using_facebook
        private ILoginResult _result;
#endif

#if using_facebook
        private void AuthCallback(ILoginResult result)
        {
            if (FB.IsLoggedIn)
            {
                loggedEvent?.Invoke(true);
                _result = result;
                // AccessToken class will have session details
                var aToken = AccessToken.CurrentAccessToken;
                // Print current access token's User ID
                SonatDebugType.Common.Log(aToken.UserId);

                SonatDebugType.Common.Log("Facebook Auth Complete! Access Token: " + AccessToken.CurrentAccessToken.TokenString +
                                          "\nLogging into PlayFab...");

                onLoggingCallback.Invoke(AccessToken.CurrentAccessToken.TokenString);
            }
            else
            {
                SonatDebugType.Common.Log("User cancelled login");
            }
        }
#endif
    }
}