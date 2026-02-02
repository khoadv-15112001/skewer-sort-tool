using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIBubble : MonoBehaviour
{
    [SerializeField] private GameObject pReward;
    private bool blockUnselectUpdate;

    public void OnClick()
    {
        if (pReward.activeSelf)
        {
            Unselect();
        }
        else
        {
            Select();
        }
    }

    public void Select()
    {
        pReward.SetActive(true);
    }

    public void Unselect()
    {
        pReward.SetActive(false);
    }

    public void SetBlockUpdate()
    {
        blockUnselectUpdate = true;
    }

    private void Update()
    {
        if (blockUnselectUpdate) return;

        if (Input.GetMouseButtonDown(0))
        {
            Unselect();
        }
    }
}
