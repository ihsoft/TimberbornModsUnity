using System.Collections.Generic;
using System.Linq;
using Timberborn.CoreUI;
using Timberborn.InputSystem;
using Timberborn.InventorySystem;
using Timberborn.StatusSystemUI;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class StockpileGoodSelectionBox : IInputProcessor
  {
    private static readonly string NoMarginClass = "good-selection-box-row--no-margin";
    private readonly GoodsStationGoodSelectionBoxItemsFactory _goodsStationGoodSelectionBoxItemsFactory;
    private readonly VisualElementLoader _visualElementLoader;
    private readonly InputService _inputService;
    private readonly StatusListFragment _statusListFragment;
    private VisualElement _goodSelectionRoot;
    private readonly List<GoodSelectionBoxRow> _rows = new();
    private LimitableGoodDisallower _limitableGoodDisallower;
    private GoodsStation _goodsStation;
    private bool _isMouseOverElement;
    private bool _isMouseOverButton;

    public StockpileGoodSelectionBox(
      GoodsStationGoodSelectionBoxItemsFactory goodsStationGoodSelectionBoxItemsFactory,
      VisualElementLoader visualElementLoader,
      InputService inputService,
      StatusListFragment statusListFragment)
    {
      _goodsStationGoodSelectionBoxItemsFactory = goodsStationGoodSelectionBoxItemsFactory;
      _visualElementLoader = visualElementLoader;
      _inputService = inputService;
      _statusListFragment = statusListFragment;
    }

    public void Initialize(VisualElement root, VisualElement goodSelectionButton)
    {
      _goodSelectionRoot = _visualElementLoader.LoadVisualElement("Game/StockpileGoodSelectionBox");
      root.Add(_goodSelectionRoot);
      _goodSelectionRoot.ToggleDisplayStyle(false);
      _goodSelectionRoot.RegisterCallback<MouseEnterEvent>(_ => _isMouseOverElement = true);
      _goodSelectionRoot.RegisterCallback<MouseLeaveEvent>(_ => _isMouseOverElement = false);
      goodSelectionButton.RegisterCallback<MouseEnterEvent>(_ => _isMouseOverButton = true);
      goodSelectionButton.RegisterCallback<MouseLeaveEvent>(_ => _isMouseOverButton = false);
    }

    public void SetGoodsStation(GoodsStation goodsStation)
    {
      _limitableGoodDisallower = goodsStation.GetComponentFast<LimitableGoodDisallower>();
      _limitableGoodDisallower.DisallowedGoodsChanged += OnDisallowedGoodsChanged;
      _goodsStation = goodsStation;
      AddItems(goodsStation);
    }

    public bool ProcessInput()
    {
      if (!_inputService.UICancel && (!_inputService.ConditionalHideDropdown || !IsMouseOutsideElement))
        return false;
      ToggleGoodSelection();
      return true;
    }

    public void Update()
    {
      foreach (var goodSelectionBoxRow in _rows)
        goodSelectionBoxRow.Update();
    }

    public void ToggleGoodSelection()
    {
      if (_goodSelectionRoot.style.display == DisplayStyle.Flex)
        HideGoodSelection();
      else
      {
        UpdateSelection();
        ShowGoodSelection();
      }
    }

    public void Clear()
    {
      _goodSelectionRoot.Clear();
      _rows.Clear();
      if ((bool) (UnityEngine.Object) _limitableGoodDisallower)
        _limitableGoodDisallower.DisallowedGoodsChanged -= OnDisallowedGoodsChanged;
      _limitableGoodDisallower = null;
      HideGoodSelection();
    }

    private bool IsMouseOutsideElement => !_isMouseOverElement && !_isMouseOverButton;

    private void AddItems(GoodsStation goodsStation)
    {
      _rows.AddRange(_goodsStationGoodSelectionBoxItemsFactory.CreateItems(goodsStation, ToggleGood, _goodSelectionRoot));
      _rows.Last().Root.AddToClassList(NoMarginClass);
      UpdateSelection();
    }

    private void ToggleGood(string value)
    {
      // var transferableGood = _goodsStation.TransferableGoods.First(good => good.GoodId == value);
      // if (transferableGood.Enabled)
      // {
      //   transferableGood.Enabled = false;
      //   transferableGood.SendingGoods = false;
      //   _limitableGoodDisallower.SetAllowedAmount(transferableGood.GoodId, 0);
      // }
      // else
      // {
      //   transferableGood.Enabled = true;
      // }
      //
      // UpdateSelection();
      // _statusListFragment.UpdateFragment();
    }

    private void HideGoodSelection()
    {
      _goodSelectionRoot.ToggleDisplayStyle(false);
      _inputService.RemoveInputProcessor(this);
    }

    private void ShowGoodSelection()
    {
      _goodSelectionRoot.ToggleDisplayStyle(true);
      _inputService.AddInputProcessor(this);
      UpdateSelection();
    }

    private void OnDisallowedGoodsChanged(object sender, DisallowedGoodsChangedEventArgs e) => UpdateSelection();

    private void UpdateSelection()
    {
      // foreach (GoodSelectionBoxRow row in _rows)
      //   row.UpdateSelectedState(_goodsStation.TransferableGoods.Where(good => good.Enabled).Select(good => good.GoodId).ToList());
    }
  }
}
