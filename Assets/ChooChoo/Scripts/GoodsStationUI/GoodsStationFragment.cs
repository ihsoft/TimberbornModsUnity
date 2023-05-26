using System.Collections.Immutable;
using Timberborn.BaseComponentSystem;
using Timberborn.BatchControl;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.DistributionSystemUI;
using Timberborn.EntityPanelSystem;
using Timberborn.GameDistricts;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodsStationFragment : IEntityPanelFragment
  {
    private readonly IBatchControlBox _batchControlBox;
    private readonly BatchControlDistrict _batchControlDistrict;
    private readonly ImportGoodIconFactory _importGoodIconFactory;
    private readonly VisualElementLoader _visualElementLoader;
    private ImmutableArray<ImportGoodIcon> _importGoodIcons;
    private GoodsStation _goodStation;
    private DistrictDistributableGoodProvider _districtDistributableGoodProvider;
    private VisualElement _root;

    public GoodsStationFragment(
      IBatchControlBox batchControlBox,
      BatchControlDistrict batchControlDistrict,
      ImportGoodIconFactory importGoodIconFactory,
      VisualElementLoader visualElementLoader)
    {
      _batchControlBox = batchControlBox;
      _batchControlDistrict = batchControlDistrict;
      _importGoodIconFactory = importGoodIconFactory;
      _visualElementLoader = visualElementLoader;
    }

    public VisualElement InitializeFragment()
    {
      _root = _visualElementLoader.LoadVisualElement("Game/EntityPanel/DistrictCrossingFragment");
      _root.ToggleDisplayStyle(false);
      _root.Q<Button>("DistributionButton", (string) null).RegisterCallback<ClickEvent>(new EventCallback<ClickEvent>(OnDistributionButtonClicked));
      _importGoodIcons = _importGoodIconFactory.CreateImportGoods(_root.Q<VisualElement>("ImportGoodsWrapper", (string) null)).ToImmutableArray<ImportGoodIcon>();
      return _root;
    }

    public void ShowFragment(BaseComponent entity)
    {
      _goodStation = entity.GetComponentFast<GoodsStation>();
      if (!(bool) (Object) _goodStation || !(bool) (Object) _goodStation.DistrictDistributableGoodProvider)
        return;
      _root.ToggleDisplayStyle(true);
      SetDistrictDistributableGoodProvider(_goodStation.DistrictDistributableGoodProvider);
    }

    public void ClearFragment()
    {
      _goodStation = null;
      _districtDistributableGoodProvider = null;
      _root.ToggleDisplayStyle(false);
      foreach (ImportGoodIcon importGoodIcon in _importGoodIcons)
        importGoodIcon.Clear();
    }

    public void UpdateFragment()
    {
      if ((bool) (Object) _goodStation && (bool) (Object) _goodStation.DistrictDistributableGoodProvider)
      {
        if ((Object) _districtDistributableGoodProvider != (Object) _goodStation.DistrictDistributableGoodProvider)
          SetDistrictDistributableGoodProvider(_goodStation.DistrictDistributableGoodProvider);
        _root.ToggleDisplayStyle(true);
        foreach (ImportGoodIcon importGoodIcon in _importGoodIcons)
          importGoodIcon.Update();
      }
      else
        _root.ToggleDisplayStyle(false);
    }

    private void SetDistrictDistributableGoodProvider(
      DistrictDistributableGoodProvider districtDistributableGoodProvider)
    {
      _districtDistributableGoodProvider = districtDistributableGoodProvider;
      foreach (ImportGoodIcon importGoodIcon in _importGoodIcons)
        importGoodIcon.SetDistrictDistributableGoodProvider(districtDistributableGoodProvider);
    }

    private void OnDistributionButtonClicked(ClickEvent evt)
    {
      _batchControlDistrict.SetDistrict(_goodStation.GetComponentFast<DistrictBuilding>().District);
      _batchControlBox.OpenDistributionTab();
    }
  }
}
