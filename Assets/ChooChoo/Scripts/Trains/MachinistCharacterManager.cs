using Bindito.Core;
using Timberborn.TickSystem;
using UnityEngine;

namespace ChooChoo
{
    public class MachinistCharacterManager : TickableComponent
    {
        private MachinistCharacterFactory _machinistCharacterFactory;
        private TrainScheduleBehavior _trainScheduleBehavior;

        private GameObject _pilot;

        private bool _previousState;

        private readonly string AnimationName = "Sitting";

        [Inject]
        public void InjectDependencies(MachinistCharacterFactory machinistCharacterFactory)
        {
            _machinistCharacterFactory = machinistCharacterFactory;
        }

        private void Awake()
        {
            _pilot = _machinistCharacterFactory.CreatePilot(transform.GetChild(0).GetChild(0).GetChild(2));
            // _flyingRootBehavior = GetComponent<FlyingRootBehavior>();
        }

        private new void Start()
        {
            _pilot.GetComponent<Animator>().SetBool(AnimationName, true);
        }

        public override void Tick()
        {
            // var newState = _flyingRootBehavior.IsReturned;
            // if (!StateHasChanged(newState))
            //     return;
            // _previousState = newState;
            // _pilot.SetActive(!newState);
            // _pilot.GetComponent<Animator>().SetBool(AnimationName, true);
        }

        private bool StateHasChanged(bool newState) => _previousState != newState;
    }
}
