using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule
{
    public abstract class View : MonoBehaviour
    {
        public bool keepCached;
        public bool pauseGame;
        //[HideInInspector] public GameState gameStateBefore;
        protected UIData uiData;
        public string id => GetType().Name;
        public bool ignoreTracking;
        [SerializeField] protected string trackingName;

        public void Init()
        {
            OnSetup();
        }

        public abstract void OnSetup();

        public abstract void Open(UIData uiData);
        public abstract void OnOpenCompleted();
        public abstract void Close();


        public abstract void OnFocus();
        public abstract void OnFocusLost();

        public virtual void CloseImmediately()
        {
            OnCloseCompleted();
        }

        protected virtual void OnCloseCompleted()
        {
            PanelManager.Instance.ReleasePanel(this);
        }

        public virtual string GetPlacement()
        {
            //return SonatUtils.ConvertToLogParam(gameObject.name);
            return trackingName;
        }
    }
}