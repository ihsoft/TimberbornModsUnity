using System.Collections.Generic;
using Timberborn.CoreUI;
using Timberborn.ResourceCountingSystem;
using Timberborn.ResourceCountingSystemUI;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodSelectionBoxItem
  {
    private static readonly string SelectedItemClass = "selected-item";
    private readonly ContextualResourceCountingService _contextualResourceCountingService;
    private readonly string _goodId;
    private readonly VisualElement _counter;

    public VisualElement Root { get; }

    public GoodSelectionBoxItem(
      ContextualResourceCountingService contextualResourceCountingService,
      string goodId,
      VisualElement root,
      VisualElement counter)
    {
      _contextualResourceCountingService = contextualResourceCountingService;
      _goodId = goodId;
      Root = root;
      _counter = counter;
    }

    public void Update()
    {
      ResourceCount contextualResourceCount = _contextualResourceCountingService.GetContextualResourceCount(_goodId);
      _counter.SetHeightAsPercent(contextualResourceCount.FillRate);
      _counter.parent.ToggleDisplayStyle(contextualResourceCount.TotalStock > 0);
    }

    public void UpdateSelectedState(List<string> selectedGoods) => Root.EnableInClassList(SelectedItemClass, selectedGoods.Contains(_goodId));
  }
}
