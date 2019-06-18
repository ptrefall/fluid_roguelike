using System;
using Fluid.Roguelike.Database;
using UnityEngine;

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

        public void GenerateTiles(Dungeon dungeon)
        {
            if (_meta == null)
                return;

            var halfWidth = _meta.Width / 2;
            var halfHeight = _meta.Height / 2;

            for(var dx = -halfWidth; dx <= halfWidth; dx++)
            {
                for(var dy = -halfHeight; dy <= halfHeight; dy++)
                {
                    var x = _meta.CenterX + dx;
                    var y = _meta.CenterY + dy;
                    var key = new Tuple<int, int>(x, y);
                    if (dungeon.Tiles.ContainsKey(key))
                    {
                        continue;
                    }

                    if (dx == -halfWidth || dx == halfWidth || dy == -halfHeight || dy == halfHeight)
                    {
                        if (dungeon.ValueMap.ContainsKey(key))
                        {
                            dungeon.ValueMap[key] = (int) DungeonTile.Index.Wall;
                        }
                        else
                        {
                            dungeon.ValueMap.Add(key, (int) DungeonTile.Index.Wall);
                        }
                    }
                    else
                    {
                        if (dungeon.ValueMap.ContainsKey(key))
                        {
                            dungeon.ValueMap[key] = (int)DungeonTile.Index.Floor;
                        }
                        else
                        {
                            dungeon.ValueMap.Add(key, (int)DungeonTile.Index.Floor);
                        }
                    }
                }
            }

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
                    if (value <= (int) DungeonTile.Index.Void)
                    {
                        continue;
                    }

                    var tile = GameObject.Instantiate(_tilePrefab, transform, true);
                    tile.transform.position = new Vector3(x, y, 0);
                    dungeon.Tiles.Add(key, tile);

                    SetSprite(tile.GroundLayer, (DungeonTile.Index) value, IsEdge(dungeon, key), IsBorder(dungeon, key));
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
            var sprite = _sprites.Find(_meta.Theme, tileIndex, isEdge, isBorder);
            tileView.sprite = sprite;
        }
    }
}