using Timberborn.CoreUI;

namespace ChooChoo
{
    public class WagonTypeDropdownOptionsSetter
    {
        private readonly DropdownOptionsSetter _dropdownOptionsSetter;

        public WagonTypeDropdownOptionsSetter(DropdownOptionsSetter dropdownOptionsSetter)
        {
            _dropdownOptionsSetter = dropdownOptionsSetter;
        }

        public void SetOptions(TrainWagonManager trainWagonManager, Dropdown dropdown, int index)
        {
            _dropdownOptionsSetter.SetLocalizableOptions(
                dropdown, 
                trainWagonManager.WagonTypes, 
                () => trainWagonManager.TrainWagons[index].ActiveWagonType, 
                value => SetPriority(value, trainWagonManager, index));
        }

        private static void SetPriority(string value, TrainWagonManager trainWagonManager, int index)
        {
            trainWagonManager.UpdateWagonType(value, index);
        }
    }
}
