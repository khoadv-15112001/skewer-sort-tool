using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.Pinata
{
    public class WidgetPinata : UIHomeWidget
    {
        [SerializeField] private GameObject lockObject;

        private List<UIPinataGroupStage> stages;
        private bool _isActive;

        public override void Setup()
        {
            stages = GetComponentsInChildren<UIPinataGroupStage>(true).ToList();

            if (!PinataService.Instance.IsUnlocked.BoolValue)
            {
                Hide(false);
                return;
            }

            PinataService.OnActive += Show;
            PinataService.OnInactive += OnInactive;
            PinataService.OnCompleteStage += OnCompleteStage;

            if (PinataService.Instance.IsActive.BoolValue)
                Show();
            else
                Hide();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ProcessTask().Forget();
            }
        }

        private void OnDestroy()
        {
            PinataService.OnActive -= Show;
            PinataService.OnInactive -= OnInactive;
            PinataService.OnCompleteStage -= OnCompleteStage;
        }

        private void OnCompleteStage()
        {
            if (!_isActive) return;
            Show();
        }

        public override async UniTask<bool> ProcessTask()
        {
            if (!_isActive) return false;

            if (!PinataService.Instance.IsAvailableCompleteStage())
                return false;

            var panel = PanelManager.Instance.OpenPanel<PopupPinata>();

            await UniTask.WaitUntil(() => panel == null);

            return true;
        }

        private void Show()
        {
            if (stages.Count != PinataService.Instance.Config.MaxStage)
            {
                Debug.LogError($"[WidgetPinata] Stage Count not equal MaxStage={PinataService.Instance.Config.MaxStage}");
                return;
            }

            _isActive = true;

            if (lockObject != null)
                lockObject.SetActive(false);

            foreach (var stage in stages)
                stage.gameObject.SetActive(false);

            ShowProgress();
        }

        private void ShowProgress()
        {
            for (int i = 0; i < stages.Count; i++)
            {
                if (i + 1 != PinataService.Instance.CurrentStage.Value)
                {
                    stages[i].gameObject.SetActive(false);
                    continue;
                }

                stages[i].Setup();
                stages[i].gameObject.SetActive(true);
            }
        }

        private void Hide(bool showCountdown = true)
        {
            _isActive = false;

            foreach (var stage in stages)
                stage.gameObject.SetActive(false);

            if (lockObject != null)
                lockObject.SetActive(showCountdown);
        }

        private void OnInactive()
        {
            Hide();
        }
    }
}
