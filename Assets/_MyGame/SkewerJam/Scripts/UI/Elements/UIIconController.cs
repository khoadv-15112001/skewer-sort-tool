using MyGame.SkewerJam.Gameplay.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class UIIconController : MonoBehaviour
{
    [SerializeField] private Image iconWin;
    [SerializeField] private Image iconLose;

    void OnEnable()
    {
        var isWin = GameplayHelper.IsWin;
        SetIcon(isWin);
    }

    public void SetIcon(bool isWin)
    {
        iconWin.gameObject.SetActive(isWin);
        iconLose.gameObject.SetActive(!isWin);
    }
}
