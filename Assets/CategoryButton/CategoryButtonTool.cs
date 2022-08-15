using System.Collections.Generic;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.ToolSystem;
using TimberbornAPI;
using UnityEngine.UIElements;
using Tool = Timberborn.ToolSystem.Tool;

namespace CategoryButton
{
  public class CategoryButtonTool : Tool
  {
    private readonly BlockObjectToolDescriber _blockObjectToolDescriber;

    public PlaceableBlockObject Prefab { get; private set; }
    public ToolButton ToolButton { get; private set; }
    public VisualElement VisualElement { get; private set; }
    public CategoryButtonComponent ToolBarCategoryComponent { get; private set; }

    public List<ToolButton> ToolButtons = new ();
    public VisualElement Root { get; set; }
    
    public bool Active { get; set; }
    public int ScreenWidth { get; set; }

    public CategoryButtonTool(BlockObjectToolDescriber blockObjectToolDescriber)
    {
      _blockObjectToolDescriber = blockObjectToolDescriber;
    }

    public override bool DevModeTool => Prefab.DevModeTool;
    
    public void SetFields(PlaceableBlockObject prefab, ToolButton toolButton, VisualElement visualElement, ToolGroup toolGroup, CategoryButtonComponent toolBarCategory)
    {
      Prefab = prefab;
      Root = toolButton.Root;
      ToolButton = toolButton;
      VisualElement = visualElement;
      ToolGroup = toolGroup;
      ToolBarCategoryComponent = toolBarCategory;
    }

    public override void Enter()
    {
      Active = true;
      TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().ChangeDescriptionPanel(60);
      TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().UpdateScreenSize(this);
      VisualElement.ToggleDisplayStyle(true);
    }

    public override void Exit()
    {
      if (Active) TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().ChangeDescriptionPanel(0);
      Active = false;

      VisualElement.ToggleDisplayStyle(false);
    }

    public override ToolDescription Description()
    {
      ToolDescription.Builder builder = _blockObjectToolDescriber.DescribePrefab(Prefab.GetComponent<PlaceableBlockObject>());
      // _blockObjectPlacer.Describe(Prefab, builder);
      // if (this.DevModeTool && !this._mapEditorMode.IsMapEditor)
      // {
      //   string str = "<color=#ff0000><b>This is a DevModeTool</b></color>";
      //   builder.AddPrioritizedSection(str.ToUpper());
      // }
      return builder.Build();
    }
  }
}
