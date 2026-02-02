#if CUSTOM_ENUM
namespace Sonat.Enums
{
    public enum GameResource : byte
    {
        None,
        Coin = 1,
        Lives = 2,
        BoosterFreeze,
        BoosterShuffle,
        BoosterMagnet,
        BoosterMagicKey,
        BoosterMagicWand,
        BoosterBlowTorch,

        NoAds = 20,
        ItemEvent = 21,

        BattlePass_Key = 25,

        LivesService_SingleLive = 26,

        // ----Gameplay_SkewerJam
        Energy = 27,
        Pumpkin = 28,
        Star = 50,
        MAX,
        PiggyPoint,
        PreBoosterFreeze,
        PreBoosterMagnet,
        PreBoosterDoubleStar,
        PreBoosterAddTimeWinstreak,
        Card_Randomx1 = 101,
        Card_Randomx2 = 102,
        Card_Randomx3 = 103,
        Card_Randomx4 = 104,
        Card_Randomx5 = 105,
        Card_Randomx6 = 106,
        X2ItemQuestEvent = 107,

        Avatar_Kitchen_Mission = 151,
        Avatar_Battle_Pass = 152,
        Badge_Kitchen_Mission = 153,
        Badge_Battle_Pass = 154,
        Avatar_Battle_Pass_Thanksgiving = 155,
        Badge_Battle_Pass_Thanksgiving = 156,

        Token_Rail,

        Avatar_Battle_Pass_Xmas,
        Badge_Battle_Pass_Xmas,

        Avatar_Pack_Xmas,
        Badge_Pack_Xmas,

        END,
    }

    public enum GameResourceType : byte
    {
        None = 0,
        Currency,
        Booster,
        PreBooster,
        Card,
        Avatar,
        Badge
    }

    public static class GameResourceHelper
    {
        public static GameResourceType ResourceType(this GameResource resource)
        {
            switch (resource)
            {
                case GameResource.Coin:
                case GameResource.Lives:
                case GameResource.Star:
                    return GameResourceType.Currency;
                case GameResource.BoosterFreeze:
                case GameResource.BoosterShuffle:
                case GameResource.BoosterMagnet:
                case GameResource.BoosterMagicKey:
                case GameResource.BoosterBlowTorch:
                    return GameResourceType.Booster;
                case GameResource.PreBoosterFreeze:
                case GameResource.PreBoosterMagnet:
                case GameResource.PreBoosterDoubleStar:
                case GameResource.PreBoosterAddTimeWinstreak:
                    return GameResourceType.PreBooster;
                case GameResource.Card_Randomx1:
                case GameResource.Card_Randomx2:
                case GameResource.Card_Randomx3:
                case GameResource.Card_Randomx4:
                case GameResource.Card_Randomx5:
                case GameResource.Card_Randomx6:
                    return GameResourceType.Card;

                case GameResource.Avatar_Kitchen_Mission:
                case GameResource.Avatar_Battle_Pass:
                case GameResource.Avatar_Battle_Pass_Thanksgiving:
                case GameResource.Avatar_Battle_Pass_Xmas:
                case GameResource.Avatar_Pack_Xmas:
                    return GameResourceType.Avatar;

                case GameResource.Badge_Kitchen_Mission:
                case GameResource.Badge_Battle_Pass:
                case GameResource.Badge_Battle_Pass_Thanksgiving:
                case GameResource.Badge_Battle_Pass_Xmas:
                case GameResource.Badge_Pack_Xmas:
                    return GameResourceType.Badge;
            }

            return GameResourceType.Currency;
        }

        public static bool HasLocalizeJapan(this GameResource resource)
        {
            if (!LocalizationUtils.IsJapanese()) return false;

            switch (resource)
            {
                case GameResource.Avatar_Battle_Pass:
                    return true;
            }

            return false;
        }
    }
}
#endif