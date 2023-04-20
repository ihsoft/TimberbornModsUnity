using System.Collections.Generic;
using Timberborn.BlockSystem;

namespace BlockOccupationTool
{
    public class BlockOccupationSpecification 
    {
        public BlockOccupationSpecification(
            string buildingName,
            IEnumerable<BlockSpecification> blockSpecifications)
        {
            BuildingName = buildingName;
            BlocksSpecifications = blockSpecifications;
        }

        public string BuildingName { get; }
        public IEnumerable<BlockSpecification> BlocksSpecifications { get; }
    }
}