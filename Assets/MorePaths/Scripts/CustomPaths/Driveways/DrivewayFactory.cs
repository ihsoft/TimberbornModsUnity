using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Timberborn.AssetSystem;
using Timberborn.PathSystem;
using Timberborn.SingletonSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MorePaths
{
    public class DrivewayFactory : ILoadableSingleton
    {
        private readonly DrivewayModelInstantiator _drivewayModelInstantiator;
        private readonly IResourceAssetLoader _resourceAssetLoader;
        private readonly MorePathsCore _morePathsCore;
        private readonly MethodInfo _methodInfo = typeof(DrivewayModelInstantiator).GetMethod("GetModelPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        private Material _pathMaterial;

        private DrivewayFactory(DrivewayModelInstantiator drivewayModelInstantiator, IResourceAssetLoader resourceAssetLoader, MorePathsCore morePathsCore)
        {
            _drivewayModelInstantiator = drivewayModelInstantiator;
            _resourceAssetLoader = resourceAssetLoader;
            _morePathsCore = morePathsCore;
        }
        
        public void Load()
        {
            _pathMaterial = _resourceAssetLoader.Load<Material>("Buildings/Paths/Path/Path.Folktails");
        }

        public Dictionary<Driveway, List<GameObject>> CreateDriveways(ImmutableArray<PathSpecification> pathSpecifications)
        {
            var drivewaysDictionary = new Dictionary<Driveway, List<GameObject>>();

            foreach (var driveway in Enum.GetValues(typeof(Driveway)).Cast<Driveway>())
            {
                if (driveway == Driveway.None) 
                    continue;

                var drivewayList = new List<GameObject>();
                var originalDrivewayModel = (GameObject)_methodInfo.Invoke(_drivewayModelInstantiator, new object[] { driveway });
                originalDrivewayModel.SetActive(false);

                foreach (var pathSpecification in pathSpecifications)
                {
                    if (pathSpecification.Name == "DefaultPath") continue;
                    var newDriveway = CreateDriveway(originalDrivewayModel, pathSpecification);

                    drivewayList.Add(newDriveway);
                }

                drivewaysDictionary.Add(driveway, drivewayList);
            }

            return drivewaysDictionary;
        }

        private GameObject CreateDriveway(GameObject originalDrivewayModel, PathSpecification pathSpecification)
        {
            GameObject driveway = Object.Instantiate(originalDrivewayModel);

            driveway.name = pathSpecification.Name;

            var material = new Material(_pathMaterial);

            material.mainTexture = _morePathsCore.TryLoadTexture(pathSpecification.Name, pathSpecification.PathTexture);
            
            material.SetFloat("_MainTexScale", pathSpecification.MainTextureScale);
            material.SetFloat("_NoiseTexScale", pathSpecification.NoiseTexScale);
            material.SetVector("_MainColor", new Vector4(pathSpecification.MainColorRed, pathSpecification.MainColorGreen, pathSpecification.MainColorBlue, 1f));

            driveway.GetComponentInChildren<MeshRenderer>().material = material;
            
            return driveway;
        }
    }
}
