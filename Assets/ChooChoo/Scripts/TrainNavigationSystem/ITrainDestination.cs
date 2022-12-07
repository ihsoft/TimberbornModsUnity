﻿using System.Collections.Generic;
using UnityEngine;

namespace ChooChoo
{
  public interface ITrainDestination
  {
    bool GeneratePath(Vector3 start, ref TrackConnection previousLastTrackConnection, List<TrackConnection> pathCorners);
  }
}
