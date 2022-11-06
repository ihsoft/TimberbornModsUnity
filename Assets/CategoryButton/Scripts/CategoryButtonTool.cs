using System.Collections.Generic;
using System.Linq;
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
    private readonly CategoryButtonService _categoryButtonService;

    private PlaceableBlockObject _prefab;
    public VisualElement VisualElement;
    public CategoryButtonComponent ToolBarCategoryComponent;

    public readonly List<ToolButton> ToolButtons = new ();
    private List<Tool> _toolList = new();
    public Tool ActiveTool = null;
    private bool _active;

    public CategoryButtonTool(BlockObjectToolDescriber blockObjectToolDescriber, ToolManager toolManager, InputService inputService, CategoryButtonService categoryButtonService)
    {
      _blockObjectToolDescriber = blockObjectToolDescriber;
      _toolManager = toolManager;
      _inputService = inputService;
      _categoryButtonService = categoryButtonService;
    }

    public override bool DevModeTool => _prefab.DevModeTool;

    public void SetFields(PlaceableBlockObject prefab, VisualElement visualElement, ToolGroup toolGroup, CategoryButtonComponent toolBarCategory)
    {
      _prefab = prefab;
      VisualElement = visualElement;
      ToolGroup = toolGroup;
      ToolBarCategoryComponent = toolBarCategory;
    }

    public override void Enter()
    {
      _inputService.AddInputProcessor(this);
      _active = true;
      // var activeToolsCount = _categoryButtonService.CategoryButtonTools.Select(tool => tool._active).Count();
      // var height = 60 * activeToolsCount;
      DependencyContainer.GetInstance<CategoryButtonService>().ChangeDescriptionPanel(60);
      DependencyContainer.GetInstance<CategoryButtonService>().UpdateScreenSize(this);
      VisualElement.ToggleDisplayStyle(true);
      
      if (ActiveTool != null)
        _toolManager.SwitchTool(ActiveTool);
    }

    public override void Exit()
    {
      _inputService.RemoveInputProcessor(this);
      
      // var activeToolsCount = _categoryButtonService.CategoryButtonTools.Select(tool => tool._active).Count();
      // var height = 60 * (activeToolsCount - 1);
      if (_active) DependencyContainer.GetInstance<CategoryButtonService>().ChangeDescriptionPanel(0);
      _active = false;

      VisualElement.ToggleDisplayStyle(false);
    }

    public bool ProcessInput()
    {
      if (!_inputService.IsShiftHeld) return false;
      
      MouseController mouse = (MouseController)_categoryButtonService.GetPrivateField(_inputService, "_mouse");
        
      int index = _toolList.IndexOf(ActiveTool);

      if (mouse.ScrollWheelAxis > 0)
      {
        while (index + 1 < _toolList.Count() && _toolList[index + 1].Locked)
        {
          index += 1;
        }
        if (index + 1 < _toolList.Count() && !_toolList[index + 1].Locked)
        {
          _toolManager.SwitchTool(_toolList[index + 1]);
        }
      }
      else if (mouse.ScrollWheelAxis < 0)
      {
        while (index - 1 >= 0 && _toolList[index - 1].Locked)
        {
          index -= 1;
        }
        if (index - 1 >= 0 && !_toolList[index - 1].Locked)
        {
          _toolManager.SwitchTool(_toolList[index - 1]);
        }
      }

      return false;
    }

    public override ToolDescription Description()
    {
      var builder = _blockObjectToolDescriber.DescribePrefab(_prefab);
      
      return builder.Build();
    }

    public void SetToolList()
    {
      _toolList = ToolButtons.Select(button => button.Tool).ToList();
    }
  }
}
