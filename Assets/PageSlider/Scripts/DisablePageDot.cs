using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablePageDot : MonoBehaviour
{
    [SerializeField] private float interval = 1f;
    [SerializeField] private GameObject dependencyObject;
    private float timer = 0f;
    
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            gameObject.SetActive(dependencyObject.activeSelf);
        }
    }
}
