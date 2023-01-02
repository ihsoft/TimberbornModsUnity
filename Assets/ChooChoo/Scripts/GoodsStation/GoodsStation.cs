using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.Common;
using Timberborn.ConstructibleSystem;
using Timberborn.EntitySystem;
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
      _goodsStationsRepository.Register(this);
      if (TransferableGoods == null)
      {
        var list = new List<TransferableGood>();
        foreach (var storableGoodAmount in Inventory.AllowedGoods)
          list.Add(new TransferableGood(storableGoodAmount.StorableGood.GoodId, false, false));

        TransferableGoods = list;
      }
      
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
  }
}
