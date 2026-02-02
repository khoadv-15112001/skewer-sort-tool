using DG.Tweening;
using GrillSort.PiggyBank;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPiggyBankProgress : MonoBehaviour
{
    [Header("Coins")]
    [SerializeField] private TMP_Text txtRewardCoins;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private Slider slider;

    [Header("Reward Slider")]
    [SerializeField] private Transform container;

    private readonly Service<PoolingContainerService> _poolingService = new();

    private PiggyTierConfig _tierConfig;

    private void Awake()
    {
        slider.value = 0;
    }

    public void Setup(PiggyTierConfig tierConfig)
    {
        _tierConfig = tierConfig;
        _poolingService.Instance.CleanContainer(container);
        for (int i = 0; i < tierConfig.piggyRewards.Count; i++)
        {
            var reward = tierConfig.piggyRewards[i];
            var item = _poolingService.Instance.CreateObject<UIRewardSlider>(container);

            item.Setup(tierConfig.GetMaxPoint(), reward.iconSpriteIdx);

            var coin = tierConfig.GetRewardCoins(i);
            item.SetData(coin, reward.point);
        }
    }

    public void SetData(PiggyBankData data)
    {
        slider.DOKill();
        var point = SonatSystem.GetService<PiggyBankService>().Point;
        var value = (float)point / _tierConfig.GetMaxPoint();
        value = value == 0 ? 0 : Mathf.Clamp(value, 0.075f, 1);// để hiển thị cho đỡ méo
        slider.DOValue(value, 0.5f);
    }

}
