using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class VendingGrillTileInProgress : MonoBehaviour, IPoolingObject
{
    [SerializeField] private float progressWidth = 1.55f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Transform gamespace;
    private float defaultWidth;

    public void OnCreateObj(params object[] args)
    {
        gamespace = transform.parent;
        transform.localScale = Vector3.one;
    }

    public void OnReturnObj()
    {
        transform.SetParent(gamespace);
    }

    public void Setup()
    {
        defaultWidth = spriteRenderer.size.x;
    }

    public void Setup(int i, int numLayer, Transform container)
    {
        transform.SetParent(container);
        var scale = 1.0f / numLayer;
        //transform.localScale = new Vector3(scale - 0.01f, 1f, 1f);
        spriteRenderer.size = new Vector2(scale * defaultWidth - 0.05f, spriteRenderer.size.y);

        var curWidth = progressWidth * scale;
        var x = -progressWidth / 2 + curWidth / 2 + i * curWidth;
        transform.localPosition = new Vector3(x, 0, 0);
    }
}