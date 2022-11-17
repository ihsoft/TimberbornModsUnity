using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using UnityEngine;

namespace CategoryButton
{
    public class CategoryButtonFactory
    {
        public List<GameObject> CreateFromSpecifications(CategoryButtonService categoryButtonService, ImageRepository imageRepository, ImmutableArray<CategoryButtonSpecification> categoryButtonsSpecifications, GameObject originalCategoryButtonPrefab)
        {
            var categoryButtons = new List<GameObject>();
            foreach (var categoryButtonsSpecification in categoryButtonsSpecifications)
                categoryButtons.Add(CreateCategoryButton(categoryButtonService, imageRepository, categoryButtonsSpecification, originalCategoryButtonPrefab));
            return categoryButtons;
        }

        private GameObject CreateCategoryButton(CategoryButtonService categoryButtonService, ImageRepository imageRepository, CategoryButtonSpecification categoryButtonSpecification, GameObject prefab)
        {
            var gameObject = Object.Instantiate(prefab);
            
            categoryButtonService.ChangePrivateField(gameObject.GetComponent<Prefab>(), "_prefabName", categoryButtonSpecification.Name);
                
            gameObject.GetComponent<CategoryButtonComponent>().ToolBarButtonNames = categoryButtonSpecification.Buildings.ToList();

            var placeableBlockObject = gameObject.GetComponent<PlaceableBlockObject>();
            categoryButtonService.ChangePrivateField(placeableBlockObject, "_toolGroupId", categoryButtonSpecification.ToolGroup);
            categoryButtonService.ChangePrivateField(placeableBlockObject, "_toolOrder", categoryButtonSpecification.ToolOrder);
            categoryButtonService.ChangePrivateField(placeableBlockObject, "_devModeTool", false);

            var labeledPrefab = gameObject.GetComponent<LabeledPrefab>();
            Texture2D spriteTexture2D = new Texture2D(1, 1);
            var spriteBytes = File.ReadAllBytes(imageRepository.Images[categoryButtonSpecification.ButtonIcon]);
            spriteTexture2D.LoadImage(spriteBytes);
            var sprite2D = Sprite.Create(spriteTexture2D, new Rect(0, 0, 112f, 112f), new Vector2(0.5f, 0.5f), 100);
                
            categoryButtonService.ChangePrivateField(labeledPrefab, "_displayNameLocKey", categoryButtonSpecification.DisplayNameLocKey);
            categoryButtonService.ChangePrivateField(labeledPrefab, "_descriptionLocKey", "");
            categoryButtonService.ChangePrivateField(labeledPrefab, "_flavorDescriptionLocKey", "");
            categoryButtonService.ChangePrivateField(labeledPrefab, "_image", sprite2D);

            return gameObject;
        }
    }
}
