using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupTutOctoChef : PopupTutNewMode
{
    private int state;
    [SerializeField] private Transform[] stateObjects;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        for (int i = 0; i < stateObjects.Length; i++)
        {
            stateObjects[i].gameObject.SetActive(i == 0);
        }
    }

    public override void Close()
    {
        state++;
        if (state == stateObjects.Length)
        {
            base.Close();
            return;
        }
        
        for (int i = 0; i < stateObjects.Length; i++)
        {
            stateObjects[i].gameObject.SetActive(i == state);
        }

        
    }
}
