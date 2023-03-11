using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.Common;
using Timberborn.ConstructibleSystem;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class GoodsStation : MonoBehaviour, IRegisteredComponent, IFinishedStateListener, IPersistentEntity
  {
    [SerializeField]
    private int _maxCapacity;
    
    private static readonly ComponentKey GoodsStationKey = new(nameof(GoodsStation));
    private static readonly ListKey<TransferableGood> TransferableGoodsKey = new("TransferableGoods");
    private TransferableGoodObjectSerializer _transferableGoodObjectSerializer;
    private GoodsStationsRepository _goodsStationsRepository;
    private LimitableGoodDisallower _limitableGoodDisallower;
    public Inventory Inventory { get; private set; }
    public TrainDestination TrainDestinationComponent { get; private set; }
    public List<TransferableGood> TransferableGoods;
    public readonly List<GoodAmount> SortedLackingGoods = new();
    
    public int MaxCapacity => _maxCapacity;

    public IEnumerable<TransferableGood> SendingGoods => TransferableGoods.Where(good => good.Enabled && good.SendingGoods);
    
    public IEnumerable<TransferableGood> ReceivingGoods => TransferableGoods.Where(good => good.Enabled && !good.SendingGoods);


    [Inject]
    public void InjectDependencies(TransferableGoodObjectSerializer transferableGoodObjectSerializer, GoodsStationsRepository goodsStationsRepository)
    {
      _transferableGoodObjectSerializer = transferableGoodObjectSerializer;
      _goodsStationsRepository = goodsStationsRepository;
    }

    public void Awake() 
    {
      _limitableGoodDisallower = GetComponent<LimitableGoodDisallower>();
      TrainDestinationComponent = GetComponent<TrainDestination>();
      enabled = false;
    }

    public void OnEnterFinishedState()
    {
      enabled = true;
      Inventory.Enable();
      Inventory.InventoryChanged += InventoryChangedEvent;
      _goodsStationsRepository.Register(this);
      if (TransferableGoods == null)
        CreateTransferableGoods();
      else
        VerifyTransferableGoods();
      foreach (var transferableGood in TransferableGoods)
        _limitableGoodDisallower.SetAllowedAmount(transferableGood.GoodId, transferableGood.SendingGoods ? _maxCapacity : 0);
    }

    public void OnExitFinishedState()
    {
      Inventory.Disable();
      enabled = false;
      _goodsStationsRepository.UnRegister(this);
    }

    public void InitializeInventory(Inventory inventory)
    {
      Asserts.FieldIsNull(this, Inventory, "Inventory");
      Inventory = inventory;
    }

    public void Save(IEntitySaver entitySaver)
    {
      if (TransferableGoods != null)
        entitySaver.GetComponent(GoodsStationKey).Set(TransferableGoodsKey, TransferableGoods, _transferableGoodObjectSerializer);
    }

    public void Load(IEntityLoader entityLoader)
    {
      if (!entityLoader.HasComponent(GoodsStationKey))
        return;
      if (entityLoader.GetComponent(GoodsStationKey).Has(TransferableGoodsKey))
        TransferableGoods = entityLoader.GetComponent(GoodsStationKey).Get(TransferableGoodsKey, _transferableGoodObjectSerializer);
    }

    public bool IsSending(string goodId) => TransferableGoods.First(good => good.GoodId == goodId).SendingGoods;
    
    public void UpdateLackingGoods(bool isSendingGood)
    {
      SortedLackingGoods.Clear();
      var transferableGoods = isSendingGood ? SendingGoods : ReceivingGoods;
      foreach (TransferableGood transferableGood in transferableGoods)
      {
        string goodId = transferableGood.GoodId;
        // Plugin.Log.LogInfo(goodId);
        // Plugin.Log.LogWarning("MaxAllowedAmount: " + distributionPost.MaxAllowedAmount(goodId) + " UnreservedAmountInStockAndIncoming: " + distributionPost.Inventory.UnreservedAmountInStockAndIncoming(goodId) + " UnreservedCapacity: " + distributionPost.Inventory.UnreservedCapacity(goodId));
        // int amount = Mathf.Min(Mathf.Max(goodsStation.MaxAllowedAmount(goodId) - goodsStation.Inventory.UnreservedAmountInStockAndIncoming(goodId), 0), goodsStation.Inventory.UnreservedCapacity(goodId));
        int amount = Mathf.Max(MaxAllowedAmount() - Inventory.UnreservedAmountInStockAndIncoming(goodId), 0);
        // Plugin.Log.LogInfo("Found amount: " + amount);
        if (amount > 0)
          SortedLackingGoods.Add(new GoodAmount(goodId, amount));
      }
      SortedLackingGoods.Sort(CompareLackingGoods);
    }

    public int TotalLackingAmount() => SortedLackingGoods.Sum(good => good.Amount);

    private void InventoryChangedEvent(object sender, InventoryChangedEventArgs e)
    {
      ChooChooCore.InvokePrivateMethod(Inventory, "CheckIfUnwantedStockAppeared");
    }

    private void CreateTransferableGoods()
    {
      var list = new List<TransferableGood>();
      foreach (var storableGoodAmount in Inventory.AllowedGoods)
        list.Add(new TransferableGood(storableGoodAmount.StorableGood.GoodId, false, false));
      TransferableGoods = list;
    }

    private void VerifyTransferableGoods()
    {
      RemoveGoods();
      AddNewGoods();
    }
    
    private void RemoveGoods()
    {
      var allowedGoods = Inventory.AllowedGoods.ToArray();
      
      foreach (var transferableGood in TransferableGoods.ToList())
      {
        if (!allowedGoods.Any(good => good.StorableGood.GoodId == transferableGood.GoodId))
          TransferableGoods.Remove(transferableGood);
      }
    }
    
    private void AddNewGoods()
    {
      foreach (var storableGoodAmount in Inventory.AllowedGoods)
      {
        if (!TransferableGoods.Any(good => good.GoodId == storableGoodAmount.StorableGood.GoodId))
        {
          TransferableGoods.Add(new TransferableGood(storableGoodAmount.StorableGood.GoodId, false, false));
        }
      }
    }

    private int MaxAllowedAmount() => _maxCapacity;
    
    private int CompareLackingGoods(
      GoodAmount x,
      GoodAmount y)
    {
      float num = LackingGoodPriority(x);
      return LackingGoodPriority(y).CompareTo(num);
    }

    private float LackingGoodPriority(GoodAmount goodAmount)
    {
      return (float) goodAmount.Amount / (float) MaxAllowedAmount();
    }
  }
}
