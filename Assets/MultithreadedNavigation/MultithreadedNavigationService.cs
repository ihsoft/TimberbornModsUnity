using System;
using System.Collections.Generic;
using Timberborn.BehaviorSystem;
using Timberborn.TickSystem;
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
                behaviorManagers[i] = new MyBehaviorManager.Data(BehaviorManagers[i]);
            }
            var job = new MultithreadedNavigationJob { MyBehaviorManagers = behaviorManagers };
            var jobHandle = job.Schedule(behaviorManagers.Length, 2);
            jobHandle.Complete();
            
            Plugin.Log.LogInfo("Complete");        
                
            behaviorManagers.Dispose();
            
            Plugin.Log.LogInfo("Dispose");        

            
            BehaviorManagers.Clear();
            
            Plugin.Log.LogInfo("Done");        

        }
        
        
        
        
        
        
        
        
        
        
        
        
        
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
