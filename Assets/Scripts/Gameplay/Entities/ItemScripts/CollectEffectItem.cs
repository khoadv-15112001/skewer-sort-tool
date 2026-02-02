using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Entities.ItemScripts
{
    public class CollectEffectItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer item;
        [SerializeField] private bool useEffect = false;
        [SerializeField, ShowIf("useEffect")] private ParticleSystem effect;

        public void SetData(int itemId)
        {
            item.gameObject.SetActive(true);
            item.SetSpriteAsync(PathManager.ItemSprite(itemId)).Forget();
            item.DOFade(1f, 0f);
        }
        
        private void OnEnable()
        {
            if (useEffect)
            {
                effect.gameObject.SetActive(false);
            }
        }

        public void HideSprite()
        {
            item.gameObject.SetActive(false);
        }

        public void PlayEffect()
        {
            if (useEffect)
            {
                effect.gameObject.SetActive(true);
            }
        }

        public void FadeToZero(){
            item.DOFade(0f, 0.3f).SetEase(Ease.Linear);
        }

    }
}
