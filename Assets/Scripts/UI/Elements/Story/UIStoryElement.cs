using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace GrillSort.Story
{
    public class UIStoryElement : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private string storyName;
        [SerializeField] private bool isLock;
        [SerializeField] private GameObject objLock;
        [SerializeField] private LocalizationParamsManager[] idTexts;
        // [SerializeField] private LocalizationParamsManager[] nameTexts;

        public void SetData(){
            foreach(var idText in idTexts){
                idText.SetParameterValue("VALUE", (id + 1).ToString());
            }
            // foreach(var nameText in nameTexts){
            //     nameText.SetParameterValue("VALUE", storyName);
            // }
            objLock.SetActive(isLock);
        }

        void OnValidate()
        {
            SetData();
        }

        public void OnClick(){
           PopupToast.Cretate("Coming soon");
        }
    }
}

