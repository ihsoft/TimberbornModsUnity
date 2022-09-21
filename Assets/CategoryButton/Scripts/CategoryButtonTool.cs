using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TimberApi.DependencyContainerSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.InputSystem;
using Timberborn.ToolSystem;
using UnityEngine.UIElements;
using Tool = Timberborn.ToolSystem.Tool;

namespace CategoryButton
{
  public class CategoryButtonTool : Tool, IInputProcessor
  {
    private readonly BlockObjectToolDescriber _blockObjectToolDescriber;
    private readonly ToolManager _toolManager;
    private readonly InputService _inputService;

    public PlaceableBlockObject Prefab { get; private set; }
    public ToolButton ToolButton { get; private set; }
    public VisualElement VisualElement { get; private set; }
    public CategoryButtonComponent ToolBarCategoryComponent { get; private set; }

    public List<ToolButton> ToolButtons = new ();
    public List<Tool> ToolList = new();
    public VisualElement Root { get; set; }
    public Tool ActiveTool = null;
    public bool Active { get; set; }

    public CategoryButtonTool(BlockObjectToolDescriber blockObjectToolDescriber, ToolManager toolManager, InputService inputService)
    {
      _blockObjectToolDescriber = blockObjectToolDescriber;
      _toolManager = toolManager;
      _inputService = inputService;
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
      _inputService.AddInputProcessor(this);
      Active = true;
      DependencyContainer.GetInstance<CategoryButtonService>().ChangeDescriptionPanel(60);
      DependencyContainer.GetInstance<CategoryButtonService>().UpdateScreenSize(this);
      VisualElement.ToggleDisplayStyle(true);
      
      if (ActiveTool != null)
      {
        _toolManager.SwitchTool(ActiveTool);
      }
    }

    public override void Exit()
    {
      _inputService.RemoveInputProcessor(this);
      
      if (Active) DependencyContainer.GetInstance<CategoryButtonService>().ChangeDescriptionPanel(0);
      Active = false;

      VisualElement.ToggleDisplayStyle(false);
    }

    public bool ProcessInput()
    {
      
      if (_inputService.IsShiftHeld)
      {
        FieldInfo type = typeof(InputService).GetField("_mouse", BindingFlags.NonPublic | BindingFlags.Instance);
        MouseController mouse = type.GetValue(_inputService) as MouseController;
        int index = ToolList.IndexOf(ActiveTool);

        if (mouse.ScrollWheelAxis > 0)
        {
          while (index + 1 < ToolList.Count() && ToolList[index + 1].Locked)
          {
            index += 1;
          }
          if (index + 1 < ToolList.Count() && !ToolList[index + 1].Locked)
          {
            _toolManager.SwitchTool(ToolList[index + 1]);
          }
        }
        else if (mouse.ScrollWheelAxis < 0)
        {
          while (index - 1 >= 0 && ToolList[index - 1].Locked)
          {
            index -= 1;
          }
          if (index - 1 >= 0 && !ToolList[index - 1].Locked)
          {
            _toolManager.SwitchTool(ToolList[index - 1]);
          }
        }
      }

      return false;
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

    public void SetToolList()
    {
      ToolList = ToolButtons.Select(button => button.Tool).ToList();
    }
  }
}
