using SonatFramework.Systems.InventoryManagement;
using UnityEngine;

namespace GrillSort.BattlePass
{

    [CreateAssetMenu(fileName = "BattlePassServicePremium", menuName = "My Services/BattlePassServicePremium")]
    public class BattlePassServicePremium : BattlePassBase
    {
        protected override string DATA_KEY => "BATTLEPASS_DATA_PREMIUM";

        public override void Reset()
        {
            base.Reset();
            _isUnlocked.Value = 0;

            MySonatFramework.GetService<BattlePassService>().ResetAvatarBadge();

            MySonatFramework.GetService<ProfileService>().CheckProfile();
        }
    }
}