﻿using Bindito.Core;
using System.Linq;
using Timberborn.ConstructibleSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using Timberborn.StatusSystem;
using Timberborn.TickSystem;

namespace ChooChoo
{
  public class UnconnectedTrainDestinationStatus : TickableComponent, IFinishedStateListener
  {
    private static readonly string LackOfResourcesLocKey = "Work.LackOfResources";
    private static readonly string LackOfResourcesShortLocKey = "Work.LackOfResourcesShort";
    private TrainDestinationService _trainDestinationService;
    private EventBus _eventBus;
    private ILoc _loc;
    private TrainDestination _trainDestination;
    private StatusToggle _unconnectedTrainDestinationStatusToggle;
    private bool _isUnconnectedToAnyTrainDestination;

    [Inject]
    public void InjectDependencies(TrainDestinationService trainDestinationService, EventBus eventBus, ILoc loc)
    {
      _trainDestinationService = trainDestinationService;
      _eventBus = eventBus;
      _loc = loc;
    }

    public void Awake()
    {
      _trainDestination = GetComponentFast<TrainDestination>();
      _unconnectedTrainDestinationStatusToggle = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon("LackOfResources", _loc.T(LackOfResourcesLocKey), _loc.T(LackOfResourcesShortLocKey));
      enabled = false;
    }

    public override void StartTickable()
    {
      GetComponentFast<StatusSubject>().RegisterStatus(_unconnectedTrainDestinationStatusToggle);
      CheckIfConnectedToTrainDestionation();
      UpdateStatusToggle();
    }

    public override void Tick() => UpdateStatusToggle();

    public void OnEnterFinishedState()
    {
      enabled = true;
      _eventBus.Register(this);
    }

    public void OnExitFinishedState()
    {
      _eventBus.Unregister(this);
      enabled = false;
    }
    
    [OnEvent]
    public void OnTrackUpdate(OnTracksUpdatedEvent onTracksUpdatedEvent)
    {
      CheckIfConnectedToTrainDestionation();
    }

    private void UpdateStatusToggle()
    {
      if (_isUnconnectedToAnyTrainDestination)
        _unconnectedTrainDestinationStatusToggle.Activate();
      else
        _unconnectedTrainDestinationStatusToggle.Deactivate();
    }

    private void CheckIfConnectedToTrainDestionation()
    {
      var connectedTrainDestinations = _trainDestinationService.GetConnectedTrainDestinations(_trainDestination);

      _isUnconnectedToAnyTrainDestination = !connectedTrainDestinations.Any();
    }
  }
}