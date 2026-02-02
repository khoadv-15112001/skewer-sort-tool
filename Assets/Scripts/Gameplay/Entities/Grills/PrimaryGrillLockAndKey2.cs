using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.BoosteeManagement;
using Gameplay.Effect;
using Gameplay.Entities.Items;
using Gameplay.LevelData;
using Sonat.Enums;
using UnityEngine;

namespace Gameplay.Entities.Grills
{
    public class PrimaryGrillLockAndKey2 : PrimaryGrill
    {
        public static PrimaryGrillLockAndKey2 grillLock;
        [SerializeField] private Transform posForKey;
        private KeyEffect keyEffect;

        public override async UniTask SetData(GrillData grillData)
        {
            lockState = 1;
            base.SetData(grillData);
            grillVisual.CloseGrill(false);
            grillLock = this;
            SetLockItems(true);
        }

        public override void OpenGrill()
        {
        }

        public override ShuffleLayerData GetShuffleLayerData()
        {
            if (IsLock)
                return null;
            return base.GetShuffleLayerData();
        }

        public override List<ShuffleLayerData> GetSubsShuffleLayerData()
        {
            if (IsLock)
                return null;
            return base.GetSubsShuffleLayerData();
        }

        public void OnCollectItemKey(ItemKey2 item)
        {
            if (!IsLock) return;
            keyEffect = MySonatFramework.poolingService.Create<KeyEffect>("KeyEffect_2");
            keyEffect.SetData(item.transform.position, posForKey.position, () =>
            {
                MySonatFramework.audioService.PlaySound(AudioId.Obstacle_Locknkey_Open_Grill_sort);
                Unlock();
                if (keyEffect != null)
                {
                    MySonatFramework.poolingService.ReturnObj(keyEffect);
                    keyEffect = null;
                }
            });
        }

        public override void OnReturnObj()
        {
            base.OnReturnObj();
            if (keyEffect != null)
            {
                MySonatFramework.poolingService.ReturnObj(keyEffect);
                keyEffect = null;
            }
        }

        public override void Unlock()
        {
            base.Unlock();
            grillVisual.OpenGrill();
        }
    }
}