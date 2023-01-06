using System;
using Timberborn.BehaviorSystem;
using UnityEngine;

namespace ChooChoo
{
  public class BringDistributableGoodTrainBehavior : RootBehavior
  {
    private DistributableGoodBringerTrain _distributableGoodBringerTrain;

    private void Awake() => _distributableGoodBringerTrain = GetComponent<DistributableGoodBringerTrain>();

    public override Decision Decide(GameObject agent) => !_distributableGoodBringerTrain.BringDistributableGoods() ? Decision.ReleaseNow() : Decision.ReleaseNextTick();
  }
}
