using System;
using System.Collections.Generic;
using Timberborn.BlockSystem;
using Timberborn.Persistence;
using UnityEngine;

namespace TobbyTools.BlockOccupationTool
{
    public class BlockSpecificationDeserializer : IObjectSerializer<BlockSpecification>
    {
        public void Serialize(BlockSpecification value, IObjectSaver objectSaver) => throw new NotSupportedException();

        public Obsoletable<BlockSpecification> Deserialize(IObjectLoader objectLoader)
        {
            return (Obsoletable<BlockSpecification>) new BlockSpecification(
                objectLoader.Get(new PropertyKey<string>("MatterBelow")),
                objectLoader.Get(new PropertyKey<string>("Occupation")),
                objectLoader.Get(new PropertyKey<string>("Stackable")),
                objectLoader.Get(new PropertyKey<bool>("OptionallyUnderground")),
                objectLoader.Get(new PropertyKey<bool>("OccupyAllBelow")));
        }
    }
}
