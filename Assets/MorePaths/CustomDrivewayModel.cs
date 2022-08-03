using System;
using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using HarmonyLib;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.PathSystem;
using Timberborn.PrefabOptimization;
using TimberbornAPI.AssetLoaderSystem.AssetSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace MorePaths
{
  public class CustomDrivewayModel : MonoBehaviour
  {
    public OptimizedPrefabInstantiator OptimizedPrefabInstantiator;
    public AssetLoader AssetLoader;
    public List<GameObject> drivewayModels = new List<GameObject>();
    
    [Inject]
    public void InjectDependencies(
      OptimizedPrefabInstantiator optimizedPrefabInstantiator,
      AssetLoader assetLoader)
    {
      OptimizedPrefabInstantiator = optimizedPrefabInstantiator;
      AssetLoader = assetLoader;
    }
    
    public void InstantiateModel(
      DrivewayModel drivewayModel,
      Vector3Int coordinates,
      Direction2D direction, 
      string drivewayName,
      List<string> drivewayList
    )
    {
      // drivewayModels.Add(new GameObject());
      // int index = drivewayModels.Count() - 1;
      // var model = drivewayModels[index];
      var model = OptimizedPrefabInstantiator.Instantiate(GetModelPrefab(drivewayModel.Driveway, drivewayList), drivewayModel.GetComponent<BuildingModel>().FinishedModel.transform);
      model.transform.localPosition = CoordinateSystem.GridToWorld(BlockCalculations.Pivot(coordinates, direction.ToOrientation()));
      model.transform.localRotation = direction.ToWorldSpaceRotation();
      model.name = drivewayName;
      Plugin.Log.LogFatal(model.name);
      drivewayModels.Add(model);
      // Plugin.Log.LogFatal(DrivewayModels.Count().ToString());
    }

    public GameObject GetModelPrefab(Driveway driveway, List<string> drivewayList)
    {
      switch (driveway)
      {
        case Driveway.NarrowLeft:
          return AssetLoader.Load<GameObject>(drivewayList[0]);
        case Driveway.NarrowCenter:
          return AssetLoader.Load<GameObject>(drivewayList[1]);
        case Driveway.NarrowRight:
          return AssetLoader.Load<GameObject>(drivewayList[2]);
        case Driveway.WideCenter:
          return AssetLoader.Load<GameObject>(drivewayList[3]);
        case Driveway.LongCenter:
          return AssetLoader.Load<GameObject>(drivewayList[4]);
        case Driveway.StraightPath:
          return AssetLoader.Load<GameObject>(drivewayList[5]);
        default:
          throw new ArgumentOutOfRangeException(nameof (driveway), (object) driveway, (string) null);
      }
    }
  }
}
