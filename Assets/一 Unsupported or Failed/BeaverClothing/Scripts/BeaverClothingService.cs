using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Timberborn.AssetSystem;
using Timberborn.Beavers;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace BeaverHats
{
    public class BeaverClothingService : ILoadableSingleton 
    {
        private readonly ClothingSpecificationDeserializer _clothingSpecificationDeserializer;
        private readonly ISpecificationService _specificationService;
        private readonly IResourceAssetLoader _resourceAssetLoader;

        public static Random Random = new();
        
        private Shader _shader;
        private ImmutableArray<ClothingSpecification> _clothingSpecifications;

        public ImmutableArray<ClothingSpecification> ClothingSpecifications => _clothingSpecifications;
        
        public readonly List<Clothing> Clothings = new()
        {
            // new Clothing
            // { 
            //     Name="FrogHat", 
            //     BodyPartName = "DEF-head",
            //     WorkPlaces = new List<string>() { "" }
            // },
            new Clothing
            { 
                Name="ConstructionHat", 
                BodyPartName = "DEF-head",
                WorkPlaces = new List<string> { "" }
            },
            // new Clothing() { 
            //     Name="ConstructionHat", 
            //     BodyPartName = "DEF-head",
            //     WorkPlaces = new List<string>() { "DistrictCenter" }
            // },
            // new Clothing() { 
            //     Name="StrawHat", 
            //     BodyPartName = "DEF-head",
            //     WorkPlaces = new List<string>() { "FarmHouse" }
            // },
            // new Clothing() { 
            //      Name="FlowerCrown", 
            //      BodyPartName = "DEF-head",
            //      WorkPlaces = new List<string>() { "GathererFlag" }
            // },
            // new Clothing() { 
            //     Name="PithHelmet", 
            //     BodyPartName = "DEF-head",
            //     WorkPlaces = new List<string>() { "ScavengerFlag" }
            // },
            // new Clothing() { 
            //     Name="ChefsHat", 
            //     BodyPartName = "DEF-head",
            //     WorkPlaces = new List<string>() { "Grill", "Bakery", "Gristmill" }
            // }
        };

        public BeaverClothingService(ClothingSpecificationDeserializer clothingSpecificationDeserializer, ISpecificationService specificationService, IResourceAssetLoader resourceAssetLoader)
        {
            _clothingSpecificationDeserializer = clothingSpecificationDeserializer;
            _specificationService = specificationService;
            _resourceAssetLoader = resourceAssetLoader;
        }

        Shader Shader
        {
            get
            {
                if (_shader != null)
                    return _shader;

                _shader = Resources.Load<GameObject>("Buildings/Paths/Platform/Platform.Full.Folktails")
                    .GetComponent<MeshRenderer>().materials[0].shader;
                return _shader;
            }
        }
        
        public void Load()
        {
            _clothingSpecifications = _specificationService.GetSpecifications(_clothingSpecificationDeserializer).Where(specification => specification.Enabled).ToImmutableArray();

            foreach (var specification in _clothingSpecifications)
            {
                switch (specification.CharacterType)
                {
                    case "BeaverAdult":
                        InitiateClothing(specification, ref BeaverFactoryPatch.AdultBeaver);
                        continue;
                    case "BeaverChild":
                        InitiateClothing(specification, ref BeaverFactoryPatch.ChildBeaver);
                        continue;
                    case "Bot":
                        InitiateClothing(specification, ref BotFactoryPatch.Bot);
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException($"Character Type: {specification.CharacterType} is not a valid character type.");
                }
            }
        }

        private void InitiateClothing(ClothingSpecification specification, ref Transform character)
        {
            var bodyPart = FindBodyPart(character, specification.BodyPartName);
            
            if (bodyPart == null)
                return;

            var clothingObject = Object.Instantiate(_resourceAssetLoader.Load<GameObject>(specification.PrefabPath));
            clothingObject.transform.position = new Vector3(specification.PositionX, specification.PositionY, specification.PositionZ);
            clothingObject.transform.rotation = Quaternion.Euler(specification.RotationX, specification.RotationY, specification.RotationZ);
            clothingObject.name = ("__" + clothingObject.name).Replace("(Clone)", "");
            ShaderFix(clothingObject.transform);
            clothingObject.transform.rotation *= bodyPart.rotation;
            clothingObject.transform.position += bodyPart.position;
            clothingObject.transform.localScale = new Vector3(specification.ScaleX, specification.ScaleY, specification.ScaleZ);
            clothingObject.transform.Rotate(5, 0, 0);
            
            
            if (bodyPart.Find(clothingObject.name) != null)
                bodyPart.Find(clothingObject.name).parent = null;
            
            clothingObject.transform.SetParent(bodyPart.transform);
            clothingObject.SetActive(false);
        }
        
        private void ShaderFix(Transform child)
        {
            foreach (var child1 in child)
            {
                ShaderFix(child1 as Transform);
            }
            
            if (!child.TryGetComponent(out MeshRenderer meshRenderer)) 
                return;
            foreach (var material in meshRenderer.materials)
            {
                material.shader = Shader;
            }
        }

        public static Transform FindBodyPart(Transform parent, string bodyPartName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == bodyPartName)
                    return child;
                
                var result = FindBodyPart(child, bodyPartName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
