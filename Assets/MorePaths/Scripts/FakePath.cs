using System.Collections.Generic;
using System.IO;
using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using Timberborn.PathSystem;
using UnityEngine;

namespace MorePaths
{
    public class FakePath
    {
        private readonly MorePathsService _morePathsService;
        public readonly GameObject PathGameObject;
        private readonly GameObject _pathCorner;
        private readonly PathSpecification _pathSpecification;

        private readonly Texture2D _groundTexture2D;
        private readonly Texture2D _railingTexture2D;
        private readonly Texture2D _spriteTexture2D;

        private Material _groundMaterial;

        public FakePath(MorePathsService morePathsService, GameObject originalFakePath, GameObject pathCorner, PathSpecification pathSpecification)
        {
            _morePathsService = morePathsService;
            PathGameObject = originalFakePath;
            _pathCorner = pathCorner;
            _pathSpecification = pathSpecification;

            _groundTexture2D = new Texture2D(1024, 1024);
            _railingTexture2D = new Texture2D(1024, 1024);
            _spriteTexture2D = new Texture2D(112, 112);

            _groundMaterial = null;
        }

        public void Create()
        {
            if (_pathSpecification.Name == "DefaultPath")
            {
                PathModelVariant pathModel = (PathModelVariant)_morePathsService.GetPrivateField(PathGameObject.GetComponent<DynamicPathModel>(), "_modelPrefab0000");
                _groundMaterial = new Material(pathModel.GroundVariant.GetComponentInChildren<MeshRenderer>().material);
                AddPathCorners();
                return;
            }

            SetGroundTexture();
            SetGroundMaterial();
            
            
            
            PathGameObject.name = _pathSpecification.Name;

            _morePathsService.ChangePrivateField(PathGameObject.GetComponent<Prefab>(), "_prefabName", _pathSpecification.Name);
            _morePathsService.ChangePrivateField(PathGameObject.GetComponent<Prefab>(), "_backwardCompatiblePrefabNames", new string[] { });

            
            if (_pathSpecification.RailingTexture != null)
            {
                var railingTextureBytes = File.ReadAllBytes(Plugin.myPath + "\\Paths\\" + _pathSpecification.Name + "\\" + _pathSpecification.RailingTexture);
                _railingTexture2D.LoadImage(railingTextureBytes);
            }

            foreach (var variantString in new List<string>
                     {
                         "_modelPrefab0000", "_modelPrefab0010", "_modelPrefab0011", "_modelPrefab0111",
                         "_modelPrefab1010", "_modelPrefab1111"
                     })
            {
                PathModelVariant pathModelVariant =
                    (PathModelVariant)_morePathsService.GetPrivateField(PathGameObject.GetComponent<DynamicPathModel>(),
                        variantString);

                var original = pathModelVariant.GroundVariant;
                original.SetActive(false);
                GameObject newGameObject = Object.Instantiate(original);
                _morePathsService.ChangePrivateField(pathModelVariant, "_groundVariant", newGameObject);

                var material = newGameObject.GetComponentInChildren<MeshRenderer>().material;
                material.mainTexture = _groundTexture2D;
                material.SetFloat("_MainTexScale", _pathSpecification.MainTextureScale);
                material.SetFloat("_NoiseTexScale", _pathSpecification.NoiseTexScale);
                material.SetVector("_MainColor", new Vector4(_pathSpecification.MainColorRed, _pathSpecification.MainColorGreen, _pathSpecification.MainColorBlue, 1f));
                original.SetActive(true);


                if (_pathSpecification.RailingTexture != null)
                {
                    var original1 = pathModelVariant.RoofVariant;
                    original1.SetActive(false);
                    GameObject newGameObject1 = Object.Instantiate(original1);
                    _morePathsService.ChangePrivateField(pathModelVariant, "_roofVariant", newGameObject1);

                    var material1 = newGameObject1.GetComponentInChildren<MeshRenderer>().material;
                    material1.mainTexture = _railingTexture2D != null ? _railingTexture2D : _groundTexture2D;
                    original1.SetActive(true);
                }
            }

            _morePathsService.ChangePrivateField(PathGameObject.GetComponent<PlaceableBlockObject>(), "_toolOrder",
                _pathSpecification.ToolOrder);

            var labeledPrefab = PathGameObject.GetComponent<LabeledPrefab>();

            var spriteBytes = File.ReadAllBytes(Plugin.myPath + "\\Paths\\" + _pathSpecification.Name + "\\" +
                                                _pathSpecification.PathIcon);
            _spriteTexture2D.LoadImage(spriteBytes);


            var sprite2D = Sprite.Create(_spriteTexture2D, labeledPrefab.Image.rect, labeledPrefab.Image.pivot,
                labeledPrefab.Image.pixelsPerUnit);

            _morePathsService.ChangePrivateField(labeledPrefab, "_displayNameLocKey",
                _pathSpecification.DisplayNameLocKey);
            _morePathsService.ChangePrivateField(labeledPrefab, "_descriptionLocKey",
                _pathSpecification.DescriptionLocKey);
            _morePathsService.ChangePrivateField(labeledPrefab, "_flavorDescriptionLocKey",
                _pathSpecification.FlavorDescriptionLocKey);
            _morePathsService.ChangePrivateField(labeledPrefab, "_image", sprite2D);

          
            AddPathCorners();
        }

        private void SetGroundTexture()
        {
            var groundTextureBytes = File.ReadAllBytes(Plugin.myPath + "\\Paths\\" + _pathSpecification.Name + "\\" + _pathSpecification.PathTexture);
            _groundTexture2D.LoadImage(groundTextureBytes);
        }

        private void SetGroundMaterial()
        {
            PathModelVariant pathModel = (PathModelVariant)_morePathsService.GetPrivateField(PathGameObject.GetComponent<DynamicPathModel>(), "_modelPrefab0000");
            
            var originalGroundMaterial = new Material(pathModel.GroundVariant.GetComponentInChildren<MeshRenderer>().material);
            
            originalGroundMaterial.SetFloat("_MainTexScale", _pathSpecification.MainTextureScale);
            originalGroundMaterial.SetFloat("_NoiseTexScale", _pathSpecification.NoiseTexScale);
            originalGroundMaterial.SetVector("_MainColor", new Vector4(_pathSpecification.MainColorRed, _pathSpecification.MainColorGreen, _pathSpecification.MainColorBlue, 1f));
            originalGroundMaterial.mainTexture = _groundTexture2D;
            originalGroundMaterial.mainTextureScale = new Vector2(0.5f, -0.5f);
            _groundMaterial =  originalGroundMaterial;
        }

        private void AddPathCorners()
        {
            _groundMaterial.SetTexture("_FadeTex", null);
            _groundMaterial.SetTexture("_NoiseTex", null);
            _groundMaterial.SetTexture("_DetailMask", null);
            _groundMaterial.renderQueue -= 1;

            _pathCorner.GetComponentInChildren<MeshRenderer>().material = _groundMaterial;

            var transform = PathGameObject.transform;

            if (PathGameObject.TryGetComponent(out DynamicPathCorner dynamicPathCorner))
            {
                var corner1 = Object.Instantiate(_pathCorner, transform);
                corner1.name = "corner1_Animated";
                dynamicPathCorner.cornerDownLeft = corner1;
                corner1.SetActive(false);
            
                var corner2 = Object.Instantiate(_pathCorner, transform, true);            
                corner2.transform.position += new Vector3(0, 0, 0.75f);
                corner2.name = "corner2_Animated";
                dynamicPathCorner.cornerUpLeft = corner2;
                corner2.SetActive(false);

                var corner3 = Object.Instantiate(_pathCorner, transform, true);
                corner3.transform.position += new Vector3(0.75f, 0, 0.75f);
                corner3.name = "corner3_Animated";
                dynamicPathCorner.cornerUpRight = corner3;
                corner3.SetActive(false);

                var corner4 = Object.Instantiate(_pathCorner, transform, true);
                corner4.transform.position += new Vector3(0.75f, 0, 0);
                corner4.name = "corner4_Animated";
                dynamicPathCorner.cornerDownRight = corner4;
                corner4.SetActive(false);
            }
        }
    }
}
