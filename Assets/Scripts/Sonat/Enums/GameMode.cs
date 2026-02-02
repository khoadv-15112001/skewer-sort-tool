#if CUSTOM_ENUM
namespace Sonat.Enums
{
    public enum GameMode : byte
    {
        Classic,
        SkewerJam,
        ClassicJapan
    }

    public enum LevelMode : byte
    {
        All,
        Target,
        MAX
    }

    public enum GameState : byte
    {
        Loading = 0,
        Playing,
        Paused,
        GameOver,
        Tool,
        UsingBooster
    }

    public enum GamePlacement : byte
    {
        None = 0,
        Loading,
        Gameplay,
        Home,
        Tool,
        PreLoading,
        Gameplay_SkewerJam
    }

    public enum NavigationType : byte
    {
        None = 0,
        Home = 1,
        Shop = 2,
        Story = 3,
        Leaderboard = 4,
        Collection = 5,
        Team = 6,

        Profile_Avatar = 11,
        Profile_Frame = 12,
        Profile_Badge = 13,

        Leaderboard_Weekly = 21,
        Leaderboard_Friends = 22,
        Leaderboard_Players = 23,
        Leaderboard_Teams = 24,
        Leaderboard_PLayers_World = 25,
        Leaderboard_PLayers_Local = 26,
        Leaderboard_PLayers_League = 27,
        Leaderboard_Teams_World = 28,
        Leaderboard_Teams_Local = 29,

        Team_Join = 41,
        Team_Search = 42,
        Team_Create = 43,
    }

    public enum StuckType : byte
    {
        OutOfTime = 0,
        OutOfMove = 1,
        OutOfTimeShipper = 3,
        OutOfMoveShipper = 4,
        SkewerJam_OutOfSpace = 5,
        SkewerJam_OutOfEnergy = 6,
        SkewerJam_BombExplosion = 7,
        OutOfItemOrder = 8,
    }

    public enum LevelDifficulty : byte
    {
        Easy = 0,
        Medium = 1,
        Hard = 2,
        SuperHard = 3,
        NightmarishlyHard = 4,
        MAX
    }

    public enum LevelType : byte
    {
        Food = 0,
        Fruit,
        Cake,
        All,
        MAX = 100
    }
}
#endif