using System.Linq;
using Timberborn.CoreUI;

namespace ChooChoo
{
    public class WagonTypeDropdownOptionsSetter
    {
        private readonly DropdownOptionsSetter _dropdownOptionsSetter;

        private readonly TrainModelSpecificationRepository _trainModelSpecificationRepository;

        public WagonTypeDropdownOptionsSetter(DropdownOptionsSetter dropdownOptionsSetter, TrainModelSpecificationRepository trainModelSpecificationRepository)
        {
            _dropdownOptionsSetter = dropdownOptionsSetter;
            _trainModelSpecificationRepository = trainModelSpecificationRepository;
        }

        public void SetOptions(WagonManager wagonManager, Dropdown dropdown, int index)
        {
            _dropdownOptionsSetter.SetLocalizableOptions(
                dropdown, 
                _trainModelSpecificationRepository.SelectableActiveWagonModels.Select(model => model.NameLocKey),
                () => wagonManager.Wagons[index].GetComponent<WagonModelManager>().ActiveWagonModel.WagonModelSpecification.NameLocKey, 
                value => SetPriority(value, wagonManager, index));
        }

        private static void SetPriority(string value, WagonManager wagonManager, int index)
        {
            wagonManager.UpdateWagonType(value, index);
        }
    }
}
