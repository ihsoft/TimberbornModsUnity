using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.ToolSystem;
using TimberbornAPI;
using UnityEngine.UIElements;

namespace CategoryButton
{
    public class ToolBarCategoriesService

    {
    private VisualElementLoader _visualElementLoader;
    private BlockObjectToolDescriber _blockObjectToolDescriber;
    private ToolButtonFactory _toolButtonFactory;
    private DescriptionPanel _descriptionPanel;

    public List<ToolBarCategoryTool> ToolBarCategoryTools = new();

    ToolBarCategoriesService(
        VisualElementLoader visualElementLoader,
        BlockObjectToolDescriber blockObjectToolDescriber,
        ToolButtonFactory toolButtonFactory,
        DescriptionPanel descriptionPanel
    )
    {
        _visualElementLoader = visualElementLoader;
        _blockObjectToolDescriber = blockObjectToolDescriber;
        _toolButtonFactory = toolButtonFactory;
        _descriptionPanel = descriptionPanel;
    }

    public ToolButton CreateFakeToolButton(PlaceableBlockObject blockObject, ToolGroup toolGroup, VisualElement parent, CategoryButtonComponent toolBarCategory)
    {
        ToolBarCategoryTool toolBarCategoryTool = new ToolBarCategoryTool(_blockObjectToolDescriber);

        var visualElement = _visualElementLoader.LoadVisualElement("Common/BottomBar/ToolGroupButton");
        visualElement.Q<VisualElement>("ToolButtons").name = "SecondToolButtons";

        parent.Add(visualElement.Q<VisualElement>("SecondToolButtons"));
        var secondToolButtons = parent.Q<VisualElement>("SecondToolButtons");
        secondToolButtons.name += blockObject.name;
        secondToolButtons.style.position = Position.Absolute;

        ToolButton button = _toolButtonFactory.Create(toolBarCategoryTool, blockObject.GetComponent<LabeledPrefab>().Image, parent);
        toolBarCategoryTool.SetFields(blockObject, button, secondToolButtons, toolGroup, toolBarCategory);

        ToolBarCategoryTools.Add(toolBarCategoryTool);
        
        return button;
    }

    public void AddButtonToCategory(ToolButton toolButton, PlaceableBlockObject placeableBlockObject)
    {
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
                categoryTool.VisualElement.Add(toolButton.Root);
            }
        }
    }

    public void SaveOrExitCategoryTool(Tool currenTool, Tool newTool)
    {
        foreach (var categoryTool in TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().ToolBarCategoryTools)
        {
            bool flag1 = categoryTool.ToolButtons.Select(button => button.Tool).Contains(newTool);
            bool flag2 = categoryTool == newTool;

            if (!flag1 || flag2)
            {
                categoryTool.Exit();
            }
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
