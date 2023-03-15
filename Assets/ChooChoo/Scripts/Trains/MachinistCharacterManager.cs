using Bindito.Core;
using Timberborn.TickSystem;
using UnityEngine;

namespace ChooChoo
{
    public class MachinistCharacterManager : MonoBehaviour
    {
        private MachinistCharacterFactory _machinistCharacterFactory;
        private TrainScheduleBehavior _trainScheduleBehavior;

        private GameObject _pilot;

        private bool _previousState;

        private readonly string AnimationName = "Sitting";

        private const float Scale = 0.64f;

        [Inject]
        public void InjectDependencies(MachinistCharacterFactory machinistCharacterFactory)
        {
            _machinistCharacterFactory = machinistCharacterFactory;
        }

        private void Start()
        {
            _pilot = _machinistCharacterFactory.CreatePilot(transform.GetChild(0).GetChild(0).GetChild(2));
            _pilot.GetComponent<Animator>().SetBool(AnimationName, true);
            _pilot.transform.localScale = new Vector3(Scale, Scale, Scale);
        }
    }
}
