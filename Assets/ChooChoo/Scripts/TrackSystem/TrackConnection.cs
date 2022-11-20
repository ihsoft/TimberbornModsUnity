using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    public class TrackConnection
    {
        public TrackConnection(Vector3Int coordinate, Direction2D direction)
        {
            Coordinates = coordinate;
            Direction = direction;
            TrackSection = null;
        }

        public Vector3Int Coordinates { get; }

        public Direction2D Direction { get; }
        
        public TrackSection TrackSection { get; set; }

        // [OnEvent]
        // public void OnTrackDeleted(OnTrackDeletedEvent onTrackDeletedEvent)
        // {
        //     if (onTrackDeletedEvent.TrackSection == TrackSection)
        //         TrackSection = null;
        // }
    }
}