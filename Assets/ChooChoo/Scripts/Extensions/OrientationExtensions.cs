using System;
using Timberborn.Coordinates;

namespace ChooChoo
{
    public static class OrientationExtensions
    {
        public static Direction2D ToDirection(this Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Cw0:
                    return Direction2D.Up;
                case Orientation.Cw90:
                    return Direction2D.Right;
                case Orientation.Cw180:
                    return Direction2D.Down;
                case Orientation.Cw270:
                    return Direction2D.Left;
                default:
                    throw new ArgumentOutOfRangeException(nameof (orientation), orientation, null);
            }
        }
    }
}
