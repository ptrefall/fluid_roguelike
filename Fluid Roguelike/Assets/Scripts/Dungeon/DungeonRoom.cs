using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonRoom : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _roomTile;
        [SerializeField] private Sprite _caveSprite;
        [SerializeField] private Sprite _forestSprite;
        [SerializeField] private Sprite _noneSprite;
        private DungeonRoomMeta _meta;

        public void SetMeta(DungeonRoomMeta meta)
        {
            _meta = meta;
            transform.position = new Vector3(_meta.X, _meta.Y);
            if (_roomTile != null)
            {
                switch (_meta.Theme)
                {
                    case DungeonTheme.Cave:
                        _roomTile.sprite = _caveSprite;
                        break;
                    case DungeonTheme.Forest:
                        _roomTile.sprite = _forestSprite;
                        break;
                    default:
                        _roomTile.sprite = _noneSprite;
                        break;
                }

                //_roomTile.size = new Vector2(_meta.Width, _meta.Height);
                //_roomTile.transform.localScale = new Vector3(_meta.Width, _meta.Height, 1);
            }
        }

        public void GenerateTiles(Dungeon dungeon)
        {
            if (_meta == null)
                return;


        }
    }
}