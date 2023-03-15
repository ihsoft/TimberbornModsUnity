using Bindito.Core;
using Timberborn.BehaviorSystem;
using Timberborn.TickSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace ChooChoo
{
    public class SmokeController : TickableComponent
    {
        [SerializeField]
        private GameObject smoke;

        private IDayNightCycle _dayNightCycle;
        
        private WaitExecutor _waitExecutor;

        [Inject]
        public void InjectDependencies(IDayNightCycle dayNightCycles)
        {
            _dayNightCycle = dayNightCycles;
        }
        
        private void Awake()
        {
            _waitExecutor = GetComponent<WaitExecutor>();
        }


        public override void Tick()
        {
            smoke.SetActive(!IsWaiting());
        }

        private bool IsWaiting()
        {
            return !(_dayNightCycle.PartialDayNumber > (float)ChooChooCore.GetInaccessibleField(_waitExecutor, "_finishTimestamp"));
        }
    }
}