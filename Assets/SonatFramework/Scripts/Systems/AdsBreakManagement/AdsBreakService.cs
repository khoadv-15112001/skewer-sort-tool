using System.Collections;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.UserData;
using SonatFramework.Templates.UI.ScriptBase;
using UnityEngine;

namespace SonatFramework.Systems.AdsBreakManagement
{
	[CreateAssetMenu(fileName = "AdsBreakService", menuName = "Sonat Services/AdsBreak Service")]
	public class AdsBreakService : SonatServiceSo, IServiceInitialize
	{
		private Coroutine countTime;
		
		private int time;
		private int levelStartAdBreak;
		private int stateStartAdBreak;

		PopupAdsBreakBase _popupAdBreakBase;
		PopupWaitAdsBreakBase popupWaitAdBreak;
		private bool ready = true;
		private bool countDone;
		private MonoBehaviour adsBreakController;
		[SerializeField] [Required] private AdsBreakConfig config;

		public void Initialize()
		{
			adsBreakController = new GameObject("AdsBreakController").AddComponent<DontDestroyOnLoadObject>();
			
			time = SonatSDKAdapter.GetRemoteInt("time_ad_break", config.timeGap);
			levelStartAdBreak = SonatSDKAdapter.GetRemoteInt("level_start_ad_break", config.levelStartAdBreak);

			new EventBinding<LevelStartedEvent>(OnStartLevel);
			new EventBinding<LevelStuckEvent>(OnLevelStuck);
			new EventBinding<LevelEndedEvent>(OnLevelEnd);
			new EventBinding<SwitchPlacementEvent>(OnSwitchPlacement);
		}

		public void OnStartLevel(LevelStartedEvent eventData)
		{
			if(eventData.level >= levelStartAdBreak)
			{
				StartWaitAdBreak();
			}
			else
			{
				StopWaitAdBreak();
			}
		}

		private void OnLevelEnd(LevelEndedEvent eventData)
		{
			StopWaitAdBreak();
		}

		private void OnLevelStuck(LevelStuckEvent eventData)
		{
			StopWaitAdBreak();
		}
    
		public void OnSwitchPlacement(SwitchPlacementEvent eventData)
		{
			if(eventData.to != GamePlacement.Gameplay)
			{
				StopWaitAdBreak();
			}
		}

		public void OnActionToShow()
		{
			if (countDone)
			{
				ready = true;
			}
		}



		public void StartWaitAdBreak()
		{
			if (SonatSDKAdapter.IsNoads()) return;
			StopWaitAdBreak();
			ready = true;
			countDone = false;
			countTime = adsBreakController.StartCoroutine(WaitForAdsBreak());
		}

		public void StopWaitAdBreak()
		{
			if (countTime != null)
			{
				adsBreakController.StopCoroutine(countTime);
				countTime = null;
				if (popupWaitAdBreak)
				{
					popupWaitAdBreak.Close();
					popupWaitAdBreak = null;
				}
			}
		}

		private void OnShowAdsDone()
		{
			_popupAdBreakBase?.Close();
			_popupAdBreakBase = null;
			//GameManager.instance.ClaimAdBreakCoin("ad_break");

			//if (LoadingTransition.Instance != null)
			//LoadingTransition.Instance.showLoadingAds = true;

			StartWaitAdBreak();
		}

		IEnumerator WaitForAdsBreak()
		{
			yield return new WaitForSeconds(time - 1);
			//if (!SonatSDKAdapter.CanShowInterAds())
			//{
			//          StartWaitAdBreak();
			//          yield break;
			//}
			//      if (SonatSDKAdapter.IsNoads())
			//{
			//          StopWaitAdBreak();
			//          yield break;
			//}
			int level = Service<UserDataService>.Get().GetLevel();
			if (!SonatSDKAdapter.CanShowInterAds())
			{
				StartWaitAdBreak();
				yield break;
			}    
		
			popupWaitAdBreak = PanelManager.Instance.OpenPanelByName<PopupWaitAdsBreakBase>("PopupWaitAdsBreak");
			yield return new WaitForSeconds(5);
			countDone = true;

			yield return new WaitForSeconds(0.3f);
			yield return new WaitUntil(() => ready == true);
			yield return new WaitForSeconds(0.55f);

			popupWaitAdBreak.Close();
			popupWaitAdBreak = null;


			_popupAdBreakBase = PanelManager.Instance.OpenPanelByName<PopupAdsBreakBase>("PopupAdsBreak");

			yield return new WaitForSeconds(1.5f);

			//if (LoadingTransition.Instance != null)
			//LoadingTransition.Instance.showLoadingAds = false;

			SonatSDKAdapter.ShowInterAds("ad_break", OnShowAdsDone);
			countTime = null;
		}


	}
}
