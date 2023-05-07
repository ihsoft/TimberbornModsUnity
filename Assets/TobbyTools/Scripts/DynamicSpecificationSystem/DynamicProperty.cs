namespace TobbyTools.DynamicSpecificationSystem
{
    public class DynamicProperty
    {
        public DynamicProperty(string name, object value = null)
        {
            OriginalName = name;
            if (name.Length == 1)
                StyledName = char.ToUpper(name[0]).ToString();
            else
                StyledName = char.ToUpper(name.Replace("_", "")[0] ) + name.Substring(2);
            Value = value;
        }
        
        public string OriginalName;
        
        public string StyledName;
        
        public object Value;
        
        public string GetName(bool styledName)
        {
            return styledName ? StyledName : OriginalName;
        }
    }
}