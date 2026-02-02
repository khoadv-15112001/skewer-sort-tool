using System;
using System.Collections;
using DG.Tweening;
using Manager;
using MyFramework.Sonat;
using Sonat;
using Sonat.AdsModule;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.SceneManagement;
using SonatFramework.Systems.UserData;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.SceneManager
{
    public class LoadingScreen : MonoBehaviour
    {
        private bool sonatSdkInited = false;
        //private float loadingTime = 2;

        private void Start()
        {

            // if (PlayerPrefs.HasKey("LoadingFirstTime"))
            // {
            //     loadingTime = 1;
            // }
            // else
            // {
            //     loadingTime = 0.5f;
            //     PlayerPrefs.SetInt("LoadingFirstTime", 1);
            // }

            //slider.DOValue(1, loadingTime).OnComplete(() => { StartCoroutine(IELoading()); });
            IntroLoadingPanel.instance.DOSlider(0.98f, 0.5f);
            StartCoroutine(IELoading(0.5f));

            // SonatSdkManager.Initialize(OnSonatSdkInited);
            OnSonatSdkInited();
        }

        private void OnSonatSdkInited()
        {
            GameSetup.Setup();
            sonatSdkInited = true;
            SonatUtils.ExecuteNextFrame(TrackingByHour.Setup, 2); 
        }

        IEnumerator IELoading(float seconds = 0.5f)
        {
            yield return new WaitForSeconds(seconds);
            yield return new WaitUntil(() => sonatSdkInited);
            yield return new WaitForSeconds(0.1f);
            // logoAnim.AnimationState.ClearTracks();
            // logoAnim.Initialize(true);
            // logoAnim.AnimationState.SetAnimation(0, "End", false);
            yield return new WaitForSeconds(0.43f);
            SonatAds.needShowAppOpenAds = false;
            int level = MySonatFramework.GetService<UserDataService>().GetLevel();
            if (level >= GameRemoteConfigValue.levelForceHome)
            {
                MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home, false, OnLoadingComplete);
            }
            else
            {
                MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Gameplay, false, OnLoadingComplete);
            }
        }

        private void OnLoadingComplete()
        {
            IntroLoadingPanel.instance.FadeOut();
        }
    }
}