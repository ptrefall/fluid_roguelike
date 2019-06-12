
namespace Fluid.Roguelike.Dungeon
{
    public partial class DungeonContext
    {
        public BuilderDirection CurrentBuilderDirection { get; set; }
        public DungeonTheme CurrentTheme { get; set; }
        public DungeonRoomMeta LastRoomMeta { get; set; }
        public DungeonSpawnPlayerMeta PlayerSpawnMeta { get; set; }
    }
}