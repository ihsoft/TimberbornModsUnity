using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bindito.Core;
using HarmonyLib;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.PathSystem;
using Timberborn.PrefabOptimization;
using UnityEngine;

namespace MorePaths
{
  public class CustomDrivewayModel : MonoBehaviour
  {
    private OptimizedPrefabInstantiator _optimizedPrefabInstantiator;
    public List<GameObject> drivewayModels = new ();
    
    [Inject]
    public void InjectDependencies(OptimizedPrefabInstantiator optimizedPrefabInstantiator)
    {
      _optimizedPrefabInstantiator = optimizedPrefabInstantiator;
    }
    
    public void InstantiateModel(
      DrivewayModel drivewayModel,
      Vector3Int coordinates,
      Direction2D direction,
      Dictionary<Driveway, List<GameObject>> driveways
    )
    {
      foreach (var prefab in driveways[drivewayModel.Driveway])
      {
        var model = _optimizedPrefabInstantiator.Instantiate(prefab, drivewayModel.GetComponent<BuildingModel>().FinishedModel.transform);
        model.transform.localPosition = CoordinateSystem.GridToWorld(BlockCalculations.Pivot(coordinates, direction.ToOrientation()));
        model.transform.localRotation = direction.ToWorldSpaceRotation();
        model.name = prefab.name;
        drivewayModels.Add(model);
      }
    }
  }
}
