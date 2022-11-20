using System;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using Timberborn.Navigation;
using Timberborn.PreviewSystem;
using UnityEngine;

namespace ChooChoo
{
  public class TrackConnectionService
  {
    private readonly BlockService _blockService;
    private readonly PreviewBlockService _previewBlockService;
    private readonly INavMeshService _navMeshService;
    private readonly TracksService _tracksService;

    public TrackConnectionService(
      BlockService blockService,
      PreviewBlockService previewBlockService,
      INavMeshService navMeshService,
      TracksService tracksService)
    {
      _blockService = blockService;
      _previewBlockService = previewBlockService;
      _navMeshService = navMeshService;
      _tracksService = tracksService;
    }

    public bool CanConnectInDirection(Vector3Int origin, Direction2D direction2D)
    {
      Vector3Int target = origin + direction2D.ToOffset();
      return CanConnect(origin, target);
    }

    private bool CanConnect(Vector3Int origin, Vector3Int target) => IsRail(origin, target) || IsStation(origin, target);

    private bool IsRail(Vector3Int origin, Vector3Int target) => _tracksService.IsRail(origin) && _tracksService.IsRail(target);

    private bool IsStation(Vector3Int origin, Vector3Int target) => IsEntranceInDirectionAt(origin, target) || IsEntranceInDirectionAt(target, origin);

    private bool IsEntranceInDirectionAt(
      Vector3Int entranceCoordinates,
      Vector3Int doorstepCoordinates)
    {
      if (!_navMeshService.AreConnectedPreview(entranceCoordinates, doorstepCoordinates) || !_tracksService.IsRail(entranceCoordinates))
        return false;
      Direction2D entranceDirection = ToEntranceDirection(entranceCoordinates - doorstepCoordinates);
      return ((_blockService.GetEntrancesAt(entranceCoordinates) | _previewBlockService.GetEntrancesAt(entranceCoordinates)) & entranceDirection.ToDirections()) != 0;
    }

    private static Direction2D ToEntranceDirection(Vector3Int direction)
    {
      if (direction == Direction2D.Down.ToOffset())
        return Direction2D.Down;
      if (direction == Direction2D.Left.ToOffset())
        return Direction2D.Left;
      if (direction == Direction2D.Right.ToOffset())
        return Direction2D.Right;
      if (direction == Direction2D.Up.ToOffset())
        return Direction2D.Up;
      throw new ArgumentOutOfRangeException(nameof (direction), (object) direction, (string) null);
    }
  }
}
