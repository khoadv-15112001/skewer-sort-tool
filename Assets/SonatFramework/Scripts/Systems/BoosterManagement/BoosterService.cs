using System;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement;

namespace SonatFramework.Systems.BoosterManagement
{
    public abstract class BoosterService : SonatServiceSo
    {
        public Action<GameResource> onUnlockBooster;
        public abstract bool IsBoosterUnlock(GameResource booster);
        public abstract void UnlockBooster(GameResource booster);
        public abstract BoosterConfig GetBoosterConfig(GameResource booster);
        public abstract BoosterData GetBoosterData(GameResource booster);
        public abstract bool CanUseBooster(GameResource booster);
        public abstract void UseBoosterSuccess(GameResource boosterType);

        public abstract bool BuyBooster(GameResource boosterType, int quantity);
        public abstract void AddBooster(GameResource boosterType, int quantity, EarnResourceLogData logData);
    }
}