using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonClickTut : MonoBehaviour
{
    [SerializeField] private List<GameObject> activeObj;
    [SerializeField] private List<GameObject> inactiveObj;
    [SerializeField] private UnityEvent onClickEvent;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Deactive();
        Active();

        onClickEvent?.Invoke();
    }

    private void Deactive()
    {
        foreach (var obj in inactiveObj)
            obj.SetActive(false);
    }

    private void Active()
    {
        foreach (var obj in activeObj)
            obj.SetActive(true);
    }
}
