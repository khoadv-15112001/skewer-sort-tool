using GrillSort.PackServiceRegistry;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PackCooldown : MonoBehaviour
{
    private readonly Service<PackServiceRegistry> registry = new();
    [SerializeField] private UITimeCounter timeCounter;
    [SerializeField] private bool active;
    [SerializeField] private BannerName banerName;
    public UnityEvent CooldownCompleted;
    void Start()
    {
        active = SonatSDKAdapter.GetRemoteBool("revive_cooldown", active);

        if(!active)
        {
            timeCounter.gameObject.SetActive(false);
            return;
        }

        timeCounter.gameObject.SetActive(true);

        if (timeCounter)
            timeCounter.SetData(registry.Instance.RemainingTimeCooldown(banerName), () => CooldownCompleted?.Invoke());
    }
}
