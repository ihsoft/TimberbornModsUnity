using System;
using Timberborn.AssetSystem;
using Timberborn.Beavers;
using Timberborn.EntitySystem;
using Timberborn.PrefabOptimization;
using Timberborn.SingletonSystem;
using TimberbornAPI.AssetLoaderSystem.AssetSystem;
using UnityEngine;

namespace BeaverHats
{
    public class BeaverHatsService : ILoadableSingleton
    {
        private readonly IAssetLoader _iAssetLoader;
        private Shader _shader;

        private GameObject _adultHat;

        BeaverHatsService(IAssetLoader assetLoader)
        {
            _iAssetLoader = assetLoader;
        }

        Shader Shader
        {
            get
            {
                if (_shader != null)
                    return _shader;
                
                _shader = Resources.Load<GameObject>("Buildings/Paths/Platform/Platform.Full.Folktails").GetComponent<MeshRenderer>().materials[0].shader;
                return _shader;
            }
        }

        public void Load()
        {
            _adultHat = _iAssetLoader.Load<GameObject>("tobbert.beaverhats/tobbert_beaverhats/FrogHat");
            _adultHat.name = "__" + _adultHat.name;
            
            ShaderFix(_adultHat.transform);
        }

        public void AddHats(ref Beaver beaver)
        {
            var head = FindHead(beaver.transform);
            
            if (head == null)
                return;

            var hat = Prefab.Instantiate(_iAssetLoader.Load<GameObject>("tobbert.beaverhats/tobbert_beaverhats/FrogHat"));
            hat.name = ("__" + hat.name).Replace("(Clone)", "");
            ShaderFix(hat.transform);
            hat.transform.rotation *= head.rotation;
            hat.transform.position += head.position;
            if (beaver.transform.name.Contains("Child"))
            {
                hat.transform.localScale -= new Vector3(0.25f, 0.25f, 0.25f);
                hat.transform.Rotate(5, 0, 0);
            }
               
            
            if (head.Find("__FrogHat") != null)
                head.Find("__FrogHat").parent = null;

            hat.transform.SetParent(head.transform);
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

        private Transform FindHead(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.name == "DEF-head")
                    return child;
                
                var result = FindHead(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
