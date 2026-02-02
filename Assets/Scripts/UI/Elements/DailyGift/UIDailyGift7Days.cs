using GrillSort.DailyGift;
using GrillSort.RealTime;
using SonatFramework.Systems;
using UnityEngine;

public class UIDailyGift7Days : MonoBehaviour
{
    // [SerializeField] private UITimeCounter timeCounter;
    [SerializeField] private UIButtonDailyGift[] btnDailyGifts;
    private readonly Service<DailyGiftService7> dailyGiftService7 = new();
    private readonly Service<RealTimeService> realTimeService = new();
    public void OnEnable()
    {
        UpdateUI();

        dailyGiftService7.Instance.onDataChanged += UpdateUI;
    }

    public void OnDestroy()
    {
        dailyGiftService7.Instance.onDataChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        // time
        var remainingTime = realTimeService.Instance.GetRemainingTimeInDay();
        // timeCounter.SetData(remainingTime, null);

        // progress
        var currentStreakDay = dailyGiftService7.Instance.data.currentStreakDay;

        foreach (var btn in btnDailyGifts)
        {
            btn.Init();
            if (btn.day <= currentStreakDay)
            {
                if (dailyGiftService7.Instance.CanClaim(btn.day))
                {
                    btn.SetCanClaim();
                }
                else
                {
                    btn.SetClaimed();
                }
                if (btn.day == currentStreakDay)
                {
                    btn.SetToday();
                }
            }
            else
            {
                btn.SetNotClaimed();
            }
        }
    }
}
