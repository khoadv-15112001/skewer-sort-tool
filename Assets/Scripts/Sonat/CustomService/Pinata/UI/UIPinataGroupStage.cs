using SonatFramework.Scripts.UIModule;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Pinata
{
    public class UIPinataGroupStage : MonoBehaviour
    {
        [SerializeField] private Button btn;

        private List<UIPinataMilestone> listMilestones = new();

        public void Setup()
        {
            SetupButton();
            SetupMilestone();
        }

        private void SetupButton()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickButton);
        }

        private void SetupMilestone()
        {
            listMilestones.Clear();
            listMilestones = GetComponentsInChildren<UIPinataMilestone>().ToList();

            for (int i = 0; i < listMilestones.Count; i++)
            {
                listMilestones[i].BindData(i + 1);
            }
        }

        private void OnClickButton()
        {
            PanelManager.Instance.OpenPanel<PopupPinataParty>();
        }
    }
}