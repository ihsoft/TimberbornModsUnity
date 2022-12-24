using Bindito.Core;
using Timberborn.BehaviorSystem;
using Timberborn.FactionSystemGame;
using Timberborn.Goods;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class TrainDistributeGoodsExecutor : MonoBehaviour, IExecutor
  {
    private DistributorTrain _distributorTrain;
   
    public void Awake() => _distributorTrain = GetComponent<DistributorTrain>();

    public bool Launch(GoodsStation startGoodStation, TransferableGood transferableGood, GoodsStation endGoodStation, int goodAmount)
    {
      return _distributorTrain.Distribute(startGoodStation, endGoodStation, new GoodAmount(transferableGood.GoodId, goodAmount));
    }

    public ExecutorStatus Tick(float deltaTimeInHours)
    {
      return _distributorTrain.Delivered() ? ExecutorStatus.Success : ExecutorStatus.Running;
    }

    public void Save(IEntitySaver entitySaver)
    {
    }

    public void Load(IEntityLoader entityLoader)
    {
    }
  }
}
