using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Timberborn.AssetSystem;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using UnityEngine;
using ExpandoObject = System.Dynamic.ExpandoObject;

namespace TobbyTools.DynamicSpecificationSystem
{
    public class DynamicSpecificationApplier : ILoadableSingleton
    {
        private readonly ActiveComponentRetriever _activeComponentRetriever;
        private readonly ISpecificationService _specificationService;
        private readonly IResourceAssetLoader _resourceAssetLoader;

        public DynamicSpecificationApplier(ActiveComponentRetriever activeComponentRetriever, ISpecificationService specificationService, IResourceAssetLoader resourceAssetLoader)
        {
            _activeComponentRetriever = activeComponentRetriever;
            _specificationService = specificationService;
            _resourceAssetLoader = resourceAssetLoader;
        }

        public void Load()
        {
            // return;
            
            foreach (var monoBehaviour in _activeComponentRetriever.GetAllComponents())
            {
                var type = monoBehaviour.GetType();
                
                if (SkippableTypes.Types.Contains(type))
                    continue;
                
                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning("Name Monobehavior: " + type);

                var fieldInfos = GetFilteredFieldInfos(type).ToArray();
                
                var values = GetDynamicProperties(fieldInfos, monoBehaviour).ToArray();

                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning("Number of fields: " + values.Length);
                if (!values.Any())
                    continue;
                
                var fileName = "DynamicSpecification." + monoBehaviour.gameObject.name + "." + monoBehaviour.GetType().Name + ".original.json";
                
                if (Plugin.LoggingEnabled) Plugin.Log.LogError(fileName);

                var fileLocation = "C:\\Users\\jordy\\SynologyDrive\\Unity Projecten\\TimberbornModsUnity Update 4\\ThunderKit\\Staging\\TobbyTools\\GeneratedSpecs\\" + fileName;

                if (!File.Exists(fileLocation))
                    continue;
                
            
                var jsonString = File.ReadAllText(fileLocation);

                ExpandoObject objectContaingValues = JObject.Parse(jsonString).ToExpandoObject();

                foreach (var property in values)
                {
                    UpdateValues(property, objectContaingValues, monoBehaviour, objectContaingValues.GetPropertyValue(property.StyledName));
                }
                
                File.Delete(fileLocation);
            }
        }
        
        private IEnumerable<DynamicProperty> GetDynamicProperties(IEnumerable<FieldInfo> fieldInfos, object instance)
        {
            return fieldInfos.Select(field => new DynamicProperty(field.Name, field.GetValue(instance)));
        }
        
        private IEnumerable<DynamicProperty> GetDynamicPropertiesWithoutValues(IEnumerable<FieldInfo> fieldInfos)
        {
            return fieldInfos.Select(field => new DynamicProperty(field.Name));
        }
        
        private IEnumerable<DynamicProperty> GetDynamicPropertiesWithoutValues(IEnumerable<PropertyInfo> fieldInfos)
        {
            return fieldInfos.Select(field => new DynamicProperty(field.Name));
        }

        private IEnumerable<FieldInfo> GetFilteredFieldInfos(Type type)
        {
            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.IsDefined(typeof(SerializeField), true))
                .Where(ReflectionUtils.IsAllowedFieldType);
        }

        private IEnumerable<PropertyInfo> GetPropertyInfos(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private void UpdateValues(DynamicProperty property, ExpandoObject objectContaingValues, object objectToBeUpdated, object newValue)
        {
            var toBeUpdatedType = objectToBeUpdated.GetType();
            var toBeUpdateFieldInfo = toBeUpdatedType.GetField(property.OriginalName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var toBeUpdatePropertyInfo = toBeUpdatedType.GetProperty(property.OriginalName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (Plugin.LoggingEnabled) Plugin.Log.LogError(newValue + "");
            if (Plugin.LoggingEnabled) Plugin.Log.LogError(newValue?.GetType() + "");
            // var test = GetDynamicProperties(GetFilteredFieldInfos(value.GetType()), value);
            // foreach (var dynamicProperty in test)
            // {
            //     UpdateValues(dynamicProperty, objectContaingValues, value);
            //     if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(dynamicProperty.OriginalName);
            // }
            
            if ((toBeUpdateFieldInfo != null && toBeUpdateFieldInfo.FieldType.IsArray) || (toBeUpdatePropertyInfo != null && toBeUpdatePropertyInfo.PropertyType.IsArray))
            {
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("ARRAY");
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(property.StyledName);
                // if (Plugin.LoggingEnabled) Plugin.Log.LogError(toBeUpdateFieldInfo.FieldType + "");
                var elementType = toBeUpdateFieldInfo != null ? 
                    toBeUpdateFieldInfo.FieldType.GetElementType() :
                    toBeUpdatePropertyInfo.PropertyType.GetElementType();
                var dynamicProperties = GetDynamicPropertiesWithoutValues(GetFilteredFieldInfos(elementType));

                var list = new List<object>();

                IEnumerable<IDictionary<string, object>> enumerable = objectContaingValues.GetList(property.StyledName);
                foreach (IDictionary<string, object> item in enumerable)
                {
                    object newObject;
                    
                    try
                    {
                        newObject = Activator.CreateInstance(elementType);
                    }
                    catch (Exception e)
                    {
                        var constructor = elementType.GetConstructors().First();   
                    
                        var parameters = constructor.GetParameters();
                        var args = new object[parameters.Length];
                    
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var parameterType = parameters[i].ParameterType;
                            args[i] = parameterType.IsValueType ? Activator.CreateInstance(parameterType) : null;
                        }
                    
                        newObject = constructor.Invoke(args);
                    }


                    if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(newObject.GetType() + "");
                    foreach (var dynamicProperty in dynamicProperties)
                    {
                        if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(dynamicProperty.StyledName);
                        if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(item.GetPropertyValue(dynamicProperty.StyledName) + "");
                        UpdateValues(dynamicProperty, objectContaingValues, newObject, item.GetPropertyValue(dynamicProperty.StyledName));
                    }
                    list.Add(newObject);
                }

                var generatedArray = list.ToArray();

                // if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("generatedArray length: " + generatedArray.Length);
                
                Array typedArray = Array.CreateInstance(elementType, generatedArray.Length);
                generatedArray.CopyTo(typedArray, 0);
                
                // if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("typedArray length: " + typedArray.Length + "");

                
                if (toBeUpdateFieldInfo != null)
                {
                    if (newValue != null)
                    {
                        toBeUpdateFieldInfo.SetValue(objectToBeUpdated, typedArray);
                    }
                }
                if (toBeUpdatePropertyInfo != null)
                {
                    if (newValue != null && toBeUpdatePropertyInfo.GetSetMethod() != null)
                    {
                        toBeUpdatePropertyInfo.SetValue(objectToBeUpdated, typedArray);
                    }
                }

                return;
            }

            if (
                (toBeUpdateFieldInfo != null && toBeUpdateFieldInfo.FieldType.IsValueType && !toBeUpdateFieldInfo.FieldType.IsPrimitive && !toBeUpdateFieldInfo.FieldType.IsEnum) || 
                (toBeUpdatePropertyInfo != null && toBeUpdatePropertyInfo.PropertyType.IsValueType && !toBeUpdatePropertyInfo.PropertyType.IsPrimitive && !toBeUpdatePropertyInfo.PropertyType.IsEnum))
            {
                ProcesStruct(property, objectContaingValues, objectToBeUpdated, toBeUpdateFieldInfo, toBeUpdatePropertyInfo);

                return;
            }

            if (toBeUpdateFieldInfo != null)
            {
                if (newValue != null)
                {
                    toBeUpdateFieldInfo.SetValue(objectToBeUpdated, Convert.ChangeType(newValue, toBeUpdateFieldInfo.FieldType));
                }
            }
            if (toBeUpdatePropertyInfo != null)
            {
                if (newValue != null && toBeUpdatePropertyInfo.GetSetMethod() != null)
                {
                    toBeUpdatePropertyInfo.SetValue(objectToBeUpdated, Convert.ChangeType(newValue, toBeUpdatePropertyInfo.PropertyType));
                }
            }

            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(property.OriginalName);
        }

        private void ProcesStruct(DynamicProperty property,  ExpandoObject objectContaingValues, object objectToBeUpdated, FieldInfo toBeUpdateFieldInfo, PropertyInfo toBeUpdatePropertyInfo)
        {
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("STRUCT");
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(property.StyledName);

            var parentObjectContainingValues = (ExpandoObject)objectContaingValues.GetPropertyValue(property.StyledName);

            if (parentObjectContainingValues == null)
                return;

            // if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(test + "");
            // var asd = (ExpandoObject)test.GetPropertyValue("Size");
            // if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(asd + "");
            // if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(asd.GetPropertyValue("x") + "");
            // if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(test.GetType() + "");
            
            // var allFields = parentObjectContainingValues.GetAllFields();
            //
            // foreach (var pair in allFields)
            // {
            //     string propertyName = pair.Key;
            //
            //     Debug.Log($"Property name: {propertyName}, Value: {parentObjectContainingValues.GetPropertyValue(propertyName)}");
            // }

            var structInstance = toBeUpdateFieldInfo != null ? 
                    Activator.CreateInstance(toBeUpdateFieldInfo.FieldType) : 
                    Activator.CreateInstance(toBeUpdatePropertyInfo.PropertyType);
            
            var dynamicProperties1 = toBeUpdateFieldInfo != null ?
                GetDynamicPropertiesWithoutValues(GetFilteredFieldInfos(toBeUpdateFieldInfo.FieldType)) :
                GetDynamicPropertiesWithoutValues(GetFilteredFieldInfos(toBeUpdatePropertyInfo.PropertyType));
            if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(dynamicProperties1.Count() + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(structInstance.GetType() + "");
            foreach (var dynamicProperty in dynamicProperties1)
            {
                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(dynamicProperty.StyledName);
                var hgf = parentObjectContainingValues.GetPropertyValue(dynamicProperty.StyledName);
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(hgf + "");
                if (hgf == null)
                {
                    hgf = parentObjectContainingValues.GetPropertyValue(dynamicProperty.OriginalName);
                }
                // if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(hgf.GetPropertyValue("x") + "  aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                UpdateValues(dynamicProperty, parentObjectContainingValues, structInstance, hgf);
            }
            
            var dynamicProperties2 =  toBeUpdateFieldInfo != null ?
                GetDynamicPropertiesWithoutValues(GetPropertyInfos(toBeUpdateFieldInfo.FieldType)) :
                GetDynamicPropertiesWithoutValues(GetPropertyInfos(toBeUpdatePropertyInfo.PropertyType));
            
            if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(dynamicProperties2.Count() + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(structInstance.GetType() + "");
            foreach (var dynamicProperty in dynamicProperties2)
            {
                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(dynamicProperty.StyledName);
                var hgf = parentObjectContainingValues.GetPropertyValue(dynamicProperty.StyledName);
                if (hgf == null)
                {
                    hgf = parentObjectContainingValues.GetPropertyValue(dynamicProperty.OriginalName);
                }
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("Property Value: " + hgf);
                // if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(hgf.GetPropertyValue("x") + "  aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                UpdateValues(dynamicProperty, parentObjectContainingValues, structInstance, hgf);
            }
            
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("structInstance: " + structInstance + "");
            if (toBeUpdateFieldInfo != null)
            {
                toBeUpdateFieldInfo.SetValue(objectToBeUpdated, structInstance);
                var test = toBeUpdateFieldInfo.GetValue(objectToBeUpdated);
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("test length: " + test + "");
            }
            else
            {
                toBeUpdatePropertyInfo.SetValue(objectToBeUpdated, structInstance);
                var test = toBeUpdatePropertyInfo.GetValue(objectToBeUpdated);
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("test length: " + test + "");
            }
        }
    }
}