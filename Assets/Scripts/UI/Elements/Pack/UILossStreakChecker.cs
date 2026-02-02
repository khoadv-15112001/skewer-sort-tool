using System;
using System.Collections;
using System.Collections.Generic;
using GrillSort.RealTime;
using Manager;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using UnityEngine;

public class UILossStreakChecker : MonoBehaviour
{
    [SerializeField] private bool increaseCountLose = false;

    [Space(10)]
    [Header("Lose Offer Pack")]
    [SerializeField] private UIPackAppearanceScheduler _packAppearanceScheduler;

    private readonly Service<RealTimeService> _realTimeService = new();
    private LongDataPref _showOfferDate;
    private IntDataPref _countLoseToShowOffer;
    private const string DATA_KEY = "UILossStreakChecker";

    void OnEnable()
    {
        // Điều kiện show offer: Ở lần thư thứ x
        CheckShowOfferDate();

        // vẫn đang trong quá trình hiện thì tiếp tục hiện dù ở bất kì đâu
        if (_packAppearanceScheduler.CheckExpired())
        {
            gameObject.SetActive(true);
            _packAppearanceScheduler.enabled = true;
            return;
        }
        else
        {
            _countLoseToShowOffer = new IntDataPref($"{DATA_KEY}_countLoseToShowOffer");
            if (increaseCountLose) _countLoseToShowOffer.Value += 1;

            if (_countLoseToShowOffer.Value >= GameRemoteConfigValue.countLoseToShowOffer && GameRemoteConfigValue.countLoseToShowOffer > 0)
            {
                gameObject.SetActive(true);
                _packAppearanceScheduler.enabled = true;
                _countLoseToShowOffer.Value = 0;
                return;
            }
        }

        gameObject.SetActive(false);


    }



    private void CheckShowOfferDate()
    {
        _showOfferDate = new LongDataPref($"{DATA_KEY}_showOfferDate");
        var currentTime = _realTimeService.Instance.GetCurrentTime();
        var showOfferDate = DateTime.UnixEpoch.AddSeconds(_showOfferDate.Value);
        if (currentTime.Date != showOfferDate.Date)
        {
            _showOfferDate.Value = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            _countLoseToShowOffer = new IntDataPref($"{DATA_KEY}_countLoseToShowOffer");
            _countLoseToShowOffer.Value = 0;
        }
    }

}
