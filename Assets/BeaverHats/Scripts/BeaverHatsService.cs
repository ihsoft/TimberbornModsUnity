using System.Collections.Generic;
using System.Reflection;
using TimberApi.AssetSystem;
using TimberApi.Common.SingletonSystem.Singletons;
using TimberApi.DependencyContainerSystem;
using Timberborn.Beavers;
using Timberborn.EntitySystem;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace BeaverHats
{
    public class BeaverHatsService : ILoadableSingleton 
    {
        private readonly IAssetLoader _assetLoader;
        private readonly BeaverFactory _beaverFactory;
        private Shader _shader;

        public Beaver AdultBeaver;
        public Beaver ChildBeaver;
        
        public readonly List<Clothing> Clothings = new()
        {
            new Clothing
            { 
                Name="FrogHat", 
                BodyPartName = "DEF-head",
                WorkPlaces = new List<string>() { "" }
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

        public BeaverHatsService(IAssetLoader assetLoader, BeaverFactory beaverFactory)
        {
            _assetLoader = assetLoader;
            _beaverFactory = beaverFactory;
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
            InitiateClothings(ref Patch.AdultBeaver);
            InitiateClothings(ref Patch.ChildBeaver);
        }
        
        public void InitiateClothings(ref Beaver beaver)
        {
            foreach (var clothing in Clothings)
            {
                InitiateClothing(clothing, beaver);
            }
        }

        private void InitiateClothing(Clothing clothing, Beaver beaver)
        {
            var bodyPart = FindBodyPart(beaver.transform, clothing.BodyPartName);
            
            if (bodyPart == null)
                return;

            var clothingObject = Prefab.Instantiate(_assetLoader.Load<GameObject>("tobbert.beaverhats", "tobbert_beaverhats", clothing.Name));
            clothingObject.name = ("__" + clothingObject.name).Replace("(Clone)", "");
            ShaderFix(clothingObject.transform);
            clothingObject.transform.rotation *= bodyPart.rotation;
            clothingObject.transform.position += bodyPart.position;
            if (beaver.transform.name.Contains("Child"))
            {
                clothingObject.transform.localScale -= new Vector3(0.25f, 0.25f, 0.25f);
                clothingObject.transform.Rotate(5, 0, 0);
            }

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
            
            if (!child.TryGetComponent(out MeshRenderer meshRenderer)) return;
            foreach (var material in meshRenderer.materials)
            {
                material.shader = Shader;
            }
        }

        public Transform FindBodyPart(Transform parent, string bodyPartName)
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
