
namespace Fluid.Roguelike.Dungeon
{
    public enum DungeonWorldState
    {
        DungeonDepth,
        PlayerNeedsSpawnPoint,
        RepeatCount,
    }

    public enum DungeonRoomShape
    {
        Random,
        Rectangular,
        Round,
    }

    public enum DungeonTheme
    {
        Forest,
        Fortress,
        House,
        Road,
        Marsh,
        Cave,
    }

    public enum BuilderDirection
    {
        Random,
        North,
        East,
        South,
        West,
        Portal,
    }
}