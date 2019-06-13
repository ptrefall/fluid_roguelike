using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class Dungeon : MonoBehaviour
    {
        [SerializeField] private DungeonAgent _agent;
        [SerializeField] private DungeonRoom _dungeonRoomPrefab;

        public Dictionary<Tuple<int, int>, DungeonTile> Tiles { get; } = new Dictionary<Tuple<int, int>, DungeonTile>();

        public void Start()
        {
            _agent.Generate(0);
            foreach (var meta in _agent.Context.AllRooms)
            {
                var room = GameObject.Instantiate(_dungeonRoomPrefab);
                room.SetMeta(meta);
                room.GenerateTiles(this);
            }
        }
    }
}