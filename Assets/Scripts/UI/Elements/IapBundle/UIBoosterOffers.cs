using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using UnityEngine;

public class UIBoosterOffers : MonoBehaviour
{
    [SerializeField] private Transform[] offerPacks;


    public void InitData(GameResource booster)
    {
        switch (booster)
        {
            case GameResource.BoosterMagnet:
                SetActivePack(0);
                break;
            case GameResource.BoosterFreeze:
                SetActivePack(1);
                break;
            case GameResource.BoosterShuffle:
                SetActivePack(2);
                break;
            case GameResource.BoosterMagicKey:
                SetActivePack(3);
                break;
            case GameResource.BoosterBlowTorch:
                SetActivePack(4);
                break;
            default:
                SetActivePack(-1);
                break;
        }
    }

    private void SetActivePack(int index)
    {
        for (int i = 0; i < offerPacks.Length; i++)
        {
            offerPacks[i].gameObject.SetActive(i == index);
        }
    }
}
