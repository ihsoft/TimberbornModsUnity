using System.Collections.Generic;
using Timberborn.CoreUI;
using Timberborn.Goods;
using Timberborn.Localization;
using UnityEngine.UIElements;

namespace ChooChoo
{
    public class GoodsStationRowsFactory
    {
        private readonly VisualElementLoader _visualElementLoader;
        private readonly IGoodService _goodService;
        private readonly GoodsStationIconService _goodsStationIconService;
        private readonly ILoc _loc;
        private readonly IGoodSelectionController _goodSelectionController;
        
        GoodsStationRowsFactory(
            IGoodService goodService, 
            VisualElementLoader visualElementLoader,
            GoodsStationIconService goodsStationIconService,
            ILoc loc,
            IGoodSelectionController goodSelectionController)
        {
            _visualElementLoader = visualElementLoader;
            _goodService = goodService;
            _goodsStationIconService = goodsStationIconService;
            _loc = loc;
            _goodSelectionController = goodSelectionController;
        }
        
        // public List<GoodsStationRow> CreateRows(VisualElement parent)
        // {
        //     var goodsStationRows = new List<GoodsStationRow>();
        //     foreach (var goodId in _goodService.Goods)
        //         goodsStationRows.Add(CreateRow(goodId, parent));
        //     return goodsStationRows;
        // }
        //
        // private GoodsStationRow CreateRow(string goodId, VisualElement parent)
        // {
        //     var goodsStationRow = new GoodsStationRow(_visualElementLoader, _goodService, _goodSelectionController, _goodsStationIconService, _loc);
        //     goodsStationRow.InitializeFragment(goodId, parent);
        //     return goodsStationRow;
        // }
    }
}