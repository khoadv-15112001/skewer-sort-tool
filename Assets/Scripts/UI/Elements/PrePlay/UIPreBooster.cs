using Cysharp.Threading.Tasks;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;

namespace GrillSort.PreBooster
{
    public class UIPreBooster : MonoBehaviour
    {
        public GameResource boosterType;
        [SerializeField] private FixedImageRatio icon;
        [SerializeField] private TMP_Text txt;
        private readonly Service<PreBoosterService> preBoosterService = new();

        private void OnEnable()
        {
            if (boosterType == GameResource.PreBoosterFreeze)
            {
                var timeToAdd = preBoosterService.Instance.GetTimeToAdd();
                txt.text = $"+{timeToAdd}s";
            }
        }

        public async UniTask UseBooster(int delay = 0)
        {
            var effect = MySonatFramework.GetService<PoolingService>().Create<UIPreBoosterEffect>("UIPreBoosterEffect", Vector3.zero, PanelManager.Instance.transform, boosterType);
            effect.Setup(icon.transform.position, null, delay);
            await UniTask.Delay(delay);
            switch (boosterType)
            {
                case GameResource.PreBoosterDoubleStar:
                    UsePreBoosterDoubleStar();
                    break;
                case GameResource.PreBoosterMagnet:
                    UsePreBoosterMagnet();
                    break;
                case GameResource.PreBoosterFreeze:
                    UsePreBoosterFreeze();
                    break;
            }
        }

        private async UniTask UsePreBoosterDoubleStar()
        {
            var effectDoubleStar = MySonatFramework.GetService<PoolingService>().Create<UIDoubleStarEffect>("UIDoubleStarEffect", Vector3.zero, PanelManager.Instance.transform);
            effectDoubleStar.Setup(icon.transform.position, () =>
            {
                MySonatFramework.audioService.PlaySound(AudioId.Items_Collected);
                MySonatFramework.GetService<StarChestService>().SetMultiplier(2);
            }, UIPreBoosterEffect.PreBoosterEffectTime);
        }

        private async UniTask UsePreBoosterMagnet()
        {
            await UniTask.Delay((int)(UIPreBoosterEffect.PreBoosterEffectTime * 1000));
            var numPreboosterMagnet = preBoosterService.Instance.GetNumPreBoosterMagnet();
            for (int i = 0; i < numPreboosterMagnet; i++)
            {
                GameplayController.instance.BoosterMagnet().Forget();
                await UniTask.Delay(1000);
            }
        }

        private async UniTask UsePreBoosterFreeze()
        {
            var timeToAdd = preBoosterService.Instance.GetTimeToAdd();
            var effectFreeze = MySonatFramework.GetService<PoolingService>().Create<UIAddTimeEffect>("UIAddTimeEffect", Vector3.zero, PanelManager.Instance.transform, "PreBooster");
            effectFreeze.Setup(icon.transform.position, timeToAdd, () =>
            {
                MySonatFramework.audioService.PlaySound(AudioId.ButtonClick);
                GameplayController.instance.AddTimeWhenStart(timeToAdd);
            }, UIPreBoosterEffect.PreBoosterEffectTime);
        }
    }
}
