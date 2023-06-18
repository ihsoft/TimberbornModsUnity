using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class ImportGoodIcon
  {
    private readonly string _goodId;
    private readonly VisualElement _importableIcon;
    private readonly VisualElement _nonImportableIcon;

    public GoodsStationDistributableGoodProvider GoodsStationDistributableGoodProvider { get; private set; }

    public ImportGoodIcon(string goodId, VisualElement importableIcon, VisualElement nonImportableIcon)
    {
      _goodId = goodId;
      _importableIcon = importableIcon;
      _nonImportableIcon = nonImportableIcon;
    }

    public void SetDistrictDistributableGoodProvider(GoodsStationDistributableGoodProvider goodsStationDistributableGoodProvider)
    {
      GoodsStationDistributableGoodProvider = goodsStationDistributableGoodProvider;
    }

    public void Update()
    {
      bool visible = GoodsStationDistributableGoodProvider.IsImportEnabled(_goodId);
      // Plugin.Log.LogInfo("visible: " + visible);
      _importableIcon.ToggleDisplayStyle(visible);
      _nonImportableIcon.ToggleDisplayStyle(!visible);
    }

    public void Clear()
    {
      GoodsStationDistributableGoodProvider = null;
    }
  }
}
