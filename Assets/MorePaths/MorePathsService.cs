using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.PathSystem;
using Timberborn.PreviewSystem;
using Timberborn.TerrainSystem;
using TimberbornAPI;
using TimberbornAPI.AssetLoaderSystem.AssetSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MorePaths
{
    public class MorePathsService
    {
        private IEnumerable<Object> _pathObjects;
        private readonly List<CustomDrivewayPath>  _customDrivewayPaths = new List<CustomDrivewayPath>
        {
            new CustomDrivewayPath { 
                Name="StonePath", 
                DrivewayList = 
                    new List<string>()
                    {      
                        "tobbert.morepaths/tobbert_morepaths/DirtDrivewayNarrowLeft_0",
                        "tobbert.morepaths/tobbert_morepaths/DirtDrivewayNarrowCenter_0",
                        "tobbert.morepaths/tobbert_morepaths/DirtDrivewayNarrowRight_0",
                        "tobbert.morepaths/tobbert_morepaths/DirtDrivewayWideCenter_0",
                        "tobbert.morepaths/tobbert_morepaths/DirtDrivewayLongCenter_0",
                        "tobbert.morepaths/tobbert_morepaths/DirtDrivewayStraightPath_0"
                    }},
            new CustomDrivewayPath { 
                Name="MetalPath", 
                DrivewayList = 
                    new List<string>()
                    {      
                        "tobbert.morepaths/tobbert_morepaths/MetalDrivewayNarrowLeft_0",
                        "tobbert.morepaths/tobbert_morepaths/MetalDrivewayNarrowCenter_0",
                        "tobbert.morepaths/tobbert_morepaths/MetalDrivewayNarrowRight_0",
                        "tobbert.morepaths/tobbert_morepaths/MetalDrivewayWideCenter_0",
                        "tobbert.morepaths/tobbert_morepaths/MetalDrivewayLongCenter_0",
                        "tobbert.morepaths/tobbert_morepaths/MetalDrivewayStraightPath_0"
                    }}
        };

        private BlockService _blockService;
        private AssetLoader _assetLoader;
        private IConnectionService _connectionService;
        
        object[] parameters = new object[] { };
        MethodInfo methodInfo;

        public PlaceableBlockObject previewPrefab;

        public MorePathsService(
            BlockService blockService,
            AssetLoader assetLoader,
            IConnectionService connectionService)
        {
            _blockService = blockService;
            _assetLoader = assetLoader;
            _connectionService = connectionService;
        }
        
        public class CustomDrivewayPath {
            public string Name { get; set; }
            public List<string> DrivewayList { get; set; }
        }
        
        public void Awake(DrivewayModel instance)
        {
            var localCoordinates = GetLocalCoordinates(instance);
            var localDirection = GetLocalDirection(instance);
            
            foreach (var customDrivewayPath in _customDrivewayPaths)
            {
                instance.GetComponent<CustomDrivewayModel>().InstantiateModel(instance, localCoordinates, localDirection, customDrivewayPath.Name, customDrivewayPath.DrivewayList);
            }

            /* This code needs to be moved somewhere else, as it now gets called every time a new driveway is initiated. */
            var timberbornpathObjects = Resources.LoadAll("", typeof(DynamicPathModel));
            var mypathObjects = _assetLoader.LoadAll<GameObject>(Plugin.PluginGuid, "tobbert_morepaths").Where(obj => obj.GetComponent<DynamicPathModel>());

            _pathObjects = timberbornpathObjects.Concat(mypathObjects);
        }
        
        public void UpdateAllDriveways(DrivewayModel instance, GameObject model, ITerrainService terrainService)
        {
            GameObject path = TimberAPI.DependencyContainer.GetInstance<MorePathsService>().GetPath(instance);
            
            var direction = GetPositionedDirection(instance);
            var coordinates = GetPositionedCoordinates(instance);
            
            Vector3Int checkObjectCoordinates =  coordinates + direction.ToOffset();
            bool onGround = terrainService.OnGround(checkObjectCoordinates);
            
            var tempList = instance.GetComponent<CustomDrivewayModel>().drivewayModels;

            foreach (var pathObject in _pathObjects)
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

        public GameObject GetPath(DrivewayModel instance)
        {
            var direction = GetPositionedDirection(instance);
            var coordinates = GetPositionedCoordinates(instance);
            
            Vector3Int checkObjectCoordinates =  coordinates + direction.ToOffset();
            IEnumerable<DynamicPathModel> paths = _blockService.GetObjectsWithComponentAt<DynamicPathModel>(checkObjectCoordinates);

            foreach (var path in paths)
            {
                return path.gameObject;
            }
            return null;
        }

        public Direction2D GetPositionedDirection(DrivewayModel instance)
        {
            methodInfo = typeof(DrivewayModel).GetMethod("GetPositionedDirection", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Direction2D)methodInfo.Invoke(instance, parameters);
        }
        
        public Vector3Int GetPositionedCoordinates(DrivewayModel instance)
        {
            methodInfo = typeof(DrivewayModel).GetMethod("GetPositionedCoordinates", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Vector3Int)methodInfo.Invoke(instance, parameters);
        }
        
        public Vector3Int GetLocalCoordinates(DrivewayModel instance)
        {
            methodInfo = typeof(DrivewayModel).GetMethod("GetLocalCoordinates", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Vector3Int)methodInfo.Invoke(instance, parameters);
        }
        
        public Direction2D GetLocalDirection(DrivewayModel instance)
        {
            methodInfo = typeof(DrivewayModel).GetMethod("GetLocalDirection", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Direction2D)methodInfo.Invoke(instance, parameters);
        }
    }
}