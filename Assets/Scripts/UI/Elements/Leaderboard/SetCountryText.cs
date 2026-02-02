using GrillSort.OnlineService;
using SonatFramework.Systems;
using System.Globalization;
using TMPro;
using UnityEngine;

public class SetCountryText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private readonly Service<OnlineService> onlineService = new();

    private void Awake()
    {
        if (text == null)
            text.GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (onlineService.Instance.UserInfo != null)
            text.text = new RegionInfo(onlineService.Instance.UserInfo.geo).EnglishName;
    }
}
