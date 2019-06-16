using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _groundLayer;

        public SpriteRenderer GroundLayer => _groundLayer;

        public void Setup(Sprite ground)
        {
            _groundLayer.sprite = ground;
        }
    }
}