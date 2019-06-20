
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character : MonoBehaviour, IBumpTarget
    {
        [SerializeField] private SpriteRenderer _view;

        public CharacterContext Context { get; private set; }
        public SpriteRenderer View => _view;

        public int2 Position => new int2((int) transform.position.x, (int) transform.position.y);

        private void Start()
        {
            Context = new CharacterContext(this);
        }

        public void Translate(int2 move)
        {
            transform.Translate(new Vector3(move.x, move.y, 0));
        }
    }
}