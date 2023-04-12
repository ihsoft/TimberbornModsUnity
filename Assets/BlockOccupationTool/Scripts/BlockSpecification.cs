using System;
using Timberborn.BlockSystem;

namespace BlockOccupationTool
{
    public class BlockSpecification
    {
        public BlockSpecification(string matterBelow, string occupation, string stackable, bool optionallyUnderground, bool occupyAllBelow)
        {
            MatterBelow = (MatterBelow)Enum.Parse(typeof(MatterBelow), matterBelow.Replace(" ", ""));
            Occupation = (Timberborn.BlockSystem.BlockOccupationSpecification)Enum.Parse(typeof(Timberborn.BlockSystem.BlockOccupationSpecification), occupation.Replace(" ", ""));
            Stackable = (BlockStackable)Enum.Parse(typeof(BlockStackable), stackable.Replace(" ", ""));
            OptionallyUnderground = optionallyUnderground;
            OccupyAllBelow = occupyAllBelow;
        }

        public MatterBelow MatterBelow { get; }
        public Timberborn.BlockSystem.BlockOccupationSpecification Occupation { get; }
        public BlockStackable Stackable { get; }
        public bool OptionallyUnderground { get; }
        public bool OccupyAllBelow { get; }
    }
}