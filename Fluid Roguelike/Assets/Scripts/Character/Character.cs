
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character : MonoBehaviour
    {
        public void Translate(Vector3 move)
        {
            transform.Translate(move);
        }
    }
}