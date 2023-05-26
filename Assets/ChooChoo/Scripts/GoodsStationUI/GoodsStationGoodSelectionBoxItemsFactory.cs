using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.Common;
using Timberborn.Goods;
using Timberborn.StockpilesUI;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodsStationGoodSelectionBoxItemsFactory
  {
    private readonly GoodSelectionBoxItemFactory _goodSelectionBoxItemFactory;
    private readonly GoodSelectionBoxRowFactory _goodSelectionBoxRowFactory;
    private readonly IGoodService _goodService;

    public GoodsStationGoodSelectionBoxItemsFactory(
      GoodSelectionBoxItemFactory goodSelectionBoxItemFactory,
      GoodSelectionBoxRowFactory goodSelectionBoxRowFactory,
      IGoodService goodService)
    {
      _goodSelectionBoxItemFactory = goodSelectionBoxItemFactory;
      _goodSelectionBoxRowFactory = goodSelectionBoxRowFactory;
      _goodService = goodService;
    }

    public IEnumerable<GoodSelectionBoxRow> CreateItems(
      GoodsStation stockpile,
      Action<string> itemAction,
      VisualElement root)
    {
      var dictionary = new Dictionary<string, GoodSelectionBoxRow>();
      var component = stockpile.GetComponentFast<GoodsStationOptionsProvider>();
      foreach (var option in component.Options)
      {
        if (option != StockpileOptionsService.NothingSelectedLocKey)
        {
          string goodGroupId = _goodService.GetGood(option).GoodGroupId;
          dictionary.GetOrAdd(goodGroupId, () => _goodSelectionBoxRowFactory.Create(goodGroupId)).AddItem(_goodSelectionBoxItemFactory.Create(option, itemAction));
        }
      }
      foreach (GoodSelectionBoxRow goodSelectionBoxRow in dictionary.Values.OrderBy(row => row.Order))
      {
        root.Add(goodSelectionBoxRow.Root);
        yield return goodSelectionBoxRow;
      }
    }
  }
}
