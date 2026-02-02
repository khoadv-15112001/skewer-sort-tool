using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Helper;
using SonatFramework.Scripts.UIModule.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class AddressableManager
{
#if UNITY_STANDALONE

    private static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();

    public static async UniTask SetSpriteAsync(this SpriteRenderer spriteRenderer, string address)
    {
        Sprite sprite = await LoadImageFromPath(address);
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }

    public static async UniTask SetSpriteAsync(this Image image, string address)
    {
        Sprite sprite = await LoadImageFromPath(address);
        if (sprite != null)
            image.sprite = sprite;
    }

    public static async UniTask SetSpriteAsync(this FixedImageRatio image, string address)
    {
        if (!File.Exists(address)) return;
        Sprite sprite = await LoadImageFromPath(address);
        if (sprite != null)
            image.SetSprite(sprite);
    }


    private static async UniTask<Sprite> LoadImageFromPath(string imagePath)
    {
        // Read the file as bytes
        byte[] fileData = await File.ReadAllBytesAsync(imagePath);

        // Create a new texture
        Texture2D texture = new Texture2D(2, 2);

        // Load the image data into the texture
        if (texture.LoadImage(fileData))
        {
            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            return sprite;
            Debug.Log($"Background image changed to: {imagePath}");
        }
        else
        {
            Debug.LogError($"Failed to load image from: {imagePath}");
        }

        return null;
    }

#else
    public static AddressableLoader<Sprite> spriteLoader = new();

    public static async UniTask SetSpriteAsync(this SpriteRenderer spriteRenderer, string address)
    {
        Sprite sprite = await spriteLoader.LoadAssetAsync(address);
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }

    public static async UniTask SetSpriteAsync(this Image image, string address)
    {
        Sprite sprite = await spriteLoader.LoadAssetAsync(address);
        if (sprite != null)
            image.sprite = sprite;
    }

    public static async UniTask SetSpriteAsync(this FixedImageRatio image, string address)
    {
        Sprite sprite = await spriteLoader.LoadAssetAsync(address);
        if (sprite != null)
            image.SetSprite(sprite);
    }
#endif
}