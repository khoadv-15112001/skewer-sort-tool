using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using UnityEngine;

[CreateAssetMenu(fileName = "ComboService", menuName = "Sonat Services/Combo Service")]
public class ComboService : SonatServiceSo, IServiceInitialize
{
    [SerializeField] private ComboConfig config;
    private int combo;
    public int Combo => combo;
    private Coroutine cooldownCoroutine;
    public Action OnComboChange;
    
    public ComboConfig Config => config;

    public void Initialize()
    {
        combo = 0;
        new EventBinding<LevelStartedEvent>(OnLevelStart);
    }

    private void OnLevelStart(LevelStartedEvent eventData)
    {
        ResetCombo();
    }

    public void ResetCombo()
    {
        combo = 0;
        if(cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
        OnComboChange?.Invoke();
    }

    public int GetComboTime()
    {
        if (config == null || config.ComboTime == null || config.ComboTime.Count == 0)
        {
            Debug.LogWarning("[ComboService] ComboConfig or ComboTime list is null/empty!");
            return 5; // Default fallback
        }
        
        if (combo >= config.ComboTime.Count) 
            return config.ComboTime[^1];
        
        return config.ComboTime[combo];
    }

    public void AddCombo()
    {
        combo++;
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
        cooldownCoroutine = StartCoroutine(ComboCooldown());
        OnComboChange?.Invoke();
    }

    IEnumerator ComboCooldown()
    {
        float time = GetComboTime();
        while (time > 0)
        {
            if (GameplayController.instance.gameState == GameState.Playing)
                time -= Time.deltaTime;
            yield return null;
        }
        ResetCombo();
    }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return SonatSystem.Instance.StartCoroutine(routine);
    }

    public void StopCoroutine(Coroutine routine)
    {
        SonatSystem.Instance.StopCoroutine(routine);
    }
}
