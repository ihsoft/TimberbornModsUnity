using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Timers;
using HarmonyLib;
using Timberborn.BehaviorSystem;
using Timberborn.CharacterModelSystem;
using Timberborn.Common;
using Timberborn.EnterableSystem;
using Timberborn.Navigation;
using Timberborn.NeedBehaviorSystem;
using Timberborn.NeedSystem;
using Timberborn.SlotSystem;
using Timberborn.TickSystem;
using Timberborn.WalkingSystem;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MultithreadedNavigation
{
    public class MultithreadedNavigationService : ITickableSingleton
    {
        // public readonly Dictionary<int, MyBehaviorManager> BehaviorManagers = new();
        public readonly List<MyBehaviorManager> BehaviorManagers = new();

        Stopwatch stopwatch = Stopwatch.StartNew();
        
        public void Tick()
        {
            Plugin.Log.LogInfo("Tick");
            
            
            stopwatch.Start();
            NativeArray<MyBehaviorManager.Data> behaviorManagers = new(BehaviorManagers.Count, Allocator.TempJob);
            for (int i = 0; i < BehaviorManagers.Count; i++)
            {
                // behaviorManagers[i] = new MyBehaviorManager.Data(BehaviorManagers[i], ref BehaviorManagers[i].returnToBehavior, ref BehaviorManagers[i].runningBehavior, ref BehaviorManagers[i].rootBehaviors, ref BehaviorManagers[i].runningExecutor);
                behaviorManagers[i] = new MyBehaviorManager.Data(BehaviorManagers[i]);
            }
            var job = new MultithreadedNavigationJob { MyBehaviorManagers = behaviorManagers };
            var jobHandle = job.Schedule(behaviorManagers.Length, behaviorManagers.Length / 10);
            jobHandle.Complete();
            behaviorManagers.Dispose();
            BehaviorManagers.Clear();
            stopwatch.Stop();
            Plugin.Log.LogFatal(stopwatch.ElapsedTicks);
            stopwatch.Reset();
        }
        
        private static readonly object PickBestActionLock = new object();
        private bool _secondCallPickBestAction;
        
        public Tuple<AppraisedAction?, bool> LockedPickBestAction(
            DistrictNeedBehaviorService __instance,
            NeedManager needManager,
            Vector3 essentialActionPosition,
            float hoursLeftForNonEssentialActions,
            NeedFilter needFilter)
        {
            lock (PickBestActionLock)
            {
                if (_secondCallPickBestAction)
                {               
                    // Plugin.Log.LogInfo("second call");

                    _secondCallPickBestAction = false;
                    return Tuple.Create(default(AppraisedAction?), true);
                }
                // Plugin.Log.LogInfo("first call");
                _secondCallPickBestAction = true;
                return Tuple.Create(__instance.PickBestAction(needManager, essentialActionPosition, hoursLeftForNonEssentialActions, needFilter), false);
                
            }
        }

        
        private static readonly object GetRandomDestinationLock = new object();
        private bool _secondCallGetRandomDestination;
        
        public Tuple<Vector3, bool> LockedGetRandomDestination(
            DistrictDestinationPicker __instance,
            District district, 
            Vector3 coordinates)
        {
            lock (GetRandomDestinationLock)
            {
                if (_secondCallPickBestAction)
                {               
                    // Plugin.Log.LogInfo("second call");

                    _secondCallPickBestAction = false;
                    return Tuple.Create(new Vector3(), true);
                }
                // Plugin.Log.LogInfo("first call");
                _secondCallPickBestAction = true;
                
                MethodInfo methodInfo = typeof(DistrictDestinationPicker).GetMethod("GetRandomDestination", BindingFlags.Public | BindingFlags.Instance);
                return Tuple.Create((Vector3)methodInfo.Invoke(__instance, new object[] {district, coordinates}), false);
                
                // return Tuple.Create(__instance.GetRandomDestination(district, coordinates), false);
            }
        }

        // private static readonly object GetAddEnterer = new object();
        // private bool _secondCallAddEnterer;
        //
        // public Tuple<bool, bool> LockedAddEnterer(
        //     SlotManager __instance,
        //     Enterer enterer, 
        //     bool assign)
        // {
        //     lock (GetAddEnterer)
        //     {
        //         if (_secondCallAddEnterer)
        //         {               
        //             // Plugin.Log.LogInfo("second call");
        //             _secondCallAddEnterer = false;
        //             return Tuple.Create(default(bool), true);
        //         }
        //         // Plugin.Log.LogInfo("first call");
        //         _secondCallAddEnterer = true;
        //         return Tuple.Create(__instance.AddEnterer(enterer, assign), false);
        //     }
        // }
        
        // private static readonly object GetUpdateVisibility = new object();
        // private bool _secondCallUpdateVisibility;
        //
        // public bool LockedUpdateVisibility(CharacterModel __instance)
        // {
        //     lock (GetUpdateVisibility)
        //     {
        //         if (_secondCallUpdateVisibility)
        //         {               
        //             // Plugin.Log.LogInfo("second call");
        //             _secondCallUpdateVisibility = false;
        //             return true;
        //         }
        //         // Plugin.Log.LogInfo("first call");
        //         _secondCallUpdateVisibility = true;
        //         
        //         MethodInfo methodInfo = typeof(CharacterModel).GetMethod("UpdateVisibility", BindingFlags.NonPublic | BindingFlags.Instance);
        //         methodInfo.Invoke(__instance, new object[] {});
        //         return false;
        //     }
        // }

        private static readonly object AddNodeLock = new object();
        private bool _secondCallAddNode;
        
        public bool LockedAddNode(
            RoadSpillFlowField __instance,
            int nodeId, 
            int parentNodeId, 
            int roadParentNodeId, 
            float distanceToRoad)
        {
            lock (AddNodeLock)
            {
                if (_secondCallAddNode)
                {               
                    // Plugin.Log.LogInfo("second call");

                    _secondCallAddNode = false;
                    return true;
                }
                // Plugin.Log.LogInfo("first call");
                _secondCallAddNode = true;
                
                MethodInfo methodInfo = typeof(RoadSpillFlowField).GetMethod("AddNode", BindingFlags.Public | BindingFlags.Instance);
                methodInfo.Invoke(__instance, new object[] { nodeId, parentNodeId, roadParentNodeId, distanceToRoad });
                return false;
                
                // return Tuple.Create(__instance.GetRandomDestination(district, coordinates), false);
            }
        }
        
        private static readonly object PreReconstructPathLock = new object();
        private bool _secondCallPreReconstructPath;
        
        public bool LockedPreReconstructPath(
            PathReconstructor __instance,
            IFlowField flowField,
            Vector3 start,
            Vector3 destination,
            List<Vector3> pathCorners)
        {
            lock (PreReconstructPathLock)
            {
                if (_secondCallPreReconstructPath)
                {
                    _secondCallPreReconstructPath = false;
                    return true;
                }
                _secondCallPreReconstructPath = true;
                
                MethodInfo methodInfo = typeof(PathReconstructor).GetMethod("PreReconstructPath", BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo.Invoke(__instance, new object[] { flowField, start, destination, pathCorners });
                return false;
            }
        }
        
        private static readonly object AddFillFlowFieldWithPath = new object();
        private bool _secondCallFillFlowFieldWithPath1;
        
        public bool LockedFillFlowFieldWithPath1(
            TerrainAStarPathfinder __instance,
            TerrainNavMeshGraph terrainNavMeshGraph,
            PathFlowField flowField,
            float maxDistance,
            int startNodeId,
            int destinationNodeId)
        {
            lock (AddFillFlowFieldWithPath)
            {
                if (_secondCallFillFlowFieldWithPath1)
                {
                    _secondCallFillFlowFieldWithPath1 = false;
                    return true;
                }
                _secondCallFillFlowFieldWithPath1 = true;

                // __instance.FillFlowFieldWithPath(terrainNavMeshGraph, flowField, maxDistance, startNodeId, destinationNodeId);
                Traverse.Create(__instance).Method("FillFlowFieldWithPath", terrainNavMeshGraph, flowField, maxDistance, startNodeId, destinationNodeId);
                // MethodInfo methodInfo = typeof(TerrainAStarPathfinder).GetMethod("FillFlowFieldWithPath", BindingFlags.Public | BindingFlags.Instance);
                // methodInfo.Invoke(__instance, new object[] { terrainNavMeshGraph, flowField, maxDistance, startNodeId, destinationNodeId });
                return false;
            }
        }
        
        private bool _secondCallFillFlowFieldWithPath2;
        
        public bool LockedFillFlowFieldWithPath2(
            TerrainAStarPathfinder __instance,
            TerrainNavMeshGraph terrainNavMeshGraph,
            PathFlowField flowField,
            float maxDistance,
            int startNodeId,
            IReadOnlyList<int> destinationNodeIds,
            int destinationNodeId)
        {
            lock (AddFillFlowFieldWithPath)
            {
                if (_secondCallFillFlowFieldWithPath2)
                {
                    _secondCallFillFlowFieldWithPath2 = false;
                    return true;
                }
                _secondCallFillFlowFieldWithPath2 = true;

                Traverse.Create(__instance).Method("FillFlowFieldWithPath", terrainNavMeshGraph, flowField, maxDistance, startNodeId, destinationNodeIds, destinationNodeId);
                return false;
            }
        }
        
        
        private bool _secondCallPushNode;
        
        // public bool LockedPushNode(
        //     TerrainAStarPathfinder __instance,
        //     int nodeId, 
        //     int parentNodeId, 
        //     float gScore)
        // {
        //     lock (AddFillFlowFieldWithPath)
        //     {
        //         if (_secondCallPushNode)
        //         {               
        //             // Plugin.Log.LogInfo("second call");
        //
        //             _secondCallPushNode = false;
        //             return true;
        //         }
        //         // Plugin.Log.LogInfo("first call");
        //         _secondCallPushNode = true;
        //         
        //         MethodInfo methodInfo = typeof(TerrainAStarPathfinder).GetMethod("PushNode", BindingFlags.NonPublic | BindingFlags.Instance);
        //         methodInfo.Invoke(__instance, new object[] { nodeId, parentNodeId, gScore });
        //         return false;
        //         
        //         // return Tuple.Create(__instance.GetRandomDestination(district, coordinates), false);
        //     }
        // }
        
        
        private static readonly object FindPathInFlowFieldLock1 = new object();
        private bool _secondCallFindPathInFlowField1;
        
        public Tuple<bool, bool> LockedFindPathInFlowField1(
            FlowFieldPathFinder __instance,
            PathFlowField flowField,
            Vector3 start,
            Vector3 destination,
            out float distance,
            List<Vector3> pathCorners)
        {
            lock (FindPathInFlowFieldLock1)
            {
                distance = 0;
                if (_secondCallFindPathInFlowField1)
                {
                    _secondCallFindPathInFlowField1 = false;
                    return Tuple.Create(default(bool), true);
                }
                _secondCallFindPathInFlowField1 = true;
                
                MethodInfo methodInfo = typeof(FlowFieldPathFinder).GetMethod("FindPathInFlowField", new[]{typeof(PathFlowField), typeof(Vector3), typeof(Vector3), typeof(float).MakeByRefType(), typeof(List<Vector3>)});
                return Tuple.Create((bool)methodInfo.Invoke(__instance, new object[] {flowField, start,destination, distance, pathCorners}), false);

                // return Tuple.Create((bool)Traverse.Create(__instance).Method("FillFlowFieldWithPath", flowField, start, destination, distance, pathCorners).GetValue(), false);
                // return Tuple.Create(__instance.GetRandomDestination(district, coordinates), false);
            }
        }
        
        private static readonly object FindPathInFlowFieldLock2 = new object();
        private bool _secondCallFindPathInFlowField2;
        
        public Tuple<bool, bool> LockedFindPathInFlowField2(
            FlowFieldPathFinder __instance,
            PathFlowField pathFlowField,
            RoadSpillFlowField roadSpillFlowField,
            Vector3 start,
            Vector3 destination,
            out float distance,
            List<Vector3> pathCorners)
        {
            lock (FindPathInFlowFieldLock2)
            {
                distance = 0;
                if (_secondCallFindPathInFlowField2)
                {
                    _secondCallFindPathInFlowField2
                        = false;
                    return Tuple.Create(default(bool), true);
                }
                _secondCallFindPathInFlowField2 = true;
                
                MethodInfo methodInfo = typeof(FlowFieldPathFinder).GetMethod("FindPathInFlowField", new[]{typeof(PathFlowField), typeof(RoadSpillFlowField), typeof(Vector3), typeof(Vector3), typeof(float).MakeByRefType(), typeof(List<Vector3>)});
                return Tuple.Create((bool)methodInfo.Invoke(__instance, new object[] {pathFlowField, roadSpillFlowField, start,destination, distance, pathCorners}), false);

                // return Tuple.Create((bool)Traverse.Create(__instance).Method("FillFlowFieldWithPath", flowField, start, destination, distance, pathCorners).GetValue(), false);
                // return Tuple.Create(__instance.GetRandomDestination(district, coordinates), false);
            }
        }
        
        
        // public readonly object GetParentIdLock = new object();
        // private bool _secondCallGetParentId;
        //
        // public Tuple<int, bool> LockedGetParentId(
        //     PathFlowField __instance,
        //     int nodeId)
        // {
        //     lock (GetParentIdLock)
        //     {
        //         if (_secondCallGetParentId)
        //         {
        //             _secondCallGetParentId = false;
        //             return Tuple.Create(default(int), true);
        //         }
        //         _secondCallGetParentId = true;
        //         
        //         MethodInfo methodInfo = typeof(PathFlowField).GetMethod("GetParentId", BindingFlags.Public | BindingFlags.Instance);
        //         return Tuple.Create((int)methodInfo.Invoke(__instance, new object[] {nodeId}), false);
        //     }
        // }
        // private bool _secondCallHasNode;
        //
        // public Tuple<bool, bool> LockedHasNode(
        //     PathFlowField __instance,
        //     int nodeId)
        // {
        //     lock (GetParentIdLock)
        //     {
        //         if (_secondCallHasNode)
        //         {
        //             _secondCallHasNode = false;
        //             return Tuple.Create(default(bool), true);
        //         }
        //         _secondCallHasNode = true;
        //         
        //         MethodInfo methodInfo = typeof(PathFlowField).GetMethod("HasNode", BindingFlags.Public | BindingFlags.Instance);
        //         return Tuple.Create((bool)methodInfo.Invoke(__instance, new object[] {nodeId}), false);
        //     }
        // }
        // private bool _secondCallClear;
        //
        // public bool LockedClear(
        //     PathFlowField __instance,
        //     int startNodeId, 
        //     float maxDistance)
        // {
        //     lock (GetParentIdLock)
        //     {
        //         if (_secondCallGetParentId)
        //         {
        //             _secondCallGetParentId = false;
        //             return true;
        //         }
        //         _secondCallGetParentId = true;
        //         
        //         MethodInfo methodInfo = typeof(PathFlowField).GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
        //         methodInfo.Invoke(__instance, new object[] { startNodeId, maxDistance });
        //         return false;
        //     }
        // }
        
        // private static readonly object PathFlowFieldSetterLock = new object();
        // private bool _secondCallPathFlowFieldSetter;
        //
        // public Tuple<int, bool> LockedPathFlowFieldSetter(
        //     PathFlowField __instance,
        //     int nodeId)
        // {
        //     lock (PathFlowFieldSetterLock)
        //     {
        //         if (_secondCallPathFlowFieldSetter)
        //         {
        //             _secondCallPathFlowFieldSetter = false;
        //             return Tuple.Create(default(int), true);
        //         }
        //         _secondCallPathFlowFieldSetter = true;
        //         
        //         MethodInfo methodInfo = typeof(PathFlowField).GetMethod("GetParentId", BindingFlags.Public | BindingFlags.Instance);
        //         return Tuple.Create((int)methodInfo.Invoke(__instance, new object[] {nodeId}), false);
        //     }
        // }




        private readonly Dictionary<string, bool> _secondCalls = new();
        private static readonly object FunctionLock = new object();
        
        public Tuple<object, bool> LockedNonVoidFunction(object __instance, object[] __args, MethodBase __originalMethod)
        {
            lock (FunctionLock)
            {
                var methodName = __originalMethod.Name;
                if (!_secondCalls.ContainsKey(methodName))
                {
                    _secondCalls.Add(methodName, false);
                }
                
                // Plugin.Log.LogFatal(methodName + "                                  " + _secondCalls[methodName]);
                
                if (_secondCalls[methodName])
                {
                    _secondCalls[methodName] = false;
                    return Tuple.Create(default(object), true);
                }
                _secondCalls[methodName] = true;
                // return Tuple.Create((object)Traverse.Create(__instance).Method(methodName, __args), false);
                return Tuple.Create(__originalMethod.Invoke(__instance, __args), false);
            }
        }
        
        // private static readonly object VoidFunctionLock = new object();
        public bool LockedVoidFunction(object __instance, object[] __args, MethodBase __originalMethod)
        {
            lock (FunctionLock)
            {
                var methodName = __originalMethod.Name;
                if (!_secondCalls.ContainsKey(methodName))
                {
                    _secondCalls.Add(methodName, false);
                }

                if (_secondCalls[methodName])
                {
                    _secondCalls[methodName] = false;
                    return true;
                }
                _secondCalls[methodName] = true;
                __originalMethod.Invoke(__instance, __args);
                return false;
            }
        }
        
        
        
        
        
        
        
        
        
        
        
        // public bool IsSecondCall()
        // {
        //     if (_secondCallPickBestAction)
        //     {
        //         _secondCallPickBestAction = true;
        //         return _secondCallPickBestAction;
        //     }
        //
        //     _secondCallPickBestAction = false;
        //     return _secondCallPickBestAction;
        // }
        
        // private static readonly object AddNodeLock = new object();
        //
        // public void LockedAddNode(ref Dictionary<int, object> ____nodes, int nodeId, int parentNodeId, int roadParentNodeId, float distanceToRoad)
        // {
        //     lock (AddNodeLock)
        //     {
        //         ____nodes[nodeId] = new Node(parentNodeId, roadParentNodeId, distanceToRoad);
        //     }
        // }
        //
        // public readonly struct Node
        // {
        //     public int ParentNodeId { get; }
        //
        //     public int RoadParentNodeId { get; }
        //
        //     public float DistanceToRoad { get; }
        //
        //     public Node(int parentNodeId, int roadParentNodeId, float distanceToRoad)
        //     {
        //         this.ParentNodeId = parentNodeId;
        //         this.RoadParentNodeId = roadParentNodeId;
        //         this.DistanceToRoad = distanceToRoad;
        //     }
        // }
            
       
        
        // public static object InterestingBuildingLock = new object();
        //
        // // public void LockedGetFirstEnabled(ref List<InterestingBuilding> ____interestingBuildings)
        // // {
        // //     lock (InterestingBuildingLock)
        // //     {
        // //         ____interestingBuildings.FirstOrDefault<InterestingBuilding>((Func<InterestingBuilding, bool>) (building => (bool) (UnityEngine.Object) building && building.enabled));
        // //     }
        // // }
        //
        // private Vector3 _previousCoordinates;
        //
        // public Tuple<InterestingBuilding, bool>  LockedGetRandom(DistrictInterestingBuildingPicker __instance, Vector3 startCoordinates, float preferableMaxDistance)
        // {
        //     lock (InterestingBuildingLock)
        //     {
        //         if (SameAsPrevious(startCoordinates))
        //         {                
        //             Plugin.Log.LogInfo("Same");
        //
        //             return Tuple.Create(new InterestingBuilding(), true);
        //             
        //         }
        //         return Tuple.Create(__instance.GetRandom(startCoordinates, preferableMaxDistance), false);
        //     }
        // }
        //
        // public bool SameAsPrevious(Vector3 newCoordinates)
        // {
        //     Plugin.Log.LogInfo(newCoordinates);
        //     Plugin.Log.LogInfo(_previousCoordinates);
        //     if (newCoordinates == _previousCoordinates)
        //     {
        //         _previousCoordinates = new Vector3();
        //         return true;
        //     }
        //
        //     _previousCoordinates = newCoordinates;
        //     return false;
        // }
        
        
        
        
        
        
        
        // private readonly VisualElementLoader _visualElementLoader;
        // private readonly DescriptionPanel _descriptionPanel;
        // private readonly ToolManager _toolManager;
        // private readonly InputService _inputService;
        // public readonly List<CategoryButtonTool> ToolBarCategoryTools = new();
        //
        // CategoryButtonService(
        //     VisualElementLoader visualElementLoader,
        //     DescriptionPanel descriptionPanel,
        //     ToolManager toolManager,
        //     InputService inputService
        // )
        // {
        //     _visualElementLoader = visualElementLoader;
        //     _descriptionPanel = descriptionPanel;
        //     _toolManager = toolManager;
        //     _inputService = inputService;
        // }
        //
        // public ToolButton CreateFakeToolButton(
        //     PlaceableBlockObject blockObject, 
        //     ToolGroup toolGroup, 
        //     VisualElement parent, 
        //     CategoryButtonComponent toolBarCategory, 
        //     BlockObjectToolDescriber ____blockObjectToolDescriber, 
        //     ToolButtonFactory ____toolButtonFactory)
        // {
        //     CategoryButtonTool categoryButtonTool = new CategoryButtonTool(____blockObjectToolDescriber, _toolManager, _inputService);
        //
        //     var visualElement = _visualElementLoader.LoadVisualElement("Common/BottomBar/ToolGroupButton");
        //     visualElement.Q<VisualElement>("ToolButtons").name = "SecondToolButtons";
        //     parent.Add(visualElement.Q<VisualElement>("SecondToolButtons"));
        //     var secondToolButtons = parent.Q<VisualElement>("SecondToolButtons");
        //     secondToolButtons.name += blockObject.name;
        //     secondToolButtons.style.position = Position.Absolute;
        //
        //     ToolButton button = ____toolButtonFactory.Create(categoryButtonTool, blockObject.GetComponent<LabeledPrefab>().Image, parent);
        //     categoryButtonTool.SetFields(blockObject, button, secondToolButtons, toolGroup, toolBarCategory);
        //
        //     ToolBarCategoryTools.Add(categoryButtonTool);
        //     
        //     return button;
        // }
        //
        // public void AddButtonToCategory(ToolButton toolButton, PlaceableBlockObject placeableBlockObject)
        // {
        //     if (placeableBlockObject.TryGetComponent(out Prefab fPrefab))
        //     {
        //         foreach (var toolBarCategoryTool in ToolBarCategoryTools)
        //         {
        //             if (toolBarCategoryTool.ToolBarCategoryComponent.ToolBarButtonNames.Contains(fPrefab.PrefabName))
        //             {
        //                 toolBarCategoryTool.ToolButtons.Add(toolButton);
        //                 toolBarCategoryTool.SetToolList();
        //             }
        //         }
        //     }
        // }
        //
        // public void AddButtonsToCategory()
        // {
        //     foreach (var categoryTool in ToolBarCategoryTools)
        //     {
        //         foreach (var toolButton in categoryTool.ToolButtons)
        //         {
        //             categoryTool.VisualElement.Add(toolButton.Root);
        //         }
        //     }
        // }
        //
        // public void SaveOrExitCategoryTool(Tool currenTool, Tool newTool, WaterOpacityToggle ____waterOpacityToggle)
        // {
        //     foreach (var categoryTool in TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().ToolBarCategoryTools)
        //     {
        //         bool ButtonPartOfCategory = categoryTool.ToolButtons.Select(button => button.Tool).Contains(newTool);
        //
        //         if (ButtonPartOfCategory)
        //         {
        //             categoryTool.ActiveTool = newTool;
        //         }
        //         
        //         bool flag1 = !ButtonPartOfCategory;
        //         bool flag2 = categoryTool == newTool;
        //         bool flag3 = currenTool != newTool;
        //
        //         if ((flag1 || flag2) && flag3)
        //         {
        //             categoryTool.Exit();
        //             ____waterOpacityToggle.ShowWater();
        //         }
        //     }
        // }
        //
        // public void ChangeDescriptionPanel(int height)
        // {
        //     FieldInfo type = typeof(DescriptionPanel).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
        //     VisualElement value = type.GetValue(_descriptionPanel) as VisualElement;
        //     value.style.bottom = height;
        //     type.SetValue(_descriptionPanel, value);
        // }
        //
        // public void UpdateScreenSize(CategoryButtonTool categoryButtonTool)
        // {
        //     float x = categoryButtonTool.VisualElement.parent.Query<VisualElement>("ToolButtons").First().resolvedStyle.width / 2 - 2;
        //     x +=  ((categoryButtonTool.ToolButtons.Count) * 54) / 2 * -1;
        //     float y = 58f;
        //     categoryButtonTool.VisualElement.style.left = x; categoryButtonTool.VisualElement.style.bottom = y;
        //
        //     // VisualElement.style.left = new Length(y, LengthUnit.Pixel);
        //     // VisualElement.style.bottom = new Length(x, LengthUnit.Pixel);
        // }
    }
}
