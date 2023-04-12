using Bindito.Core;
using Timberborn.TickSystem;
using UnityEngine;

namespace ChooChoo
{
    public class MachinistCharacterManager : TickableComponent
    {
        private MachinistCharacterFactory _machinistCharacterFactory;
        private TrainScheduleBehavior _trainScheduleBehavior;

        private bool _previousState;

        [Inject]
        public void InjectDependencies(MachinistCharacterFactory machinistCharacterFactory)
        {
            _machinistCharacterFactory = machinistCharacterFactory;
        }

        public override void Tick()
        {
            foreach (var variable in GetComponentsInChildren<Animator>())
                variable.SetBool("Sitting", true);
        }

        public override void StartTickable()
        {
            var trainModels = GetComponent<TrainModelManager>().TrainModels;
            foreach (var trainModel in trainModels)
            {
                var modelSpecification = trainModel.TrainModelSpecification;
                var machinist = _machinistCharacterFactory.CreateMachinist(ChooChooCore.FindBodyPart(trainModel.Model.transform, modelSpecification.MachinistSeatName));
                machinist.GetComponent<Animator>().SetBool(modelSpecification.MachinistAnimationName, true);
                machinist.transform.localScale = new Vector3(modelSpecification.MachinistScale, modelSpecification.MachinistScale, modelSpecification.MachinistScale);
            }
        }
    }
}
