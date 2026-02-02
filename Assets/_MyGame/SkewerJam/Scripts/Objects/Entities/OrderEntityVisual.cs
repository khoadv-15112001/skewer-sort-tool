using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay;
using MyGame.SkewerJam.Gameplay;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SettingsManagement.Vibation;
using UnityEngine;

namespace MyGame.SkewerJam.Objects.Entities
{
    public class OrderEntityVisual : MonoBehaviour
    {
        [SerializeField] private GameObject normalOrder;
        [SerializeField] private GameObject bonusOrder;
        [SerializeField] private SpriteRenderer bonusLid;

        [SerializeField] private SpriteRenderer imageLid;
        [SerializeField] private OrderEntityConfigSO orderEntityConfigSO;

        [Header("Animation & Effect")]
        [SerializeField] private ParticleSystem completeEffect;
        private Vector3 _originalPosition;

        private void Start()
        {
            _originalPosition = bonusLid.transform.localPosition;
        }

        public void ResetLid()
        {
            bonusLid.transform.DOKill();
            bonusLid.transform.localPosition = _originalPosition;
            bonusLid.gameObject.SetActive(true);
            bonusLid.transform.localScale = Vector3.one;
            bonusLid.SetAlpha(1);
        }

        public void SetNormalOrder(bool isNormal)
        {
            normalOrder.gameObject.SetActive(isNormal);
            bonusOrder.gameObject.SetActive(!isNormal);
        }

        public virtual void OpenGrill(bool doEffect = true)
        {
            bonusLid.transform.DOKill();

            if (doEffect)
            {
                bonusLid.transform.localScale = Vector3.one;
                bonusLid.gameObject.SetActive(true);
                bonusLid.transform.DOLocalMoveY(2.5f, GameDefine.grillLidAnim).From(0.035f).SetEase(Ease.OutQuad);
                bonusLid.transform.DOScale(0.95f, GameDefine.grillLidAnim);
                bonusLid.DOFade(0, GameDefine.grillLidAnim).SetEase(Ease.InQuad).OnComplete(() => { bonusLid.gameObject.SetActive(false); });
            }
            else
            {
                bonusLid.gameObject.SetActive(false);
            }
        }

        public void SetActive(bool isActive)
        {
            imageLid.gameObject.SetActive(false);
            // activeSprite.gameObject.SetActive(isActive);
            // inactiveSprite.gameObject.SetActive(!isActive);

            //init
            completeEffect.Stop();
        }

        public async UniTask PlayComplete(Action onComplete)
        {
            imageLid.gameObject.SetActive(true);
            imageLid.transform.localPosition = Vector3.up * orderEntityConfigSO.up;
            await imageLid.transform.DOLocalMove(Vector3.zero, orderEntityConfigSO.durationUp).SetEase(orderEntityConfigSO.downCurve);
            completeEffect.Play();
            MySonatFramework.audioService.PlaySound(AudioId.Items_Merge_SMode_HLW_Grill_sort);
            onComplete?.Invoke();

            MySonatFramework.GetService<VibrationService>().Vibrate(100);
            await UniTask.Delay((int)(orderEntityConfigSO.delayMoveOut * 1000));

            var orderEntity = transform.parent.GetComponent<OrderEntity>();
            var targetPos = orderEntity.transform.localPosition + Vector3.up * orderEntityConfigSO.up;
            await orderEntity.transform.DOLocalMove(targetPos, orderEntityConfigSO.durationDown).SetEase(orderEntityConfigSO.upCurve);
            MyGame.SkewerJam.Gameplay.GameFactory.Instance.ReturnEntity(orderEntity);
        }
    }
}
