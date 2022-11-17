using System.Linq;
using Timberborn.AssetSystem;
using Timberborn.FactionSystemGame;
using UnityEngine;

namespace MorePaths
{
    public class CustomPathFactory
    {
        private readonly IResourceAssetLoader _resourceAssetLoader;

        private readonly FactionService _factionService;

        private readonly MorePathsCore _morePathsCore;

        CustomPathFactory(IResourceAssetLoader resourceAssetLoader, FactionService factionService, MorePathsCore morePathsCore)
        {
            _resourceAssetLoader = resourceAssetLoader;
            _factionService = factionService;
            _morePathsCore = morePathsCore;
        }
        
        private GameObject PathCorner => _resourceAssetLoader.Load<GameObject>("tobbert.morepaths/tobbert_morepaths/PathCorner");

        public void CreatePathsFromSpecification()
        {
            // Stopwatch stopwatch = Stopwatch.StartNew();

            PreventInstantiatePatch.RunInstantiate = false;

            var originalPathGameObject = Resources.Load<GameObject>("Buildings/Paths/Path/Path." + _factionService.Current.Id);
            originalPathGameObject.AddComponent<DynamicPathCorner>();
            _morePathsCore.CustomPaths = _morePathsCore.PathsSpecifications.Select(specification => new CustomPath(_morePathsCore,
                Object.Instantiate(originalPathGameObject), Object.Instantiate(PathCorner), specification)).ToList();
            _morePathsCore.CustomPaths.ForEach(path => path.Create());
            PreventInstantiatePatch.RunInstantiate = true;

            // stopwatch.Stop();
            // Plugin.Log.LogInfo("Total: " + stopwatch.ElapsedMilliseconds);
        }
    }
}
