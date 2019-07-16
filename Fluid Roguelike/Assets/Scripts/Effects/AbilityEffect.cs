using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Effects
{
    public class AbilityEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _view;
        [SerializeField] private float _lifeTime = 0.3f;

        public void Setup(int2 dir)
        {
            if (dir.x < 0)
            {
                _view.flipX = true;
            }

            if (dir.y < 0)
            {
                _view.flipY = true;
            }

            StartCoroutine(LifeHandler(_lifeTime));
        }

        private IEnumerator LifeHandler(float lifeTime)
        {
            yield return new WaitForSeconds(lifeTime);
            GameObject.Destroy(gameObject);
        }
    }
}