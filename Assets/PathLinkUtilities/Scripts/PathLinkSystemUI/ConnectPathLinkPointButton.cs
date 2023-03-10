using System;
using Timberborn.Localization;
using Timberborn.PickObjectToolSystem;
using Timberborn.SelectionSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathLinkUtilities
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
      Func<PathLinkPoint> pathLinkPointProvider,
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

    private void ConnectToPathLinkPoint(PathLinkPoint pathLinkPoint, Action createdRouteCallback)
    {
      _pickObjectTool.StartPicking<PathLinkPoint>(
        _loc.T(PickPathLinkPointTitleLocKey), 
        _loc.T(PickPathLinkPointTipLocKey), 
        gameObject => ValidatePathLinkPoint(gameObject, pathLinkPoint), 
        dropOffPoint => FinishPathLinkPointSelection(pathLinkPoint, dropOffPoint, createdRouteCallback));
      _selectionManager.Select(pathLinkPoint.gameObject);
    }

    private string ValidatePathLinkPoint(GameObject gameObject, PathLinkPoint pathLinkPoint)
    {
      PathLinkPoint component = gameObject.GetComponent<PathLinkPoint>();
      if (!(bool) (UnityEngine.Object) component || component.PrefabName != pathLinkPoint.PrefabName)
        return _loc.T(PickPathLinkPointWarningLocKey);
      return component.AlreadyConnected(pathLinkPoint) ? _loc.T(PickPathLinkPointAlreadyConnectedLocKey) : "";
    }

    private void FinishPathLinkPointSelection(
      PathLinkPoint originPathLinkPoint,
      GameObject gameObject,
      Action createdRouteCallback)
    {
      PathLinkPoint component = gameObject.GetComponent<PathLinkPoint>();
      if (originPathLinkPoint.PrefabName != component.PrefabName)
        return;
      originPathLinkPoint.Connect(component);
      createdRouteCallback();
    }
  }
}
