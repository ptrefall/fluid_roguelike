
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _view;
        public SpriteRenderer View => _view;

        public void Translate(Vector3 move)
        {
            transform.Translate(move);
        }
    }
}