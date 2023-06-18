using System;
using System.Collections.Generic;
using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.Common;
using Timberborn.DistributionSystem;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.Persistence;

namespace ChooChoo
{
  public class GoodsStationDistributionSetting : BaseComponent, IPersistentEntity, IInitializableEntity
  {
    private static readonly ComponentKey DistrictDistributionSettingKey = new(nameof(GoodsStationDistributionSetting));
    private static readonly ListKey<GoodsStationGoodDistributionSetting> GoodDistributionSettingsKey = new(nameof (GoodDistributionSettings));
    private GoodsStationDistributionSettingSerializer _goodsStationDistributionSettingSerializer;
    private IGoodService _goodService;
    private readonly List<GoodsStationGoodDistributionSetting> _goodDistributionSettings = new();

    public event EventHandler<GoodsStationGoodDistributionSetting> SettingChanged;

    [Inject]
    public void InjectDependencies(GoodsStationDistributionSettingSerializer goodsStationDistributionSettingSerializer, IGoodService goodService)
    {
      _goodsStationDistributionSettingSerializer = goodsStationDistributionSettingSerializer;
      _goodService = goodService;
    }

    public ReadOnlyList<GoodsStationGoodDistributionSetting> GoodDistributionSettings => _goodDistributionSettings.AsReadOnlyList();

    public void InitializeEntity()
    {
      AddMissingGoodSettings();
    }

    public void Save(IEntitySaver entitySaver)
    {
      entitySaver.GetComponent(DistrictDistributionSettingKey).Set(GoodDistributionSettingsKey, _goodDistributionSettings, _goodsStationDistributionSettingSerializer);
    }
    
    public void Load(IEntityLoader entityLoader)
    {
      if (!entityLoader.HasComponent(DistrictDistributionSettingKey))
        return;
      foreach (GoodsStationGoodDistributionSetting goodDistributionSetting in entityLoader.GetComponent(DistrictDistributionSettingKey).Get(GoodDistributionSettingsKey, _goodsStationDistributionSettingSerializer)) 
        AddGoodDistributionSetting(goodDistributionSetting);
    }

    public GoodsStationGoodDistributionSetting GetGoodDistributionSetting(string goodId)
    {
      foreach (var goodsStationGoodDistributionSetting in _goodDistributionSettings)
      {
        if (goodsStationGoodDistributionSetting.GoodId == goodId)
          return goodsStationGoodDistributionSetting;
      }

      throw new ArgumentException("No good distribution setting for good " + goodId);
    }

    public IEnumerable<GoodsStationGoodDistributionSetting> GetGoodDistributionSettingsForGroup(string groupId)
    {
      foreach (GoodsStationGoodDistributionSetting distributionSetting in _goodDistributionSettings)
      {
        if (_goodService.GetGood(distributionSetting.GoodId).GoodGroupId == groupId)
          yield return distributionSetting;
      }
    }

    public void ResetToDefault()
    {
      foreach (GoodsStationGoodDistributionSetting distributionSetting in _goodDistributionSettings)
        distributionSetting.SetDefault();
    }

    public void SetDistrictExportThreshold(int threshold)
    {
      foreach (GoodsStationGoodDistributionSetting distributionSetting in _goodDistributionSettings)
        distributionSetting.SetMaxCapacity(threshold);
    }

    public void SetDistrictImportOption(ImportOption importOption)
    {
      foreach (GoodsStationGoodDistributionSetting distributionSetting in _goodDistributionSettings)
        distributionSetting.SetImportOption(importOption);
    }

    private void AddMissingGoodSettings()
    {
      foreach (string good in _goodService.Goods)
      {
        if (IsSettingMissing(good))
          AddGoodDistributionSetting(GoodsStationGoodDistributionSetting.CreateDefault(_goodService.GetGood(good)));
      }
    }

    private void AddGoodDistributionSetting(GoodsStationGoodDistributionSetting goodsStationGoodDistributionSetting)
    {
      _goodDistributionSettings.Add(goodsStationGoodDistributionSetting);
      goodsStationGoodDistributionSetting.SettingChanged += OnSettingChanged;
    }

    private void OnSettingChanged(object sender, EventArgs e)
    {
      EventHandler<GoodsStationGoodDistributionSetting> settingChanged = SettingChanged;
      if (settingChanged == null)
        return;
      settingChanged(this, (GoodsStationGoodDistributionSetting) sender);
    }

    private bool IsSettingMissing(string goodId)
    {
      foreach (GoodsStationGoodDistributionSetting distributionSetting in _goodDistributionSettings)
      {
        if (distributionSetting.GoodId == goodId)
          return false;
      }
      return true;
    }
  }
}
