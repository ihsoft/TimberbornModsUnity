using System.Linq;
using Bindito.Unity;
using Timberborn.AssetSystem;
using UnityEngine;

namespace TobbyTools.DynamicSpecificationSystem
{
    public class ActiveComponentRetriever
    {
        private readonly IResourceAssetLoader _resourceAssetLoader;

        ActiveComponentRetriever(IResourceAssetLoader resourceAssetLoader)
        {
            _resourceAssetLoader = resourceAssetLoader;
        }
        
        public Component[] GetAllComponents()
        {
            var prefabConfigurators = Object.FindObjectsOfType<PrefabConfigurator>(true);
            var allPrefabConfiguratorComponents = prefabConfigurators.SelectMany(configurator => configurator.GetComponents<Component>().Concat(configurator.GetComponentsInChildren<Component>()));
            var prefabs = _resourceAssetLoader.LoadAll<GameObject>("");
            var allPrefabComponents = prefabs.SelectMany(prefab => prefab.GetComponents<Component>());
            var allComponents = allPrefabComponents.Concat(allPrefabConfiguratorComponents);
            return allComponents.ToArray();
        }
    }
}