using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using ExpandoObject = System.Dynamic.ExpandoObject;

namespace TobbyTools.DynamicSpecificationSystem
{
    public class DynamicSpecificationApplier : ILoadableSingleton
    {
        private readonly ActiveComponentRetriever _activeComponentRetriever;
        private readonly ISpecificationService _specificationService;

        public DynamicSpecificationApplier(ActiveComponentRetriever activeComponentRetriever, ISpecificationService specificationService)
        {
            _activeComponentRetriever = activeComponentRetriever;
            _specificationService = specificationService;
        }

        public void Load()
        {
            // return;
            
            foreach (var component in _activeComponentRetriever.GetAllComponents())
            {
                var componentType = component.GetType();
                
                if (SkippableTypes.Types.Contains(componentType))
                    continue;
                
                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning("Name Monobehavior: " + componentType);

                var fieldInfos = ReflectionUtils.GetSerializeableFieldInfos(componentType);
                var dynamicProperties = DynamicPropertiesUtils.GetAll(fieldInfos, component);

                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning("Number of fields: " + dynamicProperties.Count());
                if (!dynamicProperties.Any())
                    continue;
                
                var fileName = "DynamicSpecification." + component.gameObject.name + "." + componentType.Name + ".original.json";
                
                if (Plugin.LoggingEnabled) Plugin.Log.LogError(fileName);

                var fileLocation = "C:\\Users\\jordy\\SynologyDrive\\Unity Projecten\\TimberbornModsUnity Update 4\\ThunderKit\\Staging\\TobbyTools\\GeneratedSpecs\\" + fileName;

                if (!File.Exists(fileLocation))
                    continue;
                
                ExpandoObject objectContaingValues = JObject.Parse(File.ReadAllText(fileLocation)).ToExpandoObject();

                foreach (var property in dynamicProperties)
                {
                    UpdateValues(property, objectContaingValues, component, objectContaingValues.GetPropertyValue(property.StyledName));
                }
                
                // File.Delete(fileLocation);
            }
        }

        private void UpdateValues(DynamicProperty property, ExpandoObject objectContaingValues, object objectToBeUpdated, object newValue)
        {
            if (newValue == null)
             return;
            
            var toBeUpdatedType = objectToBeUpdated.GetType();
            var toBeUpdateFieldInfo = toBeUpdatedType.GetField(property.OriginalName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var toBeUpdatePropertyInfo = toBeUpdatedType.GetProperty(property.OriginalName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (Plugin.LoggingEnabled) Plugin.Log.LogError("New value: " + newValue);
            if (Plugin.LoggingEnabled) Plugin.Log.LogError("New value type: " + newValue.GetType());

            if (toBeUpdateFieldInfo != null && toBeUpdateFieldInfo.FieldType.IsArray || (toBeUpdatePropertyInfo != null && toBeUpdatePropertyInfo.PropertyType.IsArray))
            {
                ProcessArray(property, objectContaingValues, objectToBeUpdated, toBeUpdateFieldInfo, toBeUpdatePropertyInfo);
                return;
            }

            if ((toBeUpdateFieldInfo != null && IsStruct(toBeUpdateFieldInfo.FieldType)) || 
                (toBeUpdatePropertyInfo != null && IsStruct(toBeUpdatePropertyInfo.PropertyType)))
            {
                ProcessStruct(property, objectContaingValues, objectToBeUpdated, toBeUpdateFieldInfo, toBeUpdatePropertyInfo);
                return;
            }

            UpdateOppropriateValue(objectToBeUpdated, toBeUpdateFieldInfo, toBeUpdatePropertyInfo, newValue);
        }

        private void ProcessArray(DynamicProperty property,  ExpandoObject objectContaingValues, object objectToBeUpdated, FieldInfo toBeUpdateFieldInfo, PropertyInfo toBeUpdatePropertyInfo)
        {
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("ARRAY");
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(property.StyledName);
            var elementType = toBeUpdateFieldInfo != null ? toBeUpdateFieldInfo.FieldType.GetElementType() : toBeUpdatePropertyInfo.PropertyType.GetElementType();
            var dynamicProperties = DynamicPropertiesUtils.GetAllWithoutValues(ReflectionUtils.GetSerializeableFieldInfos(elementType));

            var list = new List<object>();

            foreach (ExpandoObject item in objectContaingValues.GetList(property.StyledName))
            {
                CreateInstancedObject(elementType, out object newObject);
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
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("generatedArray length: " + generatedArray.Length);
            Array typedArray = Array.CreateInstance(elementType, generatedArray.Length);
            generatedArray.CopyTo(typedArray, 0);
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("typedArray length: " + typedArray.Length + "");
                
            UpdateOppropriateValue(objectToBeUpdated, toBeUpdateFieldInfo, toBeUpdatePropertyInfo, typedArray);
        }

        private bool IsStruct(Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        private void CreateInstancedObject(Type type, out object newObject)
        {
            try
            {
                newObject = Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                if (Plugin.LoggingEnabled) Plugin.Log.LogError(e.ToString());
                        
                var constructor = type.GetConstructors().First();   
                    
                var parameters = constructor.GetParameters();
                var args = new object[parameters.Length];
                    
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    args[i] = parameterType.IsValueType ? Activator.CreateInstance(parameterType) : null;
                }
                    
                newObject = constructor.Invoke(args);
            }
        }

        private void ProcessStruct(DynamicProperty property,  ExpandoObject objectContaingValues, object objectToBeUpdated, FieldInfo toBeUpdateFieldInfo, PropertyInfo toBeUpdatePropertyInfo)
        {
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("STRUCT");
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(property.StyledName);

            var parentObjectContainingValues = (ExpandoObject)objectContaingValues.GetPropertyValue(property.StyledName);

            if (parentObjectContainingValues == null)
                return;

            var structInstance = toBeUpdateFieldInfo != null ? 
                Activator.CreateInstance(toBeUpdateFieldInfo.FieldType) : 
                Activator.CreateInstance(toBeUpdatePropertyInfo.PropertyType);
            
            var alldynamicProperties = toBeUpdateFieldInfo != null ?
                DynamicPropertiesUtils.GetAllWithoutValues(toBeUpdateFieldInfo.FieldType) :
                DynamicPropertiesUtils.GetAllWithoutValues(toBeUpdatePropertyInfo.PropertyType);

            if (Plugin.LoggingEnabled) Plugin.Log.LogWarning("Number of dynamic properties: " + alldynamicProperties.Count());
            if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(structInstance.GetType() + "");
            foreach (var dynamicProperty in alldynamicProperties)
            {
                if (Plugin.LoggingEnabled) Plugin.Log.LogWarning(dynamicProperty.StyledName);
                var value = parentObjectContainingValues.GetPropertyValue(dynamicProperty.StyledName);
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo(value + "");
                if (value == null)
                {
                    value = parentObjectContainingValues.GetPropertyValue(dynamicProperty.OriginalName);
                }
                
                UpdateValues(dynamicProperty, parentObjectContainingValues, structInstance, value);
            }

            UpdateOppropriateValue(objectToBeUpdated, toBeUpdateFieldInfo, toBeUpdatePropertyInfo, structInstance);
        }

        private void UpdateOppropriateValue(object objectToBeUpdated, FieldInfo toBeUpdateFieldInfo, PropertyInfo toBeUpdatePropertyInfo, object newValue)
        {
            if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("New value: " + newValue);
            
            if (toBeUpdateFieldInfo != null)
            {
                var correctType = toBeUpdateFieldInfo.FieldType;
                if (newValue.GetType() != correctType)
                    newValue = Convert.ChangeType(newValue, correctType);
                toBeUpdateFieldInfo.SetValue(objectToBeUpdated, newValue);
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("Current value: " + toBeUpdateFieldInfo.GetValue(objectToBeUpdated));
                return;
            }
            if (toBeUpdatePropertyInfo != null && toBeUpdatePropertyInfo.GetSetMethod() != null)
            {
                var correctType = toBeUpdatePropertyInfo.PropertyType;
                if (newValue.GetType() != correctType)
                    newValue = Convert.ChangeType(newValue, correctType);
                toBeUpdatePropertyInfo.SetValue(objectToBeUpdated, newValue);
                if (Plugin.LoggingEnabled) Plugin.Log.LogInfo("Current value: " + toBeUpdatePropertyInfo.GetValue(objectToBeUpdated));
            }
        }
    }
}