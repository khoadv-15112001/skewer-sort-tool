using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using UnityEngine;

namespace GrillSort.BattlePass
{
    public class UITimeCounter : MonoBehaviour
    {
        [SerializeField] private SonatFramework.Scripts.UIModule.UIElements.UITimeCounter timeCounter;
        private readonly Service<BattlePassService> battlePassService = new Service<BattlePassService>();


        private void OnEnable()
        {
            UpdateUI(-1);
            battlePassService.Instance.OnUpdateUI += UpdateUI;
        }

        private void OnDisable(){
            battlePassService.Instance.OnUpdateUI -= UpdateUI;
        }

        private void UpdateUI(int idx){
            timeCounter.SetData(battlePassService.Instance.GetTimeToReset(), null);
        }
    }

}
