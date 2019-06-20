using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.AI;
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
        private List<AIController> _aiControllers = new List<AIController>();

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

            StartCoroutine(SpawnContext());
        }

        private IEnumerator SpawnContext()
        {
            yield return null;
            yield return null;

            if (_agent.Context.PlayerSpawnMeta != null)
            {
                _playerController =
                    SpawnPlayer("human", "player", _agent.Context.PlayerSpawnMeta); // player -> naked/warrior/etc
            }

            foreach (var npcMeta in _agent.Context.NpcSpawnMeta)
            {
                for (var i = 0; i < npcMeta.Count; i++)
                {
                    _aiControllers.Add(SpawnAi(npcMeta));
                }
            }
        }

        public IBumpTarget TryGetBumpTarget(Tuple<int, int> position, bool hitPlayer)
        {
            if (hitPlayer)
            {
                if (_playerController.Position.Item1 == position.Item1 &&
                    _playerController.Position.Item2 == position.Item2)
                {
                    return _playerController;
                }
            }

            foreach (var ai in _aiControllers)
            {
                if (ai.Position.Item1 == position.Item1 &&
                    ai.Position.Item2 == position.Item2)
                {
                    return ai;
                }
            }

            return null;
        }

        public Character.Character Spawn(string race, string name, Vector3 position, out CharacterDomainDefinition brain)
        {
            var character = GameObject.Instantiate(_characterPrefab);
            if (_characterDb)
            {
                character.View.sprite = _characterDb.Find(race, name, out var playerColor, out brain);
                character.View.color = playerColor;
            }
            else
            {
                brain = null;
            }

            character.transform.position = position;

            return character;
        }

        public PlayerController SpawnPlayer(string race, string name, DungeonSpawnMeta meta)
        {
            // TODO: Need to look up spawn position in room, that we ensure valid positions
            var pos = new Vector3(meta.SpawnRoom.CenterX, meta.SpawnRoom.CenterY, 0);
            var character = Spawn(race, name, pos, out var brain);

            var cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (cameraBrain != null && cameraBrain.ActiveVirtualCamera != null)
            {
                cameraBrain.ActiveVirtualCamera.LookAt = character.transform;
                cameraBrain.ActiveVirtualCamera.Follow = character.transform;
            }

            var controller = new PlayerController();
            controller.Set(character);
            return controller;
        }

        public AIController SpawnAi(DungeonSpawnNpcMeta meta)
        {
            // TODO: Need to look up spawn position in room, that we ensure valid positions
            var room = GetRoom(meta.SpawnRoom);
            if (room == null)
                return null;

            var key = room.GetValidSpawnPosition(this);
            if (key == null)
                return null;

            var pos = new Vector3(key.Item1, key.Item2, 0);
            var character = Spawn(meta.Race, meta.Name, pos, out var brain);

            var controller = new AIController(brain);
            controller.Set(character);
            return controller;
        }

        public DungeonRoom GetRoom(DungeonRoomMeta meta)
        {
            foreach (var room in _rooms)
            {
                if (room.Meta.Id == meta.Id)
                {
                    return room;
                }
            }

            return null;
        }

        private void Update()
        {
            _playerController?.Tick(this);
        }

        public void TickAI()
        {
            foreach (var ai in _aiControllers)
            {
                ai.Tick(this);
            }
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