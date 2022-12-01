using System.Collections.Generic;
using ChooChoo;

public abstract class ITrackSection
{
    public List<TrackPiece> TrackPieces;
    public bool Occupied;
    public abstract void Add(TrackPiece trackPiece);
    public abstract void Dissolve();
    public abstract void Merge(TrackSection trackSection);
}