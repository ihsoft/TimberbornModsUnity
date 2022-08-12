using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    private DescriptionPanel _descriptionPanel;

    public List<ToolBarCategoryTool> ToolBarCategoryTools = new();

    ToolBarCategoriesService(
        AssetLoader assetLoader,
        VisualElementLoader visualElementLoader,
        BlockObjectToolButtonFactory blockObjectToolButtonFactory,
        BlockObjectToolDescriber blockObjectToolDescriber,
        ToolButtonFactory toolButtonFactory,
        DescriptionPanel descriptionPanel
    )
    {
        _assetLoader = assetLoader;
        _visualElementLoader = visualElementLoader;
        _blockObjectToolButtonFactory = blockObjectToolButtonFactory;
        _blockObjectToolDescriber = blockObjectToolDescriber;
        _toolButtonFactory = toolButtonFactory;
        _descriptionPanel = descriptionPanel;
    }

    public ToolButton CreateFakeToolButton(PlaceableBlockObject blockObject, ToolGroup toolGroup, VisualElement parent,
        ToolBarCategory toolBarCategory)
    {
        ToolBarCategoryTool toolBarCategoryTool = new ToolBarCategoryTool(_blockObjectToolDescriber);
        toolBarCategoryTool.Initialize(blockObject);

        var visualElement = _visualElementLoader.LoadVisualElement("Common/BottomBar/ToolGroupButton");
        visualElement.Q<VisualElement>("ToolButtons").name = "SecondToolButtons";

        parent.Add(visualElement.Q<VisualElement>("SecondToolButtons"));
        var secondToolButtons = parent.Q<VisualElement>("SecondToolButtons");
        secondToolButtons.name += blockObject.name;
        secondToolButtons.style.position = Position.Absolute;

        ToolButton button = _toolButtonFactory.Create(toolBarCategoryTool, blockObject.GetComponent<LabeledPrefab>().Image, parent);
        toolBarCategoryTool.SetToolButton(button, secondToolButtons, toolGroup, toolBarCategory);

        ToolBarCategoryTools.Add(toolBarCategoryTool);
        
        return button;
    }

    public void AddButtonToCategory(ToolButton toolButton, PlaceableBlockObject placeableBlockObject)
    {
        Plugin.Log.LogFatal(placeableBlockObject.name);
        if (placeableBlockObject.TryGetComponent(out Prefab fPrefab))
        {
            foreach (var toolBarCategoryTool in ToolBarCategoryTools)
            {
                if (toolBarCategoryTool.ToolBarCategoryComponent.ToolBarButtonNames.Contains(fPrefab.PrefabName))
                {
                    toolBarCategoryTool.ToolButtons.Add(toolButton);
                }
            }
        }
    }

    public void AddButtonsToCategory()
    {
        foreach (var categoryTool in ToolBarCategoryTools)
        {
            foreach (var toolButton in categoryTool.ToolButtons)
            {
                Plugin.Log.LogFatal(toolButton.Tool);
                categoryTool.VisualElement.Add(toolButton.Root);
            }
        }
    }

    public void SaveOrExitCategoryTool(Tool currenTool, Tool newTool, ToolBarCategoryTool categoryTool)
    {
        foreach (var toolButton in categoryTool.ToolButtons)
        {
            bool flag1 = categoryTool.ToolButtons.Select(button => button.Tool).Contains(newTool);

            if (!flag1)
                categoryTool.Exit();
        }
    }

    public void ChangeDescriptionPanel(int height)
    {
        FieldInfo type = typeof(DescriptionPanel).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
        VisualElement value = type.GetValue(_descriptionPanel) as VisualElement;
        value.style.bottom = height;
        type.SetValue(_descriptionPanel, value);
    }
    }
}
