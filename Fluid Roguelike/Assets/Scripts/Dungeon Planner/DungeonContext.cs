
using System.Collections;
using System.Collections.Generic;

namespace Fluid.Roguelike.Dungeon
{
    public partial class DungeonContext
    {
        public BuilderDirection CurrentBuilderDirection { get; set; }
        public DungeonTheme CurrentTheme { get; set; }
        public Stack<DungeonRoomMeta> RoomStack { get; set; } = new Stack<DungeonRoomMeta>();
        public List<DungeonRoomMeta> AllRooms { get; set; } = new List<DungeonRoomMeta>();
        public DungeonSpawnPlayerMeta PlayerSpawnMeta { get; set; }
    }
}