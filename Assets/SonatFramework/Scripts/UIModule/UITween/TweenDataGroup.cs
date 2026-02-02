using UnityEngine;

namespace SonatFramework.Scripts.UIModule
{
    [CreateAssetMenu(menuName = "TweenDataGroup")]
    public class TweenDataGroup : ScriptableObject
    {
        public TweenData[] tweenDatas;
    }
}