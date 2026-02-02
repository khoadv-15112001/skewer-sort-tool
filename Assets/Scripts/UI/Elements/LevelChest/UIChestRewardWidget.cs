using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChestRewardWidget : UIHomeWidget
{
    [Header("UI progress")]

    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text txtProgress;

    [SerializeField] private GameObject pName;

    private readonly Service<ChestRewardService> _chestRewardService = new();

    public override void Setup()
    {
        UpdateProgress();
    }

    public void OnClick()
    {
        PanelManager.Instance.OpenPanel<PopupChestReward>();
    }

    private void UpdateProgress()
    {
        var data = _chestRewardService.Instance.data;
        var currentProgress = data.currentProgress;
        var maxProgress = _chestRewardService.Instance.configs.GetChestRewardData(data.currentChestIndex).levelRequired;

        var progressValue = (float)currentProgress / maxProgress;
        progressBar.DOFillAmount(progressValue, 0.3f);
        txtProgress.text = $"{currentProgress}/{maxProgress}";

        pName.SetActive(currentProgress == 0);
    }
}
