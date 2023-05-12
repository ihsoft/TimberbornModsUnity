using System;
using System.Reflection;
using TimberApi.DependencyContainerSystem;
using TimberApi.ToolSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.ConstructionMode;
using Timberborn.Coordinates;
using Timberborn.Core;
using Timberborn.Debugging;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.PrefabSystem;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using Tool = Timberborn.ToolSystem.Tool;

namespace PipetteTool
{
  public class PipetteTool : Tool, IInputProcessor, ILoadableSingleton, IPipetteTool
  {
    private static readonly string TitleLocKey = "Tobbert.PipetteTool.DisplayName";
        
    private static readonly string DescriptionLocKey = "Tobbert.PipetteTool.Description";

    public static string CursorKey => "PipetteCursor";

    private readonly EventBus _eventBus;
    
    private readonly ToolManager _toolManager;

    private readonly DevModeManager _devModeManager;

    private readonly InputService _inputService;

    private readonly MapEditorMode _mapEditorMode;
    
    private readonly CursorService _cursorService;

    private readonly EntitySelectionService _entitySelectionService;

    private readonly SelectableObjectRaycaster _selectableObjectRaycaster;

    private readonly ILoc _loc;
    
    private ToolService _toolService;

    private ToolDescription _toolDescription;

    private bool _shouldPipetNextSelection;

    protected readonly MethodInfo EnterConstructionModeMethod;

    protected readonly MethodInfo ExitConstructionModeMethod;

    private readonly FieldInfo _blockObjectToolOrientationField;

    public PipetteTool(EventBus eventBus, ToolManager toolManager, DevModeManager devModeManager, InputService inputService, MapEditorMode mapEditorMode, CursorService cursorService, EntitySelectionService entitySelectionService, SelectableObjectRaycaster selectableObjectRaycaster, ILoc loc)
    {
      _eventBus = eventBus;
      _toolManager = toolManager;
      _devModeManager = devModeManager;
      _inputService = inputService;
      _mapEditorMode = mapEditorMode;
      _cursorService = cursorService;
      _entitySelectionService = entitySelectionService;
      _selectableObjectRaycaster = selectableObjectRaycaster;
      _loc = loc;
      
      EnterConstructionModeMethod = typeof(ConstructionModeService).GetMethod("EnterConstructionMode", BindingFlags.NonPublic | BindingFlags.Instance);
      ExitConstructionModeMethod = typeof(ConstructionModeService).GetMethod("ExitConstructionMode", BindingFlags.NonPublic | BindingFlags.Instance);
      _blockObjectToolOrientationField = typeof(BlockObjectTool).GetField("_orientation", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public void Load()
    {
      _toolService = DependencyContainer.GetInstance<ToolService>();
      _inputService.AddInputProcessor(this);
      _eventBus.Register(this);
      _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();
    }

    public void SetToolGroup(ToolGroup toolGroup)
    {
      ToolGroup = toolGroup;
    }

    public override ToolDescription Description() => _toolDescription;

    public override void Enter()
    {
      _shouldPipetNextSelection = true;
      _cursorService.SetTemporaryCursor(CursorKey);
    }

    public override void Exit()
    {
      _entitySelectionService.Unselect();
      _cursorService.ResetTemporaryCursor();
      _shouldPipetNextSelection = false;
    }

    public bool ProcessInput()
    {
      if (!_shouldPipetNextSelection)
      {
        PostProcessInput();
        return false;
      }  
      
      if (!_inputService.SelectionStart || _inputService.MouseOverUI)
      {
        PostProcessInput();
        return false;
      }  
      
      if (_selectableObjectRaycaster.TryHitSelectableObject(out var hitObject))
        OnSelectableObjectSelected(hitObject);

      PostProcessInput();
      return false;
    }

    public virtual void PostProcessInput()
    {
      // only used in PipetteToolInGame
    }

    public void OnSelectableObjectSelected(BaseComponent hitObject)
    {
      if (!_inputService.IsCtrlHeld && !_shouldPipetNextSelection)
        return;

      var selectableObjectName = hitObject.GetComponentFast<Prefab>().PrefabName;

      Tool tool;
      try
      {
        tool = _toolService.GetTool(selectableObjectName);
      }
      catch (Exception)
      {
        return;
      }

      if (_mapEditorMode.IsMapEditor)
        SwitchToSelectedBuildingTool(tool, hitObject);

      if (tool.DevModeTool && !IsDevToolEnabled)
        return;

      SwitchToSelectedBuildingTool(tool, hitObject);
    }

     private void ChangeToolOrientation(Tool tool, Orientation orientation)
     {
       if (tool.GetType() != typeof(BlockObjectTool)) 
         return;
       
       BlockObjectTool blockObjectTool = tool as BlockObjectTool;

       _blockObjectToolOrientationField.SetValue(blockObjectTool, orientation);
     }
     
     protected virtual void SwitchToSelectedBuildingTool(Tool tool, BaseComponent hitObject)
     {
       ChangeToolOrientation(tool, hitObject.GetComponentFast<BlockObject>().Orientation);
       _toolManager.SwitchTool(tool);
       _shouldPipetNextSelection = false;
       _cursorService.ResetTemporaryCursor();
     }
     
     private bool IsDevToolEnabled => _devModeManager.Enabled;
  }
}
