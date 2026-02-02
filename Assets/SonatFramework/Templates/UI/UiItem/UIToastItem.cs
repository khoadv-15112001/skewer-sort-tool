using TMPro;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.Helper;

public class UIToastItem : MonoBehaviour
{
    public TMP_Text txtContent;
    
    [FoldoutGroup("Animation Config")]
    [SerializeField] private float totalDuration = 1.5f;
    
    [FoldoutGroup("Animation Config")]
    [SerializeField] private float moveUpDistance = 200f;
    
    [FoldoutGroup("Animation Config")]
    [SerializeField] private float startScale = 0.5f;
    
    public float TotalDuration => totalDuration;
    
    private Sequence _sequence;
    
    public void SetData(string content, string param = null)
    {
        txtContent.SetLocalize(content);

        if (param != null)
        {
            txtContent.SetLocalizeParam("VALUE", param);
        }
        
        // Kill previous animation
        _sequence?.Kill();
        
        // Reset state
        Color color = txtContent.color;
        color.a = 1f;
        txtContent.color = color;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one * startScale;
        
        // Tạo sequence animation
        _sequence = DOTween.Sequence();
        
        // Scale OutBack (bounce) - nhanh ở đầu
        _sequence.Append(transform.DOScale(1f, totalDuration * 0.3f).SetEase(Ease.OutBack));
        
        // Đồng thời di chuyển lên - nhanh rồi chậm dần (OutQuad)
        _sequence.Join(transform.DOLocalMoveY(moveUpDistance, totalDuration).SetEase(Ease.OutQuad));
        
        // Fade out ở nửa cuối
        _sequence.Insert(totalDuration * 0.5f, txtContent.DOFade(0f, totalDuration * 0.5f).SetEase(Ease.InQuad));
    }
    
    private void OnDisable()
    {
        _sequence?.Kill();
    }
}
