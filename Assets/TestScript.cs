using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TestScript : MonoBehaviour
{
    [Header("Shader Property Names")]
    [SerializeField] private string speedXProp = "_SpeedX";
    [SerializeField] private string speedYProp = "_SpeedY";

    [Header("Speed Settings")]
    public float normalSpeedX = 1f;
    public float normalSpeedY = 0f;
    public float maxSpeedX = 5f;
    public float maxSpeedY = 0f;

    [Header("UI Display (Optional)")]
    public TMP_Text speedXText;
    public TMP_Text speedYText;

    [Header("Acceleration Settings")]
    public float accelerationTime = 1f;
    public float decelerationTime = 1f;

    private Renderer rend;
    private MaterialPropertyBlock mpb;
    private int speedX_ID, speedY_ID;
    private int offsetX_ID, offsetY_ID;

    private float currentSpeedX, currentSpeedY;
    private Coroutine tweenCo;
    private bool isMouseDown = false;
    private float offsetX, offsetY;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        speedX_ID = Shader.PropertyToID(speedXProp);
        speedY_ID = Shader.PropertyToID(speedYProp);
        offsetX_ID = Shader.PropertyToID("_OffsetX");
        offsetY_ID = Shader.PropertyToID("_OffsetY");

        currentSpeedX = normalSpeedX;
        currentSpeedY = normalSpeedY;
        offsetX = 0f;
        offsetY = 0f;
        
        ApplyToRenderer(currentSpeedX, currentSpeedY);
    }

    private void Update()
    {
        if (rend == null) return;

        // Tích lũy offset = offset + speed * dt
        offsetX += currentSpeedX * Time.deltaTime;
        offsetY += currentSpeedY * Time.deltaTime;

        // Giữ offset trong [0,1) để tránh tràn/giảm precision
        offsetX = offsetX - Mathf.Floor(offsetX);
        offsetY = offsetY - Mathf.Floor(offsetY);

        // Cập nhật offset vào shader
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(offsetX_ID, offsetX);
        mpb.SetFloat(offsetY_ID, offsetY);
        rend.SetPropertyBlock(mpb);
        
        // Check mouse release
        if (isMouseDown && !Input.GetMouseButton(0))
        {
            OnMouseUp();
        }
    }

    private void ApplyToRenderer(float sx, float sy)
    {
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(speedX_ID, sx);
        mpb.SetFloat(speedY_ID, sy);
        rend.SetPropertyBlock(mpb);

        // Update UI text in real-time
        UpdateSpeedDisplay(sx, sy);
    }

    private void UpdateSpeedDisplay(float sx, float sy)
    {
        if (speedXText != null)
        {
            speedXText.text = $"Speed X: {sx:F2}";
        }

        if (speedYText != null)
        {
            speedYText.text = $"Speed Y: {sy:F2}";
        }
    }

    private void StopTweenIfAny()
    {
        if (tweenCo != null) { StopCoroutine(tweenCo); tweenCo = null; }
    }

    private IEnumerator TweenSpeed(float targetX, float targetY, float duration)
    {
        float startX = currentSpeedX;
        float startY = currentSpeedY;

        if (duration <= 0f)
        {
            currentSpeedX = targetX;
            currentSpeedY = targetY;
            ApplyToRenderer(currentSpeedX, currentSpeedY);
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float s = Mathf.SmoothStep(0f, 1f, t); // smoothstep

            currentSpeedX = Mathf.Lerp(startX, targetX, s);
            currentSpeedY = Mathf.Lerp(startY, targetY, s);

            ApplyToRenderer(currentSpeedX, currentSpeedY);
            yield return null;
        }

        currentSpeedX = targetX;
        currentSpeedY = targetY;
        ApplyToRenderer(currentSpeedX, currentSpeedY);
        tweenCo = null;
    }

    private void OnMouseDown()
    {
        isMouseDown = true;
        StopTweenIfAny();
        tweenCo = StartCoroutine(TweenSpeed(maxSpeedX, maxSpeedY, accelerationTime));
    }

    private void OnMouseUp()
    {
        isMouseDown = false;
        StopTweenIfAny();
        tweenCo = StartCoroutine(TweenSpeed(normalSpeedX, normalSpeedY, decelerationTime));
    }

    private void OnDisable()
    {
        StopTweenIfAny();
        currentSpeedX = normalSpeedX;
        currentSpeedY = normalSpeedY;
        ApplyToRenderer(currentSpeedX, currentSpeedY);
    }
}
