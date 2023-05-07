using System.Collections.Generic;
using Timberborn.Persistence;

namespace TobbyTools.DynamicSpecificationSystem
{
    public class DynamicSpecificationDeserializer : IObjectSerializer<List<object>>
    {
        public void Serialize(List<object> value, IObjectSaver objectSaver)
        {
            
        }

        public Obsoletable<List<object>> Deserialize(IObjectLoader objectLoader)
        {
            return (Obsoletable<List<object>>)new object();
        }
    }
}
