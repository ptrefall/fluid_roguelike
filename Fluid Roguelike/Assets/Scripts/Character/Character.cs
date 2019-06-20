
using Fluid.Roguelike.Character.State;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _view;

        public CharacterContext Context { get; private set; }
        public SpriteRenderer View => _view;

        

        private void Start()
        {
            Context = new CharacterContext();
        }

        public void Translate(Vector3 move)
        {
            transform.Translate(move);
        }
    }
}