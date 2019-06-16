using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonRoom : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _roomTile;
        [SerializeField] private DungeonTile _tilePrefab;
        [SerializeField] private Sprite _caveSprite;
        [SerializeField] private Sprite _forestSprite;
        [SerializeField] private Sprite _noneSprite;
        private DungeonRoomMeta _meta;

        public void SetMeta(DungeonRoomMeta meta)
        {
            _meta = meta;
            transform.position = new Vector3(_meta.CenterX, _meta.CenterY);
            if (_roomTile != null)
            {
                SetSprite(_roomTile);

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
                    if (dx == -halfWidth || dx == halfWidth || dy == -halfHeight || dy == halfHeight)
                        continue;

                    var x = _meta.CenterX + dx;
                    var y = _meta.CenterY + dy;
                    var key = new System.Tuple<int, int>(x, y);
                    if (dungeon.Tiles.ContainsKey(key))
                    {
                        continue;
                    }

                    if (dungeon.ValueMap.ContainsKey(key))
                    {
                        if (dungeon.ValueMap[key] == 1)
                        {
                            if(Random.value < 0.25f)
                            {
                                continue;
                            }
                        }
                        else if (dungeon.ValueMap[key] < 1)
                        {
                            if (Random.value < 0.5f)
                            {
                                continue;
                            }
                        }
                        else if (dungeon.ValueMap[key] > 1)
                        {
                            if (Random.value < 0.05f)
                            {
                                continue;
                            }
                        }

                        dungeon.ValueMap[key] += Random.Range(2, 4);
                    }
                    else
                    {
                        if (Random.value < 0.05f)
                        {
                            continue;
                        }

                        dungeon.ValueMap.Add(key, Random.Range(2, 4));
                    }
                    NeighbourValueIncrement(dungeon, key, BuilderDirection.North, Random.Range(0, 3));
                    NeighbourValueIncrement(dungeon, key, BuilderDirection.East, Random.Range(0, 3));
                    NeighbourValueIncrement(dungeon, key, BuilderDirection.South, Random.Range(0, 3));
                    NeighbourValueIncrement(dungeon, key, BuilderDirection.West, Random.Range(0, 3));
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
                    if (value <= 0)
                    {
                        continue;
                    }

                    if (value > 3 || Random.value > 0.05f * value)
                    {
                        NeighbourValueIncrement(dungeon, key, BuilderDirection.North, -Random.Range(0, 3));
                        NeighbourValueIncrement(dungeon, key, BuilderDirection.East, -Random.Range(0, 3));
                        NeighbourValueIncrement(dungeon, key, BuilderDirection.South, -Random.Range(0, 3));
                        NeighbourValueIncrement(dungeon, key, BuilderDirection.West, -Random.Range(0, 3));
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
                    if (value <= 0)
                    {
                        continue;
                    }

                    var tile = GameObject.Instantiate(_tilePrefab, transform, true);
                    tile.transform.position = new Vector3(x, y, 0);
                    dungeon.Tiles.Add(key, tile);
                    SetSprite(tile.GroundLayer);
                }
            }
        }

        private void NeighbourValueIncrement(Dungeon dungeon, System.Tuple<int, int> center, BuilderDirection dir, int value)
        {
            System.Tuple<int, int> key;
            switch(dir)
            {
                case BuilderDirection.North:
                    key = new System.Tuple<int, int>(center.Item1, center.Item2 - 1);
                    break;
                case BuilderDirection.East:
                    key = new System.Tuple<int, int>(center.Item1, center.Item2 - 1);
                    break;
                case BuilderDirection.South:
                    key = new System.Tuple<int, int>(center.Item1, center.Item2 - 1);
                    break;
                case BuilderDirection.West:
                    key = new System.Tuple<int, int>(center.Item1, center.Item2 - 1);
                    break;
                default:
                    return;
            }

            if(dungeon.ValueMap.ContainsKey(key))
            {
                dungeon.ValueMap[key] += value;
            }
            else
            {
                dungeon.ValueMap.Add(key, value);
            }
        }

        private void SetSprite(SpriteRenderer tileView)
        {
            switch (_meta.Theme)
            {
                case DungeonTheme.Cave:
                    tileView.sprite = _caveSprite;
                    break;
                case DungeonTheme.Forest:
                    tileView.sprite = _forestSprite;
                    break;
                default:
                    tileView.sprite = _noneSprite;
                    break;
            }
        }
    }
}