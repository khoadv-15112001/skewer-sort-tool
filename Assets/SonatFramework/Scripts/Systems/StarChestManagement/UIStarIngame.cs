using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStarIngame : MonoBehaviour
{
    [SerializeField] protected Image icon;
    [SerializeField] protected TMP_Text txtValue;
    [SerializeField] protected ParticleSystem blastEffect;
    [SerializeField] protected Image iconDoubleStar;
    [SerializeField] private bool spawnFxForMultiStar = false;
    [SerializeField] private string fxName;
    private int value = -1;
    private bool scaleUp;
    private Coroutine collectAnim;
    public float counterDuration = 0.5f;
    public float scaleSpeed = 3f;
    public float scaleMax = 1.15f;
    protected EventBinding<AddItemEvent> addItemEvent;
    protected EventBinding<LevelStartedEvent> startLevelEvent;
    private StarChestService starChestService;

    private void Awake()
    {
        starChestService = MySonatFramework.GetService<StarChestService>();
    }

    public void OnEnable()
    {
        addItemEvent = new EventBinding<AddItemEvent>(OnCollectStar);
        startLevelEvent = new EventBinding<LevelStartedEvent>(OnStartLevel);
        UpdateStar(starChestService.Star);

        iconDoubleStar.gameObject.SetActive(false);
        starChestService.onMultiplierChanged += UpdateMultiplier;
    }

    public void OnDisable()
    {
        EventBus<AddItemEvent>.Deregister(addItemEvent);
        EventBus<LevelStartedEvent>.Deregister(startLevelEvent);

        starChestService.onMultiplierChanged -= UpdateMultiplier;
    }

    protected void UpdateStar(int value, float duration = 0, float delay = 0)
    {
        txtValue.DOKill(true);
        int oldValue = this.value;
        txtValue.DOCounter(oldValue, value, duration).SetDelay(delay);
        this.value = value;
    }

    protected void OnStartLevel(LevelStartedEvent eventData)
    {
        UpdateStar(0);
    }

    protected void OnCollectStar(AddItemEvent itemEvent)
    {
        if (itemEvent.resource != GameResource.Star) return;
        var collect = new CollectEffectMultipleStar() {multipleCallback = true};

        var newValue = value + itemEvent.quantity;
        collect.Collect(GameResource.Star, itemEvent.quantity, itemEvent.position + Vector3.down * 0.5f, icon.transform.position, () => { PlayCollectEffect(); });
        UpdateStar(newValue, 0.2f, 1.4f);
    }

    protected void PlayCollectEffect()
    {
        if (!gameObject.activeInHierarchy) return;
        if (collectAnim != null)
        {
            //StopCoroutine(collectAnim);
            scaleUp = true;
            //blastEffect?.Play();
        }
        else
        {
            collectAnim = StartCoroutine(CollectEffect());
            //blastEffect?.gameObject.SetActive(true);
        }

        if (blastEffect && !spawnFxForMultiStar)
        {
           blastEffect.Play();
        }

        if (spawnFxForMultiStar)
        {
            if (string.IsNullOrEmpty(fxName))
                return;
            MySonatFramework.GetService<PoolingServiceAsync>().CreateAsync<EffectPoolBase>(fxName, icon.transform.position, this.transform);
        }
     
    }

    IEnumerator CollectEffect()
    {
        scaleUp = true;
        while (icon.transform.localScale.x < scaleMax)
        {
            icon.transform.localScale += Vector3.one * Time.deltaTime * scaleSpeed;
            yield return null;
        }

        //SettingManager.Vibrations(100);
        yield return null;
        scaleUp = false;

        while (icon.transform.localScale.x > 1)
        {
            if (scaleUp)
            {
                collectAnim = StartCoroutine(CollectEffect());
                yield break;
            }

            icon.transform.localScale -= Vector3.one * Time.deltaTime * scaleSpeed;
            yield return null;
        }

        icon.transform.localScale = Vector3.one;
        collectAnim = null;
    }

    private void UpdateMultiplier(int multiplier)
    {
        iconDoubleStar.gameObject.SetActive(multiplier > 1);
    }
}