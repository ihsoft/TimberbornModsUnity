using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodSelectionBoxRow
  {
    private readonly VisualElement _itemsRoot;
    private readonly List<GoodSelectionBoxItem> _items = new();

    public VisualElement Root { get; }

    public int Order { get; }

    public GoodSelectionBoxRow(VisualElement root, int order, VisualElement itemsRoot)
    {
      Root = root;
      Order = order;
      _itemsRoot = itemsRoot;
    }

    public void AddItem(GoodSelectionBoxItem item)
    {
      _items.Add(item);
      _itemsRoot.Add(item.Root);
    }

    public void Update()
    {
      foreach (var goodSelectionBoxItem in _items)
        goodSelectionBoxItem.Update();
    }

    public void UpdateSelectedState(List<string> selectedGoods)
    {
      foreach (var goodSelectionBoxItem in _items)
        goodSelectionBoxItem.UpdateSelectedState(selectedGoods);
    }
  }
}
