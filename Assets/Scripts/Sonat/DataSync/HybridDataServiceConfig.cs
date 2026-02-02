using UnityEngine;

[CreateAssetMenu(fileName = "HybridDataServiceConfig", menuName = "Custom/Data Service/HybridDataServiceConfig")]
public class HybridDataServiceConfig : ScriptableObject
{
    [Header("Delay từ lúc đăng nhập đến khi fetch data")]
    public float delayFetchData = 10f;

    [Header("Thời gian load")]
    public float delayCompletedLoading = 5f;
}