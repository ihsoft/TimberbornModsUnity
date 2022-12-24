using System.Collections.Generic;

namespace ChooChoo
{
    public class StationActions
    {
        public TrainDestination Station;

        public List<ITrainAction> Actions;

        public StationActions(TrainDestination station, List<ITrainAction> actions)
        {
            Station = station;
            Actions = actions;
        }
    }
}