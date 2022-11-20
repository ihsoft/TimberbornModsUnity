using Timberborn.BlockSystem;
using Timberborn.PreviewSystem;
using UnityEngine;

namespace ChooChoo
{
  public class TracksService
  {
    private readonly BlockService _blockService;
    private readonly PreviewBlockService _previewBlockService;

    public TracksService(BlockService blockService, PreviewBlockService previewBlockService)
    {
      _blockService = blockService;
      _previewBlockService = previewBlockService;
    }

    public bool IsFinishedPath(Vector3Int coordinates) => IsRail(coordinates, false);

    public bool IsRail(Vector3Int coordinates) => IsRail(coordinates, true);

    private bool IsRail(Vector3Int coordinates, bool allowUnfinished)
    {
      if (!allowUnfinished)
        return IsRail(_blockService.GetFloorObjectAt(coordinates), false);
      return IsRail(_blockService.GetFloorObjectAt(coordinates), true) || IsRail(_previewBlockService.GetFloorObjectAt(coordinates), true);
    }

    private static bool IsRail(BlockObject blockObject, bool allowUnfinished)
    {
      Plugin.Log.LogAssert("Checking rail");
      return (bool) (Object) blockObject && (allowUnfinished || blockObject.Finished) && blockObject.TryGetComponent(out TrackPiece _);
    }
  }
}
