using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TimberApi.Common.SingletonSystem;
using Timberborn.AssetSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.InputSystem;
using Timberborn.Persistence;
using Timberborn.ToolSystem;
using Timberborn.WaterSystemRendering;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CategoryButton
{
    public class CategoryButtonService : ITimberApiLoadableSingleton
    {
        private readonly VisualElementLoader _visualElementLoader;
        private readonly IResourceAssetLoader _assetLoader;
        private readonly DescriptionPanel _descriptionPanel;
        private readonly ToolManager _toolManager;
        private readonly InputService _inputService;
        private readonly CategoryButtonSpecificationDeserializer _categoryButtonSpecificationDeserializer;
        private readonly CategoryButtonFactory _categoryButtonFactory;
        private readonly ISpecificationService _specificationService;

        public readonly List<CategoryButtonTool> CategoryButtonTools = new();
        private ImmutableArray<CategoryButtonSpecification> _categoryButtonsSpecifications;
        private GameObject _originalCategoryButtonPrefab;
        private List<GameObject> _categoryButtonPrefabs;
        
        private readonly Dictionary<string, FieldInfo> _fieldInfos = new();

        CategoryButtonService(
            VisualElementLoader visualElementLoader,
            IResourceAssetLoader assetLoader,
            DescriptionPanel descriptionPanel,
            ToolManager toolManager,
            InputService inputService,
            ISpecificationService specificationService,
            CategoryButtonSpecificationDeserializer categoryButtonSpecificationDeserializer,
            CategoryButtonFactory categoryButtonFactory)
        {
            _visualElementLoader = visualElementLoader;
            _assetLoader = assetLoader;
            _descriptionPanel = descriptionPanel;
            _toolManager = toolManager;
            _inputService = inputService;
            _specificationService = specificationService;
            _categoryButtonSpecificationDeserializer = categoryButtonSpecificationDeserializer;
            _categoryButtonFactory = categoryButtonFactory;
        }
        
        public void Load()
        {
            _categoryButtonsSpecifications = _specificationService.GetSpecifications(_categoryButtonSpecificationDeserializer).ToImmutableArray();
            _originalCategoryButtonPrefab = _assetLoader.Load<GameObject>("tobbert.categorybutton/tobbert_categorybutton/CategoryButtonPrefab");
        }
        
        public void AddCategoryButtonsToObjectsPatch(ref IEnumerable<Object> objects)
        {
            if (_categoryButtonPrefabs == null)
                CreateCategoryButtons();
            var objectList = objects.ToList();
            objects = objectList.Concat(_categoryButtonPrefabs);
        }

        private void CreateCategoryButtons()
        {
            PreventInstantiatePatch.RunInstantiate = false;
            _categoryButtonPrefabs = _categoryButtonFactory.CreateFromSpecifications(this, _categoryButtonsSpecifications, _originalCategoryButtonPrefab);
            PreventInstantiatePatch.RunInstantiate = true;
        }
        
        public ToolButton CreateCategoryToolButton(
            PlaceableBlockObject blockObject, 
            ToolGroup toolGroup, 
            VisualElement parent, 
            CategoryButtonComponent toolBarCategory, 
            BlockObjectToolDescriber blockObjectToolDescriber, 
            ToolButtonFactory toolButtonFactory)
        {
            CategoryButtonTool categoryButtonTool = new CategoryButtonTool(blockObjectToolDescriber, _toolManager, _inputService, this);

            var visualElement = _visualElementLoader.LoadVisualElement("Common/BottomBar/ToolGroupButton");
            visualElement.Q<VisualElement>("ToolButtons").name = "SecondToolButtons";
            parent.Add(visualElement.Q<VisualElement>("SecondToolButtons"));
            var secondToolButtons = parent.Q<VisualElement>("SecondToolButtons");
            secondToolButtons.name += blockObject.name;
            secondToolButtons.style.position = Position.Absolute;

            ToolButton button = toolButtonFactory.Create(categoryButtonTool, blockObject.GetComponent<LabeledPrefab>().Image, parent);
            categoryButtonTool.SetFields(blockObject, secondToolButtons, toolGroup, toolBarCategory);

            CategoryButtonTools.Add(categoryButtonTool);
            
            return button;
        }

        public void AddButtonToCategoryTool(ToolButton toolButton, PlaceableBlockObject placeableBlockObject)
        {
            if (!placeableBlockObject.TryGetComponent(out Prefab fPrefab)) return;
            foreach (var categoryToolButton in CategoryButtonTools)
            {
                if (!categoryToolButton.ToolBarCategoryComponent.ToolBarButtonNames.Contains(fPrefab.PrefabName))
                    continue;
                categoryToolButton.ToolButtons.Add(toolButton);
                categoryToolButton.SetToolList();
            }
        }

        public void AddButtonsToCategory()
        {
            foreach (var categoryTool in CategoryButtonTools)
            {
                foreach (var toolButton in categoryTool.ToolButtons)
                {
                    categoryTool.VisualElement.Add(toolButton.Root);
                }
            }
        }

        public void SaveOrExitCategoryTool(Tool currenTool, Tool newTool, WaterOpacityToggle waterOpacityToggle)
        {
            foreach (var categoryTool in CategoryButtonTools)
            {
                bool buttonPartOfCategory = categoryTool.ToolButtons.Select(button => button.Tool).Contains(newTool);

                if (buttonPartOfCategory)
                {
                    categoryTool.ActiveTool = newTool;
                }
                
                bool flag1 = !buttonPartOfCategory;
                bool flag2 = categoryTool == newTool;
                bool flag3 = currenTool != newTool;

                if ((flag1 || flag2) && flag3)
                {
                    categoryTool.Exit();
                    waterOpacityToggle.ShowWater();
                }
            }
        }

        public void ChangeDescriptionPanel(int height)
        {
            VisualElement value = (VisualElement)GetPrivateField(_descriptionPanel, "_root");
            value.style.bottom = height;
            ChangePrivateField(_descriptionPanel, "_root", value);
        }

        public void UpdateScreenSize(CategoryButtonTool categoryButtonTool)
        {
            float x = categoryButtonTool.VisualElement.parent.Query<VisualElement>("ToolButtons").First().resolvedStyle.width / 2 - 2;
            x +=  categoryButtonTool.ToolButtons.Count * 54 / 2 * -1;
            float y = 58f;
            categoryButtonTool.VisualElement.style.left = x; 
            categoryButtonTool.VisualElement.style.bottom = y;
        }

        public void ChangePrivateField(object instance, string fieldName, object newValue)
        {
            var fieldInfo = _fieldInfos.GetOrAdd(fieldName, () => AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));

            fieldInfo.SetValue(instance, newValue);
        }
        
        public object GetPrivateField(object instance, string fieldName)
        {
            var fieldInfo = _fieldInfos.GetOrAdd(fieldName, () => AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));

            return fieldInfo.GetValue(instance);
        }
    }
}
