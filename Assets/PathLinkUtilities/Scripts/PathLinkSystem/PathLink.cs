// Decompiled with JetBrains decompiler
// Type: PathLinkUtilities.PathLink
// Assembly: PathLinkUtilities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F1DF98D9-C62A-4C38-B939-33BC1734727D
// Assembly location: C:\Users\Tobbert\Desktop\PathLinkUtilities\PathLinkUtilities.dll

using UnityEngine;

namespace PathLinkUtilities
{
  public class PathLink
  {
    public readonly PathLinkPoint StartLinkPoint;
    public readonly PathLinkPoint EndLinkPoint;
    public readonly float WaitingTimeInHours;

    public PathLink(
      PathLinkPoint startLinkPoint,
      PathLinkPoint endLinkPoint,
      float waitingTimeInHours)
    {
      this.StartLinkPoint = startLinkPoint;
      this.EndLinkPoint = endLinkPoint;
      this.WaitingTimeInHours = waitingTimeInHours;
    }

    public bool ValidLink() => (Object) this.StartLinkPoint != (Object) null && this.StartLinkPoint.enabled && (Object) this.EndLinkPoint != (Object) null && this.EndLinkPoint.enabled;
  }
}
