using System.Collections.Immutable;
using Timberborn.BaseComponentSystem;
using Timberborn.BatchControl;
using Timberborn.CoreUI;
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
    private GoodsStationDistributableGoodProvider _goodsStationDistributableGoodProvider;
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
      _root.Q<Button>("DistributionButton").RegisterCallback(new EventCallback<ClickEvent>(OnDistributionButtonClicked));
      _importGoodIcons = _importGoodIconFactory.CreateImportGoods(_root.Q<VisualElement>("ImportGoodsWrapper")).ToImmutableArray();
      return _root;
    }

    public void ShowFragment(BaseComponent entity)
    {
      _goodStation = entity.GetComponentFast<GoodsStation>();
      if (!(bool) (Object) _goodStation || !(bool) (Object) _goodStation.GoodsStationDistributableGoodProvider)
        return;
      _root.ToggleDisplayStyle(true);
      SetDistrictDistributableGoodProvider(_goodStation.GoodsStationDistributableGoodProvider);
    }

    public void ClearFragment()
    {
      _goodStation = null;
      _goodsStationDistributableGoodProvider = null;
      _root.ToggleDisplayStyle(false);
      foreach (ImportGoodIcon importGoodIcon in _importGoodIcons)
        importGoodIcon.Clear();
    }

    public void UpdateFragment()
    {
      if ((bool) (Object) _goodStation && (bool) (Object) _goodStation.GoodsStationDistributableGoodProvider)
      {
        if (_goodsStationDistributableGoodProvider != _goodStation.GoodsStationDistributableGoodProvider)
          SetDistrictDistributableGoodProvider(_goodStation.GoodsStationDistributableGoodProvider);
        _root.ToggleDisplayStyle(true);
        foreach (ImportGoodIcon importGoodIcon in _importGoodIcons)
          importGoodIcon.Update();
      }
      else
        _root.ToggleDisplayStyle(false);
    }

    private void SetDistrictDistributableGoodProvider(GoodsStationDistributableGoodProvider goodsStationDistributableGoodProvider)
    {
      _goodsStationDistributableGoodProvider = goodsStationDistributableGoodProvider;
      foreach (ImportGoodIcon importGoodIcon in _importGoodIcons)
        importGoodIcon.SetDistrictDistributableGoodProvider(goodsStationDistributableGoodProvider);
    }

    private void OnDistributionButtonClicked(ClickEvent evt)
    {
      _batchControlDistrict.SetDistrict(_goodStation.GetComponentFast<DistrictBuilding>().District);
      ChooChooCore.InvokePrivateMethod(_batchControlBox, "OpenTab", new object[] { DistributionBatchControlTab.TabIndex - 1 });
    }
  }
}
