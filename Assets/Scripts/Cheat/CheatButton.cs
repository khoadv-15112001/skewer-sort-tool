using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class CheatButton : MonoBehaviour
{
    private int count;
    public Panel needOff;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnClickCheat()
	{
        count++;
        if(count >= 5)
		{
			if (needOff)
			{
                needOff.Close();
			}
            PanelManager.Instance.OpenForget<CheatPanel>();
		}
	}
}
