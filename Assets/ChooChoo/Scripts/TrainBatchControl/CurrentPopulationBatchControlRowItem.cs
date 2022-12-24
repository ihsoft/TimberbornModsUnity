using Timberborn.BatchControl;
using Timberborn.GameDistrictsMigration;
using UnityEngine.UIElements;

namespace ChooChoo
{
  internal class CurrentPopulationBatchControlRowItem : 
    IBatchControlRowItem,
    IUpdateableBatchControlRowItem
  {
    private readonly Label _currentPopulationLabel;
    private readonly PopulationDistributor _populationDistributor;

    public VisualElement Root { get; }

    public CurrentPopulationBatchControlRowItem(
      VisualElement root,
      Label currentPopulationLabel
      // PopulationDistributor populationDistributor
      )
    {
      Root = root;
      _currentPopulationLabel = currentPopulationLabel;
      // _populationDistributor = populationDistributor;
    }

    // public void UpdateRowItem() => _currentPopulationLabel.text = _populationDistributor.Current.ToString();
    public void UpdateRowItem() => _currentPopulationLabel.text = "";
  }
}
