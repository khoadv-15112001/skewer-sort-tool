using DG.Tweening;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.LavaQuest
{
    public class PopupLavaQuestWarning : Panel
    {
        [SerializeField] private float delayClose = 0.5f;
        [SerializeField] private Transform panel;

        [Header("Animation")]
        [SerializeField] private Transform startPos;
        [SerializeField] private Transform holdToViewPos;
        [SerializeField] private Transform endPos;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Ease ease;

        [SerializeField] private AudioClip loseAudio;

        private bool isClosed = false;

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            SonatSystem.GetService<AudioService>().PlayAudio("", loseAudio);

            panel.position = startPos.position;

            panel.DOMove(holdToViewPos.position, duration).SetEase(ease).OnComplete(() =>
            {
            });

            isClosed = false;
            SonatUtils.DelayCall(delayClose, Close, this);
        }

        public override void Close()
        {
            if (isClosed) return;
            isClosed = true;
            panel.transform.DOMove(endPos.position, duration).SetEase(ease).OnComplete(() =>
            {
                base.Close();
            });
        }
    }
}