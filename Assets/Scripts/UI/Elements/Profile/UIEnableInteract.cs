using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIEnableInteract : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;

    public void OnClick()
    {
        inputField.interactable = !inputField.interactable;
    }
}
