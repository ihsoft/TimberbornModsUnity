using System.Collections.Generic;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
  internal class GoodsStationInventoryFragment : IEntityPanelFragment
  {
    private readonly VisualElementLoader _visualElementLoader;
    private readonly GoodsStationRowsFactory _goodsStationRowsFactory;
    private readonly IGoodSelectionController _goodSelectionController;
    private GoodsStation _goodsStationInventory;
    private VisualElement _root;

    private List<GoodsStationRow> _goodsStationRows;

    public GoodsStationInventoryFragment(
      VisualElementLoader visualElementLoader,
      GoodsStationRowsFactory goodsStationRowsFactory,
      IGoodSelectionController goodSelectionController)
    {
      _visualElementLoader = visualElementLoader;
      _goodsStationRowsFactory = goodsStationRowsFactory;
      _goodSelectionController = goodSelectionController;
    }

    public VisualElement InitializeFragment()
    {
      _root = new VisualElement();
      
      var rootFragment = _visualElementLoader.LoadVisualElement("Master/EntityPanel/StockpileInventoryFragment");
      
      rootFragment.Q<Button>("Unselect").ToggleDisplayStyle(false);
      rootFragment.Q<VisualElement>("ItemAndCapacity").ToggleDisplayStyle(false);

      _goodSelectionController.Initialize(rootFragment);

      _root.Add(rootFragment);
      _goodsStationRows = _goodsStationRowsFactory.CreateRows(_root);
      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      _goodsStationInventory = entity.GetComponent<GoodsStation>();
      if ((bool)(Object)_goodsStationInventory)
      {
        foreach (var goodsStationRow in _goodsStationRows)
        {
          goodsStationRow.ShowFragment(_goodsStationInventory);
        }
        _goodSelectionController.SetGoodsStation(_goodsStationInventory);
      }
      else
        _goodsStationInventory = null;
    }

    public void ClearFragment()
    {
      _goodsStationInventory = null;
      foreach (var goodsStationRow in _goodsStationRows)
        goodsStationRow.ClearFragment();
      _goodSelectionController.Clear();
      _root.ToggleDisplayStyle(false);
    }

    public void UpdateFragment()
    {
      if ((bool) (Object) _goodsStationInventory && _goodsStationInventory.enabled)
      {
        _root.ToggleDisplayStyle(true);
        _goodSelectionController.Update();
        foreach (var goodsStationRow in _goodsStationRows)
        {
          goodsStationRow.UpdateFragment();
        }
      }
      else
        _root.ToggleDisplayStyle(false);
    }
  }
}
