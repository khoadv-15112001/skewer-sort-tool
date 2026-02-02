using UnityEngine;
using UnityEngine.UI;

public class ScrollerBackground : MonoBehaviour
{
    public RawImage Texture;
    public SpriteRenderer Renderer;
    public Material Material;
    public float SpeedX;
    public float SpeedY;

    private void FixedUpdate()
    {
        Texture.uvRect = new Rect(Texture.uvRect.position + new Vector2(SpeedX, SpeedY) * Time.deltaTime, Texture.uvRect.size);
        //Material.uvRect
    }
}
