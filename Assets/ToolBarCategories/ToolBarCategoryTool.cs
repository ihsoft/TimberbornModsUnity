using System.Collections.Generic;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.ToolSystem;
using TimberbornAPI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ToolBarCategories
{
  public class ToolBarCategoryTool : Tool
  {
    private readonly BlockObjectToolDescriber _blockObjectToolDescriber;

    public PlaceableBlockObject Prefab { get; private set; }
    public ToolButton ToolButton { get; private set; }
    public VisualElement VisualElement { get; private set; }
    public ToolBarCategory ToolBarCategoryComponent { get; private set; }

    public List<ToolButton> ToolButtons = new ();
    public VisualElement Root { get; set; }

    public ToolBarCategoryTool(
      BlockObjectToolDescriber blockObjectToolDescriber
      )
    {
      _blockObjectToolDescriber = blockObjectToolDescriber;
    }

    public override bool DevModeTool => Prefab.DevModeTool;

    public void Initialize(PlaceableBlockObject prefab)
    {
      Prefab = prefab;
    }
    
    public void SetToolButton(ToolButton toolButton, VisualElement visualElement, ToolGroup toolGroup, ToolBarCategory toolBarCategory)
    {
      Root = toolButton.Root;
      ToolButton = toolButton;
      VisualElement = visualElement;
      ToolGroup = toolGroup;
      ToolBarCategoryComponent = toolBarCategory;
    }

    public override void Enter()
    {
      TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().ChangeDescriptionPanel(60);

      var localCoordinates = ToolButton.Root.worldTransform.GetPosition();
      // Plugin.Log.LogFatal(localCoordinates);
      localCoordinates.x = Screen.width / 2 - 2;
      localCoordinates.x += ((ToolButtons.Count) * 54) / 2 * -1;
      // localCoordinates.x -= 2;
      var localPosition = VisualElement.WorldToLocal(localCoordinates);
      localPosition.y = 56;
      // localPosition.x =  ((ToolButtons.Count) * 54) / 2 * -1;
      // localPosition.x -= ((ToolButtons.Count - 1) * 54) / 2;
      // Plugin.Log.LogFatal(localPosition);
      VisualElement.style.left = localPosition.x;
      VisualElement.style.bottom = localPosition.y;
      VisualElement.ToggleDisplayStyle(true);
    }

    public override void Exit()
    {
      TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().ChangeDescriptionPanel(0);

      VisualElement.ToggleDisplayStyle(false);
    }

    public override ToolDescription Description()
    {
      ToolDescription.Builder builder = _blockObjectToolDescriber.DescribePrefab(Prefab.GetComponent<PlaceableBlockObject>());
      // this._blockObjectPlacer.Describe(this.Prefab, builder);
      // if (this.DevModeTool && !this._mapEditorMode.IsMapEditor)
      // {
      //   string str = "<color=#ff0000><b>This is a DevModeTool</b></color>";
      //   builder.AddPrioritizedSection(str.ToUpper());
      // }
      return builder.Build();
    }
  }
}
