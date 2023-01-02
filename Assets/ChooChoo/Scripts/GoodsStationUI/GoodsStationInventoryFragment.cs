using System.Collections.Generic;
using TimberApi.UiBuilderSystem;
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
    private readonly UIBuilder _uiBuilder;
    private GoodsStation _goodsStationInventory;
    private VisualElement _root;
    private ScrollView _scrollView;

    private List<GoodsStationRow> _goodsStationRows;

    public GoodsStationInventoryFragment(
      VisualElementLoader visualElementLoader,
      GoodsStationRowsFactory goodsStationRowsFactory,
      IGoodSelectionController goodSelectionController,
      UIBuilder uiBuilder)
    {
      _visualElementLoader = visualElementLoader;
      _goodsStationRowsFactory = goodsStationRowsFactory;
      _goodSelectionController = goodSelectionController;
      _uiBuilder = uiBuilder;
    }

    public VisualElement InitializeFragment()
    {
      _root = new VisualElement();
      
      var topFragment = _uiBuilder.CreateFragmentBuilder()
        .AddComponent(builder => builder
          .SetJustifyContent(Justify.Center)
          
          .AddPreset(builder =>
          {
            var button = builder.Buttons().ButtonGame();
            button.name = "Selection";
            button.TextLocKey = "Tobbert.GoodsStation.GoodSelector";
            return button;
          }))
        
        .BuildAndInitialize();
      
      _root.Add(topFragment);
      
      var stockpileInventoryFragment = _visualElementLoader.LoadVisualElement("Master/EntityPanel/StockpileInventoryFragment");
      topFragment.Add(stockpileInventoryFragment.Q<VisualElement>("GoodSelectionWrapper"));
      _goodSelectionController.Initialize(topFragment);

      _scrollView = new ScrollView()
      {
        style =
        {
          maxHeight = new Length(500, LengthUnit.Pixel)
        },
        mode = ScrollViewMode.Vertical,
        horizontalScrollerVisibility = ScrollerVisibility.Hidden,
        verticalScrollerVisibility = ScrollerVisibility.Hidden,
      };
      _goodsStationRows = _goodsStationRowsFactory.CreateRows(_scrollView);
      _root.Add(_scrollView);
      
      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      _goodsStationInventory = entity.GetComponent<GoodsStation>();
      if ((bool)(Object)_goodsStationInventory && _goodsStationInventory.Inventory.enabled)
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
        _scrollView.scrollOffset = new Vector2(0, _scrollView.scrollOffset.y);
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
