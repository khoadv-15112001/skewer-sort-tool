using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    public static bool isShowedPopupWarningLevel = false;
    public static bool isShowedPopupUnlockBooster = false;
    public static bool isShowedTut = false;
    public static bool isShowedConsecutiveWin = false;
    public static bool isShowedEffectPreBooster = false;

    private void OnEnable()
    {
        GameplayController.OnLoadLevel += OnStartLevel;
    }

    private void OnDisable()
    {
        GameplayController.OnLoadLevel -= OnStartLevel;
    }

    private void OnStartLevel(int level)
    {
        isShowedTut = false;
        isShowedPopupUnlockBooster = false;
        isShowedPopupWarningLevel = false;
        isShowedConsecutiveWin = false;
        isShowedEffectPreBooster = false;
    }

    public static bool CheckConditionShowPopupWarningLevel()
    {
        return true;
    }


    public static bool CheckConditionShowPopupUnlockBooster()
    {
        return isShowedPopupWarningLevel == false;
    }

    public static bool CheckConditionShowPopupTutNewMode()
    {
        return isShowedPopupWarningLevel == false && isShowedPopupUnlockBooster == false;
    }

    public static bool CheckConditionShowEffectConsecutiveWin()
    {
        return isShowedPopupWarningLevel == false && isShowedPopupUnlockBooster == false && isShowedTut == false;
    }

    public static bool CheckConditionShowEffectPreBooster()
    {
        return isShowedPopupWarningLevel == false && isShowedPopupUnlockBooster == false && isShowedTut == false && isShowedConsecutiveWin == false;
    }

    public static bool CheckConditionStartPlay()
    {
        return isShowedPopupWarningLevel == false && isShowedPopupUnlockBooster == false && isShowedTut == false && isShowedConsecutiveWin == false && isShowedEffectPreBooster == false;
    }
}
