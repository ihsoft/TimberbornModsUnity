using System;
using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.Beavers;
using Timberborn.Bots;
using Timberborn.WorkSystem;
using UnityEngine;

namespace BeaverHats
{
    public class ClothingComponent : MonoBehaviour
    {
        private BeaverClothingService _beaverClothingService;

        private IEnumerable<ClothingSpecification> _clothingSpecifications;

        private readonly Dictionary<ClothingSpecification, GameObject> _clothingObjects = new();

        [Inject]
        public void InjectDependencies(BeaverClothingService beaverClothingService)
        {
            _beaverClothingService = beaverClothingService;
        }

        void Start()
        {
            if (TryGetComponent(out Child _))
            {
                _clothingSpecifications = _beaverClothingService.ClothingSpecifications.Where(specification => specification.CharacterType == "BeaverChild");
            }

            if (TryGetComponent(out Worker worker))
            {
                worker.GotUnemployed += OnGotUnemployed;
                worker.GotEmployed += OnGotEmployed;
                
                if (TryGetComponent(out Beaver _))
                {
                    _clothingSpecifications = _beaverClothingService.ClothingSpecifications.Where(specification => specification.CharacterType == "BeaverAdult");
                }
                if (TryGetComponent(out Bot _))
                {
                    _clothingSpecifications = _beaverClothingService.ClothingSpecifications.Where(specification => specification.CharacterType == "Bot");
                }
            }
            IndexClothing();
            UpdateClothing();
        }

        private void IndexClothing()
        {
            foreach (var clothingSpecification in _clothingSpecifications)
            {
                var bodyPart = BeaverClothingService.FindBodyPart(transform, "__" + clothingSpecification.PrefabPath.Split("/").Last());
                // Plugin.Log.LogError(bodyPart + "");
                if (bodyPart != null)
                {
                    _clothingObjects.Add(clothingSpecification, bodyPart.gameObject);
                }
            }
        }
        
        private void UpdateClothing()
        {
            foreach (var clothingSpecification in _clothingSpecifications)
            {
                var bodyPart = _clothingObjects[clothingSpecification];
                
                if (TryGetComponent(out Worker worker))
                {
                    if (clothingSpecification.WorkPlaces.Count == 0)
                    {
                        var flag = WearBasedOnWearChance(clothingSpecification);
                        bodyPart.gameObject.SetActive(flag);
                        if (flag)
                            return;
                    }
                    // Plugin.Log.LogError(worker.Workplace.name);
                    bodyPart.gameObject.SetActive(clothingSpecification.WorkPlaces.Contains(worker.Workplace.name.Split(".")[0]));
                }
                else
                {
                    var flag = WearBasedOnWearChance(clothingSpecification);
                    bodyPart.gameObject.SetActive(flag);
                    if (flag)
                        return;
                }
            }
        }

        private bool WearBasedOnWearChance(ClothingSpecification clothingSpecification) => BeaverClothingService.Random.Next(100) < clothingSpecification.WearChance;

        private void OnGotUnemployed(object sender, EventArgs e) => UpdateClothing();

        private void OnGotEmployed(object sender, EventArgs e) => UpdateClothing();
    }
}
