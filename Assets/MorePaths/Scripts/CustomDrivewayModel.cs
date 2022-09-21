using System;
using System.Collections.Generic;
using Bindito.Core;
using TimberApi.AssetSystem;
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
    private IAssetLoader _assetLoader;
    public List<GameObject> drivewayModels = new ();
    
    [Inject]
    public void InjectDependencies(
      OptimizedPrefabInstantiator optimizedPrefabInstantiator,
      IAssetLoader assetLoader)
    {
      _optimizedPrefabInstantiator = optimizedPrefabInstantiator;
      _assetLoader = assetLoader;
    }
    
    public void InstantiateModel(
      DrivewayModel drivewayModel,
      Vector3Int coordinates,
      Direction2D direction, 
      string drivewayName,
      List<string> drivewayList
    )
    {
      var model = _optimizedPrefabInstantiator.Instantiate(GetModelPrefab(drivewayModel.Driveway, drivewayList), drivewayModel.GetComponent<BuildingModel>().FinishedModel.transform);
      model.transform.localPosition = CoordinateSystem.GridToWorld(BlockCalculations.Pivot(coordinates, direction.ToOrientation()));
      model.transform.localRotation = direction.ToWorldSpaceRotation();
      model.name = drivewayName;
      drivewayModels.Add(model);
    }

    public GameObject GetModelPrefab(Driveway driveway, List<string> drivewayList)
    {
      switch (driveway)
      {
        case Driveway.NarrowLeft:
          return _assetLoader.Load<GameObject>(drivewayList[0]);
        case Driveway.NarrowCenter:
          return _assetLoader.Load<GameObject>(drivewayList[1]);
        case Driveway.NarrowRight:
          return _assetLoader.Load<GameObject>(drivewayList[2]);
        case Driveway.WideCenter:
          return _assetLoader.Load<GameObject>(drivewayList[3]);
        case Driveway.LongCenter:
          return _assetLoader.Load<GameObject>(drivewayList[4]);
        case Driveway.StraightPath:
          return _assetLoader.Load<GameObject>(drivewayList[5]);
        default:
          throw new ArgumentOutOfRangeException(nameof (driveway), driveway, null);
      }
    }
  }
}
