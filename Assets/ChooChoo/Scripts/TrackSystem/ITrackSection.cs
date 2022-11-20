using System.Collections.Generic;
using ChooChoo;

public abstract class ITrackSection
{
    public List<TrackPiece> TrackPieces;
    public abstract void Add(TrackPiece trackPiece);
    public abstract void Remove(TrackPiece trackPiece);
    public abstract void Merge(TrackSection trackSection);
}