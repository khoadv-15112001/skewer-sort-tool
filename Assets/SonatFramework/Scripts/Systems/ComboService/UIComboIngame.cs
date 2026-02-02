using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIComboIngame : MonoBehaviour
{
    private ComboService comboService;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text txtCombo;
    [SerializeField] private GameObject mainObject;
    [SerializeField] private ParticleSystem blastEffect;
    [SerializeField] private LayoutElement layoutElement;
    private int combo;
    private Coroutine cooldownCoroutine;

    private void Awake()
    {
        comboService = MySonatFramework.GetService<ComboService>();
    }

    private void OnEnable()
    {
        comboService.OnComboChange += OnComboUpdate;
        OnComboUpdate();
    }

    private void OnDisable()
    {
        comboService.OnComboChange -= OnComboUpdate;
    }

    private void OnComboUpdate()
    {
        combo = comboService.Combo;
        if (combo < 1)
        {
            mainObject.SetActive(false);
            layoutElement.ignoreLayout = true;
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
                cooldownCoroutine = null;
            }
        }
        else
        {
            mainObject.SetActive(true);
            layoutElement.ignoreLayout = false;
            blastEffect.gameObject.SetActive(true);
            blastEffect.Play();
            txtCombo.SetLocalizeParam("VALUE", combo.ToString());
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }

            cooldownCoroutine = StartCoroutine(ComboCooldown());
        }
    }

    IEnumerator ComboCooldown()
    {
        float maxTime = comboService.GetComboTime();
        float time = maxTime;
        while (time > 0)
        {
            if (GameplayController.instance.gameState == GameState.Playing)
            {
                time -= Time.deltaTime;
                slider.value = time / maxTime;
            }

            yield return null;
        }
    }
}