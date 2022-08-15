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
    public class CategoryButtonService

    {
        private readonly VisualElementLoader _visualElementLoader;
        private readonly DescriptionPanel _descriptionPanel;
        public readonly List<CategoryButtonTool> ToolBarCategoryTools = new();

        CategoryButtonService(
            VisualElementLoader visualElementLoader,
            DescriptionPanel descriptionPanel
        )
        {
            _visualElementLoader = visualElementLoader;
            _descriptionPanel = descriptionPanel;
        }

        public ToolButton CreateFakeToolButton(
            PlaceableBlockObject blockObject, 
            ToolGroup toolGroup, 
            VisualElement parent, 
            CategoryButtonComponent toolBarCategory, 
            BlockObjectToolDescriber ____blockObjectToolDescriber, 
            ToolButtonFactory ____toolButtonFactory)
        {
            CategoryButtonTool categoryButtonTool = new CategoryButtonTool(____blockObjectToolDescriber);

            var visualElement = _visualElementLoader.LoadVisualElement("Common/BottomBar/ToolGroupButton");
            visualElement.Q<VisualElement>("ToolButtons").name = "SecondToolButtons";
            parent.Add(visualElement.Q<VisualElement>("SecondToolButtons"));
            var secondToolButtons = parent.Q<VisualElement>("SecondToolButtons");
            secondToolButtons.name += blockObject.name;
            secondToolButtons.style.position = Position.Absolute;

            ToolButton button = ____toolButtonFactory.Create(categoryButtonTool, blockObject.GetComponent<LabeledPrefab>().Image, parent);
            categoryButtonTool.SetFields(blockObject, button, secondToolButtons, toolGroup, toolBarCategory);

            ToolBarCategoryTools.Add(categoryButtonTool);
            
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
            foreach (var categoryTool in TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().ToolBarCategoryTools)
            {
                bool flag1 = !categoryTool.ToolButtons.Select(button => button.Tool).Contains(newTool);
                bool flag2 = categoryTool == newTool;
                bool flag3 = currenTool != newTool;

                if ((flag1 || flag2) && flag3)
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

        public void UpdateScreenSize(CategoryButtonTool categoryButtonTool)
        {
            float x = categoryButtonTool.VisualElement.parent.Query<VisualElement>("ToolButtons").First().resolvedStyle.width / 2 - 2;
            x +=  ((categoryButtonTool.ToolButtons.Count) * 54) / 2 * -1;
            float y = 58f;
            categoryButtonTool.VisualElement.style.left = x; categoryButtonTool.VisualElement.style.bottom = y;

            // VisualElement.style.left = new Length(y, LengthUnit.Pixel);
            // VisualElement.style.bottom = new Length(x, LengthUnit.Pixel);
        }
    }
}
