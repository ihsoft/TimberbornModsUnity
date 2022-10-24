using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Timberborn.AssetSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
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
  public class PipetteTool : Tool, IInputProcessor, ILoadableSingleton
  {
    private static readonly string TitleLocKey = "Tobbert.PipetteTool.DisplayName";
        
    private static readonly string DescriptionLocKey = "Tobbert.PipetteTool.Description";
    
    public readonly string CursorKey = "PipetteCursor";

    private readonly EventBus _eventBus;
    
    private readonly ToolManager _toolManager;

    private readonly DevModeManager _devModeManager;
    
    private readonly InputService _inputService;

    private readonly MapEditorMode _mapEditorMode;
    
    private readonly CursorService _cursorService;

    private readonly DescriptionPanel _descriptionPanel;

    private readonly ILoc _loc;

    public readonly IResourceAssetLoader _resourceAssetLoader;
    
    private readonly PipetteToolGroup _pipetteToolGroup;
    
    private readonly Dictionary<string, ToolButton> _toolButtons = new();
        
    private ToolDescription _toolDescription;

    private bool _shouldPipetNextSelection;

    private MethodInfo _hideDescriptionPanelMethod;

    private FieldInfo _blockObjectToolOrientationField;
    
    private Texture2D _pipetteToolCursor = new(150, 150);

    private const CursorMode CursorMode = UnityEngine.CursorMode.Auto;
    
    private readonly Vector2 _hotSpot = Vector2.zero;
    
    public PipetteTool(EventBus eventBus, ToolManager toolManager, DevModeManager devModeManager, InputService inputService, MapEditorMode mapEditorMode, CursorService cursorService, DescriptionPanel descriptionPanel, ILoc loc, PipetteToolGroup pipetteToolGroup, IResourceAssetLoader resourceAssetLoader)
    {
      _eventBus = eventBus;
      _toolManager = toolManager;
      _devModeManager = devModeManager;
      _inputService = inputService;
      _mapEditorMode = mapEditorMode;
      _cursorService = cursorService;
      _descriptionPanel = descriptionPanel;
      _loc = loc;
      _pipetteToolGroup = pipetteToolGroup;
      _resourceAssetLoader = resourceAssetLoader;
    }

    public void Load()
    {
      _inputService.AddInputProcessor(this);
      _eventBus.Register(this);
      ToolGroup = _pipetteToolGroup;
      _hideDescriptionPanelMethod = AccessTools.TypeByName(_descriptionPanel.GetType().Name).GetMethod("Hide", BindingFlags.NonPublic | BindingFlags.Instance);
      _blockObjectToolOrientationField = typeof(BlockObjectTool).GetField("_orientation", BindingFlags.NonPublic | BindingFlags.Instance);
      // InitializeCustomCursor();
      InitializeToolDescription();
    }
    
    private void InitializeToolDescription()
    {
      _toolDescription = new ToolDescription.Builder(
          _loc.T(TitleLocKey))
        .AddSection(_loc.T(DescriptionLocKey))
        .Build();
    }

    private void InitializeCustomCursor()
    {
      _pipetteToolCursor = _resourceAssetLoader.Load<Sprite>("tobbert.pipettetool/tobbert_pipettetool/PipetteToolCursor").texture;
      // var bytes = _pipetteToolCursor.GetRawTextureData();
      
    
      // var fieldInfo = AccessTools.TypeByName(_cursorService.GetType().Name).GetField("_cursors", BindingFlags.NonPublic | BindingFlags.Instance);
      // var cursors =  fieldInfo.GetValue(_cursorService) as Dictionary<string, object>;
      // Texture2D CustomCursorTexture = 
    
      //
      // _cursorService.SetCursor(CursorKey);
    
      // var copy = Helper.CreateDeepCopy(cursors.First().Value);
      //
      // // var test = ScriptableObject.CreateInstance<CustomCursor>();
      // // test.Initialize(CustomCursorTexture, CustomCursorTexture, new Vector2(0, 0));
      // var test = new CustomCursor(CustomCursorTexture, CustomCursorTexture, new Vector2(0, 0));
      // cursors.Add(CursorKey, copy);
    }
    
    public override ToolDescription Description() => _toolDescription;

    public override void Enter()
    {
      // Cursor.SetCursor(_pipetteToolCursor, _hotSpot, CursorMode);
      _shouldPipetNextSelection = true;
      _toolManager.SwitchToDefaultTool();
    }

    public override void Exit()
    {
      //dont need
    }

    public bool ProcessInput()
    {
      if (_inputService.Cancel)
      {
        _shouldPipetNextSelection = false;
        _hideDescriptionPanelMethod.Invoke(_descriptionPanel, new object[]{});
      }
      
      return false;
    }
    
     public void AddToolButtonToDictionary(GameObject gameObject, ToolButton toolButton)
     {
       if (gameObject.TryGetComponent(out Prefab prefab))
       {
         _toolButtons.Add(prefab.PrefabName, toolButton);
       }
     }

     public void OnGameObjectSelected(SelectableObject selectableObject)
     {
       if (!_inputService.IsCtrlHeld && !_shouldPipetNextSelection) 
         return;
       
       if (!IsBlockObject(selectableObject.gameObject)) 
         return;
       
       var selectableObjectName = selectableObject.GetComponent<Prefab>().PrefabName;
       
       var tool = _toolButtons[selectableObjectName].Tool;
       
       ChangeToolOrientation(tool, selectableObject.GetComponent<BlockObject>().Orientation);
       
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

     private void SwitchToSelectedBuildingTool(Tool tool)
     {
       _toolManager.SwitchTool(tool);
       _shouldPipetNextSelection = false;
     }
     
     private void ChangeToolOrientation(Tool tool, Orientation orientation)
     {
       if (tool.GetType() == typeof(BlockObjectTool))
       {
         BlockObjectTool blockObjectTool = tool as BlockObjectTool;

         _blockObjectToolOrientationField.SetValue(blockObjectTool, orientation);
       }
     }
     
     private bool IsDevToolEnabled => _devModeManager.Enabled;
  }
}
