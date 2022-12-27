using System.Collections.Generic;

namespace ChooChoo
{
    public interface ITrackFollower
    {
        public int CurrentCornerIndex { get; }

        // public List<TrackConnection> PathConnections { get;  }
    }
}