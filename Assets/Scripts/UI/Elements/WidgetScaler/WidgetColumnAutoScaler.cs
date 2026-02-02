using Sonat.GrillSort.Config;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sonat.GrillSort.UI
{
    public class WidgetColumnAutoScaler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform leftColumn;
        [SerializeField] private RectTransform rightColumn;
        [SerializeField] private VerticalLayoutGroup leftLayout;
        [SerializeField] private VerticalLayoutGroup rightLayout;

        [Header("Config")]
        [SerializeField] private UIAspectConfig aspectConfig;

        public event Action OnStartScale;

        protected void Start()
        {
            RecalculateScale();
        }

        /// <summary>
        /// Gọi khi bật/tắt widget hoặc thay đổi layout.
        /// </summary>
        public void RecalculateScale()
        {
            OnStartScale?.Invoke();

            float leftScale = CalculateColumnScale(leftColumn, leftLayout);
            float rightScale = CalculateColumnScale(rightColumn, rightLayout);

            // Giữ scale nhỏ hơn để hai cột đồng bộ, không bị lệch
            float finalScale = Mathf.Min(leftScale, rightScale, aspectConfig.maxScale);

            if (leftColumn != null) leftColumn.localScale = Vector3.one * finalScale;
            if (rightColumn != null) rightColumn.localScale = Vector3.one * finalScale;
        }

        private float CalculateColumnScale(RectTransform column, VerticalLayoutGroup layout)
        {
            if (column == null || layout == null || aspectConfig == null) return 1f;

            // Đếm widget đang active
            int activeCount = 0;
            foreach (Transform child in column)
                if (child.gameObject.activeSelf) activeCount++;

            if (activeCount == 0) return 1f;

            // Tính tổng chiều cao cần thiết (dựa vào config widget height & spacing)
            float totalWidgetHeight = aspectConfig.defaultWidgetHeight * activeCount;
            float totalSpacing = layout.spacing * (activeCount - 1);
            float totalHeightNeeded = totalWidgetHeight + totalSpacing;

            // Chiều cao hiển thị thực tế của column
            float availableHeight = column.rect.height;

            // Tính tỉ lệ scale, clamp để không vượt maxScale
            float scale = availableHeight / totalHeightNeeded;
            return Mathf.Clamp(scale, aspectConfig.minScale, aspectConfig.maxScale);
        }
    }
}
