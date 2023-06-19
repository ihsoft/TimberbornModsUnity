using System;
using System.Collections.Generic;
using ChooChoo.Scripts.GoodsStation;
using HarmonyLib;
using Timberborn.BaseComponentSystem;
using Timberborn.Common;
using Timberborn.DistributionSystem;
using Timberborn.GameDistricts;
using Timberborn.InventorySystem;
using Timberborn.SingletonSystem;

namespace ChooChoo
{
  public class GoodsStationDistributableGoodProvider : BaseComponent
  {    
    private GoodsStationReceivingInventory _goodsStationReceivingInventory;
    private GoodsStationSendingInventory _goodsStationSendingInventory;
    private GoodsStationDistributionSetting _goodsStationDistributionSetting;
    private readonly Dictionary<string, TrainDistributableGood> _exportCache = new();
    private readonly Dictionary<string, ImportableGood> _importCache = new();

    private readonly Type _distributionInventoryRegistryType = AccessTools.TypeByName("DistributionInventoryRegistry");

    public void Awake()
    {
      _goodsStationReceivingInventory = GetComponentFast<GoodsStationReceivingInventory>();
      _goodsStationSendingInventory = GetComponentFast<GoodsStationSendingInventory>();
      _goodsStationDistributionSetting = GetComponentFast<GoodsStationDistributionSetting>();
      _goodsStationDistributionSetting.SettingChanged += (_, setting) => ClearCache(setting.GoodId);
    }
    
    public void GetDistributableGoodsForImport(List<TrainDistributableGood> distributableGoods)
    {
      foreach (GoodsStationGoodDistributionSetting distributionSetting in _goodsStationDistributionSetting.GoodDistributionSettings)
      {
        if (TryGetDistributableGoodForImport(distributionSetting.GoodId, out var trainDistributableGood))
          distributableGoods.Add(trainDistributableGood);
      }
      distributableGoods.Sort();
    }

    public bool TryGetDistributableGoodForImport(string goodId, out TrainDistributableGood trainDistributableGood)
    {
      ImportableGood importableGood = GetImportableGood(goodId);
      trainDistributableGood = importableGood.TrainDistributableGood;
      return importableGood.IsImportable;
    }

    public TrainDistributableGood GetDistributableGoodForExport(string goodId)
    {
      return _exportCache.TryGetValue(goodId, out var distributableGood) ? distributableGood : GetAndCacheExportDistributableGood(_goodsStationDistributionSetting.GetGoodDistributionSetting(goodId));
    }

    public bool IsImportEnabled(string goodId)
    {
      ImportableGood importableGood = GetImportableGood(goodId);
      return importableGood.IsImportable || importableGood.HasCapacity;
    }

    public ImportOption GetGoodImportOption(string goodId) => _goodsStationDistributionSetting.GetGoodDistributionSetting(goodId).ImportOption;

    [OnEvent]
    private void OnGoodStorageChangedEvent(GoodStorageChangedEvent goodStorageChangedEvent)
    {
      ClearCache(goodStorageChangedEvent.GoodId);
    }

    private void ClearCache(string goodId)
    {
      _exportCache.Remove(goodId);
      _importCache.Remove(goodId);
    }

    private ImportableGood GetImportableGood(string goodId)
    {
      if (_importCache.TryGetValue(goodId, out var importableGood1))
        return importableGood1;
      GoodsStationGoodDistributionSetting distributionSetting = _goodsStationDistributionSetting.GetGoodDistributionSetting(goodId);
      ImportableGood importableGood2;
      var canBeImported = CanBeImported(distributionSetting, out var hasCapacity);
      // Plugin.Log.LogInfo("canBeImported: " + canBeImported);
      if (canBeImported)
        importableGood2 = ImportableGood.CreateImportableWithCapacity(GetDistributableGood(distributionSetting, true));
      else if (!hasCapacity)
        importableGood2 = ImportableGood.CreateNonImportable();
      else
        importableGood2 = ImportableGood.CreateNonImportableWithCapacity();
      _importCache.Add(goodId, importableGood2);
      return importableGood2;
    }

    private bool CanBeImported(GoodsStationGoodDistributionSetting goodDistributionSetting, out bool hasCapacity)
    {
      hasCapacity = goodDistributionSetting.ImportOption == ImportOption.Forced;
      return goodDistributionSetting.ImportOption == ImportOption.Forced && HasUnreservedCapacity(goodDistributionSetting, out hasCapacity);
    }

    private bool HasUnreservedCapacity(GoodsStationGoodDistributionSetting goodDistributionSetting, out bool hasCapacity)
    {
      var capacity = GetCapacity(goodDistributionSetting);
      // Plugin.Log.LogInfo("Capacity: " + capacity);
      hasCapacity = capacity > 0;
      // hasCapacity = _goodsStationReceivingInventory.Inventory.UnreservedCapacity(goodDistributionSetting.GoodId) > 0;
      var unreservedAmount =
        _goodsStationReceivingInventory.Inventory.UnreservedCapacity(goodDistributionSetting.GoodId);
      // Plugin.Log.LogInfo("Unreserved capacity: " + unreservedAmount);
      var flag = _goodsStationReceivingInventory.Inventory.HasUnreservedCapacity(goodDistributionSetting.GoodId);
      // Plugin.Log.LogInfo("HasUnreservedCapacity: " + flag);
      return unreservedAmount > 0;
      
      // hasCapacity = false;
      // foreach (Inventory capacityInventory in this._distributionInventoryRegistry.CapacityInventories(goodDistributionSetting.GoodId))
      // {
      //   if (capacityInventory.IsUnblocked && GoodsStationDistributableGoodProvider.GetInventoryCapacity(capacityInventory, goodDistributionSetting.GoodId) > 0)
      //   {
      //     hasCapacity = true;
      //     if (capacityInventory.HasUnreservedCapacity(goodDistributionSetting.GoodId))
      //       return true;
      //   }
      // }
      // return false;
    }

    private TrainDistributableGood GetAndCacheExportDistributableGood(GoodsStationGoodDistributionSetting goodDistributionSetting)
    {
      TrainDistributableGood trainDistributableGood = GetDistributableGood(goodDistributionSetting, false);
      _exportCache.Add(goodDistributionSetting.GoodId, trainDistributableGood);
      return trainDistributableGood;
    }

    private TrainDistributableGood GetDistributableGood(GoodsStationGoodDistributionSetting goodDistributionSetting, bool withDistrictCrossingIncomingStock)
    {
      return new TrainDistributableGood(
        GetStock(goodDistributionSetting.GoodId, withDistrictCrossingIncomingStock), 
        GetCapacity(goodDistributionSetting), 
        goodDistributionSetting);
    }

    private int GetCapacity(GoodsStationGoodDistributionSetting goodDistributionSetting)
    {
      return _goodsStationReceivingInventory.Inventory.LimitedAmount(goodDistributionSetting.GoodId);

      // string goodId = goodDistributionSetting.GoodId;
      // int capacity = 0;
      //
      // var district = GetComponentFast<DistrictBuilding>().District;
      // if ( district == null)
      //   return capacity;
      // var distributionInventoryRegistry = district.GameObjectFast.GetComponent(distributionInventoryRegistryType);
      // var storingInventories = (ReadOnlyHashSet<Inventory>)ChooChooCore.InvokePrivateMethod(distributionInventoryRegistry, "StoringInventories", new object[] { goodId });
      //
      //
      // foreach (Inventory storingInventory in storingInventories)
      // {
      //   if (storingInventory.IsUnblocked)
      //     capacity += GetInventoryCapacity(storingInventory, goodId);
      // }
      // // if (goodDistributionSetting.ImportOption == ImportOption.Forced || capacity == 0 && this.HasTakingInventory(goodId))
      // //   capacity += this.GetDistrictCrossingsCapacity(goodId);
      // return capacity;
    }

    // private static int GetInventoryCapacity(Inventory inventory, string goodId)
    // {
    //   Emptiable componentFast1 = inventory.GetComponentFast<Emptiable>();
    //   if (componentFast1 == null || !componentFast1.IsMarkedForEmptying)
    //   {
    //     GoodSupplier componentFast2 = inventory.GetComponentFast<GoodSupplier>();
    //     if (componentFast2 == null || !componentFast2.IsSupplying)
    //       return inventory.LimitedAmount(goodId);
    //   }
    //   return 0;
    // }

    // private bool HasTakingInventory(string goodId)
    // {
    //   foreach (Inventory capacityInventory in this._distributionInventoryRegistry.CapacityInventories(goodId))
    //   {
    //     if (capacityInventory.IsUnblocked && GoodsStationDistributableGoodProvider.GetInventoryCapacity(capacityInventory, goodId) > 0)
    //       return true;
    //   }
    //   return false;
    // }
    //
    // private int GetDistrictCrossingsCapacity(string goodId)
    // {
    //   ReadOnlyHashSet<Inventory> crossingInventories = this._distributionInventoryRegistry.DistrictCrossingInventories;
    //   int crossingsCapacity = 0;
    //   foreach (Inventory inventory in crossingInventories)
    //     crossingsCapacity += inventory.LimitedAmount(goodId);
    //   return crossingsCapacity;
    // }

    private int GetStock(string goodId, bool withDistrictCrossingIncomingStock)
    {
      int stock = 0;
      var district = GetComponentFast<DistrictBuilding>().District;
      if (district == null)
        return stock;
      var distributionInventoryRegistry = district.GameObjectFast.GetComponent(_distributionInventoryRegistryType);
      var stockInventories = (ReadOnlyHashSet<Inventory>)ChooChooCore.InvokePrivateMethod(distributionInventoryRegistry, "StockInventories", new object[] { goodId });
      foreach (Inventory stockInventory in stockInventories)
        stock += GetInventoryStock(stockInventory, goodId, withDistrictCrossingIncomingStock);
      return stock;
    }

    private static int GetInventoryStock(Inventory inventory, string goodId, bool withDistrictCrossingIncomingStock)
    {
      int inventoryStock = inventory.UnreservedAmountInStock(goodId);
      inventoryStock += inventory.ReservedCapacity(goodId);
      return inventoryStock;
    }

    // private static bool IsDistrictCrossingInventory(Inventory inventory) => inventory.ComponentName == "DistrictCrossing";
  }
}
