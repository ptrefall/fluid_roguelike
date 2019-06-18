using System;
using Fluid.Roguelike.Database;
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

        public void SetMeta(DungeonRoomMeta meta)
        {
            _meta = meta;
            transform.position = new Vector3(_meta.CenterX, _meta.CenterY);
            if (_roomTile != null)
            {
                SetSprite(_roomTile, DungeonTile.Index.WorldMap, false, false);

                //_roomTile.size = new Vector2(_meta.Width, _meta.Height);
                //_roomTile.transform.localScale = new Vector3(_meta.Width, _meta.Height, 1);
            }
        }

        public void GenerateMapValues(Dungeon dungeon)
        {
            if (_meta == null)
                return;

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

            AddDecoration(dungeon);
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
                    var key = new Tuple<int, int>(x, y);

                    if (dungeon.ValueMap.ContainsKey(key))
                    {
                        if (dungeon.ValueMap[key] != (int) DungeonTile.Index.Wall)
                        {
                            dungeon.ValueMap[key] = (int) DungeonTile.Index.Floor;
                        }
                    }
                    else
                    {
                        dungeon.ValueMap.Add(key, (int) DungeonTile.Index.Floor);
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
                    var key = new Tuple<int, int>(x, y);

                    if (dungeon.ValueMap.ContainsKey(key))
                    {
                        if (dungeon.ValueMap[key] != (int)DungeonTile.Index.Wall)
                        {
                            dungeon.ValueMap[key] = (int)DungeonTile.Index.Floor;
                        }
                    }
                    else
                    {
                        dungeon.ValueMap.Add(key, (int)DungeonTile.Index.Floor);
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
                    var key = new Tuple<int, int>(x, y);

                    if (dungeon.ValueMap.ContainsKey(key) == false)
                        continue;

                    var value = dungeon.ValueMap[key];
                    if (value == (int) DungeonTile.Index.Wall)
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
        private bool TryConvertIndexAtoB(Dungeon dungeon, Tuple<int, int> key, BuilderDirection direction, DungeonTile.Index a, DungeonTile.Index b, int maxCount)
        {
            var count = 0;
            CountIndexInDirection(dungeon, key, direction, a, ref count);
            count = Mathf.Min(count, maxCount);
            var chance = Mathf.Max((count / (float)maxCount) - 0.1f, 0);
            if (Random.value < chance)
            {
                dungeon.ValueMap[key] = (int) b;
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
                    var globalKey = new Tuple<int, int>(kvp.Key.Item1 + _meta.CenterX, kvp.Key.Item2 + _meta.CenterY);
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

        private void CountIndexInDirection(Dungeon dungeon, Tuple<int, int> key, BuilderDirection direction, DungeonTile.Index indexType, ref int count)
        {
            if (dungeon.ValueMap.ContainsKey(key) == false)
            {
                return;
            }

            var index = (DungeonTile.Index) dungeon.ValueMap[key];
            if (index != indexType)
                return;

            count++;

            var adjacentKey = key;
            switch (direction)
            {
                case BuilderDirection.North:
                    adjacentKey = new Tuple<int, int>(key.Item1, key.Item2 - 1);
                    break;
                case BuilderDirection.East:
                    adjacentKey = new Tuple<int, int>(key.Item1 + 1, key.Item2);
                    break;
                case BuilderDirection.South:
                    adjacentKey = new Tuple<int, int>(key.Item1, key.Item2 + 1);
                    break;
                case BuilderDirection.West:
                    adjacentKey = new Tuple<int, int>(key.Item1 - 1, key.Item2);
                    break;
            }

            if (adjacentKey.Item1 == key.Item1 && adjacentKey.Item2 == key.Item2)
                return;

            CountIndexInDirection(dungeon, adjacentKey, direction, indexType, ref count);
        }

        private DungeonTile.Index GetAdjacentIndex(Dungeon dungeon, Tuple<int, int> key, BuilderDirection direction)
        {
            var adjacentKey = key;
            switch (direction)
            {
                case BuilderDirection.North:
                    adjacentKey = new Tuple<int, int>(key.Item1, key.Item2 - 1);
                    break;
                case BuilderDirection.East:
                    adjacentKey = new Tuple<int, int>(key.Item1 + 1, key.Item2);
                    break;
                case BuilderDirection.South:
                    adjacentKey = new Tuple<int, int>(key.Item1, key.Item2 + 1);
                    break;
                case BuilderDirection.West:
                    adjacentKey = new Tuple<int, int>(key.Item1 - 1, key.Item2);
                    break;
            }
            if (dungeon.ValueMap.ContainsKey(adjacentKey))
                return (DungeonTile.Index)dungeon.ValueMap[adjacentKey];

            return DungeonTile.Index.Void;
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
                    var key = new System.Tuple<int, int>(x, y);

                    if (dungeon.Tiles.ContainsKey(key))
                    {
                        continue;
                    }
                    if (dungeon.ValueMap.ContainsKey(key) == false)
                    {
                        continue;
                    }

                    var value = dungeon.ValueMap[key];
                    if (value <= (int)DungeonTile.Index.Void)
                    {
                        continue;
                    }

                    var tile = GameObject.Instantiate(_tilePrefab, transform, true);
                    tile.transform.position = new Vector3(x, y, 0);
                    dungeon.Tiles.Add(key, tile);

                    SetSprite(tile.GroundLayer, (DungeonTile.Index)value, IsEdge(dungeon, key), IsBorder(dungeon, key));
                }
            }
        }

        private bool IsEdge(Dungeon dungeon, Tuple<int, int> key)
        { 
            if (dungeon.ValueMap.ContainsKey(key) == false)
                return false;

            var value = dungeon.ValueMap[key];
            var southKey = new Tuple<int, int>(key.Item1, key.Item2 + 1);
            if (dungeon.ValueMap.ContainsKey(southKey) == false)
                return true;

            var southValue = dungeon.ValueMap[southKey];
            if (southValue != value)
                return true;

            return false;
        }

        private bool IsBorder(Dungeon dungeon, Tuple<int, int> key)
        {
            if (dungeon.ValueMap.ContainsKey(key) == false)
                return false;

            var value = dungeon.ValueMap[key];
            var  northKey = new Tuple<int, int>(key.Item1, key.Item2 - 1);
            if (dungeon.ValueMap.ContainsKey(northKey) == false)
                return true;

            var northValue = dungeon.ValueMap[northKey];
            if (northValue != value)
                return true;

            return false;
        }

        private void SetSprite(SpriteRenderer tileView, DungeonTile.Index tileIndex, bool isEdge, bool isBorder)
        {
            Color color;
            var sprite = _sprites.Find(_meta.Theme, tileIndex, isEdge, isBorder, out color);
            tileView.sprite = sprite;
            tileView.color = color;
        }
    }
}