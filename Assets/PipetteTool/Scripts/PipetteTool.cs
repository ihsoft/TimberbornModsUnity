using System.Collections.Generic;
using System.Reflection;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.ConstructionMode;
using Timberborn.Coordinates;
using Timberborn.Core;
using Timberborn.Debugging;
using Timberborn.EntitySystem;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using Tool = Timberborn.ToolSystem.Tool;

namespace PipetteTool
{
  public class PipetteTool : Tool, IInputProcessor, ILoadableSingleton, IPipetteTool
  {
    private static readonly string TitleLocKey = "Tobbert.PipetteTool.DisplayName";
        
    private static readonly string DescriptionLocKey = "Tobbert.PipetteTool.Description";

    public string CursorKey => "PipetteCursor";

    private readonly EventBus _eventBus;
    
    private readonly ToolManager _toolManager;

    private readonly DevModeManager _devModeManager;

    private readonly InputService _inputService;

    private readonly MapEditorMode _mapEditorMode;
    
    private readonly CursorService _cursorService;

    private readonly SelectionManager _selectionManager;

    private readonly SelectableObjectRaycaster _selectableObjectRaycaster;

    private readonly ILoc _loc;

    private readonly Dictionary<string, ToolButton> _toolButtons = new();
        
    private ToolDescription _toolDescription;

    private bool _shouldPipetNextSelection;

    protected MethodInfo EnterConstructionModeMethod;

    protected MethodInfo ExitConstructionModeMethod;

    private FieldInfo _blockObjectToolOrientationField;
    
    public PipetteTool(EventBus eventBus, ToolManager toolManager, DevModeManager devModeManager, InputService inputService, MapEditorMode mapEditorMode, CursorService cursorService, SelectionManager selectionManager, SelectableObjectRaycaster selectableObjectRaycaster, ILoc loc)
    {
      _eventBus = eventBus;
      _toolManager = toolManager;
      _devModeManager = devModeManager;
      _inputService = inputService;
      _mapEditorMode = mapEditorMode;
      _cursorService = cursorService;
      _selectionManager = selectionManager;
      _selectableObjectRaycaster = selectableObjectRaycaster;
      _loc = loc;
    }

    public void Load()
    {
      _inputService.AddInputProcessor(this);
      _eventBus.Register(this);
      InitializeToolDescription();
      InitializeMethodInfos();
    }
    
    private void InitializeToolDescription()
    {
      _toolDescription = new ToolDescription.Builder(
          _loc.T(TitleLocKey))
        .AddSection(_loc.T(DescriptionLocKey))
        .Build();
    }

    private void InitializeMethodInfos()
    {
      EnterConstructionModeMethod = typeof(ConstructionModeService).GetMethod("EnterConstructionMode", BindingFlags.NonPublic | BindingFlags.Instance);
      ExitConstructionModeMethod = typeof(ConstructionModeService).GetMethod("ExitConstructionMode", BindingFlags.NonPublic | BindingFlags.Instance);
      _blockObjectToolOrientationField = typeof(BlockObjectTool).GetField("_orientation", BindingFlags.NonPublic | BindingFlags.Instance);
    }
    
    public void AddToolButtonToDictionary(GameObject gameObject, ToolButton toolButton)
    {
      if (gameObject.TryGetComponent(out Prefab prefab))
      {
        _toolButtons.Add(prefab.PrefabName, toolButton);
      }
    }

    public override ToolDescription Description() => _toolDescription;

    public override void Enter()
    {
      _shouldPipetNextSelection = true;
      _cursorService.SetTemporaryCursor(CursorKey);
    }

    public override void Exit()
    {
      _selectionManager.Unselect();
      _cursorService.ResetTemporaryCursor();
      _shouldPipetNextSelection = false;
    }

    public bool ProcessInput()
    {
      if (!_shouldPipetNextSelection) 
        return false;
      
      if (!_inputService.SelectionStart || _inputService.MouseOverUI)
        return false;
      
      if (_selectableObjectRaycaster.TryHitSelectableObject(out var hitObject))
        OnSelectableObjectSelected(hitObject);

      return false;
    }

    public virtual void PostProcessInput()
    {
      
    }

    public void OnSelectableObjectSelected(GameObject hitObject)
     {
       if (!_inputService.IsCtrlHeld && !_shouldPipetNextSelection) 
         return;
       
       if (!IsBlockObject(hitObject.gameObject)) 
         return;
       
       var selectableObjectName = hitObject.GetComponent<Prefab>().PrefabName;
       
       var tool = _toolButtons[selectableObjectName].Tool;
       
       ChangeToolOrientation(tool, hitObject.GetComponent<BlockObject>().Orientation);
       
       if (_mapEditorMode.IsMapEditor)
         SwitchToSelectedBuildingTool(tool);
       
       if (tool.DevModeTool && !IsDevToolEnabled) 
         return;
       
       SwitchToSelectedBuildingTool(tool);
     }

     private bool IsBlockObject(GameObject gameObject)
     {
       return gameObject.TryGetComponent(out BlockObject _);
     }

     private void ChangeToolOrientation(Tool tool, Orientation orientation)
     {
       if (tool.GetType() != typeof(BlockObjectTool)) 
         return;
       
       BlockObjectTool blockObjectTool = tool as BlockObjectTool;

       _blockObjectToolOrientationField.SetValue(blockObjectTool, orientation);
     }
     
     protected virtual void SwitchToSelectedBuildingTool(Tool tool)
     {
       _toolManager.SwitchTool(tool);
       _shouldPipetNextSelection = false;
       _cursorService.ResetTemporaryCursor();
     }
     
     private bool IsDevToolEnabled => _devModeManager.Enabled;
  }
}
