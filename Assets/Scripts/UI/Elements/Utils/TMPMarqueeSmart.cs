using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class TMPMarqueeSmart : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text tmp;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float waitBeforeScroll = 1f;
    [SerializeField] private float waitAtEnd = 1f;

    private RectTransform tmpRect;
    private Coroutine scrollRoutine;

    void Reset()
    {
        tmp = GetComponent<TMP_Text>();
        tmpRect = tmp.GetComponent<RectTransform>();
        container = GetComponentInParent<RectTransform>();
    }

    void Awake()
    {
        if (!tmp) tmp = GetComponent<TMP_Text>();
        tmpRect = tmp.GetComponent<RectTransform>();
    }

    public void SetText(string text)
    {
        tmp.text = text;
        tmp.ForceMeshUpdate();

        // Dừng routine cũ nếu có
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }

        float textWidth = tmp.preferredWidth;
        float containerWidth = container.rect.width;

        // Nếu text vừa khung → anchor giữa
        if (textWidth <= containerWidth)
        {
            SetAnchorCenter();
            tmpRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            // Nếu text dài hơn khung → anchor trái để cuộn mượt
            SetAnchorLeft();
            scrollRoutine = StartCoroutine(ScrollLoop(textWidth, containerWidth));
        }
    }

    private IEnumerator ScrollLoop(float textWidth, float containerWidth)
    {
        yield return new WaitForSeconds(waitBeforeScroll);

        float maxOffset = textWidth - containerWidth;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(-maxOffset, 0);

        while (true)
        {
            // Move left
            while (tmpRect.anchoredPosition.x > endPos.x)
            {
                tmpRect.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(waitAtEnd);

            // Move back to start
            while (tmpRect.anchoredPosition.x < startPos.x)
            {
                tmpRect.anchoredPosition += Vector2.right * scrollSpeed * Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(waitBeforeScroll);
        }
    }

    private void SetAnchorCenter()
    {
        tmpRect.anchorMin = new Vector2(0.5f, 0.5f);
        tmpRect.anchorMax = new Vector2(0.5f, 0.5f);
        tmpRect.pivot = new Vector2(0.5f, 0.5f);
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private void SetAnchorLeft()
    {
        tmpRect.anchorMin = new Vector2(0f, 0.5f);
        tmpRect.anchorMax = new Vector2(0f, 0.5f);
        tmpRect.pivot = new Vector2(0f, 0.5f);
        tmp.alignment = TextAlignmentOptions.Left;
    }
}
