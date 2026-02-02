using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnlimitedLive : MonoBehaviour
{
    [SerializeField] private Image[] imagesDisactive;
    [SerializeField] private Image[] imagesActive;

    public void OnEnable()
    {
        foreach (var image in imagesDisactive)
        {
            image.enabled = false;
        }
        foreach (var image in imagesActive)
        {
            image.enabled = true;
        }
    }

    // reset
    public void OnDisable()
    {
        foreach (var image in imagesDisactive)
        {
            image.enabled = true;
        }
        foreach (var image in imagesActive)
        {
            image.enabled = false;
        }
    }
}
