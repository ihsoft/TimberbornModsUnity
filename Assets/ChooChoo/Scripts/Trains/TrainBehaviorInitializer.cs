﻿using Bindito.Core;
using Bindito.Unity;
using Timberborn.BehaviorSystem;
using Timberborn.WorkSystem;
using UnityEngine;

namespace ChooChoo
{
  internal class TrainBehaviorInitializer : MonoBehaviour
  {
    private IInstantiator _instantiator;

    [Inject]
    public void InjectDependencies(IInstantiator instantiator) => _instantiator = instantiator;

    public void Awake()
    {
      InitializeExecutors();
      InitializeBehaviors();
    }

    private void InitializeExecutors()
    {
      AddExecutor<TrainDistributeGoodsExecutor>();
      AddExecutor<MoveToStationExecutor>();
      AddExecutor<WaitExecutor>();
    }

    private void AddExecutor<T>() where T : MonoBehaviour, IExecutor => _instantiator.AddComponent<T>(gameObject);

    private void InitializeBehaviors()
    {
      BehaviorManager component = GetComponent<BehaviorManager>();
      // component.AddRootBehavior<WorkerRootBehavior>();
      component.AddRootBehavior<TrainDistributorBehavior>();
      // component.AddRootBehavior<TrainScheduleBehavior>();
    }
  }
}
