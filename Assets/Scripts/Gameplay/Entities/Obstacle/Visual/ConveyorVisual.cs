using System.Collections;
using UnityEngine;

public class ConveyorVisual : MonoBehaviour
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite boostedSprite;

    private Renderer rend;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock mpb;
    private int offsetX_ID, offsetY_ID, fillAmount_ID, oldTex_ID, newTex_ID, fillDirection_ID, fillBounds_ID;
    private float offsetX, offsetY;
    private bool init = false;
    private Coroutine fillCoroutine;
    private Vector3 speed;

    public void Initialize(Vector3 speed)
    {
        this.speed = speed;

        rend = GetComponent<Renderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        offsetX_ID = Shader.PropertyToID("_OffsetX");
        offsetY_ID = Shader.PropertyToID("_OffsetY");
        fillAmount_ID = Shader.PropertyToID("_FillAmount");
        oldTex_ID = Shader.PropertyToID("_OldTex");
        newTex_ID = Shader.PropertyToID("_NewTex");
        fillDirection_ID = Shader.PropertyToID("_FillDirection");
        fillBounds_ID = Shader.PropertyToID("_FillBounds");

        offsetX = 0f;
        offsetY = 0f;

        // Calculate and set sprite bounds for proper fill normalization
        UpdateFillBounds();

        init = true;
        UpdateVisual(speed);
    }

    public void SwitchSpeedSprite(bool isBoosted)
    {
        if (normalSprite == null || boostedSprite == null) return;

        // Use flipX directly - we'll compensate for UV flip in UpdateVisual()
        spriteRenderer.flipX = speed.x > 0;
        spriteRenderer.flipY = speed.y > 0;

        FillDirection direction = FillDirection.LeftToRight;
        if (speed.x > 0)
            direction = FillDirection.RightToLeft;
        else if (speed.x < 0)
            direction = FillDirection.LeftToRight;
        else if (speed.y > 0)
            direction = FillDirection.TopToBottom;
        else if (speed.y < 0)
            direction = FillDirection.BottomToTop;

        SwitchSpriteWithFill(isBoosted ? boostedSprite : normalSprite, direction: direction);
    }

    /// <summary>
    /// Switches the sprite with a smooth fill transition effect
    /// </summary>
    /// <param name="newSprite">The new sprite to transition to</param>
    /// <param name="duration">Duration of the transition in seconds (default: 0.5s)</param>
    /// <param name="direction">Direction of the fill effect</param>
    public void SwitchSpriteWithFill(Sprite newSprite, float duration = 0.5f, FillDirection direction = FillDirection.BottomToTop)
    {
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);
        
        fillCoroutine = StartCoroutine(FillSpriteTransition(newSprite, duration, direction));
    }

    /// <summary>
    /// Instantly switches the sprite without any transition effect
    /// </summary>
    /// <param name="newSprite">The new sprite to display</param>
    public void SwitchSpriteInstant(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            if (fillCoroutine != null)
                StopCoroutine(fillCoroutine);
            
            spriteRenderer.sprite = newSprite;
            
            // Reset fill amount
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat(fillAmount_ID, 0);
            rend.SetPropertyBlock(mpb);
        }
    }

    private IEnumerator FillSpriteTransition(Sprite newSprite, float duration, FillDirection direction)
    {
        if (rend == null || spriteRenderer == null || newSprite == null) 
            yield break;

        // Capture the current sprite as the old texture
        Sprite currentSprite = spriteRenderer.sprite;
        if (currentSprite == null || currentSprite.texture == null)
            yield break;

        // Set the fill direction vector based on enum
        Vector4 fillDirectionVector = GetFillDirectionVector(direction);
        
        // Get the renderer's world-space bounds for proper fill calculation
        // Use the CURRENT bounds (before sprite change) for consistent fill animation
        Bounds worldBounds = rend.bounds;
        Vector4 fillBounds = new Vector4(
            worldBounds.min.x, 
            worldBounds.min.y, 
            worldBounds.max.x, 
            worldBounds.max.y
        );
        
        // Set both old and new textures, direction, and bounds to the shader
        rend.GetPropertyBlock(mpb);
        mpb.SetTexture(oldTex_ID, currentSprite.texture);
        mpb.SetTexture(newTex_ID, newSprite.texture);
        mpb.SetVector(fillDirection_ID, fillDirectionVector);
        mpb.SetVector(fillBounds_ID, fillBounds);
        mpb.SetFloat(fillAmount_ID, 0.001f); // Start with tiny value to trigger transition
        rend.SetPropertyBlock(mpb);

        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fillAmount = Mathf.Clamp01(elapsed / duration);
            
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat(fillAmount_ID, fillAmount);
            rend.SetPropertyBlock(mpb);
            
            yield return null;
        }

        // Complete the transition - actually switch the sprite
        spriteRenderer.sprite = newSprite;
        
        // Wait one frame for sprite bounds to stabilize after the switch
        yield return null;
        
        // Update bounds for the new sprite AFTER waiting
        UpdateFillBounds();
        
        // Reset fill amount for next transition
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(fillAmount_ID, 0);
        rend.SetPropertyBlock(mpb);
        
        fillCoroutine = null;
    }

    private Vector4 GetFillDirectionVector(FillDirection direction)
    {
        switch (direction)
        {
            case FillDirection.BottomToTop:
                return new Vector4(0, 1, 0, 0);
            case FillDirection.TopToBottom:
                return new Vector4(0, -1, 0, 0);
            case FillDirection.LeftToRight:
                return new Vector4(1, 0, 0, 0);
            case FillDirection.RightToLeft:
                return new Vector4(-1, 0, 0, 0);
            default:
                return new Vector4(0, 1, 0, 0);
        }
    }

    private void UpdateFillBounds()
    {
        if (rend == null) return;

        // Get the renderer's world-space bounds
        // This gives us the actual physical extents in world space
        Bounds worldBounds = rend.bounds;
        
        // Set the bounds as a Vector4 (minX, minY, maxX, maxY) in world space
        Vector4 fillBounds = new Vector4(
            worldBounds.min.x, 
            worldBounds.min.y, 
            worldBounds.max.x, 
            worldBounds.max.y
        );
        
        // Apply to material
        rend.GetPropertyBlock(mpb);
        mpb.SetVector(fillBounds_ID, fillBounds);
        rend.SetPropertyBlock(mpb);
    }

    public void UpdateVisual(Vector3 speed)
    {
        if (!init) return;

        if (rend == null) return;

        // Compensate for flipX by inverting offsetX direction
        // When sprite is flipped, UV coordinates are mirrored, so we need to reverse the animation
        float xMultiplier = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        float yMultiplier = spriteRenderer != null && spriteRenderer.flipY ? -1f : 1f;

        // accumulate offset over time = offset + speed * dt
        offsetX += speed.x * Time.deltaTime * xMultiplier;
        offsetY += speed.y * Time.deltaTime * yMultiplier;
        //Debug.LogError("UpdateVisual: " + offsetX + " " + offsetY);

        // keep offset in [0,1) to avoid overflow/precision loss
        offsetX = offsetX - Mathf.Floor(offsetX);
        offsetY = offsetY - Mathf.Floor(offsetY);

        // update offset to shader
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(offsetX_ID, offsetX);
        mpb.SetFloat(offsetY_ID, offsetY);
        rend.SetPropertyBlock(mpb);
    }

    private void OnDisable()
    {
        init = false;
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        init = false;
    }
}

public enum FillDirection
{
    BottomToTop,
    TopToBottom,
    LeftToRight,
    RightToLeft
}
