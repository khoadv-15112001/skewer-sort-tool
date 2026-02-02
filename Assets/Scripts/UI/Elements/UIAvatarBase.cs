using Cysharp.Threading.Tasks;
using Manager;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIAvatarBase : MonoBehaviour
{
    [SerializeField] private Image frameImg;
    [SerializeField] private Image avtImg;

    private int _badgeId = 0;

    public int BadgeId => _badgeId;

    public void Init(int frameID, int avtID)
    {
        frameImg.SetSpriteAsync(PathManager.FrameSprite(frameID)).Forget();
        avtImg.SetSpriteAsync(PathManager.AvatarSprite(avtID)).Forget();
    }

    public void InitSelf()
    {
        int frameID = MySonatFramework.GetService<ProfileService>().FrameId;
        int avtID = MySonatFramework.GetService<ProfileService>().AvatarId;

        frameImg.SetSpriteAsync(PathManager.FrameSprite(frameID)).Forget();
        avtImg.SetSpriteAsync(PathManager.AvatarSprite(avtID)).Forget();
    }

    public void Init(string avatar)
    {
        int avatarId = 0;
        int frameId = 0;
        _badgeId = 0;

        if (IsAvatarFormatAscii(avatar))
        {
            var list = avatar.Split(',');

            if (list.Length >= 1)
            {
                avatarId = int.Parse(list[0]);
            }

            if (list.Length >= 2)
            {
                frameId = int.Parse(list[1]);
            }
            else
            {
                Debug.LogError($"[UIAvatarBase] Avatar string missing frameId. Expected format: 'avatarId,frameId[,badgeId]'. Received: '{avatar}'");
            }

            if (list.Length >= 3)
            {
                int.TryParse(list[2], out _badgeId);
            }
        }
        else
        {
            Debug.LogError($"[UIAvatarBase] Invalid avatar format. Expected numeric values separated by commas (e.g., '1,2,3'). Received: '{avatar}'");
        }

        _ = avtImg.SetSpriteAsync(PathManager.AvatarSprite(avatarId));
        _ = frameImg.SetSpriteAsync(PathManager.FrameSprite(frameId));
    }

    private bool IsAvatarFormatAscii(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;

        ReadOnlySpan<char> span = s.AsSpan();
        int start = 0;
        bool foundAtLeastOne = false;

        for (int i = 0; i <= span.Length; i++)
        {
            // Check if we've reached a comma or the end of the string
            if (i == span.Length || span[i] == ',')
            {
                // Extract the segment between start and current position
                int segmentLength = i - start;

                // Empty segment is invalid (e.g., "1,,2" or ",1" or "1,")
                if (segmentLength == 0) return false;

                ReadOnlySpan<char> segment = span.Slice(start, segmentLength);

                // Validate that the segment contains only digits
                if (!IsDigitsAscii(segment)) return false;

                foundAtLeastOne = true;
                start = i + 1; // Move past the comma
            }
        }

        return foundAtLeastOne;
    }

    private bool IsDigitsAscii(ReadOnlySpan<char> span)
    {
        if (span.Length == 0) return false;
        foreach (var ch in span)
            if (ch < '0' || ch > '9') return false;
        return true;
    }
}
