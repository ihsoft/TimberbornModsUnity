using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChooChoo
{
  public class TrainPositionDestination : ITrainDestination, IEquatable<TrainPositionDestination>
  {
    private readonly TrainNavigationService _trainNavigationService;

    public Vector3 Destination { get; }

    public TrainPositionDestination(TrainNavigationService trainNavigationService, Vector3 destination)
    {
      _trainNavigationService = trainNavigationService;
      Destination = destination;
    }

    public bool GeneratePath(Vector3 start, List<TrackConnection> pathCorners)
    {
      return _trainNavigationService.FindRailTrackPath(start, Destination, pathCorners);
    }

    public bool Equals(TrainPositionDestination other)
    {
      if ((object) other == null)
        return false;
      if ((object) this == (object) other)
        return true;
      return Destination.Equals(other.Destination);
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      if ((object) this == obj)
        return true;
      return !(obj.GetType() != GetType()) && Equals((TrainPositionDestination) obj);
    }

    public static bool operator ==(TrainPositionDestination left, TrainPositionDestination right) => object.Equals((object) left, (object) right);

    public static bool operator !=(TrainPositionDestination left, TrainPositionDestination right) => !object.Equals((object) left, (object) right);
  }
}
