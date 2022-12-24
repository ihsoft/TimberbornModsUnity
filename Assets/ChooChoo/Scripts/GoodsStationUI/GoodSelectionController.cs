using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodSelectionController : IGoodSelectionController
  {
    private readonly StockpileGoodSelectionBox _stockpileGoodSelectionBox;
    private Button _goodSelectionButton;

    public GoodSelectionController(StockpileGoodSelectionBox stockpileGoodSelectionBox)
    {
      _stockpileGoodSelectionBox = stockpileGoodSelectionBox;
    }

    public void Initialize(VisualElement root)
    {
      _goodSelectionButton = root.Q<Button>("Selection");
      _goodSelectionButton.clicked += ShowGoodSelectionBox;
      _stockpileGoodSelectionBox.Initialize(root.Q<VisualElement>("GoodSelectionWrapper"), _goodSelectionButton);
    }

    public void Update() => _stockpileGoodSelectionBox.Update();

    public void SetGoodsStation(GoodsStation goodsStation)
    {
      _stockpileGoodSelectionBox.SetGoodsStation(goodsStation);
    }

    public void ShowGoodSelectionBox() => _stockpileGoodSelectionBox.ToggleGoodSelection();

    public void Clear()
    {
      _stockpileGoodSelectionBox.Clear();
    }
  }
}
