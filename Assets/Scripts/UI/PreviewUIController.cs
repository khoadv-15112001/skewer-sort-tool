using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewUIController : MonoBehaviour
{
    [SerializeField] private GameObject _obj;
    [SerializeField] private UIRewardGrid _rewardGrid;
    private Transform _transform;
    public void Awake()
    {
        _obj.SetActive(false);
    }
    public void ShowPreview(RewardData rewardData, Transform parrent = null)
    {
        if (!_transform) _transform = transform;

        _rewardGrid.SetReward(rewardData);

        if (_transform && parrent != null)
        {
            _transform.parent = parrent;

            _transform.position = new Vector3(parrent.position.x, _transform.position.y, 0);
        }
        _obj.SetActive(true);
    }
}
