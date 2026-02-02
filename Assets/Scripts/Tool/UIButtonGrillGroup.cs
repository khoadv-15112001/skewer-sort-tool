using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonGrillGroup : MonoBehaviour
{
    [SerializeField] private Transform[] containers;
    [SerializeField] private UIToolGrillGroupButton[] buttons;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].index = i;
        }
        OnButtonSelected(0);
    }

    public void OnButtonSelected(int buttonIndex)
    {
        for (int i = 0; i < containers.Length; i++)
        {
            containers[i].gameObject.SetActive(i == buttonIndex);
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetSelected(i == buttonIndex);
        }
    }
}
