using UnityEngine;

[CreateAssetMenu(fileName = "BannedWordsFilterConfig", menuName = "Sonat Configs Custom/Banned Words Filter Config")]
public class BannedWordsFilterConfig : ScriptableObject
{
    [Tooltip("TextAsset chứa danh sách từ cấm (mỗi dòng 1 từ, ASCII a-z).")]
    public TextAsset profanityListAsset;
}
