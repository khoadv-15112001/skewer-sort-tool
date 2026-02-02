#if using_admob
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace Sonat.AdsModule.Admob
{
    public class GDPRScript
    {
        ConsentForm _consentForm;

        public bool consenDone
        {
            get => PlayerPrefs.GetInt("CONSENT", 0) == 1;
            set => PlayerPrefs.SetInt("CONSENT", value ? 1 : 0);
        }

        public bool consenShowed;


        public void InitConsent()
        {
            if (consenShowed || consenDone) return;
            var debugSettings = new ConsentDebugSettings
            {
                // Geography appears as in EEA for debug devices.
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = new List<string>
                {
                    "965E4A26737DF85475A353251709C315"
                }
            };

            // Here false means users are not under age.
            ConsentRequestParameters request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = debugSettings,
            };

            // Check the current consent information status.
            ConsentInformation.Update(request, OnConsentInfoUpdated);
        }

        void OnConsentInfoUpdated(FormError error)
        {
            if (error != null)
            {
                // Handle the error.
                SonatAds.ConsentReady = true;
                UnityEngine.Debug.LogError(error);
                return;
            }

            if (ConsentInformation.IsConsentFormAvailable())
            {
                LoadConsentForm();
            }
            else
            {
                //consenDone = true;
                SonatAds.ConsentReady = true;
            }
            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
        }

        void LoadConsentForm()
        {
            // Loads a consent form.
            ConsentForm.Load(OnLoadConsentForm);
        }

        void OnLoadConsentForm(ConsentForm consentForm, FormError error)
        {
            if (error != null)
            {
                // Handle the error.
                SonatAds.ConsentReady = true;
                UnityEngine.Debug.LogError(error);
                return;
            }

            // The consent form was loaded.
            // Save the consent form for future requests.
            _consentForm = consentForm;

            // You are now ready to show the form.
            if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
            {
                _consentForm.Show(OnShowForm);
            }
            else
            {
                //consenDone = true;
                SonatAds.ConsentReady = true;
            }
        }


        void OnShowForm(FormError error)
        {
            if (error != null)
            {
                // Handle the error.
                SonatAds.ConsentReady = true;
                UnityEngine.Debug.LogError(error);
                return;
            }

            consenDone = true;
            SonatAds.ConsentReady = true;
            // Handle dismissal by reloading form.
            //LoadConsentForm();
        }
    }
}
#endif