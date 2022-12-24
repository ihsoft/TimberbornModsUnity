using Timberborn.Persistence;

namespace ChooChoo
{
  public class TrainScheduleObjectSerializer : IObjectSerializer<StationActions>
  {
    private static readonly PropertyKey<TrainDestination> StationKey = new("Station");
    private static readonly ListKey<ITrainAction> TrainActionsKey = new("TrainActions");

    private readonly TrainActionObjectSerializer _trainActionObjectSerializer;

    TrainScheduleObjectSerializer(TrainActionObjectSerializer trainActionObjectSerializer) => _trainActionObjectSerializer = trainActionObjectSerializer;
    
    public void Serialize(StationActions value, IObjectSaver objectSaver)
    {
      objectSaver.Set(StationKey, value.Station);
      objectSaver.Set(TrainActionsKey,value.Actions, _trainActionObjectSerializer);
    }

    public Obsoletable<StationActions> Deserialize(IObjectLoader objectLoader)
    {
      var stationActions = new StationActions(objectLoader.Get(StationKey), objectLoader.Get(TrainActionsKey, _trainActionObjectSerializer));

      foreach (var trainAction in stationActions.Actions)
        trainAction.SetTrainStation(stationActions.Station.gameObject);

      return stationActions;
    }
  }
}
