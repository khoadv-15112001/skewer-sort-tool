using Gameplay.Entities.GrillScripts;
using UnityEngine;

public class GrillVisualShutter : GrillVisual
{
    [SerializeField] private Animator shutterAnim;
    public void SetUpShutter(bool isClosed)
    {
        if (isClosed)
        {
            CloseShutter();
        }
        else
        {
            OpenShutter();
        }
    }

    public void OpenShutter()
    {
        shutterAnim.SetTrigger("Open");
    }

    public void CloseShutter()
    {
        shutterAnim.SetTrigger("Close");
    }
}
