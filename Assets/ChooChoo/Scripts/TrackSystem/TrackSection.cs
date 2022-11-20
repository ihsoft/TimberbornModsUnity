using System.Collections.Generic;
using ChooChoo;

public class TrackSection : ITrackSection
{
    public List<TrackPiece> TrackPieces = new();

    public TrackSection(TrackPiece firstTrackPiece)
    {
        TrackPieces.Add(firstTrackPiece);
    }

    public override void Add(TrackPiece trackPiece)
    {
        TrackPieces.Add(trackPiece);
        trackPiece.TrackSection = this;
    }

    public override void Remove(TrackPiece trackPiece)
    {
        TrackPieces.Remove(trackPiece);
    }

    public override void Merge(TrackSection trackSection)
    {
        foreach (var trackPiece in trackSection.TrackPieces)
        {
            trackPiece.TrackSection = this;
        }
        TrackPieces.AddRange(trackSection.TrackPieces);
    }
}
