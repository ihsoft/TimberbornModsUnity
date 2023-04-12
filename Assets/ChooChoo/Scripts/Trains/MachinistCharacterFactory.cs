using System.Collections.Generic;
using System.Reflection;
using Timberborn.AssetSystem;
using Timberborn.Beavers;
using Timberborn.Common;
using Timberborn.FactionSystemGame;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace ChooChoo
{
    public class MachinistCharacterFactory : IPostLoadableSingleton
    {
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        private readonly IResourceAssetLoader _resourceAssetLoader;

        private readonly FactionService _factionService;
        
        private readonly BeaverFactory _beaverFactory;

        private GameObject _machinistPrefab;

        private readonly List<string> _bodyPartsToDisable = new()
        {
            "__Barrel",
            "__Box",
            "__Log",
            "__Bag",
            "__Scrap",
            "__TreatedPlank",
            "__Plank",
            "__MetalBlock",
            "__Backpack",
            "__None"
        };

        MachinistCharacterFactory(IRandomNumberGenerator randomNumberGenerator, IResourceAssetLoader resourceAssetLoader, FactionService factionService, BeaverFactory beaverFactory)
        {
            _randomNumberGenerator = randomNumberGenerator;
            _resourceAssetLoader = resourceAssetLoader;
            _factionService = factionService;
            _beaverFactory = beaverFactory;
        }

        public void PostLoad()
        {
            InitializeMachinistCharacter();
        }

        public GameObject CreateMachinist(Transform parent)
        {
            return Object.Instantiate(_machinistPrefab, parent).gameObject;
        }

        private void InitializeMachinistCharacter()
        {
            Beaver beaver = typeof(BeaverFactory).GetField("_adultPrefab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_beaverFactory) as Beaver;
            Transform beaverModel = beaver.transform.GetChild(0).GetChild(0);
            var skinnedMeshRenderer = beaver.GetComponentInChildren<SkinnedMeshRenderer>();
            var current = _factionService.Current;
            skinnedMeshRenderer.sharedMaterial = _resourceAssetLoader.Load<Material>(_randomNumberGenerator.GetEnumerableElement(current.Materials));
            DisableBodyParts(beaverModel);
            _machinistPrefab = beaverModel.gameObject;
        }

        private void DisableBodyParts(Transform beaver)
        {
            foreach (var bodyPartName in _bodyPartsToDisable)
            {
                DisableBodyPart(beaver, bodyPartName);
            }
        }

        private void DisableBodyPart(Transform parent, string bodyPartName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == bodyPartName)
                    child.gameObject.SetActive(false);

                DisableBodyPart(child, bodyPartName);
            }
        }
    }
}