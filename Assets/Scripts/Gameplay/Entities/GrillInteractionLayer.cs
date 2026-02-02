using System;
using Gameplay.Entities;
using Gameplay.Entities.GrillScripts;
using UnityEngine;

public class GrillInteractionLayer : MonoBehaviour
{
    [SerializeField] private PrimaryGrill primaryGrill;

    // public static Action<PrimaryGrill> OnSelectGrill;
    //
    // private void OnMouseUpAsButton()
    // {
    //     OnSelectGrill?.Invoke(primaryGrill);
    // }
}
