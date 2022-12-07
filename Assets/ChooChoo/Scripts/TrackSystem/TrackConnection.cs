using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    public class TrackConnection
    {
        public TrackConnection(Vector3Int coordinate, Direction2D direction, Vector3[] pathCorners)
        {
            Coordinates = coordinate;
            Direction = direction;
            PathCorners = pathCorners;
            ConnectedTrackPiece = null;
            ConnectedTrackConnection = null;
        }

        public Vector3Int Coordinates { get; }

        public Direction2D Direction { get; set; }

        public Vector3[] PathCorners { get; set; }
        
        public TrackPiece ConnectedTrackPiece { get; set; }
        
        public TrackConnection ConnectedTrackConnection { get; set; }

        // [OnEvent]
        // public void OnTrackDeleted(OnTrackDeletedEvent onTrackDeletedEvent)
        // {
        //     if (onTrackDeletedEvent.TrackSection == TrackSection)
        //         TrackSection = null;
        // }
    }
}