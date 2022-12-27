using Timberborn.BehaviorSystem;
using Timberborn.Goods;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class TrainDistributeGoodsExecutor : MonoBehaviour, IExecutor
  {
    private DistributorTrain _distributorTrain;
   
    public void Awake() => _distributorTrain = GetComponent<DistributorTrain>();

    public ExecutorStatus Launch(GoodsStation startGoodStation, GoodsStation endGoodStation, GoodAmount goodAmount) => _distributorTrain.Distribute(startGoodStation, endGoodStation, goodAmount);

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
