using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TextMeshProController : MonoBehaviour
{
    [Header("Curve Settings")]
    [SerializeField]
    private TextMeshProUGUI textMeshPro;

    [SerializeField] private bool useCurve = false;

    [SerializeField] private float radius = 50f;
    [SerializeField] private float arcAngle = 180f;

    [Header("Animation Settings")]
    [SerializeField]
    private float animDuration = 0.5f;

    [SerializeField] private bool playOnEnable = false;
    [SerializeField] private float delayBetweenChars = 0.1f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine currentAnimCoroutine;
    private Vector3[] originalVertices;
    private CancellationTokenSource animationCancellation;

    private void Awake()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    private async void OnEnable()
    {
        await UniTask.DelayFrame(2);

        // Prepare vertices
        if (useCurve) ApplyCurve();
        else textMeshPro.ForceMeshUpdate();

        if (playOnEnable) PlayScaleAnimation();
    }

    // private void OnDisable()
    // {
    //     TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    // }

    // private void OnTextChanged(UnityEngine.Object obj)
    // {
    //     if (obj == textMeshPro && useCurve)
    //     {
    //         ApplyCurve();
    //     }
    // }

    [Button]
    private void ApplyCurve()
    {
        textMeshPro.ForceMeshUpdate();

        var textInfo = textMeshPro.textInfo;
        var meshInfo = textInfo.meshInfo[0];

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;

            float t = (float)i / (textInfo.characterCount - 1);
            float angle = Mathf.Lerp(-arcAngle / 2, arcAngle / 2, t) * Mathf.Deg2Rad;

            Vector3 center = Vector3.zero;
            for (int j = 0; j < 4; j++)
            {
                center += meshInfo.vertices[vertexIndex + j];
            }

            center /= 4f;

            for (int j = 0; j < 4; j++)
            {
                Vector3 vertex = meshInfo.vertices[vertexIndex + j];

                vertex -= center;
                float cosAngle = Mathf.Cos(angle);
                float sinAngle = Mathf.Sin(angle);
                float rotatedX = vertex.x * cosAngle - vertex.y * sinAngle;
                float rotatedY = vertex.x * sinAngle + vertex.y * cosAngle;
                vertex = new Vector3(rotatedX, rotatedY, vertex.z);
                vertex += center;

                float x = vertex.x;
                float y = vertex.y + radius;

                float newX = x * cosAngle - y * sinAngle;
                float newY = x * sinAngle + y * cosAngle;

                meshInfo.vertices[vertexIndex + j] = new Vector3(newX, newY - radius, vertex.z);
            }
        }

        textMeshPro.UpdateVertexData();
    }

    public async void PlayScaleAnimation(float customDuration = -1f, float customDelay = -1f)
    {
        animationCancellation?.Cancel();
        animationCancellation = new CancellationTokenSource();
        float duration = customDuration > 0 ? customDuration : animDuration;
        float delay = customDelay > 0 ? customDelay : delayBetweenChars;

        try
        {
            await AnimateScaleSequence(duration, delay, animationCancellation.Token);
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    private async UniTask AnimateScaleSequence(float duration, float delay, CancellationToken cancellationToken)
    {
        if (textMeshPro == null) return;

        var textInfo = textMeshPro.textInfo;
        if (textInfo?.meshInfo == null || textInfo.meshInfo.Length == 0) return;

        var meshInfo = textInfo.meshInfo[0];
        if (meshInfo.vertices == null) return;

        if (originalVertices?.Length != meshInfo.vertices.Length)
            originalVertices = new Vector3[meshInfo.vertices.Length];
        System.Array.Copy(meshInfo.vertices, originalVertices, meshInfo.vertices.Length);

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (charInfo.isVisible)
                ApplyScaleToCharacter(charInfo.vertexIndex, GetCharacterCenter(charInfo.vertexIndex), 0f);
        }

        textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var charInfo = textInfo.characterInfo[i];
            if (charInfo.isVisible)
            {
                AnimateCharacterScale(charInfo.vertexIndex, duration);
                if (delay > 0) await UniTask.Delay((int)(delay * 1000), cancellationToken: cancellationToken);
            }
        }
    }

    private void AnimateCharacterScale(int vertexIndex, float duration)
    {
        Vector3 charCenter = GetCharacterCenter(vertexIndex);

        DOTween.To(() => 0f, scale =>
            {
                ApplyScaleToCharacter(vertexIndex, charCenter, scale);
                textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            }, 1f, duration)
            .SetEase(scaleCurve);
    }

    private Vector3 GetCharacterCenter(int vertexIndex)
    {
        Vector3 center = Vector3.zero;

        for (int j = 0; j < 4; j++)
        {
            center += originalVertices[vertexIndex + j];
        }

        return center / 4f;
    }

    private void ApplyScaleToCharacter(int vertexIndex, Vector3 center, float scale)
    {
        var meshInfo = textMeshPro.textInfo.meshInfo[0];

        for (int j = 0; j < 4; j++)
        {
            Vector3 vertex = originalVertices[vertexIndex + j];
            Vector3 direction = vertex - center;
            meshInfo.vertices[vertexIndex + j] = center + direction * scale;
        }
    }

    public void StopAnimation()
    {
        if (currentAnimCoroutine != null)
        {
            StopCoroutine(currentAnimCoroutine);
            currentAnimCoroutine = null;
        }

        if (originalVertices != null)
        {
            var meshInfo = textMeshPro.textInfo.meshInfo[0];
            System.Array.Copy(originalVertices, meshInfo.vertices, originalVertices.Length);
            textMeshPro.UpdateVertexData();
        }
    }

    [Button]
    private void TestAnimation()
    {
        PlayScaleAnimation();
    }

    [Button]
    private void TestStopAnimation()
    {
        StopAnimation();
    }

    private void OnDestroy()
    {
        if (currentAnimCoroutine != null)
        {
            StopCoroutine(currentAnimCoroutine);
        }

        // Kill all DOTween animations on this object
        DOTween.Kill(this);
    }
}