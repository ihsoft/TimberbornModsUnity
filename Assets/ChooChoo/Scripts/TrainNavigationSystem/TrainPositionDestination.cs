using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChooChoo
{
  public class TrainPositionDestination : ITrainDestination, IEquatable<TrainPositionDestination>
  {
    private readonly TrainNavigationService _trainNavigationService;

    public TrainDestination Destination { get; }

    public TrainPositionDestination(TrainNavigationService trainNavigationService, TrainDestination destination)
    {
      _trainNavigationService = trainNavigationService;
      Destination = destination;
    }

    public bool GeneratePath(Transform transform, List<TrackRoute> pathCorners)
    {
      return _trainNavigationService.FindRailTrackPath(transform, Destination, pathCorners);
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
