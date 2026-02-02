using System.Collections;
using TMPro;
using UnityEngine;

public class UITextLoading : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text textComponent;
    
    [Header("Settings")]
    [SerializeField] private float dotInterval = 0.5f; // Thời gian giữa mỗi dấu chấm
    [SerializeField] private int maxDots = 3; // Số dấu chấm tối đa
    [SerializeField] private bool autoStart = true;
    
    private Coroutine animationCoroutine;
    
    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }
    
    private void OnEnable()
    {
        if (autoStart)
        {
            StartLoadingAnimation();
        }
    }
    
    private void OnDisable()
    {
        StopLoadingAnimation();
    }
    
    /// <summary>
    /// Bắt đầu animation loading
    /// </summary>
    public void StartLoadingAnimation()
    {
        if (textComponent == null) return;
        
        // Bắt đầu animation
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        
        animationCoroutine = StartCoroutine(AnimateDotsCoroutine());
    }
    
    /// <summary>
    /// Dừng animation loading
    /// </summary>
    public void StopLoadingAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
    
    /// <summary>
    /// Coroutine animate dấu chấm bằng maxVisibleCharacters
    /// Text đã có "Loading..." sẵn, chỉ cần -3 rồi +1, +2, +3
    /// </summary>
    private IEnumerator AnimateDotsCoroutine()
    {
        // Đếm tổng số ký tự của text (ví dụ: "Loading..." = 10 ký tự)
        int totalLength = textComponent.text.Length;
        int baseLength = totalLength - maxDots; // Trừ 3 dấu chấm
        
        while (true)
        {
            // -3: Ẩn hết 3 dấu chấm → "Loading"
            // +1: Hiện 1 dấu chấm → "Loading."
            // +2: Hiện 2 dấu chấm → "Loading.."
            // +3: Hiện 3 dấu chấm → "Loading..."
            for (int dotCount = 0; dotCount <= maxDots; dotCount++)
            {
                textComponent.maxVisibleCharacters = baseLength + dotCount;
                yield return new WaitForSeconds(dotInterval);
            }
        }
    }
}
