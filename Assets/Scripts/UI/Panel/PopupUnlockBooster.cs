using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.EventBus;
using UnityEngine;

public class PopupUnlockBooster : Panel
{
    public class Data : UIData
    {
        public UIBooster uiBooster;
    }

    [SerializeField] private Transform center;
    private BoosterConfig boosterConfig;
    private Data data;
    [SerializeField] private float delaySound = 1;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        data = uiData as Data;
        boosterConfig = MySonatFramework.GetService<BoosterService>().GetBoosterConfig(data.uiBooster.boosterType);
        //DOVirtual.DelayedCall(0.75f, ClaimBooster);
        MySonatFramework.customTrackingService.LogTutorialBegin(gameObject.name.ToLogString(), 0, 0);
    }

    public void ClaimBooster()
    {
        CollectEffectSingle collectEffectSingle = new CollectEffectSingle()
        {
            collectEffectName = "CollectResourceSingleItemBig"
        };

        SonatUtils.DelayCall(delaySound, () =>
        {
            MySonatFramework.audioService.PlaySound(AudioId.Booster_Unlock_Grill_sort);
        });
        EventBus<AddItemEvent>.Raise(new AddItemEvent()
            { position = center.position, resource = boosterConfig.booster, quantity = boosterConfig.defaultValue, collectEffect = collectEffectSingle });
        
        Close();
        
        if (GameRemoteConfigValue.forceTutBooster)
        {
            var data = new PopupTutorialBooster.Data()
            {
                uiBooster = this.data.uiBooster,
            };
            SonatUtils.DelayCall(0.75f,
                () => { PanelManager.Instance.OpenPanelByNameAsync<PopupTutorialBooster>($"PopupTutorial{boosterConfig.booster}", data).Forget(); });
        }
    }

    public override void Close()
    {
        MySonatFramework.customTrackingService.LogTutorialBegin(gameObject.name.ToLogString(), 0, 0);
        base.Close();
        UIFlowController.isShowedPopupUnlockBooster = false;
    }
}