using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.AI;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Database;
using Unity.Mathematics;
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
        private readonly List<AIController> _aiControllers = new List<AIController>();
        private readonly List<Character.Character> _characters = new List<Character.Character>();

        public List<Character.Character> Characters => _characters;

        public Dictionary<int2, DungeonTile> Tiles { get; } = new Dictionary<int2, DungeonTile>();
        public Dictionary<int2, MapValue> ValueMap { get; } = new Dictionary<int2, MapValue>();
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
                    var ai = SpawnAi(npcMeta);
                    if (ai != null)
                    {
                        _aiControllers.Add(ai);
                    }
                }
            }
        }

        public IBumpTarget TryGetBumpTarget(int2 position, bool hitPlayer)
        {
            if (hitPlayer)
            {
                var equality = (_playerController.Position == position);
                if (equality.x && equality.y)
                {
                    return _playerController.Character;
                }
            }

            foreach (var ai in _aiControllers)
            {
                var equality = (ai.Position == position);
                if (equality.x && equality.y)
                {
                    return ai.Character;
                }
            }

            return null;
        }

        public Character.Character Spawn(string race, string name, int2 position, out CharacterDomainDefinition brain)
        {
            brain = null;
            var character = GameObject.Instantiate(_characterPrefab);
            if (character.Context != null)
            {
                character.Context.Dungeon = this;
            }

            if (_characterDb)
            {
                if (_characterDb.Find(race, name, out var data))
                {
                    character.View.sprite = data.Sprite;
                    character.View.color = data.Color;
                    foreach (var sensor in data.Sensors)
                    {
                        character.AddSensor(sensor);
                    }

                    foreach (var stat in data.Stats)
                    {
                        character.AddStat(stat.Type, stat.Value);
                    }

                    brain = data.Brain;
                }
            }

            character.transform.position = new Vector3(position.x, position.y, 0);
            _characters.Add(character);
            return character;
        }

        public PlayerController SpawnPlayer(string race, string name, DungeonSpawnMeta meta)
        {
            // TODO: Need to look up spawn position in room, that we ensure valid positions
            var character = Spawn(race, name, new int2(meta.SpawnRoom.CenterX, meta.SpawnRoom.CenterY), out var brain);

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
            var room = GetRoom(meta.SpawnRoom);
            if (room == null)
                return null;

            if (room.GetValidSpawnPosition(this, out var position) == false)
                return null;

            var character = Spawn(meta.Race, meta.Name, position, out var brain);

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
                    UnityEditor.Handles.Label(new Vector3(value.Key.x, value.Key.y + 0.25f, 0), ((int)value.Value.Index).ToString());
                }
            }
#endif
        }
    }
}