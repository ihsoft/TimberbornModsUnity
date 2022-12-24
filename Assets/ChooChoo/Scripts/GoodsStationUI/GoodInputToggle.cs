using Timberborn.CoreUI;
using Timberborn.InventorySystem;
using Timberborn.Localization;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodInputToggle
  {
    private static readonly string BeaverClass = "worker-type-toggle__icon--beaver";
    private static readonly string BotClass = "worker-type-toggle__icon--bot";
    private static readonly string LockedClass = "slider-toggle--locked";
    private static readonly string UnlockableClass = "unlockable";
    private static readonly string ToggleSelectedLocKey = "Toggle.Selected";
    private static readonly string WorkplaceUnlockTooltipLocKey = "Work.WorkplaceUnlock.Tooltip";
    private readonly SliderToggleFactory _sliderToggleFactory;
    private readonly ILoc _loc;
    private SliderToggle _sliderToggle;
    private LimitableGoodDisallower _limitableGoodDisallower;
    private GoodsStation _goodsStation;
    private bool _botEnabled;
    private string _goodId;

    public VisualElement Root { get; private set; }

    public GoodInputToggle(SliderToggleFactory sliderToggleFactory, ILoc loc)
    {
      _sliderToggleFactory = sliderToggleFactory;
      _loc = loc;
    }

    public void Initialize(VisualElement parent, string goodId)
    {
      _goodId = goodId;
      SliderToggleItem inputToggleItem = SliderToggleItem.Create(GetInputButtonTooltip, BeaverClass, SetToInput, IsInput);
      SliderToggleItem outputToggleItem = SliderToggleItem.Create(GetOutputButtonTooltip, BotClass, SetToOutput, IsOutput);
      _sliderToggle = _sliderToggleFactory.Create(parent, inputToggleItem, outputToggleItem);
      Root = _sliderToggle.Root;
    }

    public void Show(LimitableGoodDisallower limitableGoodDisallower, GoodsStation goodsStation)
    {
      _limitableGoodDisallower = limitableGoodDisallower;
      _goodsStation = goodsStation;
    }

    public void Update()
    {
      _sliderToggle.Update();
    }

    public void Clear()
    {
      _limitableGoodDisallower = null;
    }

    private void SetToInput() => _limitableGoodDisallower.SetAllowedAmount(_goodId, _goodsStation.MaxCapacity);

    private void SetToOutput() => _limitableGoodDisallower.SetAllowedAmount(_goodId, 0);

    private TooltipContent GetInputButtonTooltip() => TooltipContent.Create(GetInputTooltipText);

    private TooltipContent GetOutputButtonTooltip() =>TooltipContent.Create(GetOutputTooltipText);

    private string GetInputTooltipText() => _loc.T("Tobbert.GoodToggle.Input") + SelectionAppendix(IsInput());

    private string GetOutputTooltipText() => _loc.T("Tobbert.GoodToggle.Output") + SelectionAppendix(IsOutput());

    private string SelectionAppendix(bool showText) => !showText ? "" : " " + _loc.T(ToggleSelectedLocKey);

    private bool IsInput() => _limitableGoodDisallower.AllowedAmount(_goodId) > 0;

    private bool IsOutput() => _limitableGoodDisallower.AllowedAmount(_goodId) == 0;
  }
}
