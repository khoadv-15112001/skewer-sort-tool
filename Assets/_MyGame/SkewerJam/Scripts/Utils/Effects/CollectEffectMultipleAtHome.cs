using System;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace SkewerJam.Utils.Effects
{
    public class CollectEffectMultipleAtHome : SonatCollectEffect
    {
        public string collectEffectName = "UICollectResouceEffectAtHome";
        public int maxCount = 5;
        public float radiusX = 1f;
        public float radiusY = 0.25f;
        public float delaySpawn = 0.1f;
        public bool isShowText = true;

        public override void Collect(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action callback)
        {
            if (isShowText)
            {
                var text = SonatSystem.GetService<PoolingServiceAsync>().CreateAsync<UICollectResouceText>("UICollectResouceText", PanelManager.Instance.transform,
                        resource, quantity, startPos);

            }
            callback = null;
            float timeSpawn = 0;
            for (int i = 0; i < Mathf.Min(maxCount, quantity); i++)
            {
                Vector3 ran = UnityEngine.Random.insideUnitCircle * new Vector2(radiusX, radiusY);
                Vector3 pos = startPos + ran;

                SonatUtils.DelayCall(timeSpawn, () =>
                        {
                            var item = SonatSystem.GetService<PoolingServiceAsync>().CreateAsync<UICollectResouceEffectAtHome>(collectEffectName, PanelManager.Instance.transform,
                            resource, quantity, pos, endPos, callback);
                            callback = null;
                        });
                timeSpawn += delaySpawn;
            }
        }
    }
}
