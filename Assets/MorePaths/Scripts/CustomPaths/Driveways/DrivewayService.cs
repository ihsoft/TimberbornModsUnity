using System.Collections.Generic;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using Timberborn.PathSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using UnityEngine;

namespace MorePaths
{
    public class DrivewayService : ILoadableSingleton
    {
        private readonly MorePathsCore _morePathsCore;

        private readonly BlockService _blockService;
        
        private readonly DrivewayFactory _drivewayFactory;

        private Dictionary<Driveway, List<GameObject>> _driveways;
        
        DrivewayService(MorePathsCore morePathsCore, BlockService blockService, DrivewayFactory drivewayFactory)
        {
            _morePathsCore = morePathsCore;
            _blockService = blockService;
            _drivewayFactory = drivewayFactory;
        }

        public void Load()
        {
            _driveways = _drivewayFactory.CreateDriveways(_morePathsCore.PathsSpecifications);
        }

        public void InstantiateDriveways(DrivewayModel instance)
        {
            var localCoordinates = (Vector3Int)_morePathsCore.InvokePrivateMethod(instance, "GetLocalCoordinates");
            var localDirection = (Direction2D)_morePathsCore.InvokePrivateMethod(instance, "GetLocalDirection");

            instance.GetComponent<CustomDrivewayModel>().InstantiateModel(instance, localCoordinates, localDirection, _driveways);
        }

        public void UpdateAllDriveways(DrivewayModel instance, GameObject model, ITerrainService terrainService)
        {
            GameObject path = GetPath(instance);

            var direction = (Direction2D)_morePathsCore.InvokePrivateMethod(instance, "GetPositionedDirection");
            var coordinates = (Vector3Int)_morePathsCore.InvokePrivateMethod(instance, "GetPositionedCoordinates");

            Vector3Int checkObjectCoordinates = coordinates + direction.ToOffset();
            bool onGround = terrainService.OnGround(checkObjectCoordinates);

            var tempList = instance.GetComponent<CustomDrivewayModel>().drivewayModels;

            foreach (var pathObject in _morePathsCore.PathObjects)
            {
                if (path != null)
                {
                    if (path.name.Replace("(Clone)", "") == pathObject.name)
                    {
                        if (pathObject.name == "Path.Folktails" | pathObject.name == "Path.IronTeeth")
                        {
                            model.SetActive(true & onGround);

                            foreach (var tempModel in tempList)
                            {
                                tempModel.SetActive(false);
                            }
                        }
                        else
                        {
                            model.SetActive(false);

                            foreach (var tempModel in tempList)
                            {
                                var flag1 = tempModel.name == path.name.Replace("(Clone)", "");
                                var flag2 = path.GetComponent<BlockObject>().Finished;
                                var enabled = flag1 & flag2 & onGround;
                                tempModel.SetActive(enabled);
                            }
                        }
                    }
                }
                else
                {
                    model.SetActive(false);

                    foreach (var tempModel in tempList)
                    {
                        tempModel.SetActive(false);
                    }
                }
            }
        }

        private GameObject GetPath(DrivewayModel instance)
        {
            var direction = (Direction2D)_morePathsCore.InvokePrivateMethod(instance, "GetPositionedDirection");
            var coordinates = (Vector3Int)_morePathsCore.InvokePrivateMethod(instance, "GetPositionedCoordinates");

            Vector3Int checkObjectCoordinates = coordinates + direction.ToOffset();
            IEnumerable<DynamicPathModel> paths = _blockService.GetObjectsWithComponentAt<DynamicPathModel>(checkObjectCoordinates);

            foreach (var path in paths)
            {
                return path.gameObject;
            }

            return null;
        }
    }
}
