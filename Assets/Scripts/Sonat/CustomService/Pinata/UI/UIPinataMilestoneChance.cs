using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Pinata
{
    public class UIPinataMilestoneChance : MonoBehaviour
    {
        [SerializeField] private GameObject tick;
        [SerializeField] private GameObject bigObj;
        [SerializeField] private Image icon;

        private State _state;

        public void BindData(State state)
        {
            _state = state;

            SetTick();
            SetBigObj();
        }

        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        private void SetTick()
        {
            tick.SetActive(_state == State.Done);
        }

        private void SetBigObj()
        {
            bigObj.SetActive(_state == State.Doing);
        }

        public enum State
        {
            Done,
            Doing,
            Locked
        }
    }
}