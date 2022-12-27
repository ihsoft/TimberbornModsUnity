using System.Collections.Generic;

namespace ChooChoo
{
    public class TrackSectionService
    {
        private readonly TrackMap _trackMap;

        TrackSectionService(TrackMap trackMap)
        {
            _trackMap = trackMap;
        }

        public bool CanEnterSection(TrackSection trackSection) => !trackSection.Occupied;
    }
}
