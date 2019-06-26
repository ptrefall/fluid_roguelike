using Fluid.Roguelike.Dungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonArea : MonoBehaviour
    {
        public DungeonTheme Theme { get; set; }
        public List<DungeonRoom> Rooms { get; } = new List<DungeonRoom>();
        public List<DungeonArea> Connections { get; } = new List<DungeonArea>();

        public void Add(DungeonRoom room)
        {
            room.transform.SetParent(transform, true);
            Rooms.Add(room);
        }

        public bool IsInArea(DungeonRoom room)
        {
            return Rooms.Contains(room);
        }

        public bool IsConnectedTo(DungeonRoom room)
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
    }
}
