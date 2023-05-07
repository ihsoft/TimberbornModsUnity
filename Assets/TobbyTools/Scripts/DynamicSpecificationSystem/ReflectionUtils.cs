using System.Linq;
using System.Reflection;

namespace TobbyTools.DynamicSpecificationSystem
{
    public static class ReflectionUtils
    {
        public static bool IsAllowedFieldType(FieldInfo fieldInfo)
        {
            return !UnallowedTypes.Types.Contains(fieldInfo.FieldType);
        }
    }
}