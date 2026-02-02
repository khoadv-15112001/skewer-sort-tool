using UnityEngine;

namespace Sonat.GrillSort.Config
{
    [CreateAssetMenu(menuName = "Sonat/Config/UIAspectConfig")]
    public class UIAspectConfig : ScriptableObject
    {
        [Header("Widget Layout")]
        [Tooltip("Chiều cao mặc định của 1 widget ở scale = 1")]
        public float defaultWidgetHeight = 220f;

        [Header("Scale Limits")]
        [Tooltip("Tỉ lệ nhỏ nhất cho widget (nếu màn quá nhỏ)")]
        public float minScale = 0.7f;

        [Tooltip("Tỉ lệ lớn nhất cho widget (1 = kích thước chuẩn thiết kế)")]
        public float maxScale = 1f;
    }
}
