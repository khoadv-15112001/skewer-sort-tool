using SonatFramework.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ScaleFitParent : MonoBehaviour
{
    [SerializeField] private RectTransform parentTransform;
    [SerializeField] private RectTransform rectTransform;

    private void Awake()
    {
        parentTransform ??= transform.parent.GetComponent<RectTransform>();
    }

#if UNITY_EDITOR
    private void Update()
    {
        Fit();
    }
#endif

    void OnEnable()
    {
        SonatUtils.ExecuteNextFrame(() =>
        {
            Fit();
        });
    }

    private void Fit()
    {
        float imgWidth = rectTransform.rect.width;
        float imgHeight = rectTransform.rect.height;

        RectTransform canvasRect = parentTransform;
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float scale = Mathf.Max(canvasWidth / imgWidth, canvasHeight / imgHeight);

        rectTransform.localScale = scale * Vector3.one;
    }

}
