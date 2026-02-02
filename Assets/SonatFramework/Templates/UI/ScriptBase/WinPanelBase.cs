using System;
using DG.Tweening;
using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.CollectEffect;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.UserData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinPanelBase : Panel
{
	public class Data : UIData
	{
		public int level;
		public ResourceData reward;
		public Action nextLevel;
		public GameMode gameMode;
	}
	//protected CollectEffectCurveStream collectEffectCurveStream;
	protected bool collected;

	[SerializeField] protected bool showNative;
	[SerializeField] protected Image icon;
	[SerializeField] protected Transform panel;
	[SerializeField] protected Transform claimBtn;
	[SerializeField] protected Transform x2CoinBtn;
	[SerializeField] protected float delayToCollect = 2f;
	[SerializeField] private TMP_Text txtReward, txtReward_x2;
	protected Data data;
	protected bool nativeHided;
	
	#region DEFAULT

	public override void OnSetup()
	{
		base.OnSetup();
		//collectEffectCurveStream = CollectEffectManager.Instance.GetICollectEffectType<CollectEffectCurveStream>();
	}

	public override void Open(UIData uiData)
	{
		base.Open(uiData);
		data = (Data)uiData;
		collected = false;
		//panel.gameObject.SetActive(false);
		if(x2CoinBtn != null)
		{
			x2CoinBtn.gameObject.SetActive(Service<UserDataService>.Get().GetLevel() >= SonatSDKAdapter.GetValueBySegment<int>("level_start_x2_coin_by_segment", UserData.UserCampaignSegment.Value, 5));
		}
		txtReward.text = data.reward.quantity.ToString();
		txtReward_x2.text = (data.reward.quantity * 2).ToString();
		if (showNative)
		{
			SonatSDKAdapter.ShowNativeAds();
		}
	}

	public override void OnOpenCompleted()
	{
		base.OnOpenCompleted();
	}

	public override void Close()
	{
		base.Close();
		if (showNative)
		{
			SonatSDKAdapter.HideNavtiveAds();
		}
	}

	protected override void OnCloseCompleted()
	{
		base.OnCloseCompleted();
	}

	public override void OnFocus()
	{
		base.OnFocus();
		if (showNative && nativeHided)
		{
			SonatSDKAdapter.ShowNativeAds();
		}
	}

	public override void OnFocusLost()
	{
		base.OnFocusLost();
		if (!showNative) return;
		nativeHided = true;
		SonatSDKAdapter.HideNavtiveAds();
	}

	#endregion

	public virtual void OnClaimClick()
	{
		if(collected) return;
		collected = true;
		//collectEffectCurveStream.CreateEffect(icon.transform.position, data.reward.resource, 10, AfterClaim);
		OnClaimReward(claimBtn, data.reward.quantity);
	}

	protected virtual void AfterClaim()
	{
		SonatSDKAdapter.ShowInterAds("win", NextLevel);
	}

	public virtual void OnClaimX2Click()
	{
		if(collected) return;
		if (SonatSDKAdapter.IsRewardAdsReady())
		{
			collected = true;
			
		}
		SonatSDKAdapter.ShowRewardAds(OnWatchedVideo, "x2_coin_win", "x2_coin_win");
	}

	protected virtual void OnWatchedVideo()
	{
		collected = true;
		var log = new EarnResourceLogData()
		{
			spendType = "win_x2",
			spendId = "win_x2"
		};
		Service<InventoryService>.Get().AddResource(data.reward.resource, data.reward.quantity, log, false);
		OnClaimReward(x2CoinBtn, data.reward.quantity * 2);
		//collectEffectCurveStream.CreateEffect(icon.transform.position, data.reward.resource, 15, NextLevel);
		
	}

	protected virtual void OnClaimReward(Transform btn, int quantity)
	{
		EventBus<AddItemEvent>.Raise(new AddItemEvent(){resource = data.reward.resource, quantity = quantity, position = btn.position, collectEffect = new CollectEffectMultiple()});
		DOVirtual.DelayedCall(delayToCollect, NextLevel);
	}

	public virtual void NextLevel()
	{
		Close();
		//Play
		data.nextLevel?.Invoke();
	}

}