using Bindito.Core;
using System.Collections.Generic;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using UnityEngine;

namespace ChooChoo
{
  public class TrainYardDescriber : MonoBehaviour, IEntityDescriber
  {
    private static readonly string CapacityLocKey = "Inventory.Capacity";
    private ILoc _loc;
    private TrainYard _trainYard;

    [Inject]
    public void InjectDependencies(ILoc loc) => _loc = loc;

    public void Awake() => _trainYard = GetComponent<TrainYard>();

    public IEnumerable<EntityDescription> DescribeEntity()
    {
      if (!_trainYard.enabled)
        yield return EntityDescription.CreateTextSection($"{SpecialStrings.RowStarter}{_loc.T(CapacityLocKey)} {_trainYard.MaxCapacity}", 100);
    }
  }
}
