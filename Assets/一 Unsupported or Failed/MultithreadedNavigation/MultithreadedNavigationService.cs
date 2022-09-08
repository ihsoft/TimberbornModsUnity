using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Timers;
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
        public readonly List<MyBehaviorManager> BehaviorManagers = new();


        public void Tick()
        {
            Plugin.Log.LogInfo("Tick");
            
            NativeArray<MyBehaviorManager.Data> behaviorManagers = new(BehaviorManagers.Count, Allocator.TempJob);
            for (int i = 0; i < this.BehaviorManagers.Count; i++)
            {
                // behaviorManagers[i] = new MyBehaviorManager.Data(BehaviorManagers[i], ref BehaviorManagers[i].returnToBehavior, ref BehaviorManagers[i].runningBehavior, ref BehaviorManagers[i].rootBehaviors, ref BehaviorManagers[i].runningExecutor);
                behaviorManagers[i] = new MyBehaviorManager.Data(BehaviorManagers[i]);
            }
            var job = new MultithreadedNavigationJob { MyBehaviorManagers = behaviorManagers };
            var jobHandle = job.Schedule(behaviorManagers.Length, behaviorManagers.Length);
            Stopwatch stopwatch = Stopwatch.StartNew();
            jobHandle.Complete();
            stopwatch.Stop();
            Plugin.Log.LogFatal(stopwatch.ElapsedTicks);
            stopwatch.Reset();
            behaviorManagers.Dispose();
            BehaviorManagers.Clear();
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
