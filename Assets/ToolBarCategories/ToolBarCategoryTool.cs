using System.Collections.Generic;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.ToolSystem;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ToolBarCategories
{
  public class ToolBarCategoryTool : Tool
  {
    private readonly BlockObjectToolDescriber _blockObjectToolDescriber;
    // private readonly InputService _inputService;
    // private readonly BlockObjectPlacerService _blockObjectPlacerService;
    // private readonly MapEditorMode _mapEditorMode;
    private int _previewCount;

    public PlaceableBlockObject Prefab { get; private set; }
    public ToolButton ToolButton { get; private set; }
    public VisualElement VisualElement { get; private set; }
    public ToolBarCategory ToolBarCategoryComponent { get; private set; }

    public List<ToolButton> ToolButtons = new List<ToolButton>();

    public ToolBarCategoryTool(
      BlockObjectToolDescriber blockObjectToolDescriber
      )
    {
      this._blockObjectToolDescriber = blockObjectToolDescriber;
    }

    public override bool DevModeTool => Prefab.DevModeTool;

    public void Initialize(PlaceableBlockObject prefab)
    {
      Prefab = prefab;
    }
    
    public void SetToolButton(ToolButton toolButton, VisualElement visualElement, ToolGroup toolGroup, ToolBarCategory toolBarCategory)
    {
      ToolButton = toolButton;
      VisualElement = visualElement;
      ToolGroup = toolGroup;
      ToolBarCategoryComponent = toolBarCategory;
    }

    public override void Enter()
    {
      Plugin.Log.LogFatal("I got called");
      var localCoordinates = ToolButton.Root.worldTransform.GetPosition();
      
      Plugin.Log.LogFatal(localCoordinates);
      var localPosition = ToolButton.Root.WorldToLocal(localCoordinates);
      Plugin.Log.LogFatal(localPosition);
      localPosition.x -= 2;
      localPosition.y += 56;
      VisualElement.style.left = localPosition.x;
      VisualElement.style.bottom = localPosition.y;
      VisualElement.ToggleDisplayStyle(true);
    }

    public override void Exit()
    {
      Plugin.Log.LogFatal("I'm exiting");
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
