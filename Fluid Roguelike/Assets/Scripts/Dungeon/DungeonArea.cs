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
        public List<DungeonRoom> _rooms { get; } = new List<DungeonRoom>();

        public void Add(DungeonRoom room)
        {
            room.transform.SetParent(transform, true);
            _rooms.Add(room);
        }

        public bool IsInArea(DungeonRoom room)
        {
            return _rooms.Contains(room);
        }

        public bool IsConnectedTo(DungeonRoom room)
        {
            foreach(var r in _rooms)
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
