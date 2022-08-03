using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using Timberborn.PathSystem;
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
                Name="TestPath.Folktails", 
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
        public MorePathsService(
            BlockService blockService,
            AssetLoader assetLoader)
        {
            _blockService = blockService;
            _assetLoader = assetLoader;
        }
        
        public class CustomDrivewayPath {
            public string Name { get; set; }
            public List<string> DrivewayList { get; set; }
        }
        
        public void Awake(DrivewayModel instance)
        {
            var parameters = new object[] { };
            MethodInfo methodInfo;
            
            methodInfo = typeof(DrivewayModel).GetMethod("GetLocalCoordinates", BindingFlags.NonPublic | BindingFlags.Instance);
            var LocalCoordinates = (Vector3Int)methodInfo.Invoke(instance, parameters);
            methodInfo = typeof(DrivewayModel).GetMethod("GetLocalDirection", BindingFlags.NonPublic | BindingFlags.Instance);
            var LocalDirection = (Direction2D)methodInfo.Invoke(instance, parameters);
            
            foreach (var customDrivewayPath in _customDrivewayPaths)
            {
                instance.GetComponent<CustomDrivewayModel>().InstantiateModel(instance, LocalCoordinates, LocalDirection, customDrivewayPath.Name, customDrivewayPath.DrivewayList);
            }

            /* This code needs to be moved somewhere else, as it now gets called every time a new driveway is initiated. */
            var timberbornpathObjects = Resources.LoadAll("", typeof(DynamicPathModel));
            var mypathObjects = _assetLoader.LoadAll<GameObject>(Plugin.PluginGuid, "tobbert_morepaths").Where(obj => obj.GetComponent<DynamicPathModel>());

            _pathObjects = timberbornpathObjects.Concat(mypathObjects);
        }
        
        public void UpdateAllDriveways(DrivewayModel instance, GameObject model)
        {
            GameObject path = TimberAPI.DependencyContainer.GetInstance<MorePathsService>().GetPath(instance);

            foreach (var pathObject in _pathObjects)
            {
                if (path != null)
                {
                    if (path.name.Replace("(Clone)", "") == pathObject.name)
                    {
                        if (pathObject.name == "Path.Folktails" | pathObject.name == "Path.IronTeeth")
                        {
                            model.SetActive(true);
                            
                            var tempList = instance.GetComponent<CustomDrivewayModel>().drivewayModels;
                            foreach (var tempModel in tempList)
                            {
                                tempModel.SetActive(false);
                            }
                        }
                        else
                        {
                            var tempList = instance.GetComponent<CustomDrivewayModel>().drivewayModels;
                            foreach (var tempModel in tempList)
                            {
                                model.SetActive(false);
                                tempModel.SetActive(tempModel.name == path.name.Replace("(Clone)", ""));
                            }
                        }
                    }
                }
                else
                {
                    var tempList = instance.GetComponent<CustomDrivewayModel>().drivewayModels;
                    foreach (var tempModel in tempList)
                    {
                        model.SetActive(false);
                        tempModel.SetActive(false);
                    }
                }
            }
        }

        public GameObject GetPath(DrivewayModel instance)
        {
            var parameters = new object[] { };
            MethodInfo methodInfo;
            
            methodInfo = typeof(DrivewayModel).GetMethod("GetPositionedDirection", BindingFlags.NonPublic | BindingFlags.Instance);
            var direction = (Direction2D)methodInfo.Invoke(instance, parameters);
            methodInfo = typeof(DrivewayModel).GetMethod("GetPositionedCoordinates", BindingFlags.NonPublic | BindingFlags.Instance);
            var coordinates = (Vector3Int)methodInfo.Invoke(instance, parameters);
            
            Vector3Int checkObjectCoordinates =  coordinates + direction.ToOffset();
            IEnumerable<DynamicPathModel> paths = _blockService.GetObjectsWithComponentAt<DynamicPathModel>(checkObjectCoordinates);

            foreach (var path in paths)
            {
                return path.gameObject;
            }
            return null;
        }
    }
}