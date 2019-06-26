﻿
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Database;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character : MonoBehaviour, IBumpTarget
    {
        [SerializeField] private SpriteRenderer _view;

        public CharacterDbEntry Meta { get; set; }
        public CharacterContext Context { get; private set; }
        public SpriteRenderer View => _view;

        public int2 Position => new int2((int) transform.position.x, (int) transform.position.y);

        public Character Init()
        {
            Context = new CharacterContext(this);
            return this;
        }

        public void Translate(int2 move)
        {
            transform.Translate(new Vector3(move.x, move.y, 0));
        }

        public void Visibility(bool isVisible)
        {
            View.gameObject.SetActive(isVisible);
        }
    }
}