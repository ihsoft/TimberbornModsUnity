using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TimberApi.Common.SingletonSystem;
using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.Persistence;

namespace BlockOccupationTool
{
    public class BlockOccupationService : ITimberApiPreLoadableSingleton
    {
        private readonly BlockOccupationSpecificationDeserializer _blockOccupationSpecificationDeserializer;
        private readonly ISpecificationService _specificationService;
        private readonly IResourceAssetLoader _resourceAssetLoader;

        private ImmutableArray<BlockOccupationSpecification> _blockOccupationSpecifications;

        BlockOccupationService(
            BlockOccupationSpecificationDeserializer blockOccupationSpecificationDeserializer,
            ISpecificationService specificationService,
            IResourceAssetLoader resourceAssetLoader)
        {
            _blockOccupationSpecificationDeserializer = blockOccupationSpecificationDeserializer;
            _specificationService = specificationService;
            _resourceAssetLoader = resourceAssetLoader;
        }

        void ITimberApiPreLoadableSingleton.PreLoad()
        {
            _blockOccupationSpecifications = _specificationService
                .GetSpecifications(_blockOccupationSpecificationDeserializer)
                .OrderBy(specification => specification.BuildingName == null)
                .ThenBy(specification => specification.BuildingName).ToImmutableArray();
        
            // foreach (var specification in _blockOccupationSpecifications)
            // {
            //     Plugin.Log.LogError(specification.BuildingName);
            // }
            
            foreach (var specification in _blockOccupationSpecifications)
            {
                OverwriteBlockSpecifications(specification);
            }
        }

        private void OverwriteBlockSpecifications(BlockOccupationSpecification specification)
        {
            var blockObject = _resourceAssetLoader.Load<BlockObject>(specification.BuildingName);
            
            OverwriteBlockSpecification(blockObject, specification);
        }

        private void OverwriteBlockSpecification(BlockObject blockObject, BlockOccupationSpecification specification)
        {
            var list = new List<Timberborn.BlockSystem.BlockSpecification>();
            
            foreach (var blockSpecification in specification.BlocksSpecifications)
            {
                Plugin.Log.LogWarning(blockSpecification.Occupation + "");
                var officialBlockSpecification = new Timberborn.BlockSystem.BlockSpecification();
                BlockOccupationCore.SetInaccessibleField(officialBlockSpecification, "_matterBelow", blockSpecification.MatterBelow);
                BlockOccupationCore.SetInaccessibleField(officialBlockSpecification, "_occupation", blockSpecification.Occupation);
                BlockOccupationCore.SetInaccessibleField(officialBlockSpecification, "_stackable", blockSpecification.Stackable);
                BlockOccupationCore.SetInaccessibleField(officialBlockSpecification, "_optionallyUnderground", blockSpecification.OptionallyUnderground);
                BlockOccupationCore.SetInaccessibleField(officialBlockSpecification, "_occupyAllBelow", blockSpecification.OccupyAllBelow);
                list.Add(officialBlockSpecification);
            }

            var blocksSpecification = BlockOccupationCore.GetInaccessibleField(blockObject, "_blocksSpecification");
            BlockOccupationCore.SetInaccessibleField(blocksSpecification,"_blockSpecifications", list.ToArray());
            BlockOccupationCore.SetInaccessibleField(blockObject, "_blocksSpecification", blocksSpecification);
            BlockOccupationCore.SetInaccessibleField(blockObject, "_blocks", null);
            blockObject.Awake();
        }
    }
}
