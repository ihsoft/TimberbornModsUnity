using System;
using Bindito.Core;
using System.Collections.Generic;
using Timberborn.CharacterModelSystem;
using Timberborn.CharacterMovementSystem;
using Timberborn.TickSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace ChooChoo
{
  public class Passenger : TickableComponent
  {
    private PassengerStationLinkRepository _passengerStationLinkRepository;
    private IDayNightCycle _dayNightCycle;
    private CharacterModel _characterModel;
    private float _waitUntillTimestamp;

    private bool IsWaiting => (double) _waitUntillTimestamp > (double) _dayNightCycle.PartialDayNumber;

    [Inject]
    public void InjectDependencies(PassengerStationLinkRepository passengerStationLinkPointService, IDayNightCycle dayNightCycle) 
    {
      _passengerStationLinkRepository = passengerStationLinkPointService;
      _dayNightCycle = dayNightCycle;
    }

    private void Awake()
    {
      _characterModel = GetComponentFast<CharacterModel>();
    }

    public override void Tick()
    {
      _characterModel.Model.gameObject.SetActive(!IsWaiting);
    }

    public bool ShouldWait(IReadOnlyList<Vector3> pathCorners, ref int nextCornerIndex, MovementAnimator movementAnimator)
    {
      return IsWaiting || CheckForPathLink(ref pathCorners, ref nextCornerIndex, movementAnimator);
    }

    private bool CheckForPathLink(ref IReadOnlyList<Vector3> pathCorners, ref int nextCornerIndex, MovementAnimator movementAnimator)
    {
      // Plugin.Log.LogInfo("index: " + nextCornerIndex + "  Count: " + pathCorners.Count);
      PassengerStationLink passengerStationLink = _passengerStationLinkRepository.GetPathLink(pathCorners[nextCornerIndex - 1], pathCorners[nextCornerIndex]);
      if (passengerStationLink == null)
        return false;
      movementAnimator.StopAnimatingMovement();
      if (nextCornerIndex + 1 < pathCorners.Count)
      {
        TransformFast.position = passengerStationLink.EndLinkPoint.Location;
        ++nextCornerIndex;
      }
      else
      {
        TransformFast.position = pathCorners[nextCornerIndex];
        return false;
      }
      _waitUntillTimestamp = _dayNightCycle.DayNumberHoursFromNow(passengerStationLink.WaitingTimeInHours);
      return true;
    }
  }
}
