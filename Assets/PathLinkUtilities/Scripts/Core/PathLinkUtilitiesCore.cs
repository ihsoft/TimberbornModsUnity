// Decompiled with JetBrains decompiler
// Type: PathLinkUtilities.PathLinkUtilitiesCore
// Assembly: PathLinkUtilities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F1DF98D9-C62A-4C38-B939-33BC1734727D
// Assembly location: C:\Users\Tobbert\Desktop\PathLinkUtilities\PathLinkUtilities.dll

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace PathLinkUtilities
{
  public class PathLinkUtilitiesCore
  {
    private readonly Dictionary<string, FieldInfo> _fieldInfos = new Dictionary<string, FieldInfo>();

    public void SetInaccessibleField(object instance, string fieldName, object newValue)
    {
      if (!this._fieldInfos.ContainsKey(fieldName))
        this._fieldInfos.Add(fieldName, AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
      this._fieldInfos[fieldName].SetValue(instance, newValue);
    }

    public object GetInaccessibleField(object instance, string fieldName)
    {
      if (!this._fieldInfos.ContainsKey(fieldName))
        this._fieldInfos.Add(fieldName, AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
      return this._fieldInfos[fieldName].GetValue(instance);
    }
  }
}
