using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.LevelData;
using UnityEngine;

namespace Gameplay.Entities.Grills
{
    public class PrimaryGrillSingle : PrimaryGrill
    {
        public override async UniTask SetData(GrillData grillData)
        {
            if (grillData.layer != null)
                foreach (var layer in grillData.layer)
                {
                    if (layer.itemData != null)
                        if (layer.itemData.Length > slots.Length)
                        {
                            layer.itemData = layer.itemData.Where(e => e != null).ToArray();
                        }
                }

            base.SetData(grillData);
        }

        protected override async UniTask<SubGrill> CreateSubGrill(int layer)
        {
            Vector3 pos = subContainer.position + Vector3.up * layer * 0.035f + Vector3.back * layer * 0.03f + subOffset;
            string subGrillName = "SubGrillSingle";
            return await grillBaseBehaviorSO.gameFactorySO.CreateItem<SubGrill>(subGrillName, pos, subContainer);
        }

        public override bool CreateSpecialItem(int itemId)
        {
            return false;
        }
    }
}