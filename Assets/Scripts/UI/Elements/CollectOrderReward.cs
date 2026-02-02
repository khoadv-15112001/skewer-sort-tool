using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectOrderReward : EffectPoolBase
{
    [SerializeField] private TMP_Text txtValue;

    public void SetData(int value)
    {
        txtValue.text = $"+{value}";
    }
}
