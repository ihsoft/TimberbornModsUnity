using System;
using TimberApi.UiBuilderSystem;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class TrainActionObjectSerializer : IObjectSerializer<ITrainAction>
  {
    private static readonly PropertyKey<string> TrainActionIdKey = new("TrainActionName");
    private static readonly PropertyKey<GameObject> TrainKey = new("Train");

    private readonly UIBuilder _builder;

    TrainActionObjectSerializer(UIBuilder builder) => _builder = builder;
    
    public void Serialize(ITrainAction value, IObjectSaver objectSaver)
    {
      objectSaver.Set(TrainActionIdKey, SerializeTrainAction(value));
      objectSaver.Set(TrainKey, value.Train);
      value.Save(objectSaver);
    }

    public Obsoletable<ITrainAction> Deserialize(IObjectLoader objectLoader)
    {
      var trainAction = Activator.CreateInstance(Type.GetType(objectLoader.Get(TrainActionIdKey)), new object[]{_builder}) as ITrainAction;
      trainAction.SetTrain(objectLoader.Get(TrainKey));
      trainAction.Load(objectLoader);
      
      return new Obsoletable<ITrainAction>(trainAction);
    }
    
    private string SerializeTrainAction(ITrainAction trainAction) => trainAction.GetType().FullName;
  }
}
