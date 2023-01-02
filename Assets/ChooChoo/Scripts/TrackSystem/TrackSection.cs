using System.Collections.Generic;
using ChooChoo;
using UnityEngine;

public class TrackSection
{
    public readonly List<TrackPiece> TrackPieces = new();

    public GameObject OccupyingTrain;

    public bool Occupied;

    public bool HasWaitingTrain;

    public TrackSection(TrackPiece firstTrackPiece)
    {
        TrackPieces.Add(firstTrackPiece);
    }

    public void Add(TrackPiece trackPiece)
    {
        TrackPieces.Add(trackPiece);
    }

    public void Merge(TrackSection trackSection)
    {
        foreach (var trackPiece in trackSection.TrackPieces)
        {
            trackPiece.TrackSection = this;
        }
        TrackPieces.AddRange(trackSection.TrackPieces);
    }
    
    public void Dissolve()
    {
        foreach (var track in TrackPieces)
            track.ResetSection();
        foreach (var track in TrackPieces)
            track.LookForTrackSection();
    }

    public void Enter(GameObject train)
    {
        Occupied = true;
        OccupyingTrain = train;
    }

    public void Leave()
    {
        Occupied = false;
        OccupyingTrain = null;
        HasWaitingTrain = false;
    }
}
