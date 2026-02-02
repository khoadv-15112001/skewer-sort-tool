using DG.Tweening;
using Gameplay.LevelData;
using MyGame.SkewerJam.Scripts.SO.Behavior;
using SonatFramework.Scripts.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Entities.GrillScripts
{
    public class GrillVisual : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer stove;
        [SerializeField] protected SpriteRenderer lid;
        [SerializeField] protected SortingGroup sortingGroup;
        [SerializeField] protected string lidType;
        [SerializeField] protected string stoveType;

        protected float defaultLidPos = 0.225f;

        protected virtual void Awake()
        {
            defaultLidPos = lid.transform.localPosition.y;
            if (sortingGroup != null) sortingGroup.enabled = false;
        }

        public virtual void SetDefaultGrill(GrillData grillData)
        {
            SetVisual();
            lid.gameObject.SetActive(true);
            lid.transform.localPosition = new Vector3(0, defaultLidPos, 0);
            lid.transform.localScale = Vector3.one;
            lid.SetAlpha(1);

            UnHighlightGrill();
        }


        protected virtual void SetVisual()
        {
            var grillBase = GetComponentInParent<GrillBase>();
            var grillVisualSO = grillBase.GrillBaseBehaviorSO.grillVisualSO;
            grillVisualSO.SetGrillBaseVisual(this, stove, lid, stoveType, lidType);
        }

        public virtual void OpenGrill(bool doEffect = true)
        {
            lid.transform.DOKill();

            if (doEffect)
            {
                lid.transform.localScale = Vector3.one;
                lid.gameObject.SetActive(true);
                lid.transform.DOLocalMoveY(2.5f, GameDefine.grillLidAnim).From(defaultLidPos).SetEase(Ease.OutQuad);
                lid.transform.DOScale(0.95f, GameDefine.grillLidAnim);
                lid.DOFade(0, GameDefine.grillLidAnim).SetEase(Ease.InQuad).OnComplete(() => { lid.gameObject.SetActive(false); });
            }
            else
            {
                lid.gameObject.SetActive(false);
            }
        }

        public virtual void CloseGrill(bool doEffect = true)
        {
            lid.transform.DOKill();
            lid.gameObject.SetActive(true);
            if (doEffect)
            {
                lid.transform.localScale = Vector3.one * 0.95f;
                lid.transform.localPosition = new Vector3(0, 2.5f, 0);
                lid.SetAlpha(0);
                lid.transform.DOScale(1, GameDefine.grillLidAnim);
                lid.transform.DOLocalMoveY(defaultLidPos, GameDefine.grillLidAnim).SetEase(Ease.InQuad);
                lid.DOFade(1, GameDefine.grillLidAnim).SetEase(Ease.OutQuad);
            }
            else
            {
                lid.transform.localScale = Vector3.one;
                lid.transform.SetLocalPositionY(defaultLidPos);
                lid.SetAlpha(1);
            }
        }

        private bool isConveyorState = false;

        public virtual void SetMaskVisible(bool state)
        {
            switch (isConveyorState)
            {
                case false when state:
                    isConveyorState = true;
                    stove.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    lid.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    break;
                case true:
                    isConveyorState = false;
                    stove.maskInteraction = SpriteMaskInteraction.None;
                    lid.maskInteraction = SpriteMaskInteraction.None;
                    break;
            }
        }

        public virtual void UpdateSubGrill()
        {
        }

        public virtual void OnReturnGrill()
        {
        }

        public void HighlightGrill()
        {
            // stove.material = GameResourceReference.Instance.grillHighlightMaterial;
            if (sortingGroup != null)
            {
                sortingGroup.enabled = true;
                sortingGroup.sortingLayerName = "UI_Top";
            }
        }

        public void UnHighlightGrill()
        {
            // stove.material = GameResourceReference.Instance.itemMaterials[0];
            if (sortingGroup != null)
            {
                sortingGroup.enabled = false;
                sortingGroup.sortingLayerName = "Object";
            }
        }

        public Bounds GetGrillBounds()
        {
            return stove != null ? stove.bounds : new Bounds(transform.position, Vector3.one);
        }
    }
}