using Gameplay;
using Sonat;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class PopupWarningLevel : Panel
{
    [SerializeField] private float delayClose = 0.5f;
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject pHard;
    [SerializeField] private GameObject pSuperHard;
    private LevelDifficulty levelDifficulty;
    private Transform target;
    private bool isClose = false;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        SonatUtils.DelayCall(delayClose, Close);

        levelDifficulty = (LevelDifficulty)uiData.Get("levelDifficulty");
        target = uiData.Get<Transform>("Target");

        pHard.SetActive(levelDifficulty == LevelDifficulty.Hard);
        pSuperHard.SetActive(levelDifficulty == LevelDifficulty.SuperHard);
        isClose = false;
    }

    public override void Close()
    {
        if (isClose) return;
        isClose = true;
        var activeObj = levelDifficulty == LevelDifficulty.Hard ? pHard : pSuperHard;
        var effect = SonatSystem.GetService<PoolingService>().Create<UILevelDifficultEffect>("UILevelDifficultEffect", activeObj.transform.position, PanelManager.Instance.transform);
        effect.Setup(levelDifficulty, target, () =>
        {
            MySonatFramework.audioService.PlaySound(AudioId.ButtonClick);
        });
        container.SetActive(false);
        SonatUtils.DelayCall(0.2f, () =>
        {
            base.Close();
            UIFlowController.isShowedPopupWarningLevel = false;
        });
    }
}
