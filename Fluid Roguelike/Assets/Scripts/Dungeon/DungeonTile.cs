using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonTile : MonoBehaviour
    {
        public enum Index
        {
            Void = 0,
            Floor,
            Wall,
            Water,
            Lava,
            WorldMap,
        }
        [SerializeField] private SpriteRenderer _groundLayer;

        public SpriteRenderer GroundLayer => _groundLayer;

        public void Setup(Sprite ground)
        {
            _groundLayer.sprite = ground;
        }

        public void Visibility(bool isVisible)
        {
            if (GroundLayer != null)
            {
                GroundLayer.gameObject.SetActive(isVisible);
            }
        }
    }
}