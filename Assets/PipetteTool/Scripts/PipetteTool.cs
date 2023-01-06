using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    protected readonly MethodInfo EnterConstructionModeMethod;

    protected readonly MethodInfo ExitConstructionModeMethod;

    private readonly FieldInfo _blockObjectToolOrientationField;
    
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
      
      EnterConstructionModeMethod = typeof(ConstructionModeService).GetMethod("EnterConstructionMode", BindingFlags.NonPublic | BindingFlags.Instance);
      ExitConstructionModeMethod = typeof(ConstructionModeService).GetMethod("ExitConstructionMode", BindingFlags.NonPublic | BindingFlags.Instance);
      _blockObjectToolOrientationField = typeof(BlockObjectTool).GetField("_orientation", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public void Load()
    {
      _inputService.AddInputProcessor(this);
      _eventBus.Register(this);
      _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();
    }

    public void AddToolButtonToDictionary(GameObject gameObject, ToolButton toolButton)
    {
      if (!gameObject.TryGetComponent(out Prefab prefab)) 
        return;
      if (!_toolButtons.ContainsKey(prefab.PrefabName))
        _toolButtons.Add(prefab.PrefabName, toolButton);
      else
        Plugin.Log.LogInfo($"Button of {prefab.PrefabName} already exists: Skipping.");
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

    public void OnSelectableObjectSelected(GameObject hitObject)
     {
       if (!_inputService.IsCtrlHeld && !_shouldPipetNextSelection) 
         return;

       var selectableObjectName = hitObject.GetComponent<Prefab>().PrefabName;

       if (!_toolButtons.ContainsKey(selectableObjectName))
         return;
       
       var tool = _toolButtons[selectableObjectName].Tool;
       
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
     
     protected virtual void SwitchToSelectedBuildingTool(Tool tool, GameObject hitObject)
     {
       ChangeToolOrientation(tool, hitObject.GetComponent<BlockObject>().Orientation);
       _toolManager.SwitchTool(tool);
       _shouldPipetNextSelection = false;
       _cursorService.ResetTemporaryCursor();
     }
     
     private bool IsDevToolEnabled => _devModeManager.Enabled;
  }
}
