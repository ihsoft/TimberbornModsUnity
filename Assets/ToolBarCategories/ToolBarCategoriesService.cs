using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.InputSystem;
using Timberborn.PrefabOptimization;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using Timberborn.WaterSystemRendering;
using TimberbornAPI;
using TimberbornAPI.AssetLoaderSystem.AssetSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace ToolBarCategories
{
    public class ToolBarCategoriesService
    {
        private AssetLoader _assetLoader;
        private VisualElementLoader _visualElementLoader;
        private BlockObjectToolButtonFactory _blockObjectToolButtonFactory;
        private BlockObjectToolDescriber _blockObjectToolDescriber;
        private ToolButtonFactory _toolButtonFactory;
        private OptimizedPrefabInstantiator _optimizedPrefabInstantiator;

        public List<ToolBarCategoryTool> ToolBarCategoryTools = new ();

        public VisualElement VisualElement;

        ToolBarCategoriesService(
            AssetLoader assetLoader, 
            VisualElementLoader visualElementLoader,
            BlockObjectToolButtonFactory blockObjectToolButtonFactory,
            BlockObjectToolDescriber blockObjectToolDescriber,
            ToolButtonFactory toolButtonFactory,
            OptimizedPrefabInstantiator optimizedPrefabInstantiator)
        {
            _assetLoader = assetLoader;
            _visualElementLoader = visualElementLoader;
            _blockObjectToolButtonFactory = blockObjectToolButtonFactory;
            _blockObjectToolDescriber = blockObjectToolDescriber;
            _toolButtonFactory = toolButtonFactory;
            _optimizedPrefabInstantiator = optimizedPrefabInstantiator;
        }
        
        public GameObject GetObject()
        {
            return _assetLoader.Load<GameObject>("tobbert.toolbarcategories/tobbert_toolbarcategories/Test");
            
        }

        public ToolButton CreateFakeToolButton(PlaceableBlockObject blockObject, ToolGroup toolGroup, VisualElement parent, ToolBarCategory toolBarCategory)
        {
            ToolBarCategoryTool toolBarCategoryTool = new ToolBarCategoryTool(_blockObjectToolDescriber);
            toolBarCategoryTool.Initialize(blockObject);

            var visualElement = _visualElementLoader.LoadVisualElement("Common/BottomBar/ToolGroupButton");
            visualElement.Q<VisualElement>("ToolButtons").name = "SecondToolButtons";

            parent.Add(visualElement.Q<VisualElement>("SecondToolButtons"));
            var secondToolButtons = parent.Q<VisualElement>("SecondToolButtons");
            secondToolButtons.style.position = Position.Absolute;

            ToolButton button = _toolButtonFactory.Create(toolBarCategoryTool, blockObject.GetComponent<LabeledPrefab>().Image, parent);
            toolBarCategoryTool.SetToolButton(button, secondToolButtons, toolGroup, toolBarCategory);
            
            ToolBarCategoryTools.Add(toolBarCategoryTool);

            // blockObject = _optimizedPrefabInstantiator.Instantiate(_assetLoader.Load<GameObject>("tobbert.ladder/tobbert_ladder/Ladder.Folktails"), null).GetComponent<PlaceableBlockObject>();
            //
            // toolBarCategoryTool = new ToolBarCategoryTool(_blockObjectToolDescriber);
            // toolBarCategoryTool.Initialize(blockObject);
            // var SecondWrapperButton = _toolButtonFactory.Create(toolBarCategoryTool, blockObject.GetComponent<LabeledPrefab>().Image, secondToolButtons);
            // toolBarCategoryTool.SetToolButton(SecondWrapperButton, secondToolButtons, toolGroup);
            
            return button;
        }

        public void TryToAddButtonToCategory(ToolButton toolButton, PlaceableBlockObject placeableBlockObject)
        {
            if (placeableBlockObject.TryGetComponent(out Prefab fPrefab))
            {
                foreach (var toolBarCategoryTool in ToolBarCategoryTools)
                {
                    if (toolBarCategoryTool.ToolBarCategoryComponent.ToolBarButtonNames.Contains(fPrefab.PrefabName))
                    {
                        toolBarCategoryTool.VisualElement.Add(toolButton.Root);
                        toolBarCategoryTool.ToolButtons.Add(toolButton);
                    }
                } 
            }
        }

        public bool PreventToolSwitch(
            ToolManager __instance, 
            Tool currentTool, 
            Tool newTool, 
            Tool ____defaultTool, 
            InputService ____inputService, 
            EventBus ____eventBus,
            WaterOpacityToggle ____waterOpacityToggle)
        {
            foreach (var toolBarCategoryTool in ToolBarCategoryTools)
            {
                if (currentTool != toolBarCategoryTool) continue;
                
                foreach (var toolButton in toolBarCategoryTool.ToolButtons)
                {
                    if (newTool == toolButton.Tool)
                    {
                        if (newTool.Locked || currentTool == newTool)
                            return true;
                    
                        if (currentTool == null)
                            return true;
                        // this.ActiveTool.Exit();
                        currentTool = (Tool) null;
                        ____inputService.RemoveInputProcessor((IInputProcessor) __instance);
                        ____eventBus.Post((object) new ToolExitedEvent());
                        ____waterOpacityToggle.ShowWater();
                    
                    
                        ____inputService.AddInputProcessor((IInputProcessor) __instance);
                        currentTool = newTool;
                        newTool.Enter();
                        ____eventBus.Post((object) new ToolEnteredEvent(newTool, newTool == ____defaultTool));
                        if (currentTool == ____defaultTool)
                            return true;
                        ____waterOpacityToggle.HideWater();

                        return false;
                        Plugin.Log.LogFatal(currentTool);
                        Plugin.Log.LogFatal(newTool);
                    }
                }
            }

            return true;
        }

        public void DisableAllCategoryTools(
            ToolManager __instance, 
            Tool currentTool, 
            Tool newTool, 
            Tool ____defaultTool, 
            InputService ____inputService, 
            EventBus ____eventBus,
            WaterOpacityToggle ____waterOpacityToggle)
        {
            if (currentTool == null)
                return;
            currentTool.Exit();
            currentTool = (Tool) null;
            ____inputService.RemoveInputProcessor((IInputProcessor) __instance);
            ____eventBus.Post((object) new ToolExitedEvent());
            ____waterOpacityToggle.ShowWater();
            
            foreach (var toolBarCategoryTool in ToolBarCategoryTools)
            {
                toolBarCategoryTool.Exit();
            }
        }

        private ToolBarCategoryTool _currentlyOpenCategoryTool;
        public void SaveOrExitCategoryTool(Tool currenTool, Tool newTool, ToolBarCategoryTool categoryTool)
        {
            foreach (var toolButton in categoryTool.ToolButtons)
            {
                bool flag1 = categoryTool.ToolButtons.Select(button => button.Tool).Contains(newTool);
               
                if (!flag1)
                    categoryTool.Exit();
                
                // bool flag2 = categoryTool.ToolButtons.Select(button => button.Tool).Contains(currenTool);
                // bool flag3 = ToolBarCategoryTools.Contains(newTool);
                //
                // Plugin.Log.LogFatal("flag1: " + flag2 + " flag2: " + flag3);
                // if (flag2 && flag3)
                // {
                //     // currenTool = categoryTool;
                //     // doNotSkipNext = true;
                // }
                    
                // categoryTool.Exit();
            }
            
            
            // if (_currentlyOpenCategoryTool == null)
            // {
            //     _currentlyOpenCategoryTool = categoryTool;
            // }
            // else
            // {
            //     foreach (var toolButton in categoryTool.ToolButtons)
            //     {
            //         if (currenTool == toolButton.Tool)
            //             return;
            //     }
            //     
            // }
        }

        private bool doNotSkipNext = false;
        
        public void SetActiveTool(Tool currentTool, Tool oldTool)
        {
            currentTool = oldTool;
        }

        public bool ShouldExitToolBeSkipped()
        {
            bool skip = doNotSkipNext;
            doNotSkipNext = false;
            
            return skip;
        }
    }
}
