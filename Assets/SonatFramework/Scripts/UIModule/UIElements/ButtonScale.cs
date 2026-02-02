using DG.Tweening;
using Sonat.Enums;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScale : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.1f;
    public float clickScale = 0.9f;
    public float duration = 0.09f;
    //public bool soundEffect = true;
    public bool isDisableEnter;
    [SerializeField] private Vector3 baseScale = new Vector3(1, 1, 1);
    //public AudioId audioId = AudioId.ButtonClick;

    //private void Start()
    //{
    //       baseScale = transform.localScale;
    //   }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDisableEnter) return;

        transform.DOScale(hoverScale * baseScale, duration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(baseScale, duration);
        // if (soundEffect)
        // {
        //     Systems.serviceManager.Get<AudioService>().PlaySound(audioId);
        // }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(clickScale * baseScale, duration);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //transform.DOScale(hoverScale, duration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(baseScale, duration);
    }

    private void OnDestroy()
    {
        //transform.DOKill(soundEffect)
        // {
        //     Systems.serviceManager.Get<AudioService>().PlaySound(audioId);
        // }
        transform.DOKill();
    }
}