using System;
using Timberborn.DistributionSystem;
using Timberborn.Goods;

namespace ChooChoo
{
  public class GoodsStationGoodDistributionSetting
  {
    private readonly GoodSpecification _goodSpecification;

    public event EventHandler SettingChanged;

    public int MaxCapacity { get; private set; }

    public ImportOption ImportOption { get; private set; }

    public float LastImportTimestamp { get; set; }

    private GoodsStationGoodDistributionSetting(GoodSpecification goodSpecification)
    {
      _goodSpecification = goodSpecification;
    }

    public static GoodsStationGoodDistributionSetting CreateDefault(GoodSpecification goodSpecification)
    {
      GoodsStationGoodDistributionSetting distributionSetting = new GoodsStationGoodDistributionSetting(goodSpecification);
      distributionSetting.SetDefault();
      return distributionSetting;
    }

    public static GoodsStationGoodDistributionSetting Create(
      GoodSpecification goodSpecification,
      int maxCapacity,
      ImportOption importOption,
      float lastImportTimestamp)
    {
      return new GoodsStationGoodDistributionSetting(goodSpecification)
      {
        MaxCapacity = maxCapacity,
        ImportOption = importOption,
        LastImportTimestamp = lastImportTimestamp
      };
    }

    public string GoodId => _goodSpecification.Id;

    public void SetDefault()
    {
      MaxCapacity = 0;
      ImportOption = ImportOption.Disabled;
      EventHandler settingChanged = SettingChanged;
      if (settingChanged == null)
        return;
      settingChanged(this, EventArgs.Empty);
    }

    public void SetImportOption(ImportOption importOption)
    {
      ImportOption = importOption;
      EventHandler settingChanged = SettingChanged;
      if (settingChanged == null)
        return;
      settingChanged(this, EventArgs.Empty);
    }

    public void SetMaxCapacity(int maxCapacity)
    {
      if (maxCapacity > GoodsStation.Capacity)
        maxCapacity = GoodsStation.Capacity;
      MaxCapacity = maxCapacity;
      EventHandler settingChanged = SettingChanged;
      if (settingChanged == null)
        return;
      settingChanged(this, EventArgs.Empty);
    }
  }
}
