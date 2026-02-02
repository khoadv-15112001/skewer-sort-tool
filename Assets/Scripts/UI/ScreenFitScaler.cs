using SonatFramework.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFitScaler : MonoBehaviour
{
    [SerializeField] Canvas parentCanvas;
    [SerializeField] RectTransform rectTransform;

    void OnEnable()
    {
        SonatUtils.ExecuteNextFrame(() =>
        {
            // Kích thước gốc của ảnh (pixel)
            float imgWidth = rectTransform.rect.width;
            float imgHeight = rectTransform.rect.height;

            // Lấy kích thước vùng hiển thị của Canvas (UI units)
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            // Tính tỉ lệ scale để fit (giữ nguyên aspect ratio)
            float scale = Mathf.Max(canvasWidth / imgWidth, canvasHeight / imgHeight);

            // Chỉnh sizeDelta thay vì localScale để an toàn hơn
            rectTransform.localScale = scale * Vector3.one;
        });
    }
}
