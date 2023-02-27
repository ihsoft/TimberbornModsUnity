using System;
using Timberborn.Localization;
using Timberborn.PickObjectToolSystem;
using Timberborn.SelectionSystem;
using UnityEngine;
using UnityEngine.UIElements;
using ToolManager = Timberborn.ToolSystem.ToolManager;

namespace ChooChoo
{
  public class ConnectPathLinkPointButton
  {
    private static readonly string PickDropOffPointTipLocKey = "Distribution.PickDropOffPointTip";
    private static readonly string PickDropOffPointTitleLocKey = "Distribution.PickDropOffPointTitle";
    private static readonly string PickDropOffPointWarningLocKey = "Distribution.PickDropOffPointWarning";
    private static readonly string AddNewStationLocKey = "Tobbert.TrainSchedule.AddNewStation";
    private readonly ILoc _loc;
    private readonly PickObjectTool _pickObjectTool;
    private readonly SelectionManager _selectionManager;
    private readonly ToolManager _toolManager;
    private Button _button;

    public ConnectPathLinkPointButton(
      ILoc loc,
      PickObjectTool pickObjectTool,
      SelectionManager selectionManager,
      ToolManager toolManager)
    {
      _loc = loc;
      _pickObjectTool = pickObjectTool;
      _selectionManager = selectionManager;
      _toolManager = toolManager;
    }

    public void Initialize(
      VisualElement root,
      Func<PathLinkPoint> trainScheduleControllerProvider,
      Action createdRouteCallback)
    {
      _button = root.Q<Button>("PathLinkPointButton");
      _button.text = _loc.T(AddNewStationLocKey);
      _button.clicked += () => AddNewStation(trainScheduleControllerProvider(), createdRouteCallback);
    }

    public void StopRouteAddition()
    {
      if (_toolManager.ActiveTool != _pickObjectTool)
        return;
      _toolManager.SwitchToDefaultTool();
    }

    private void AddNewStation(
      PathLinkPoint trainScheduleController,
      Action createdRouteCallback)
    {
      _pickObjectTool.StartPicking<PathLinkPoint>(
        _loc.T(PickDropOffPointTitleLocKey), 
        _loc.T(PickDropOffPointTipLocKey), 
        gameObject => ValidatePathLinkPoint(gameObject, trainScheduleController), 
        dropOffPoint => FinishPathLinkPointSelection(trainScheduleController, dropOffPoint, createdRouteCallback));
      _selectionManager.Select(trainScheduleController.gameObject);
    }

    private string ValidatePathLinkPoint(GameObject gameObject, PathLinkPoint pathLinkPoint)
    {
      PathLinkPoint component = gameObject.GetComponent<PathLinkPoint>();
      
      if (!component)
        return _loc.T(PickDropOffPointWarningLocKey);

      return "";
    }

    private void FinishPathLinkPointSelection(
      PathLinkPoint originPathLinkPoint,
      GameObject gameObject,
      Action createdRouteCallback)
    {
      PathLinkPoint pathLinkPoint = gameObject.GetComponent<PathLinkPoint>();
      if (originPathLinkPoint.PrefabName != pathLinkPoint.PrefabName)
        return;
      originPathLinkPoint.Connect(pathLinkPoint);
      createdRouteCallback();
    }
  }
}
