using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using UnityEngine;
using UnityEngine.UI;

public class PopupTutorialBooster : Panel
{
    public class Data: UIData
    {
        public UIBooster uiBooster;
    }
    [SerializeField] private GameResource boosterType;
    [SerializeField] private Transform handTransform;
    private UIBooster uiBooster;
    private EventBinding<UseBoosterEvent> useBoosterEvent;
    private Canvas canvas;
    private GraphicRaycaster raycaster;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        Data data = (Data)uiData;
        this.uiBooster = data.uiBooster;
        handTransform.position = uiBooster.transform.position;
        handTransform.gameObject.SetActive(false);
        canvas = uiBooster.gameObject.AddComponent<Canvas>();
        raycaster = uiBooster.gameObject.AddComponent<GraphicRaycaster>();
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "UI_Top";
        canvas.sortingOrder = 1;
        
        useBoosterEvent = new EventBinding<UseBoosterEvent>(OnUseBooster);
        DOVirtual.DelayedCall(0.5f, () => handTransform.gameObject.SetActive(true));
    }

    private void OnUseBooster(UseBoosterEvent eventData)
    {
        EventBus<UseBoosterEvent>.Deregister(useBoosterEvent);
        useBoosterEvent = null;
        Destroy(raycaster);
        Destroy(canvas);
       
        Close();
    }
}