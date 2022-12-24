using System;
using System.Linq;
using Timberborn.Localization;
using Timberborn.PickObjectToolSystem;
using Timberborn.SelectionSystem;
using UnityEngine;
using UnityEngine.UIElements;
using ToolManager = Timberborn.ToolSystem.ToolManager;

namespace ChooChoo
{
  public class NewStationButton
  {
    private static readonly string PickDropOffPointTipLocKey = "Distribution.PickDropOffPointTip";
    private static readonly string PickDropOffPointTitleLocKey = "Distribution.PickDropOffPointTitle";
    private static readonly string PickDropOffPointWarningLocKey = "Distribution.PickDropOffPointWarning";
    private static readonly string PickStationSameStationWarningLocKey = "Tobbert.PickStation.SameStationWarning";
    private static readonly string AddNewStationLocKey = "Tobbert.TrainSchedule.AddNewStation";
    private readonly ILoc _loc;
    private readonly PickObjectTool _pickObjectTool;
    private readonly SelectionManager _selectionManager;
    private readonly ToolManager _toolManager;
    private Button _button;

    public NewStationButton(
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
      Func<TrainScheduleController> trainScheduleControllerProvider,
      Action createdRouteCallback)
    {
      _button = root.Q<Button>("NewStationButton");
      _button.text = _loc.T(AddNewStationLocKey);
      _button.clicked += () => AddNewStation(trainScheduleControllerProvider(), createdRouteCallback);
    }

    public void StopRouteAddition()
    {
      if (_toolManager.ActiveTool != _pickObjectTool)
        return;
      _toolManager.SwitchToDefaultTool();
    }

    // public void UpdateRemainingRouteSlots(int currentRoutes, int maxRoutes)
    // {
    //   _button.text = $"{(object)_loc.T(AddNewRouteLocKey)} ({currentRoutes}/{maxRoutes})";
    //   _button.SetEnabled(currentRoutes < maxRoutes);
    // }

    private void AddNewStation(
      TrainScheduleController trainScheduleController,
      Action createdRouteCallback)
    {
      _pickObjectTool.StartPicking<TrainDestination>(
        _loc.T(PickDropOffPointTitleLocKey), 
        _loc.T(PickDropOffPointTipLocKey), 
        gameObject => ValidateTrainDestination(gameObject, trainScheduleController), 
        dropOffPoint => FinishTrainDestinationSelection(trainScheduleController, dropOffPoint, createdRouteCallback));
      _selectionManager.Select(trainScheduleController.gameObject);
    }

    private string ValidateTrainDestination(GameObject gameObject, TrainScheduleController trainScheduleController)
    {
      TrainDestination component = gameObject.GetComponent<TrainDestination>();
      
      if (!component)
        return _loc.T(PickDropOffPointWarningLocKey);
      
      if (trainScheduleController.TrainSchedule.Count > 0 && trainScheduleController.TrainSchedule.Last().Station == component)
        return _loc.T(PickStationSameStationWarningLocKey);
      
      return "";
    }

    private void FinishTrainDestinationSelection(
      TrainScheduleController trainScheduleController,
      GameObject gameObject,
      Action createdRouteCallback)
    {
      TrainDestination trainDestination = gameObject.GetComponent<TrainDestination>();
      if (trainScheduleController.TrainSchedule.Count > 0 && trainScheduleController.TrainSchedule.Last().Station == trainDestination)
        return;
      trainScheduleController.AddStation(trainDestination);
      createdRouteCallback();
    }
  }
}
