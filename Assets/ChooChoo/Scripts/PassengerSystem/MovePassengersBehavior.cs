using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using UnityEngine;

namespace ChooChoo
{
  public class MovePassengersBehavior : RootBehavior, IDeletableEntity
  {
    private TrainDestinationsRepository _trainDestinationsRepository;
    private BlockService _blockService;
    private ClosestTrainWaitingLocationPicker _closestTrainWaitingLocationPicker;
    private MoveToStationExecutor _moveToStationExecutor;
    private TrainWaitingLocation _currentWaitingLocation;

    private readonly List<Passenger> _passengers = new();
    private readonly List<Passenger> _reservedPassengers = new();

    public List<Passenger> Passengers => _passengers;

    [Inject]
    public void InjectDependencies(TrainDestinationsRepository trainDestinationsRepository, BlockService blockService, ClosestTrainWaitingLocationPicker closestTrainWaitingLocationPicker)
    {
      _trainDestinationsRepository = trainDestinationsRepository;
      _blockService = blockService;
      _closestTrainWaitingLocationPicker = closestTrainWaitingLocationPicker;
    }
    
    public void Awake()
    {
      _moveToStationExecutor = GetComponentFast<MoveToStationExecutor>();
    }

    public override Decision Decide(BehaviorAgent agent)
    {
      var passengerStations = _trainDestinationsRepository.TrainDestinations.Where(destionation => destionation.TryGetComponentFast(out PassengerStation _));

      
      var currentPassengerStation = _blockService.GetFloorObjectComponentAt<PassengerStation>(TransformFast.position.ToBlockServicePosition());

      if (_passengers.Any())
      {
        var destination = _passengers.First().PassengerStationLink.EndLinkPoint;
        if (currentPassengerStation == destination)
        {
          DropPassengersOff(currentPassengerStation);
          return Decision.ReleaseNow();
        }

        return GoToPassengerStation(destination.GetComponentFast<TrainDestination>());
      }
      
      if (_reservedPassengers.Any() && _reservedPassengers.First().PassengerStationLink.StartLinkPoint == currentPassengerStation)
      {
        LoadPassengers(currentPassengerStation);
        return Decision.ReleaseNow();
      }

      foreach (var trainDestination  in passengerStations)
      {
        var passengerStation = trainDestination.GetComponentFast<PassengerStation>();
        
        if (passengerStation.UnreservedPassengerQueue.Any())
        {
          ReservePassengers(passengerStation);
          return GoToPassengerStation(trainDestination);
        }
      }
      
      return Decision.ReleaseNow();
    }
    
    
    public void DeleteEntity()
    {
      foreach (var passenger in _passengers)
        passenger.ArrivedAtDestination();
    }
    
    private void DropPassengersOff(PassengerStation passengerStation)
    {
      // Plugin.Log.LogInfo("Dropping off passengers");
      foreach (var passenger in _passengers.ToList())
      {
        if (passenger.PassengerStationLink.EndLinkPoint == passengerStation)
        {
          passenger.ArrivedAtDestination();
          _passengers.Remove(passenger);
        }
      }
    }
    
    private void ReservePassengers(PassengerStation passengerStation)
    {      
      // Plugin.Log.LogInfo("Reserving Up passengers");
      var unreservedPassengers = passengerStation.UnreservedPassengerQueue;
      passengerStation.ReservedPassengerQueue.AddRange(unreservedPassengers);
      _reservedPassengers.AddRange(unreservedPassengers);
    }

    private void LoadPassengers(PassengerStation passengerStation)
    {
      // Plugin.Log.LogInfo("Picking Up passengers");
      foreach (var passenger in _reservedPassengers)
      {
        passengerStation.PassengerQueue.Remove(passenger);
        passengerStation.ReservedPassengerQueue.Remove(passenger);
        _passengers.Add(passenger);
      }
      _reservedPassengers.Clear();
      foreach (var passenger in passengerStation.UnreservedPassengerQueue)
      {
        passengerStation.PassengerQueue.Remove(passenger);
        _passengers.Add(passenger);
      }
    }

    private Decision OccupyWaitingLocation(TrainWaitingLocation trainWaitingLocation)
    {
      if (_currentWaitingLocation != null)
        _currentWaitingLocation.UnOccupy();
      _currentWaitingLocation = trainWaitingLocation;
      if (_currentWaitingLocation == null)
        return Decision.ReleaseNow();
      _currentWaitingLocation.Occupy(GameObjectFast);
      return GoToPassengerStation(_currentWaitingLocation.TrainDestinationComponent);
    }
    
    private Decision GoToPassengerStation(TrainDestination trainDestination)
    {
      switch (_moveToStationExecutor.Launch(trainDestination))
      {
        case ExecutorStatus.Success:
          return Decision.ReleaseNow();
        case ExecutorStatus.Failure:
          return Decision.ReleaseNow();
        case ExecutorStatus.Running:
          return Decision.ReturnWhenFinished(_moveToStationExecutor);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private Decision GoToClosestWaitingLocation()
    {
      var closestWaitingLocation = _closestTrainWaitingLocationPicker.ClosestWaitingLocation(TransformFast.position);
      return OccupyWaitingLocation(closestWaitingLocation);
    }
  }
}
