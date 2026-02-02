using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class BlockPanel : Panel
{
    public static BlockPanel Current;

    public static void Set(bool state)
    {
        if (state)
            BlockUI();
        else
            UnlockUI();
    }

    private static void BlockUI()
    {
        if (Current != null) return;
        Current = PanelManager.Instance.OpenPanel<BlockPanel>();
    }

    private static void UnlockUI()
    {
        if (Current == null) return;
        Current.Close();
        Current = null;
    }
}