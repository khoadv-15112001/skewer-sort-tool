using Gameplay.LevelData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GrillSort.KitchenMission
{
    public class UIPlayerRaceKitchenBanner : MonoBehaviour
    {
        [SerializeField] private UIAvatarBase uiAvatar;
        [SerializeField] private TMPMarqueeSmart nameTxt;
        [SerializeField] private TMP_Text nameSelfTxt;
        [SerializeField] private TMP_Text progressTxt;
        [SerializeField] private List<GameObject> medals;
        [SerializeField] private TMP_Text medalTxt;

        private KitchenMissionConfig.Players.Player _playerData;

        public void Setup(KitchenMissionConfig.Players.Player playerData, int rank)
        {
            _playerData = playerData;

            SetupAvatar();
            SetupName();
            SetupProgress();
            SetMedal(rank);
        }

        private void SetupAvatar()
        {
            uiAvatar.Init(_playerData.fID, _playerData.aID);
        }

        private void SetupName()
        {
            if (_playerData.isYourself)
            {
                nameSelfTxt.gameObject.SetActive(true);
                nameTxt.gameObject.SetActive(false);
                return;
            }

            nameSelfTxt.gameObject.SetActive(false);
            nameTxt.gameObject.SetActive(true);

            nameTxt.SetText(_playerData.name);
        }

        private void SetupProgress()
        {
            progressTxt.text = $"{_playerData.step}/{KitchenMissionService.Instance.GetCurrentStageData().maxStep}";
        }

        public void SetMedal(int rank)
        {
            for (int i = 0; i < medals.Count; i++)
            {
                medals[i].SetActive(i + 1 == rank);
            }

            if (rank >= 4)
            {
                medals[^1].SetActive(true);
                medalTxt.text = rank.ToString();
            }
        }
    }
}