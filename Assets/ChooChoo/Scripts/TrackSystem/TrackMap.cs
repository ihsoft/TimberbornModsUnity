using System.Collections.Generic;

public class TrackMap
{
    private readonly List<TrackSection> _trackSections = new();

    public List<TrackSection> TrackSections => _trackSections;

    public void Add(TrackSection trackSection)
    {
        _trackSections.Add(trackSection);
    }
    
    public void Remove(TrackSection trackSection)
    {
        _trackSections.Remove(trackSection);
    }
}
