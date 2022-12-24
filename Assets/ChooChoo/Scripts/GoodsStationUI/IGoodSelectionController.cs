using UnityEngine.UIElements;

namespace ChooChoo
{
  public interface IGoodSelectionController
  {
    void Initialize(VisualElement root);

    void Update();

    void SetGoodsStation(GoodsStation goodsStation);

    void ShowGoodSelectionBox();

    void Clear();
  }
}
