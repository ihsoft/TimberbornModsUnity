﻿using System;
using TimberApi.ObjectSelectionSystem;
using Timberborn.Localization;
using Timberborn.SelectionSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class ConnectPathLinkPointButton
  {
    private static readonly string PickPathLinkPointTipLocKey = "Tobbert.PathLinkPoint.Tip";
    private static readonly string PickPathLinkPointTitleLocKey = "Tobbert.PathLinkPoint.Title";
    private static readonly string PickPathLinkPointWarningLocKey = "Tobbert.PathLinkPoint.Warning";
    private static readonly string PickPathLinkPointAlreadyConnectedLocKey = "Tobbert.PathLinkPoint.AlreadyConnected";
    private static readonly string CreateLinkLocKey = "Tobbert.PathLinkPoint.CreateLink";
    private readonly ILoc _loc;
    private readonly PickObjectTool _pickObjectTool;
    private readonly EntitySelectionService _entitySelectionService;
    private readonly ToolManager _toolManager;
    private Button _button;

    public ConnectPathLinkPointButton(
      ILoc loc,
      PickObjectTool pickObjectTool,
      EntitySelectionService entitySelectionService,
      ToolManager toolManager)
    {
      _loc = loc;
      _pickObjectTool = pickObjectTool;
      _entitySelectionService = entitySelectionService;
      _toolManager = toolManager;
    }

    public void Initialize(
      VisualElement root,
      Func<PassengerStation> pathLinkPointProvider,
      Action createdRouteCallback)
    {
      _button = root.Q<Button>("PathLinkPointButton");
      _button.text = _loc.T(CreateLinkLocKey);
      _button.clicked += () => ConnectToPathLinkPoint(pathLinkPointProvider(), createdRouteCallback);
    }

    public void StopRouteAddition()
    {
      if (_toolManager.ActiveTool != _pickObjectTool)
        return;
      _toolManager.SwitchToDefaultTool();
    }

    private void ConnectToPathLinkPoint(PassengerStation passengerStation, Action createdRouteCallback)
    {
      _pickObjectTool.StartPicking<PassengerStation>(
        _loc.T(PickPathLinkPointTitleLocKey), 
        _loc.T(PickPathLinkPointTipLocKey), 
        gameObject => ValidatePathLinkPoint(gameObject, passengerStation), 
        dropOffPoint => FinishPathLinkPointSelection(passengerStation, dropOffPoint, createdRouteCallback));
      _entitySelectionService.Select(passengerStation);
    }

    private string ValidatePathLinkPoint(GameObject gameObject, PassengerStation passengerStation)
    {
      PassengerStation component = gameObject.GetComponent<PassengerStation>();
      if (!(bool) (UnityEngine.Object) component || component.PrefabName != passengerStation.PrefabName)
        return _loc.T(PickPathLinkPointWarningLocKey);
      return component.AlreadyConnected(passengerStation) ? _loc.T(PickPathLinkPointAlreadyConnectedLocKey) : "";
    }

    private void FinishPathLinkPointSelection(
      PassengerStation originPassengerStation,
      GameObject gameObject,
      Action createdRouteCallback)
    {
      PassengerStation component = gameObject.GetComponent<PassengerStation>();
      if (originPassengerStation.PrefabName != component.PrefabName)
        return;
      originPassengerStation.Connect(component);
      createdRouteCallback();
    }
  }
}
