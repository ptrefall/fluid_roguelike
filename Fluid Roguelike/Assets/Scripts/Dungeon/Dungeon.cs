using System;
using System.Collections.Generic;
using Fluid.Roguelike.Database;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class Dungeon : MonoBehaviour
    {
        public class MapValue
        {
            public DungeonTile.Index Index { get; set; }
            public DungeonTheme Theme { get; set; }
            public bool IsSpecial { get; set; } = false;
        }

        [SerializeField] private DungeonAgent _agent;
        [SerializeField] private DungeonRoom _dungeonRoomPrefab;
        [SerializeField] private Character.Character _characterPrefab;
        [SerializeField] private CharacterDatabaseManager _characterDb;

        private PlayerController _playerController;

        public Dictionary<Tuple<int, int>, DungeonTile> Tiles { get; } = new Dictionary<Tuple<int, int>, DungeonTile>();
        public Dictionary<Tuple<int, int>, MapValue> ValueMap { get; } = new Dictionary<Tuple<int, int>, MapValue>();
        private List<DungeonRoom> _rooms = new List<DungeonRoom>();
        private List<DungeonArea> _areas = new List<DungeonArea>();

        private void Start()
        {
            _agent.Generate(0);
            foreach (var meta in _agent.Context.AllRooms)
            {
                var room = GameObject.Instantiate(_dungeonRoomPrefab);
                room.SetMeta(meta);
                room.GenerateMapValues(this, 0);
                _rooms.Add(room);

                var foundArea = false;
                foreach (var area in _areas)
                {
                    if (area.Theme == meta.Theme && area.IsConnectedTo(room))
                    {
                        area.Add(room);
                        foundArea = true;
                    }
                }

                if (!foundArea)
                {
                    var area = new GameObject(meta.Theme.ToString()).AddComponent<DungeonArea>();
                    area.Theme = meta.Theme;
                    area.Add(room);
                    _areas.Add(area);
                }
            }

            foreach (var room in _rooms)
            {
                room.GenerateMapValues(this, 1);
            }

            _rooms[0].WallIn(this, DungeonTheme.Cave);
            _rooms[0].AddTilesForAllMapValues(this);

            _playerController = new PlayerController();
            _playerController.Set(Spawn("human", "player"));

            var koboldTest = Spawn("kobold", "warrior");
        }

        public Character.Character Spawn(string race, string name)
        {
            var character = GameObject.Instantiate(_characterPrefab);
            if (_characterDb)
            {
                character.View.sprite = _characterDb.Find(race, name, out var playerColor);
                character.View.color = playerColor;
            }

            return character;
        }

        private void Update()
        {
            _playerController?.Tick(this);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (ValueMap != null)
            {
                foreach (var value in ValueMap)
                {
                    UnityEditor.Handles.Label(new Vector3(value.Key.Item1, value.Key.Item2 + 0.25f, 0), ((int)value.Value.Index).ToString());
                }
            }
#endif
        }
    }
}