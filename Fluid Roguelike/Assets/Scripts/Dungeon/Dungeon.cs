
using System;
using System.Collections;
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.AI;
using Fluid.Roguelike.Database;
using Fluid.Roguelike.UI;
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
            public DungeonRoom Room { get; set; }
        }

        [SerializeField] private DungeonAgent _agent;
        [SerializeField] private DungeonRoom _dungeonRoomPrefab;
        [SerializeField] private Character.Character _characterPrefab;
        [SerializeField] private SpriteRenderer _itemViewPrefab;
        [SerializeField] private CharacterDatabaseManager _characterDb;
        [SerializeField] private ItemDatabaseManager _itemDb;
        [SerializeField] private UiManager _uiManager;

        private PlayerController _playerController;
        private readonly List<AIController> _aiControllers = new List<AIController>();
        private readonly List<Character.Character> _characters = new List<Character.Character>();

        public List<Character.Character> Characters => _characters;

        public Dictionary<int2, DungeonTile> Tiles { get; } = new Dictionary<int2, DungeonTile>();
        public Dictionary<int2, MapValue> ValueMap { get; } = new Dictionary<int2, MapValue>();
        public readonly List<DungeonRoom> Rooms = new List<DungeonRoom>();
        public readonly List<DungeonArea> Areas = new List<DungeonArea>();

        public ItemDatabaseManager ItemDb => _itemDb;
        public SpriteRenderer ItemViewPrefab => _itemViewPrefab;
        private readonly List<Item.Item> _worldItems = new List<Item.Item>();
        public List<Item.Item> WorldItems => _worldItems;

        private Queue<Color> _areaColors = new Queue<Color>();

        private void Start()
        {
            _areaColors.Enqueue(Color.red);
            _areaColors.Enqueue(Color.green);
            _areaColors.Enqueue(Color.blue);
            _areaColors.Enqueue(Color.yellow);

            _agent.Generate(0);
            foreach (var meta in _agent.Context.AllRooms)
            {
                var room = GameObject.Instantiate(_dungeonRoomPrefab);
                room.Init();
                room.SetMeta(meta);
                room.GenerateMapValues(this, 0);
                Rooms.Add(room);
            }

            foreach (var room in Rooms)
            {
                room.GenerateMapValues(this, 1);
            }

            var splitThemes = new List<DungeonTheme>
            {
                DungeonTheme.Forest,
            };

            SplitDisconnectedRooms(splitThemes);

            DungeonRoom.WallIn(this, DungeonTheme.Cave);
            DungeonRoom.WallIn(this, DungeonTheme.Forest);

            foreach (var room in Rooms)
            {
                var foundArea = false;
                foreach (var area in Areas)
                {
                    if (area.Theme == room.Meta.Theme && area.IsConnectedTo(this, room))
                    {
                        area.Add(room);
                        foundArea = true;
                    }
                }

                if (!foundArea)
                {
                    var area = new GameObject(room.Meta.Theme.ToString()).AddComponent<DungeonArea>();
                    area.Theme = room.Meta.Theme;
                    area.DebugColor = _areaColors.Count > 0 ? _areaColors.Dequeue() : Color.black;
                    area.Add(room);
                    Areas.Add(area);
                }
            }

            DungeonRoom.AddAreaConnections(this);

            Rooms[0].AddTilesForAllMapValues(this);

            StartCoroutine(SpawnContext());
        }

        private void SplitDisconnectedRooms(List<DungeonTheme> splitThemes)
        {
            List<int2> closed = new List<int2>();
            for (var i = 0; i < Rooms.Count; i++)
            {
                if (splitThemes.Contains(Rooms[i].Meta.Theme) == false)
                    continue;

                closed.Clear();
                List<int2> splitTiles = null;
                var room = Rooms[i];
                foreach (var tile in room.ValueMap)
                {
                    if (tile.Value.Index != DungeonTile.Index.Floor)
                        continue;

                    if (closed.Contains(tile.Key))
                        continue;

                    closed.Add(tile.Key);

                    foreach (var tile2 in room.ValueMap)
                    {
                        if (closed.Contains(tile2.Key))
                            continue;

                        if (tile2.Value.Index != DungeonTile.Index.Floor)
                            continue;

                        var connected = DungeonRoom.IsConnected(this, room.Meta.Theme, tile.Key, tile2.Key);
                        if (!connected)
                        {
                            if (splitTiles == null)
                                splitTiles = new List<int2>();

                            splitTiles.Add(tile2.Key);
                        }
                    }
                }

                if (splitTiles != null)
                {
                    var splitRoom = GameObject.Instantiate(_dungeonRoomPrefab);
                    splitRoom.Init();
                    splitRoom.SetMeta(room.Meta);
                    foreach (var key in splitTiles)
                    {
                        if (room.ValueMap.ContainsKey(key))
                        {
                            var tile = room.ValueMap[key];
                            if (tile != null)
                            {
                                room.ValueMap.Remove(key);
                                splitRoom.ValueMap.Add(key, tile);
                                tile.Room = splitRoom;
                            }
                        }
                    }
                    Rooms.Add(splitRoom);
                }
            }
        }

        private IEnumerator SpawnContext()
        {
            yield return null;
            yield return null;

            if (_agent.Context.PlayerSpawnMeta != null)
            {
                _playerController =
                    SpawnPlayer("human", "naked", _agent.Context.PlayerSpawnMeta); // player -> naked/warrior/etc
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

            foreach (var character in Characters)
            {
                character.TickTurn_Sensors();
            }

            _playerController?.UpdateVisibility(this);
            _playerController?.UpdateMap();
            _playerController?.UpdateInventory();
            _playerController?.UpdateScraps();
        }

        public IBumpTarget TryGetBumpTarget(int2 position, bool hitPlayer)
        {
            if (hitPlayer)
            {
                if (_playerController != null)
                {
                    var equality = (_playerController.Position == position);
                    if (equality.x && equality.y)
                    {
                        return _playerController.Character;
                    }
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
            var character = GameObject.Instantiate(_characterPrefab).Init();
            if (character.Context != null)
            {
                character.Context.Dungeon = this;
            }

            character.name = $"{race} {name}";

            if (_characterDb)
            {
                if (_characterDb.Find(race, name, out var data))
                {
                    character.Meta = data;
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

                    foreach (var item in character.Meta.Items)
                    {
                        character.GiveItem(item);
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
            var room = GetRoom(meta.SpawnRoom);
            if (room == null)
                return null;

            if (room.GetValidSpawnPosition(this, out var position) == false)
                return null;

            // TODO: Need to look up spawn position in room, that we ensure valid positions
            var character = Spawn(race, name, position, out var brain);
            if (character != null)
            {
                var controller = new PlayerController();
                controller.Set(_uiManager);
                controller.Set(character);

                return controller;
            }

            return null;
        }

        public AIController SpawnAi(DungeonSpawnNpcMeta meta)
        {
            var room = GetRoom(meta.SpawnRoom);
            if (room == null)
                return null;

            if (room.GetValidSpawnPosition(this, out var position) == false)
                return null;

            var character = Spawn(meta.Race, meta.Name, position, out var brain);
            if (character != null)
            {
                var controller = new AIController(brain);
                controller.Set(character);

                return controller;
            }

            return null;
        }

        public SpriteRenderer SpawnItemInWorld(Item.Item item, ItemDbEntry meta)
        {
            if (item.WorldView != null)
            {
                item.WorldView.gameObject.SetActive(true);
                return item.WorldView;
            }

            var itemView = Instantiate(_itemViewPrefab);
            itemView.name = meta.Name;
            itemView.sprite = meta.Sprite;
            itemView.color = meta.Color;
            return itemView;
        }

        public void DropItemIntoWorld(Item.Item item, int2 position)
        {
            item.Drop(position);

            if (_worldItems.Contains(item) == false)
            {
                _worldItems.Add(item);
            }
        }

        public void PickupItemFromWorld(Item.Item item)
        {
            item.Pickup();

            _worldItems.Remove(item);
        }

        public int Destroy(Item.Item item)
        {
            item.Destroy();
            _worldItems.Remove(item);

            foreach (var character in Characters)
            {
                if (character.Inventory.Contains(item))
                {
                    character.Inventory.Remove(item);
                    if (character.PrimaryWeapon == item)
                    {
                        character.SetPrimaryWeapon(null);
                    }

                    break;
                }
            }

            return item.Meta.ScrapsValue;
        }

        public void Destroy(Character.Character character)
        {
            character.Destroy();
            _characters.Remove(character);

            if (_playerController.Character == character)
            {
                _playerController.Unset(character);
            }
            else
            {
                foreach (var controller in _aiControllers)
                {
                    if (controller.Character == character)
                    {
                        _aiControllers.Remove(controller);
                        break;
                    }
                }
            }
        }

        public Item.Item GetItemAt(int2 position)
        {
            foreach (var item in _worldItems)
            {
                if (item.WorldPosition.x == position.x && item.WorldPosition.y == position.y)
                {
                    return item;
                }
            }

            return null;
        }

        public DungeonRoom GetRoom(DungeonRoomMeta meta)
        {
            foreach (var room in Rooms)
            {
                if (room.Meta.Id == meta.Id)
                {
                    return room;
                }
            }

            return null;
        }

        public DungeonRoom GetRoom(int2 position)
        {
            foreach (var room in Rooms)
            {
                if (room.ValueMap.ContainsKey(position))
                    return room;
            }

            return null;
        }

        public DungeonArea GetArea(DungeonRoom room)
        {
            foreach (var area in Areas)
            {
                if (area.IsInArea(room))
                {
                    return area;
                }
            }

            return null;
        }

        public int GetMinX()
        {
            var x = int.MaxValue;
            foreach (var p in ValueMap)
            {
                if (p.Key.x < x)
                {
                    x = p.Key.x;
                }
            }

            return x;
        }

        public int GetMaxX()
        {
            var x = int.MinValue;
            foreach (var p in ValueMap)
            {
                if (p.Key.x > x)
                {
                    x = p.Key.x;
                }
            }

            return x;
        }

        public int GetMinY()
        {
            var y = int.MaxValue;
            foreach (var p in ValueMap)
            {
                if (p.Key.y < y)
                {
                    y = p.Key.y;
                }
            }

            return y;
        }

        public int GetMaxY()
        {
            var y = int.MinValue;
            foreach (var p in ValueMap)
            {
                if (p.Key.y > y)
                {
                    y = p.Key.y;
                }
            }

            return y;
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
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;

                foreach (var value in ValueMap)
                {
                    if (value.Value.Room == null)
                    {
                        style.normal.textColor = Color.magenta;
                    }
                    else
                    {
                        var area = GetArea(value.Value.Room);
                        if (area == null)
                        {
                            style.normal.textColor = Color.cyan;
                        }
                        else
                        {
                            style.normal.textColor = area.DebugColor;
                        }
                    }

                    var text = $"{value.Value.Room?.Id ?? 0}:{((int) value.Value.Index)}{(value.Value.IsSpecial ? "*" : "")}";
                    UnityEditor.Handles.Label(new Vector3(value.Key.x, value.Key.y + 0.25f, 0), text, style);
                }
            }
#endif
        }
    }
}