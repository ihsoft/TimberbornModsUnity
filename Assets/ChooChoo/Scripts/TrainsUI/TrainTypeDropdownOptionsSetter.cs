using System.Linq;
using Timberborn.CoreUI;

namespace ChooChoo
{
    public class TrainTypeDropdownOptionsSetter
    {
        private readonly DropdownOptionsSetter _dropdownOptionsSetter;

        public TrainTypeDropdownOptionsSetter(DropdownOptionsSetter dropdownOptionsSetter)
        {
            _dropdownOptionsSetter = dropdownOptionsSetter;
        }

        public void SetOptions(TrainModelManager trainModelManager, Dropdown dropdown)
        {
            _dropdownOptionsSetter.SetLocalizableOptions(
                dropdown, 
                trainModelManager.TrainModels.Select(model => model.TrainModelSpecification.NameLocKey), 
                () => trainModelManager.ActiveTrainModel.TrainModelSpecification.NameLocKey, 
                value => SetPriority(value, trainModelManager));
        }

        private static void SetPriority(string value, TrainModelManager trainWagonManager)
        {
            trainWagonManager.UpdateTrainType(value);
        }
    }
}
