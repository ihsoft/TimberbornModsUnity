// Decompiled with JetBrains decompiler
// Type: PathLinkUtilities.Vector3Extensions
// Assembly: PathLinkUtilities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F1DF98D9-C62A-4C38-B939-33BC1734727D
// Assembly location: C:\Users\Tobbert\Desktop\PathLinkUtilities\PathLinkUtilities.dll

using System;
using UnityEngine;

namespace PathLinkUtilities
{
  public static class Vector3Extensions
  {
    public static Vector3Int ToBlockServicePosition(this Vector3 vector3) => new Vector3Int((int) Math.Floor((double) vector3.x), (int) Math.Floor((double) vector3.z), (int) Math.Round((double) vector3.y));
  }
}
