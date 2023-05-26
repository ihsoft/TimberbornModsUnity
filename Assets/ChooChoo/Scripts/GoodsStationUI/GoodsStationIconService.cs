using TimberApi.AssetSystem;
using Timberborn.AssetSystem;
using UnityEngine;

namespace ChooChoo
{
    public class GoodsStationIconService
    {
        public Sprite EmptySprite;
        public Sprite ObtainSprite;
        
        GoodsStationIconService(IResourceAssetLoader assetLoader)
        {
            EmptySprite = assetLoader.Load<Sprite>("ui/images/game/empty-icon");
            ObtainSprite = assetLoader.Load<Sprite>("ui/images/game/obtain-icon");
        }
    }
}