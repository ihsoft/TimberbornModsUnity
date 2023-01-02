using System;
using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    public class TrackIntersection : TrackPiece
    {
        TrackIntersection() => IsIntersection = true;
    }
}