using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonLanguage : MonoBehaviour
{
    [SerializeField] public ELanguage language;
    [SerializeField] Image imgSelected;
    [SerializeField] TMP_Text text;
    void Awake()
    {
        if (imgSelected != null)
        {
            imgSelected.gameObject.SetActive(false);
        }
    }

    public void Unselected()
    {
        if (imgSelected != null)
        {
            imgSelected.gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        PanelManager.Instance.GetPanel<PopupLanguage>().OnClickButtonLanguage(language);
        Selected();

        // if (LocalizationManager.HasLanguage(language.ToString()))
        // {
        LocalizationManager.CurrentLanguage = PopupLanguage.languageName[(int)language]; //language.ToString();
        // I2Helper.UpdateLocalize();
        // }
        // SoundManager.Instance.PlaySound(KeySound.ItemChoose);
    }

    public void Selected()
    {
        if (imgSelected != null)
        {
            imgSelected.gameObject.SetActive(true);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        text.text = PopupLanguage.txtLanguages[(int)language];
    }
#endif
}
