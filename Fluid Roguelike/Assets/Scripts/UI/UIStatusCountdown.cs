using UnityEngine;

namespace Fluid.Roguelike.UI
{
    public class UIStatusCountdown : MonoBehaviour, IUIStatusUpdater
    {
        [SerializeField] private UILabel _label;
        private int _turns;

        public void Setup(int turns)
        {
            _turns = turns;
            string t = "";
            for (var i = 0; i < turns; i++)
            {
                t += "o";
            }
            _label.SetLabel(t);
        }

        public void Tick()
        {
            Setup(_turns - 1);
        }
    }
}