using System.Linq;
using Timberborn.CoreUI;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Localization;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodsStationRow
  {
    private static readonly string ButtonHighlightClass = "highlight";
    private static readonly string EmptyClass = "stockpile-priority-toggle__icon--empty";
    private static readonly string ObtainClass = "stockpile-priority-toggle__icon--obtain";
    private readonly VisualElementLoader _visualElementLoader;
    private readonly IGoodService _goodService;
    private readonly GoodsStationIconService _goodsStationIconService;
    private readonly ILoc _loc;
    private LimitableGoodDisallower _limitableGoodDisallower;
    private string _goodId;
    private Inventory _inventory;
    private GoodsStation _goodsStation;
    private IGoodSelectionController _goodSelectionController;
    private VisualElement _root;
    private Label _capacityAmount;
    private Label _capacityLimit;
    private Timberborn.CoreUI.ProgressBar _progressBar;
    private Button _goodSelectionButton;
    private Button _goodUnselectionButton;
    private VisualElement _outputGood;
    private Image _inputOutputImage;
    private Label _loadingStateLabel;
    
    private Image _selectedGoodIcon;
    private Label _selectedGoodText;

    private GoodInputToggle _goodInputToggle;

    private TransferableGood _transferableGood;

    public GoodsStationRow(
      VisualElementLoader visualElementLoader,
      IGoodService goodService,
      IGoodSelectionController goodSelectionController,
      GoodsStationIconService goodsStationIconService,
      ILoc loc)
    {
      _visualElementLoader = visualElementLoader;
      _goodService = goodService;
      _goodSelectionController = goodSelectionController;
      _goodsStationIconService = goodsStationIconService;
      _loc = loc;
    }

    public void InitializeFragment(string goodId, VisualElement parent)
    {
      _goodId = goodId;
      
      _root = _visualElementLoader.LoadVisualElement("Master/EntityPanel/StockpileInventoryFragment");
      _capacityAmount = _root.Q<Label>("CapacityAmount");
      _capacityLimit = _root.Q<Label>("CapacityLimit");
      _progressBar = _root.Q<Timberborn.CoreUI.ProgressBar>("Progress");
      _goodSelectionButton = _root.Q<Button>("Selection");
      _goodSelectionButton.clicked += ToggleInputOutput;
      _goodUnselectionButton = _root.Q<Button>("Unselect");
      _goodUnselectionButton.clicked += UnselectGood;
      _outputGood = _root.Q<VisualElement>("OutputGood");
      
      _inputOutputImage = _root.Q<Image>("GoodIcon");
      _inputOutputImage.EnableInClassList("icon--hidden", true);
      _root.Q<Label>("SelectionItem").ToggleDisplayStyle(false);
      
      _root.Q<VisualElement>("GoodSelectionWrapper").Clear();
      
      var itemAndCapacity = _root.Q<VisualElement>("ItemAndCapacity");
      itemAndCapacity.Q<Image>("Type").ToggleDisplayStyle(false);
      
      GoodSpecification good = _goodService.GetGood(_goodId);
      itemAndCapacity.Q<Image>("Image").image = good.UIIcon.texture;
      itemAndCapacity.Q<Label>("Name").text = good.PluralDisplayName;
      
      _loadingStateLabel = _visualElementLoader.LoadVisualElement("Master/EntityPanel/StockpileInventoryFragment").Q<Label>("Name");
      _outputGood.Insert(0, _loadingStateLabel);
      
      itemAndCapacity.Insert(0, _outputGood);

      parent.Add(_root);
    }

    public void ShowFragment(GoodsStation goodsStation)
    {
      _inventory = goodsStation.Inventory;
      _goodsStation = goodsStation;
      _limitableGoodDisallower = goodsStation.GetComponent<LimitableGoodDisallower>();
      if (_goodsStation.enabled)
        _transferableGood = _goodsStation.TransferableGoods.First(good => good.GoodId == _goodId);
    }

    public void ClearFragment()
    {
      ToggleButtonHighlight(false);
      _inventory = null;
      _root.ToggleDisplayStyle(false);
    }

    public void UpdateFragment()
    {
      if ((bool) (UnityEngine.Object) _inventory)
      {
        int totalAmountInStock = _inventory.AmountInStock(_goodId);
        int maxCapacity = _limitableGoodDisallower.AllowedAmount(_goodId);
        _progressBar.SetProgress((float) totalAmountInStock / (float) maxCapacity);
        _capacityAmount.text = totalAmountInStock.ToString();
        _capacityLimit.text = maxCapacity.ToString();
        
        _inputOutputImage.sprite = _transferableGood.CanReceiveGoods ?  _goodsStationIconService.ObtainSprite :  _goodsStationIconService.EmptySprite;
        _loadingStateLabel.text = _loc.T(_transferableGood.CanReceiveGoods ? "Tobbert.GoodsStation.Loading" : "Tobbert.GoodsStation.Unloading");
        _root.ToggleDisplayStyle(_transferableGood.Enabled);
      }
      else
        _root.ToggleDisplayStyle(false);
    }

    public void ToggleButtonHighlight(bool highlight)
    {
      if (!(bool) (UnityEngine.Object) _inventory)
        return;
      _goodSelectionButton.EnableInClassList(ButtonHighlightClass, highlight);
    }

    private void ToggleInputOutput()
    {
      if (_transferableGood.CanReceiveGoods)
      {
        _transferableGood.CanReceiveGoods = false;
        _inputOutputImage.sprite = _goodsStationIconService.EmptySprite;
        _limitableGoodDisallower.SetAllowedAmount(_goodId, 0);
      }
      else
      {
        _transferableGood.CanReceiveGoods = true;
        _inputOutputImage.sprite = _goodsStationIconService.ObtainSprite;
        _limitableGoodDisallower.SetAllowedAmount(_goodId, _goodsStation.MaxCapacity);
      }
    }

    private void UnselectGood()
    {
      _limitableGoodDisallower.SetAllowedAmount(_goodId, 0);
      _transferableGood.Enabled = false;
      _transferableGood.CanReceiveGoods = false;  
      _goodSelectionController.Update();
    }
  }
}
