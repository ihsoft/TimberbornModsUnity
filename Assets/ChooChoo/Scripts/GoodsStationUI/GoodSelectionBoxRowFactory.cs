using Timberborn.CoreUI;
using Timberborn.Goods;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodSelectionBoxRowFactory
  {
    private readonly VisualElementLoader _visualElementLoader;
    private readonly GoodsGroupSpecificationService _goodsGroupSpecificationService;

    public GoodSelectionBoxRowFactory(
      VisualElementLoader visualElementLoader,
      GoodsGroupSpecificationService goodsGroupSpecificationService)
    {
      this._visualElementLoader = visualElementLoader;
      this._goodsGroupSpecificationService = goodsGroupSpecificationService;
    }

    public GoodSelectionBoxRow Create(string goodGroupId)
    {
      GoodGroupSpecification specification = this._goodsGroupSpecificationService.GetSpecification(goodGroupId);
      VisualElement visualElement = this._visualElementLoader.LoadVisualElement("Game/EntityPanel/GoodSelectionBoxRow");
      visualElement.Q<Image>("HeaderIcon", (string) null).sprite = specification.Icon;
      return new GoodSelectionBoxRow(visualElement, specification.Order, visualElement.Q<VisualElement>("Icons", (string) null));
    }
  }
}
