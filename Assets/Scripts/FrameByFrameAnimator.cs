using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FrameByFrameAnimator : MonoBehaviour 
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 12f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool playOnEnable = true;
   
    private SpriteRenderer spriteRenderer;
    private Image image;
    private int currentFrame = 0;
    private CancellationTokenSource animationCTS;
    private bool isPlaying = false;
   
    public bool IsPlaying => isPlaying;
    public int CurrentFrame => currentFrame;
    public int TotalFrames => frames?.Length ?? 0;
   
    void Start()
    {
        // Auto detect component type
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            image = GetComponent<Image>();
       
        if (playOnStart && frames != null && frames.Length > 0)
        {
            PlayAnimation().Forget();
        }
    }
   
    void OnEnable()
    {
        if (playOnEnable && frames != null && frames.Length > 0 && HasValidRenderer())
        {
            PlayAnimation().Forget();
        }
    }
   
    void OnDisable()
    {
        StopAnimation();
    }
   
    private bool HasValidRenderer()
    {
        return spriteRenderer != null || image != null;
    }
   
    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;
        else if (image != null)
            image.sprite = sprite;
    }
   
    public async UniTask PlayAnimation()
    {
        if (frames == null || frames.Length == 0 || !HasValidRenderer()) return;
       
        StopAnimation();
       
        animationCTS = new CancellationTokenSource();
        isPlaying = true;
        currentFrame = 0;
       
        try
        {
            var token = CancellationTokenSource.CreateLinkedTokenSource(
                animationCTS.Token, 
                this.GetCancellationTokenOnDestroy()
            ).Token;
           
            await PlayAnimationLoop(token);
        }
        catch (OperationCanceledException)
        {
            // Animation was cancelled
        }
        finally
        {
            isPlaying = false;
        }
    }
   
    private async UniTask PlayAnimationLoop(CancellationToken token)
    {
        var frameDelay = TimeSpan.FromSeconds(1f / frameRate);
       
        do
        {
            SetSprite(frames[currentFrame]);
            currentFrame = (currentFrame + 1) % frames.Length;
           
            await UniTask.Delay(frameDelay, cancellationToken: token);
           
        } while (loop || currentFrame != 0);
    }
   
    public void StopAnimation()
    {
        if (animationCTS != null)
        {
            animationCTS.Cancel();
            animationCTS.Dispose();
            animationCTS = null;
        }
        isPlaying = false;
    }
   
    public void PauseAnimation()
    {
        StopAnimation();
    }
   
    public void ResumeAnimation()
    {
        if (!isPlaying)
        {
            PlayAnimation().Forget();
        }
    }
   
    public void SetFrame(int frameIndex)
    {
        if (frames != null && frameIndex >= 0 && frameIndex < frames.Length)
        {
            currentFrame = frameIndex;
            SetSprite(frames[currentFrame]);
        }
    }
   
    public void SetFrameRate(float newFrameRate)
    {
        frameRate = Mathf.Max(0.1f, newFrameRate);
    }
   
    void OnDestroy()
    {
        StopAnimation();
    }
}