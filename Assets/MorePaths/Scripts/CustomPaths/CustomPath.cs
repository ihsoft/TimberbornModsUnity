using System.Collections.Generic;
using System.IO;
using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using Timberborn.PathSystem;
using UnityEngine;

namespace MorePaths
{
    public class CustomPath
    {
        private readonly MorePathsCore _morePathsCore;
        public readonly GameObject PathGameObject;
        private readonly GameObject _pathCorner;
        private readonly PathSpecification _pathSpecification;

        private readonly Texture2D _groundTexture2D;
        private readonly Texture2D _railingTexture2D;
        private readonly Texture2D _spriteTexture2D;

        private Material _groundMaterial;

        public CustomPath(MorePathsCore morePathsCore, GameObject originalFakePath, GameObject pathCorner, PathSpecification pathSpecification)
        {
            _morePathsCore = morePathsCore;
            PathGameObject = originalFakePath;
            _pathCorner = pathCorner;
            _pathSpecification = pathSpecification;

            _groundTexture2D = _morePathsCore.TryLoadTexture(_pathSpecification.Name, _pathSpecification.PathTexture);
            _railingTexture2D = _morePathsCore.TryLoadTexture(_pathSpecification.Name, _pathSpecification.RailingTexture);
            _spriteTexture2D = _morePathsCore.TryLoadTexture(_pathSpecification.Name, _pathSpecification.PathIcon, 112, 112);
        }

        public void Create()
        {
            SetGroundMaterial();
            
            if (_pathSpecification.Name == "DefaultPath")
            {
                AddPathCorners();
                return;
            }

            SetObjectName(_pathSpecification.Name);
            SetPrefabName(_pathSpecification.Name);
            RemoveBackwardCompatiblePrefabNames();
            SetToolOrder(_pathSpecification.ToolOrder);
            SetLocalisationAndSprite();
            
            AddPathCorners();

            foreach (var variantString in new List<string> { "_modelPrefab0000", "_modelPrefab0010", "_modelPrefab0011", "_modelPrefab0111", "_modelPrefab1010", "_modelPrefab1111" })
            {
                PathModelVariant pathModelVariant = (PathModelVariant)_morePathsCore.GetPrivateField(PathGameObject.GetComponent<DynamicPathModel>(), variantString);
                var originalGroundVariant = pathModelVariant.GroundVariant;
                originalGroundVariant.SetActive(false);
                GameObject newGameObject = Object.Instantiate(originalGroundVariant);
                _morePathsCore.ChangePrivateField(pathModelVariant, "_groundVariant", newGameObject);
                
                newGameObject.GetComponentInChildren<MeshRenderer>().material = _groundMaterial;
                originalGroundVariant.SetActive(true);

                if (_pathSpecification.RailingTexture != null)
                {
                    var originalRoofVariant = pathModelVariant.RoofVariant;
                    originalRoofVariant.SetActive(false);
                    GameObject roofVariant = Object.Instantiate(originalRoofVariant);
                    _morePathsCore.ChangePrivateField(pathModelVariant, "_roofVariant", roofVariant);
                    roofVariant.GetComponentInChildren<MeshRenderer>().material.mainTexture = _railingTexture2D;
                }

                if (!_pathSpecification.RailingEnabled)
                {
                    _morePathsCore.ChangePrivateField(pathModelVariant, "_roofVariant", new GameObject());
                }
            }
        }

        private void SetGroundMaterial()
        {
            PathModelVariant pathModel = (PathModelVariant)_morePathsCore.GetPrivateField(PathGameObject.GetComponent<DynamicPathModel>(), "_modelPrefab0000");
            var originalMaterial = pathModel.GroundVariant.GetComponentInChildren<MeshRenderer>().material;
            var newGroundMaterial = new Material(originalMaterial);
            newGroundMaterial.SetFloat("_MainTexScale", _pathSpecification.MainTextureScale);
            newGroundMaterial.SetFloat("_NoiseTexScale", _pathSpecification.NoiseTexScale);
            newGroundMaterial.SetVector("_MainColor", new Vector4(_pathSpecification.MainColorRed, _pathSpecification.MainColorGreen, _pathSpecification.MainColorBlue, 1f));
            if (_pathSpecification.PathTexture != null)
                newGroundMaterial.mainTexture = _groundTexture2D;
            _groundMaterial =  newGroundMaterial;
        }
        
        private void SetObjectName(string name) => PathGameObject.name = name;
        
        private void SetPrefabName(string name) => _morePathsCore.ChangePrivateField(PathGameObject.GetComponent<Prefab>(), "_prefabName", name);
        
        private void RemoveBackwardCompatiblePrefabNames() => _morePathsCore.ChangePrivateField(PathGameObject.GetComponent<Prefab>(), "_backwardCompatiblePrefabNames", new string[] { });
        
        private void SetToolOrder(int toolOrder) => _morePathsCore.ChangePrivateField(PathGameObject.GetComponent<PlaceableBlockObject>(), "_toolOrder", toolOrder);

        private void SetLocalisationAndSprite()
        {
            var labeledPrefab = PathGameObject.GetComponent<LabeledPrefab>();
            _morePathsCore.ChangePrivateField(labeledPrefab, "_displayNameLocKey", _pathSpecification.DisplayNameLocKey);
            _morePathsCore.ChangePrivateField(labeledPrefab, "_descriptionLocKey", _pathSpecification.DescriptionLocKey);
            _morePathsCore.ChangePrivateField(labeledPrefab, "_flavorDescriptionLocKey", _pathSpecification.FlavorDescriptionLocKey);
            
            if (_pathSpecification.PathIcon == null) return;

            var sprite2D = Sprite.Create(_spriteTexture2D, labeledPrefab.Image.rect, labeledPrefab.Image.pivot, labeledPrefab.Image.pixelsPerUnit);
            _morePathsCore.ChangePrivateField(labeledPrefab, "_image", sprite2D);
        }

        private void AddPathCorners()
        {
            var material = new Material(_groundMaterial);
            material.SetTexture("_FadeTex", null);
            material.SetTexture("_NoiseTex", null);
            material.SetTexture("_DetailMask", null);
            material.renderQueue -= 1;

            _pathCorner.GetComponentInChildren<MeshRenderer>().material = material;

            var transform = PathGameObject.transform;

            if (!PathGameObject.TryGetComponent(out DynamicPathCorner dynamicPathCorner)) return;
            
            var corner1 = Object.Instantiate(_pathCorner, transform);
            corner1.name = "corner1_Animated";
            dynamicPathCorner.CornerDownLeft = corner1;
            corner1.SetActive(false);
            
            var corner2 = Object.Instantiate(_pathCorner, transform, true);            
            corner2.transform.position += new Vector3(0, 0, 0.75f);
            corner2.name = "corner2_Animated";
            dynamicPathCorner.CornerUpLeft = corner2;
            corner2.SetActive(false);

            var corner3 = Object.Instantiate(_pathCorner, transform, true);
            corner3.transform.position += new Vector3(0.75f, 0, 0.75f);
            corner3.name = "corner3_Animated";
            dynamicPathCorner.CornerUpRight = corner3;
            corner3.SetActive(false);

            var corner4 = Object.Instantiate(_pathCorner, transform, true);
            corner4.transform.position += new Vector3(0.75f, 0, 0);
            corner4.name = "corner4_Animated";
            dynamicPathCorner.CornerDownRight = corner4;
            corner4.SetActive(false);
        }
    }
}
