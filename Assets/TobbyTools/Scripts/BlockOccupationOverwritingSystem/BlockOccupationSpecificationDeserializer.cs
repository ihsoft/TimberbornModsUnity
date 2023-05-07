using System;
using Timberborn.BlockSystem;
using Timberborn.Persistence;

namespace TobbyTools.BlockOccupationTool
{
    public class BlockOccupationSpecificationDeserializer : IObjectSerializer<BlockOccupationSpecification>
    {
        private readonly BlockSpecificationDeserializer _blockSpecificationDeserializer;
        
        BlockOccupationSpecificationDeserializer(BlockSpecificationDeserializer blockSpecificationDeserializer)
        {
            _blockSpecificationDeserializer = blockSpecificationDeserializer;
        }
        
        public void Serialize(BlockOccupationSpecification value, IObjectSaver objectSaver) => throw new NotSupportedException();

        public Obsoletable<BlockOccupationSpecification> Deserialize(IObjectLoader objectLoader)
        {
            return (Obsoletable<BlockOccupationSpecification>) new BlockOccupationSpecification(
                objectLoader.GetValueOrNull(new PropertyKey<string>("BuildingName")),
                objectLoader.Get(new ListKey<BlockSpecification>("BlocksSpecifications"), _blockSpecificationDeserializer));
        }
    }
}
