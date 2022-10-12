namespace MorePaths
{
    public class PathSpecification 
    {
        public PathSpecification(
            bool enabled,
            string name,
            string pathTexture,
            string railingTexture,
            int toolOrder,
            string pathIcon,
            string displayNameLocKey,
            string descriptionLocKey,
            string flavorDescriptionLocKey,
            
            float mainTextureScale,
            float noiseTexScale,
            float mainColorRed,
            float mainColorGreen,
            float mainColorBlue)
        {
            Enabled = enabled;
            Name = name;
            PathTexture = pathTexture;
            RailingTexture = railingTexture;
            ToolOrder = toolOrder;
            PathIcon = pathIcon;
            DisplayNameLocKey = displayNameLocKey;
            DescriptionLocKey = descriptionLocKey;
            FlavorDescriptionLocKey = flavorDescriptionLocKey;

            MainTextureScale = mainTextureScale;
            NoiseTexScale = noiseTexScale;
            MainColorRed = mainColorRed;
            MainColorGreen = mainColorGreen;
            MainColorBlue = mainColorBlue;
        }

        public bool Enabled { get;  }
        public string Name { get; }
        public string PathTexture { get; }
        public string RailingTexture { get; }
        public int ToolOrder { get; }
        public string PathIcon { get; }
        public string DisplayNameLocKey { get; }
        public string DescriptionLocKey { get;  }
        public string FlavorDescriptionLocKey { get; }
        
        public float MainTextureScale { get; }
        public float NoiseTexScale { get; }
        public float MainColorRed { get; }
        public float MainColorGreen { get; }
        public float MainColorBlue { get; }
    }
}
