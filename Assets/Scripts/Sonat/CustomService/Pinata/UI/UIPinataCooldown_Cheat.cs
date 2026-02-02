using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GrillSort.Pinata
{
    public class UIPinataCooldown_Cheat : MonoBehaviour
    {
        [SerializeField] private TMP_Text txt;

        private void Start()
        {
            Tick();
            PinataService.OnTick += Tick;
        }

        private void OnDestroy()
        {
            PinataService.OnTick -= Tick;
        }

        private void Tick()
        {
            if (!CheatPanel.IsCheating)
            {
                gameObject.SetActive(false);
                return;
            }

            if (!PinataService.Instance.IsUnlocked.BoolValue)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(!PinataService.Instance.IsActive.BoolValue);

            txt.text = PinataService.Instance.GetRemainTime();
        }

    }
}