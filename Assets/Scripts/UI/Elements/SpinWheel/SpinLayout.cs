using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GrillSort.LuckySpin
{
    public class SpinLayout : MonoBehaviour
    {
        [Header("Spin layout")]
        [SerializeField] private RectTransform container;
        [SerializeField] private float radius;
        [SerializeField] private float angleOffset;

        private void OnValidate()
        {
            var childCount = container.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = container.GetChild(i);
                var childRect = child.GetComponent<RectTransform>();
                var childAngle = angleOffset + i * 360f / childCount;
                child.rotation = Quaternion.Euler(0, 0, -90 + childAngle);
                childRect.anchoredPosition = new Vector2(Mathf.Cos(childAngle * Mathf.Deg2Rad) * radius, Mathf.Sin(childAngle * Mathf.Deg2Rad) * radius);
            }
        }
    }
}

