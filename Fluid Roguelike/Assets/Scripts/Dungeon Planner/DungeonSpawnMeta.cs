namespace Fluid.Roguelike.Dungeon
{
    public class DungeonSpawnMeta
    {
        public DungeonRoomMeta SpawnRoom { get; set; }
    }

    public class DungeonSpawnNpcMeta : DungeonSpawnMeta
    {
        public string Race;
        public string Name;
        public int Count = 1;
    }
}