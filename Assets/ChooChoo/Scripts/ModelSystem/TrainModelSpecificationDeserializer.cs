using System;
using Timberborn.Persistence;

namespace ChooChoo
{
    public class TrainModelSpecificationDeserializer : IObjectSerializer<TrainModelSpecification>
    {
        public void Serialize(TrainModelSpecification value, IObjectSaver objectSaver) => throw new NotSupportedException();

        public Obsoletable<TrainModelSpecification> Deserialize(IObjectLoader objectLoader)
        {
            return (Obsoletable<TrainModelSpecification>) new TrainModelSpecification(
                objectLoader.Get(new PropertyKey<string>("Faction")),
                objectLoader.Get(new PropertyKey<string>("NameLocKey")),
                objectLoader.Get(new PropertyKey<string>("ModelLocation")),
                objectLoader.Get(new PropertyKey<float>("Length")), 
                objectLoader.Get(new PropertyKey<string>("MachinistSeatName")),
                objectLoader.Get(new PropertyKey<float>("MachinistScale")),
                objectLoader.Get(new PropertyKey<string>("MachinistAnimationName")));
        }
    }
}
