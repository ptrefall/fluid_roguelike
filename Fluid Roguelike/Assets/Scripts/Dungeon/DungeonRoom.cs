using System;
using System.Collections.Generic;
using Fluid.Roguelike.Database;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonRoom : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _roomTile;
        [SerializeField] private DungeonTile _tilePrefab;
        [SerializeField] private SpriteDatabaseManager _sprites;
        private DungeonRoomMeta _meta;

        public DungeonRoomMeta Meta => _meta;

        public void SetMeta(DungeonRoomMeta meta)
        {
            _meta = meta;
            transform.position = new Vector3(_meta.CenterX, _meta.CenterY);
            if (_roomTile != null)
            {
                SetSprite(_roomTile, new Dungeon.MapValue { Theme = _meta.Theme, Index = DungeonTile.Index.WorldMap  }, false, false);

                //_roomTile.size = new Vector2(_meta.Width, _meta.Height);
                //_roomTile.transform.localScale = new Vector3(_meta.Width, _meta.Height, 1);
            }
        }

        public bool GetValidSpawnPosition(Dungeon dungeon, out int2 position)
        {
            var halfWidth = _meta.Width / 2;
            var halfHeight = _meta.Height / 2;

            var validPositions = new List<int2>();

            for (var dx = -halfWidth; dx <= halfWidth; dx++)
            {
                for (var dy = -halfHeight; dy <= halfHeight; dy++)
                {
                    var x = _meta.CenterX + dx;
                    var y = _meta.CenterY + dy;
                    var key = new int2(x, y);

                    if (dungeon.ValueMap.ContainsKey(key))
                    {
                        if (dungeon.ValueMap[key].Index == DungeonTile.Index.Floor)
                        {
                            var collision = dungeon.TryGetBumpTarget(key, true);
                            if (collision == null)
                            {
                                validPositions.Add(key);
                            }
                        }
                    }
                }
            }

            if (validPositions.Count > 1)
            {
                position = validPositions[Random.Range(0, validPositions.Count)];
                return true;
            }

            if (validPositions.Count == 1)
            {
                position = validPositions[0];
                return true;
            }

            position = int2.zero;
            return false;
        }

        public void GenerateMapValues(Dungeon dungeon, int pass)
        {
            if (_meta == null)
                return;

            if (pass == 0)
            {
                if (_meta.Shape == DungeonRoomShape.Rectangular)
                {
                    AddRectangularFloor(dungeon);
                }
                else
                {
                    AddCircularFloor(dungeon);
                }

                switch (_meta.Theme)
                {
                    case DungeonTheme.Cave:
                        AddCaveFeatures(dungeon, true, true, _meta.MaxModifications);
                        AddCaveFeatures(dungeon, false, false, _meta.MaxModifications);
                        break;
                }
            }
            else if (pass == 1)
            {
                AddDecoration(dungeon);
            }
        }

        public void GenerateTiles(Dungeon dungeon)
        {
            if (_meta == null)
                return;

            AddTiles(dungeon);
        }

        private void AddRectangularFloor(Dungeon dungeon)
        {
            var halfWidth = _meta.Width / 2;
            var halfHeight = _meta.Height / 2;

            for (var dx = -halfWidth; dx <= halfWidth; dx++)
            {
                for (var dy = -halfHeight; dy <= halfHeight; dy++)
                {
                    var x = _meta.CenterX + dx;
                    var y = _meta.CenterY + dy;
                    var key = new int2(x, y);

                    if (dungeon.ValueMap.ContainsKey(key))
                    {
                        if (dungeon.ValueMap[key].Index != DungeonTile.Index.Wall)
                        {
                            dungeon.ValueMap[key].Theme = _meta.Theme;
                            dungeon.ValueMap[key].Index = DungeonTile.Index.Floor;
                        }
                    }
                    else
                    {
                        dungeon.ValueMap.Add(key, new Dungeon.MapValue
                        {
                            Theme = _meta.Theme,
                            Index = DungeonTile.Index.Floor
                        });
                    }
                }
            }
        }

        private void AddCircularFloor(Dungeon dungeon)
        {
            var halfWidth = _meta.Width / 2;
            var halfHeight = _meta.Height / 2;

            for (var dx = -halfWidth; dx <= halfWidth; dx++)
            {
                for (var dy = -halfHeight; dy <= halfHeight; dy++)
                {
                    var x = _meta.CenterX + dx;
                    var y = _meta.CenterY + dy;
                    var key = new int2(x, y);

                    if (dungeon.ValueMap.ContainsKey(key))
                    {
                        if (dungeon.ValueMap[key].Index != DungeonTile.Index.Wall)
                        {
                            dungeon.ValueMap[key].Theme = _meta.Theme;
                            dungeon.ValueMap[key].Index = DungeonTile.Index.Floor;
                        }
                    }
                    else
                    {
                        dungeon.ValueMap.Add(key, new Dungeon.MapValue
                        {
                            Theme = _meta.Theme,
                            Index = DungeonTile.Index.Floor
                        });
                    }
                }
            }
        }

        private int Rad(int width, int height)
        {
            return (int)(((width * width) / (8f * height) + height / 2f));
        }

        private void AddCaveFeatures(Dungeon dungeon, bool leftToRight, bool upToDown, int maxModifications)
        {
            if (maxModifications <= 0)
                return;

            var halfWidth = _meta.Width / 2;
            var halfHeight = _meta.Height / 2;

            int modifications = 0;
            var startIndexW = leftToRight ? -halfWidth : halfWidth;
            var endIndexW = leftToRight ? halfWidth : -halfWidth;
            var incrementW = leftToRight ? 1 : -1;
            var startIndexH = upToDown ? -halfHeight : halfHeight;
            var endIndexH = upToDown ? halfHeight : -halfHeight;
            var incrementH = upToDown ? 1 : -1;
            for (var dx = startIndexW; dx <= endIndexW; dx += incrementW)
            {
                for (var dy = startIndexH; dy <= endIndexH; dy += incrementH)
                {
                    var x = _meta.CenterX + dx;
                    var y = _meta.CenterY + dy;
                    var key = new int2(x, y);

                    if (dungeon.ValueMap.ContainsKey(key) == false)
                        continue;

                    var value = dungeon.ValueMap[key];
                    if (value.Index == DungeonTile.Index.Wall)
                    {
                        if (GetAdjacentIndex(dungeon, key, BuilderDirection.East) == DungeonTile.Index.Wall &&
                            GetAdjacentIndex(dungeon, key, BuilderDirection.West) == DungeonTile.Index.Floor)
                        {
                            if (TryConvertIndexAtoB(dungeon, key, BuilderDirection.West, DungeonTile.Index.Floor,
                                DungeonTile.Index.Wall, 10))
                            {
                                modifications++;
                            }
                        }
                        else if (GetAdjacentIndex(dungeon, key, BuilderDirection.East) == DungeonTile.Index.Floor &&
                                 GetAdjacentIndex(dungeon, key, BuilderDirection.West) == DungeonTile.Index.Wall)
                        {
                            if (TryConvertIndexAtoB(dungeon, key, BuilderDirection.East, DungeonTile.Index.Floor,
                                DungeonTile.Index.Wall, 10))
                            {
                                modifications++;
                            }
                        }

                        if (GetAdjacentIndex(dungeon, key, BuilderDirection.North) == DungeonTile.Index.Wall &&
                                 GetAdjacentIndex(dungeon, key, BuilderDirection.South) == DungeonTile.Index.Floor)
                        {
                            if (TryConvertIndexAtoB(dungeon, key, BuilderDirection.South, DungeonTile.Index.Floor,
                                DungeonTile.Index.Wall, 10))
                            {
                                modifications++;
                            }
                        }
                        else if (GetAdjacentIndex(dungeon, key, BuilderDirection.North) == DungeonTile.Index.Floor &&
                                 GetAdjacentIndex(dungeon, key, BuilderDirection.South) == DungeonTile.Index.Wall)
                        {
                            if (TryConvertIndexAtoB(dungeon, key, BuilderDirection.North, DungeonTile.Index.Floor,
                                DungeonTile.Index.Wall, 10))
                            {
                                modifications++;
                            }
                        }
                    }

                    if (modifications >= maxModifications)
                        return;
                }
            }
        }

        /// <summary>
        /// The higher the count of tiles in a direction with the same index, the higher the chance is to covert
        private bool TryConvertIndexAtoB(Dungeon dungeon, int2 key, BuilderDirection direction, DungeonTile.Index a, DungeonTile.Index b, int maxCount)
        {
            var count = 0;
            CountIndexInDirection(dungeon, key, direction, a, ref count);
            count = Mathf.Min(count, maxCount);
            var chance = Mathf.Max((count / (float)maxCount) - 0.1f, 0);
            if (Random.value < chance)
            {
                dungeon.ValueMap[key].Theme = _meta.Theme;
                dungeon.ValueMap[key].Index = b;
                return true;
            }

            return false;
        }

        private void AddDecoration(Dungeon dungeon)
        {
            foreach (var decorator in _meta.DecorationMeta)
            {
                foreach (var kvp in decorator.ValueMap)
                {
                    var globalKey = new int2(kvp.Key.x + _meta.CenterX, kvp.Key.y + _meta.CenterY);
                    if (dungeon.ValueMap.ContainsKey(globalKey))
                    {
                        dungeon.ValueMap[globalKey] = kvp.Value;
                    }
                    else
                    {
                        dungeon.ValueMap.Add(globalKey, kvp.Value);
                    }
                }
            }
        }

        private void CountIndexInDirection(Dungeon dungeon, int2 key, BuilderDirection direction, DungeonTile.Index indexType, ref int count)
        {
            if (dungeon.ValueMap.ContainsKey(key) == false)
            {
                return;
            }

            var index = dungeon.ValueMap[key].Index;
            if (index != indexType)
                return;

            count++;

            var adjacentKey = key;
            switch (direction)
            {
                case BuilderDirection.North:
                    adjacentKey = new int2(key.x, key.y - 1);
                    break;
                case BuilderDirection.East:
                    adjacentKey = new int2(key.x + 1, key.y);
                    break;
                case BuilderDirection.South:
                    adjacentKey = new int2(key.x, key.y + 1);
                    break;
                case BuilderDirection.West:
                    adjacentKey = new int2(key.x - 1, key.y);
                    break;
            }

            if (adjacentKey.x == key.x && adjacentKey.y == key.y)
                return;

            CountIndexInDirection(dungeon, adjacentKey, direction, indexType, ref count);
        }

        private DungeonTile.Index GetAdjacentIndex(Dungeon dungeon, int2 key, BuilderDirection direction)
        {
            var adjacentKey = key;
            switch (direction)
            {
                case BuilderDirection.North:
                    adjacentKey = new int2(key.x, key.y - 1);
                    break;
                case BuilderDirection.East:
                    adjacentKey = new int2(key.x + 1, key.y);
                    break;
                case BuilderDirection.South:
                    adjacentKey = new int2(key.x, key.y + 1);
                    break;
                case BuilderDirection.West:
                    adjacentKey = new int2(key.x - 1, key.y);
                    break;
            }
            if (dungeon.ValueMap.ContainsKey(adjacentKey))
                return dungeon.ValueMap[adjacentKey].Index;

            return DungeonTile.Index.Void;
        }

        private int2 GetAdjacentKey(Dungeon dungeon, int2 key, BuilderDirection direction)
        {
            var adjacentKey = key;
            switch (direction)
            {
                case BuilderDirection.North:
                    adjacentKey = new int2(key.x, key.y - 1);
                    break;
                case BuilderDirection.East:
                    adjacentKey = new int2(key.x + 1, key.y);
                    break;
                case BuilderDirection.South:
                    adjacentKey = new int2(key.x, key.y + 1);
                    break;
                case BuilderDirection.West:
                    adjacentKey = new int2(key.x - 1, key.y);
                    break;
            }
            return adjacentKey;
        }

        private void AddTiles(Dungeon dungeon)
        {
            var halfWidth = _meta.Width / 2;
            var halfHeight = _meta.Height / 2;

            for (var dx = -halfWidth; dx < halfWidth; dx++)
            {
                for (var dy = -halfHeight; dy < halfHeight; dy++)
                {
                    var x = _meta.CenterX + dx;
                    var y = _meta.CenterY + dy;
                    var key = new int2(x, y);

                    if (dungeon.Tiles.ContainsKey(key))
                    {
                        continue;
                    }
                    if (dungeon.ValueMap.ContainsKey(key) == false)
                    {
                        continue;
                    }

                    var value = dungeon.ValueMap[key];
                    if (value.Index == (int)DungeonTile.Index.Void)
                    {
                        continue;
                    }

                    var tile = GameObject.Instantiate(_tilePrefab, transform, true);
                    tile.transform.position = new Vector3(x, y, 0);
                    dungeon.Tiles.Add(key, tile);

                    SetSprite(tile.GroundLayer, value, IsEdge(dungeon, key), IsBorder(dungeon, key));
                }
            }
        }

        public void WallIn(Dungeon dungeon, DungeonTheme theme)
        {
            var map = new Dictionary<int2, Dungeon.MapValue>(dungeon.ValueMap);
            foreach (var kvp in map)
            {
                TryWallIn(dungeon, theme, kvp.Key, BuilderDirection.North);
                TryWallIn(dungeon, theme, kvp.Key, BuilderDirection.East);
                TryWallIn(dungeon, theme, kvp.Key, BuilderDirection.South);
                TryWallIn(dungeon, theme, kvp.Key, BuilderDirection.West);
            }
        }

        private void TryWallIn(Dungeon dungeon, DungeonTheme theme, int2 key, BuilderDirection direction)
        {
            var value = dungeon.ValueMap[key];
            if (value.Theme != theme)
                return;

            var adjacentKey = GetAdjacentKey(dungeon, key, direction);
            if (dungeon.ValueMap.ContainsKey(adjacentKey) == false)
            {
                dungeon.ValueMap.Add(adjacentKey, new Dungeon.MapValue
                {
                    Theme = theme,
                    Index = DungeonTile.Index.Wall,
                });
            }

            var adjacent = dungeon.ValueMap[adjacentKey];
            if (adjacent.Theme != theme)
            {
                if (Random.value < 0.9f)
                {
                    dungeon.ValueMap[key].Index = DungeonTile.Index.Wall;
                }
            }
        }

        public void AddTilesForAllMapValues(Dungeon dungeon)
        {
            var tiles = new GameObject("Tiles");
            foreach(var kvp in dungeon.ValueMap)
            {
                var tile = GameObject.Instantiate(_tilePrefab, tiles.transform, true);
                tile.transform.position = new Vector3(kvp.Key.x, kvp.Key.y, 0);
                dungeon.Tiles.Add(kvp.Key, tile);

                SetSprite(tile.GroundLayer, kvp.Value, IsEdge(dungeon, kvp.Key), IsBorder(dungeon, kvp.Key));
            }
        }

        private bool IsEdge(Dungeon dungeon, int2 key)
        { 
            if (dungeon.ValueMap.ContainsKey(key) == false)
                return false;

            var value = dungeon.ValueMap[key];
            var southKey = new int2(key.x, key.y + 1);
            if (dungeon.ValueMap.ContainsKey(southKey) == false)
                return true;

            var southValue = dungeon.ValueMap[southKey];
            if (southValue != value)
                return true;

            return false;
        }

        private bool IsBorder(Dungeon dungeon, int2 key)
        {
            if (dungeon.ValueMap.ContainsKey(key) == false)
                return false;

            var value = dungeon.ValueMap[key];
            var  northKey = new int2(key.x, key.y - 1);
            if (dungeon.ValueMap.ContainsKey(northKey) == false)
                return true;

            var northValue = dungeon.ValueMap[northKey];
            if (northValue != value)
                return true;

            return false;
        }

        private void SetSprite(SpriteRenderer tileView, Dungeon.MapValue value, bool isEdge, bool isBorder)
        {
            Color color;
            var sprite = _sprites.Find(value.Theme, value.Index, isEdge, isBorder, out color);
            tileView.sprite = sprite;
            tileView.color = color;
        }
    }
}