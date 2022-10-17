using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TimberApi.Common.SingletonSystem;
using TimberApi.DependencyContainerSystem;
using Timberborn.AssetSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.FactionSystemGame;
using Timberborn.MasterSceneLoading;
using Timberborn.PathSystem;
using Timberborn.Persistence;
using Timberborn.SceneLoading;
using Timberborn.TerrainSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MorePaths
{
    public class MorePathsService : ITimberApiLoadableSingleton
    {
        private IEnumerable<Object> _pathObjects;

        private readonly BlockService _blockService;
        private readonly ISpecificationService _specificationService;
        private readonly IResourceAssetLoader _resourceAssetLoader;
        private readonly FactionService _factionService;
        private readonly SceneLoader _sceneLoader;
        private readonly PathSpecificationObjectDeserializer _pathSpecificationObjectDeserializer;
        private readonly DrivewayFactory _drivewayFactory;

        private MethodInfo _methodInfo;
        private ImmutableArray<PathSpecification> _pathsSpecifications;
        public List<FakePath> FakePaths;
        private Dictionary<Driveway, List<GameObject>> _driveways;
        private readonly Dictionary<string, FieldInfo> _fieldInfos = new();
        private readonly Dictionary<string, MethodInfo> _methodInfos = new();
        
        
        // private static GameObject OriginalPathGameObject {
        //     get
        //     {
        //         var folktails = Resources.Load<GameObject>("Buildings/Paths/Path/Path.Folktails");
        //         if (folktails != null)
        //         {
        //             return folktails;
        //         }
        //         
        //         var ironTeeth = Resources.Load<GameObject>("Buildings/Paths/Path/Path.IronTeeth");
        //         if (ironTeeth != null)
        //         {
        //             return ironTeeth;
        //         }
        //         
        //         return  null;
        //     }
        // }
            
        private GameObject PathCorner => _resourceAssetLoader.Load<GameObject>("tobbert.morepaths/tobbert_morepaths/PathCorner");

        public MorePathsService(
            BlockService blockService, 
            ISpecificationService specificationService,
            IResourceAssetLoader resourceAssetLoader,
            FactionService factionService,
            SceneLoader sceneLoader,
            PathSpecificationObjectDeserializer pathSpecificationObjectDeserializer,
            DrivewayFactory drivewayFactory,
            EntityService entityService,
            BlockObjectFactory blockObjectFactory,
            BlockObjectPlacerService blockObjectPlacerService)
        {
            _blockService = blockService;
            _specificationService = specificationService;
            _resourceAssetLoader = resourceAssetLoader;
            _factionService = factionService;
            _sceneLoader = sceneLoader;
            _pathSpecificationObjectDeserializer = pathSpecificationObjectDeserializer;
            _drivewayFactory = drivewayFactory;
        }

        public void Load()
        {
            LoadPathSpecifications();
        }

        private void LoadPathSpecifications()
        {
            _pathsSpecifications = _specificationService.GetSpecifications(_pathSpecificationObjectDeserializer).Where(specification => specification.Enabled).ToImmutableArray();
        }

        private void CreatePathsFromSpecification()
        {
            // Stopwatch stopwatch = Stopwatch.StartNew();
            
            PreventInstantiatePatch.RunInstantiate = false;
            
            var originalPathGameObject = Resources.Load<GameObject>("Buildings/Paths/Path/Path." + _factionService.Current.Id);
            originalPathGameObject.AddComponent<DynamicPathCorner>();
            FakePaths = _pathsSpecifications.Select(specification => new FakePath(this, Object.Instantiate(originalPathGameObject), Object.Instantiate(PathCorner), specification)).ToList();
            FakePaths.ForEach(path => path.Create());
            _driveways = _drivewayFactory.CreateDriveways(_pathsSpecifications);
            PreventInstantiatePatch.RunInstantiate = true;
            
            
            // stopwatch.Stop();
            // Plugin.Log.LogInfo("Total: " + stopwatch.ElapsedMilliseconds);
        }

        private void LoadAllPathObjects()
        {
            var timberbornPathObjects = Resources.LoadAll("", typeof(DynamicPathModel));
            _pathObjects = timberbornPathObjects.Concat(FakePaths.Select(path => path.PathGameObject));
        }

        public void AddFakePathsToObjectsPatch(ref IEnumerable<Object> result)
        {
            if (FakePaths == null)
            {
                CreatePathsFromSpecification();
                LoadAllPathObjects();
            }

            
            var pathGameObject = result.First(o => o.name.Split(".")[0] == "Path");
            if (pathGameObject == null) return;

            var resultList = result.ToList();
            resultList.Remove(pathGameObject);

            var newObjectsList = resultList.Concat(FakePaths.Select(path => path.PathGameObject));

            result = newObjectsList;
        }

        public void InstantiateDriveways(DrivewayModel instance)
        {
            var localCoordinates = (Vector3Int)InvokePrivateMethod(instance, "GetLocalCoordinates");
            var localDirection = (Direction2D)InvokePrivateMethod(instance, "GetLocalDirection");
            
            instance.GetComponent<CustomDrivewayModel>().InstantiateModel(instance, localCoordinates, localDirection, _driveways);
        }
        
        public void UpdateAllDriveways(DrivewayModel instance, GameObject model, ITerrainService terrainService)
        {
            GameObject path = GetPath(instance);

            var direction = (Direction2D)InvokePrivateMethod(instance, "GetPositionedDirection");
            var coordinates = (Vector3Int)InvokePrivateMethod(instance, "GetPositionedCoordinates");

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

        private GameObject GetPath(DrivewayModel instance)
        {
            var direction = (Direction2D)InvokePrivateMethod(instance, "GetPositionedDirection");
            var coordinates = (Vector3Int)InvokePrivateMethod(instance, "GetPositionedCoordinates");
            
            Vector3Int checkObjectCoordinates =  coordinates + direction.ToOffset();
            IEnumerable<DynamicPathModel> paths = _blockService.GetObjectsWithComponentAt<DynamicPathModel>(checkObjectCoordinates);

            foreach (var path in paths)
            {
                return path.gameObject;
            }
            return null;
        }

        private object InvokePrivateMethod(object instance, string methodName)
        {
            if (!_methodInfos.ContainsKey(methodName))
            {
                _methodInfos.Add(methodName, AccessTools.TypeByName(instance.GetType().Name).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            
            return _methodInfos[methodName].Invoke(instance, new object[]{});
        }

        public void ChangePrivateField(object instance, string fieldName, object newValue)
        {
            if (!_fieldInfos.ContainsKey(fieldName))
            {
                _fieldInfos.Add(fieldName, AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            
            _fieldInfos[fieldName].SetValue(instance, newValue);
        }

        public object GetPrivateField(object instance, string fieldName)
        {
            if (!_fieldInfos.ContainsKey(fieldName))
            {
                _fieldInfos.Add(fieldName, AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            
            return _fieldInfos[fieldName].GetValue(instance);
        }
    }
}