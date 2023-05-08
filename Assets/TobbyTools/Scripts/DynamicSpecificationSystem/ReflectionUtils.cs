using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace TobbyTools.DynamicSpecificationSystem
{
    public static class ReflectionUtils
    {
        public static IEnumerable<FieldInfo> GetSerializeableFieldInfos(Type type)
        {
            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.IsDefined(typeof(SerializeField), true))
                .Where(IsAllowedFieldType);
        }

        public static IEnumerable<FieldInfo> GetFieldInfos(Type type)
        {
            return type?
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(IsAllowedFieldType);
        }
        
        public static IEnumerable<PropertyInfo> GetPropertyInfos([CanBeNull] Type type)
        {
            return type?
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
        
        public static bool IsAllowedFieldType(FieldInfo fieldInfo)
        {
            return !UnallowedTypes.Types.Contains(fieldInfo.FieldType);
        }
    }
}