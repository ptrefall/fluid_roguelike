using Fluid.Roguelike.Dungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonArea : MonoBehaviour
    {
        public DungeonTheme Theme { get; set; }
        public List<DungeonRoom> Rooms { get; } = new List<DungeonRoom>();
        public List<DungeonArea> Connections { get; } = new List<DungeonArea>();
        public Color DebugColor { get; set; }

        public void Add(DungeonRoom room)
        {
            room.transform.SetParent(transform, true);
            Rooms.Add(room);
        }

        public bool IsInArea(DungeonRoom room)
        {
            return Rooms.Contains(room);
        }

        public bool IsMetaConnectedTo(DungeonRoom room)
        {
            foreach(var r in Rooms)
            {
                foreach(var connection in r.Meta.Connections)
                {
                    if(connection.To.Id == room.Meta.Id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsConnectedTo(Dungeon dungeon, DungeonRoom room)
        {
            List<int2> closed = new List<int2>();
            foreach (var tile in room.ValueMap)
            {
                if (tile.Value.Index != DungeonTile.Index.Floor)
                    continue;

                foreach (var r in Rooms)
                {
                    foreach (var t in r.ValueMap)
                    {
                        if (t.Value.Index != DungeonTile.Index.Floor)
                            continue;

                        closed.Clear();
                        closed.Add(t.Key);
                        if (Floodfill(dungeon, Theme, t.Key, tile.Key, closed))
                            return true;

                        break;
                    }
                }
            }

            return false;
        }

        private bool Floodfill(Dungeon dungeon, DungeonTheme theme, int2 start, int2 end, List<int2> closed)
        {
            if (FloodFillInDirection(dungeon, theme, start, end, closed, BuilderDirection.North)) return true;
            if (FloodFillInDirection(dungeon, theme, start, end, closed, BuilderDirection.East)) return true;
            if (FloodFillInDirection(dungeon, theme, start, end, closed, BuilderDirection.South)) return true;
            if (FloodFillInDirection(dungeon, theme, start, end, closed, BuilderDirection.West)) return true;

            return false;
        }

        private bool FloodFillInDirection(Dungeon dungeon, DungeonTheme theme, int2 start, int2 end, List<int2> closed,
            BuilderDirection dir)
        {
            var key = DungeonRoom.GetAdjacentKey(dungeon, start, dir);
            if (key.x == end.x && key.y == end.y)
                return true;

            if (closed.Contains(key))
                return false;

            closed.Add(key);

            if (IsValid(dungeon, theme, key))
            {
                return Floodfill(dungeon, theme, key, end, closed);
            }

            return false;
        }

        private bool IsValid(Dungeon dungeon, DungeonTheme theme, int2 key)
        {
            if (dungeon.ValueMap.ContainsKey(key))
            {
                var tile = dungeon.ValueMap[key];
                if (tile.Theme == theme && tile.Index == DungeonTile.Index.Floor)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
