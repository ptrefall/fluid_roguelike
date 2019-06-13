﻿using System.Collections.Generic;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonRoomMeta
    {
        private static int RoomIds = 0;
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public DungeonRoomShape Shape { get; set; }
        public DungeonTheme Theme { get; set; }
        public int Id { get; }

        public List<DungeonRoomConnectionMeta> Connections { get; } = new List<DungeonRoomConnectionMeta>();

        public DungeonRoomMeta()
        {
            Id = ++RoomIds;
        }
    }

    public class DungeonRoomConnectionMeta
    {
        public DungeonRoomMeta From { get; set; }
        public DungeonRoomMeta To { get; set; }
        public BuilderDirection Direction { get; }
    }
}