using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Sonat_Screw_Jam;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class TMPMarquee : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float speed = 60f; // px/giây
    [SerializeField] private float edgePause = 0.7f; // dừng ở 2 mép
    [SerializeField] private bool pingPong = true; // false = quay đầu ngay (loop)
    [SerializeField] private bool startOnEnable = true;

    [SerializeField] private RectTransform rect;
    private CancellationTokenSource cts;

    [SerializeField] private RectTransform containerMask;
    private float maxWidth = 9999;

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
        if (containerMask == null)
            containerMask = GetComponentInParent<RectMask2D>().rectTransform;
    }

    public void SetText(string content)
    {
        text.text = content;
        CheckContent();
    }

    private void OnEnable()
    {
        rect.SetAnchorPositionX(0);

        if (startOnEnable)
        {
            CheckContent();
        }
    }

    private void OnDisable()
    {
        StopMarquee();
    }

    public void StartMarquee()
    {
        StopMarquee();
        cts = new();
        Run(cts.Token).Forget();
    }

    public void StopMarquee()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    private async UniTaskVoid Run(CancellationToken ct)
    {
        if (!text)
        {
            Debug.LogWarning("TMPMarquee: Text reference is missing.");
            return;
        }

        // Bảo đảm cài đặt hợp lý để clip và không xuống dòng
        text.overflowMode = TextOverflowModes.Masking;
        text.enableWordWrapping = false;

        var viewport = (RectTransform)transform; // khung ngoài
        var content = text.rectTransform; // rect của text

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct); // đợi layout 1 frame
        text.ForceMeshUpdate();

        float viewW = viewport.rect.width;
        float contentW = text.preferredWidth;

        // Nếu đã vừa khung thì thôi
        if (contentW <= viewW + 0.5f) return;

        float maxOffset = contentW - viewW;
        content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);

        while (!ct.IsCancellationRequested)
        {
            // Đi từ 0 -> -maxOffset
            while (content.anchoredPosition.x > -maxOffset && !ct.IsCancellationRequested)
            {
                float dx = -speed * Time.unscaledDeltaTime;
                content.anchoredPosition += new Vector2(dx, 0f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (ct.IsCancellationRequested) break;

            await UniTask.Delay(TimeSpan.FromSeconds(edgePause), cancellationToken: ct);

            if (!pingPong)
            {
                content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);
                continue;
            }

            // Quay lại -maxOffset -> 0
            while (content.anchoredPosition.x < 0f && !ct.IsCancellationRequested)
            {
                float dx = speed * Time.unscaledDeltaTime;
                content.anchoredPosition += new Vector2(dx, 0f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);
            await UniTask.Delay(TimeSpan.FromSeconds(edgePause), cancellationToken: ct);
        }
    }

    // Gọi hàm này khi text đổi nội dung để tính lại
    public void RestartAfterTextChanged() => StartMarquee();

    private void CheckContent()
    {
        float width = Mathf.Min(text.preferredWidth, maxWidth, containerMask.rect.width);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        if (text.preferredWidth > width)
            StartMarquee();
        else
        {
            StopMarquee();
            rect.SetAnchorPositionX(0);
        }
    }

    public void SetMaxWidth(float newMaxWidth)
    {
        maxWidth = newMaxWidth;
    }
}