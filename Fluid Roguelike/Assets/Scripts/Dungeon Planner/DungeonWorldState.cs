
namespace Fluid.Roguelike.Dungeon
{
    public enum DungeonWorldState
    {
        DungeonDepth,
        PlayerNeedsSpawnPoint,
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
    }
}