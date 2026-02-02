using UnityEngine;

namespace GrillSort.BattlePass
{

    [CreateAssetMenu(fileName = "BattlePassServiceFree", menuName = "My Services/BattlePassServiceFree")]
    public class BattlePassServiceFree : BattlePassBase
    {
        protected override string DATA_KEY => "BATTLEPASS_DATA_FREE";

        public override void Initialize()
        {
            base.Initialize();
            Unlock();
        }

    }
}