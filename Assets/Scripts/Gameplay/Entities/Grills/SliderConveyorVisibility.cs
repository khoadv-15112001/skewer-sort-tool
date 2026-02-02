using UnityEngine;

namespace Gameplay.Entities.GrillScripts
{
    /// <summary>
    /// Component gắn vào Slider để tự động show/hide khi vào/ra khỏi vùng băng chuyền
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SliderConveyorVisibility : MonoBehaviour
    {
        [Header("Target Object")]
        [Tooltip("Object cần tắt/bật (thường là Slider UI). Nếu để trống sẽ tự động lấy child đầu tiên")]
        [SerializeField] private GameObject targetObject;

        [Header("Settings")]
        [Tooltip("Tag vùng cần ẩn targetObject")]
        [SerializeField] private string hideZoneTag = "OutConveyor";

        [Header("Debug")]
        [SerializeField] private bool showDebugLog = false;

        private BoxCollider2D detectionCollider;
        private Rigidbody2D rb2D;
        private int hideZoneContactCount = 0; // Đếm số collider hide zone đang chạm

        private void Awake()
        {
            SetupRigidbody();
            SetupCollider();
            SetupTargetObject();
            
            // Mặc định: HIỆN
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                
                if (showDebugLog)
                {
                    Debug.Log($"[SliderVisibility] Initial state: VISIBLE");
                }
            }
        }

        private void SetupTargetObject()
        {
            // Nếu chưa assign target object, tự động lấy child đầu tiên
            if (targetObject == null && transform.childCount > 0)
            {
                targetObject = transform.GetChild(0).gameObject;
                
                if (showDebugLog)
                {
                    Debug.Log($"[SliderVisibility] Auto-assigned target: {targetObject.name}");
                }
            }

            if (targetObject == null)
            {
                Debug.LogError($"[SliderVisibility] No target object assigned on {gameObject.name}! Please assign a target object in Inspector.", this);
            }
        }

        private void SetupRigidbody()
        {
            rb2D = GetComponent<Rigidbody2D>();
            if (rb2D == null)
            {
                rb2D = gameObject.AddComponent<Rigidbody2D>();
            }
            
            // Setup Rigidbody2D để không bị ảnh hưởng bởi physics
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.gravityScale = 0;
            rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            if (showDebugLog)
            {
                Debug.Log($"[SliderVisibility] Setup Rigidbody2D (Kinematic)");
            }
        }

        private void SetupCollider()
        {
            detectionCollider = GetComponent<BoxCollider2D>();
            if (detectionCollider == null)
            {
                detectionCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            
            // Setup collider là trigger
            detectionCollider.isTrigger = true;
            
            if (showDebugLog)
            {
                Debug.Log($"[SliderVisibility] Setup BoxCollider2D (Trigger)");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(hideZoneTag))
            {
                hideZoneContactCount++;
                
                if (showDebugLog)
                {
                    Debug.Log($"[SliderVisibility] Entered hide zone: {other.name}, count: {hideZoneContactCount}");
                }
                
                HideSlider(); // Vào vùng OutConveyor → ẨN
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Đảm bảo slider ẨN khi đang trong vùng hide zone
            if (other.CompareTag(hideZoneTag))
            {
                if (targetObject != null && targetObject.activeSelf)
                {
                    HideSlider();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(hideZoneTag))
            {
                hideZoneContactCount--;
                hideZoneContactCount = Mathf.Max(0, hideZoneContactCount); // Không để âm
                
                if (showDebugLog)
                {
                    Debug.Log($"[SliderVisibility] Exited hide zone: {other.name}, count: {hideZoneContactCount}");
                }
                
                // Chỉ HIỆN lại khi không còn hide zone nào chạm
                if (hideZoneContactCount == 0)
                {
                    ShowSlider(); // Ra khỏi vùng OutConveyor → HIỆN
                }
            }
        }

        /// <summary>
        /// Ẩn slider (SetActive false)
        /// </summary>
        private void HideSlider()
        {
            if (targetObject == null) return;
            if (!targetObject.activeSelf) return; // Đã ẩn rồi
            
            targetObject.SetActive(false);
            
            if (showDebugLog)
            {
                Debug.Log($"[SliderVisibility] Hiding target: {targetObject.name} (Inside OutConveyor zone)");
            }
        }

        /// <summary>
        /// Hiện slider (SetActive true)
        /// </summary>
        private void ShowSlider()
        {
            if (targetObject == null) return;
            if (targetObject.activeSelf) return; // Đã hiện rồi
            
            targetObject.SetActive(true);
            
            if (showDebugLog)
            {
                Debug.Log($"[SliderVisibility] Showing target: {targetObject.name} (Outside OutConveyor zone)");
            }
        }

        /// <summary>
        /// Force set visibility (có thể gọi từ code khác nếu cần)
        /// </summary>
        public void SetVisibility(bool visible)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(visible);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Vẽ collider detection zone trong editor (chỉ khi selected)
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(col.offset, col.size);
            }
        }
#endif
    }
}

