using System.Collections.Generic;
using System.Linq;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace ChooChoo
{
  public class PassengerStationLinkRepository : IPostLoadableSingleton, ITickableSingleton
  {
    private readonly TrainDestinationConnectedRepository _trainDestinationConnectedRepository;
    private readonly TrainDestinationService _trainDestinationService;
    private readonly IDayNightCycle _dayNightCycle;
    private readonly EventBus _eventBus;
    
    private readonly HashSet<PassengerStationLink> _pathLinks = new();
    private bool _tracksUpdated;

    PassengerStationLinkRepository(TrainDestinationConnectedRepository trainDestinationConnectedRepository, TrainDestinationService trainDestinationService, IDayNightCycle dayNightCycle, EventBus eventBus)
    {
      _trainDestinationConnectedRepository = trainDestinationConnectedRepository;
      _trainDestinationService = trainDestinationService;
      _dayNightCycle = dayNightCycle;
      _eventBus = eventBus;
    }

    public void PostLoad()
    {
      _eventBus.Register(this);
      UpdateLinks();
    }

    public void Tick()
    {
      if (!_tracksUpdated)
        return;
      UpdateLinks();
      _tracksUpdated = false;
    }

    [OnEvent]
    public void OnTrackUpdate(OnTracksUpdatedEvent onTracksUpdatedEvent)
    {
      _tracksUpdated = true;
    }

    private void UpdateLinks()
    {
      // Plugin.Log.LogWarning("Updating PassengerLinks");
      _pathLinks.Clear();
      var trainDestinations = _trainDestinationConnectedRepository.TrainDestinations;
      foreach (var trainDestination in trainDestinations.Keys)
      {
        // Plugin.Log.LogInfo(trainDestination.GameObjectFast.name);
        if (!trainDestination.TryGetComponentFast(out PassengerStation passengerStation))
          continue;
        var connectedTrainDestinations = trainDestinations[trainDestination];
        foreach (var connectedTrainDestination in connectedTrainDestinations)
        {
          // Plugin.Log.LogError(connectedTrainDestination.GameObjectFast.name);
          if (!connectedTrainDestination.TryGetComponentFast(out PassengerStation connectedPassengerStation))
            continue;
          if (passengerStation == connectedPassengerStation)
            continue;
          if (_trainDestinationService.TrainDestinationsConnectedBothWays(trainDestination, connectedTrainDestination))
          {
            // Plugin.Log.LogWarning("connecting");
            passengerStation.Connect(connectedPassengerStation);
          }
        }
      }
      _eventBus.Post(new OnConnectedPassengerStationsUpdated());
    }

    public void AddNew(PassengerStationLink passengerStationLink) => _pathLinks.Add(passengerStationLink);

    public PassengerStationLink GetPathLink(Vector3 startBeaverPosition, Vector3 endBeaverPosition)
    {
      foreach (PassengerStationLink pathLink in _pathLinks)
      {
        Vector3 location1 = pathLink.StartLinkPoint.Location;
        Vector3 location2 = pathLink.EndLinkPoint.Location;
        if (Vector3Int.CeilToInt(location1) == Vector3Int.CeilToInt(startBeaverPosition) && Vector3Int.FloorToInt(location2) == Vector3Int.FloorToInt(endBeaverPosition) || Vector3Int.FloorToInt(location1) == Vector3Int.FloorToInt(startBeaverPosition) && Vector3Int.CeilToInt(location2) == Vector3Int.CeilToInt(endBeaverPosition))
          return pathLink;
      }
      return null;
    }

    public PassengerStationLink GetPathLink(
      PassengerStation startPassengerStation,
      PassengerStation endPassengerStation)
    {
      foreach (PassengerStationLink pathLink in _pathLinks)
      {
        if (startPassengerStation == pathLink.StartLinkPoint && endPassengerStation == pathLink.EndLinkPoint)
          return pathLink;
      }
      return null;
    }

    public void RemoveInvalidLinks() => _pathLinks.RemoveWhere(link => !link.ValidLink());

    public IEnumerable<PassengerStationLink> PathLinks(PassengerStation a) => _pathLinks.Where(link => link.StartLinkPoint == a);

    public void RemoveLinks(PassengerStation a)
    {
      _pathLinks.RemoveWhere(link => link.StartLinkPoint == a || link.EndLinkPoint == a);
      _eventBus.Post(new OnConnectedPassengerStationsUpdated());
    }

    public bool AlreadyConnected(PassengerStation a, PassengerStation b)
    {
      if (!a.ConnectsTwoWay)
        return GetPathLink(a, b) != null;
      return GetPathLink(a, b) != null || GetPathLink(b, a) != null;
    }

    private float CalculateWaitingTimeInHours(PassengerStation startPoint, PassengerStation endPoint) => _dayNightCycle.SecondsToHours(Vector3.Distance(startPoint.Location, endPoint.Location) / (2.7f * startPoint.MovementSpeedMultiplier));
  }
}
