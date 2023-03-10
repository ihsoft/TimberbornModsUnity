// Decompiled with JetBrains decompiler
// Type: PathLinkUtilities.PathLinkWaiter
// Assembly: PathLinkUtilities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F1DF98D9-C62A-4C38-B939-33BC1734727D
// Assembly location: C:\Users\Tobbert\Desktop\PathLinkUtilities\PathLinkUtilities.dll

using Bindito.Core;
using System.Collections.Generic;
using Timberborn.CharacterMovementSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace PathLinkUtilities
{
  public class PathLinkWaiter : MonoBehaviour
  {
    private PathLinkRepository _pathLinkRepository;
    private IDayNightCycle _dayNightCycle;
    private float _finishTimestamp;

    private bool IsWaiting => (double) this._finishTimestamp > (double) this._dayNightCycle.PartialDayNumber;

    [Inject]
    public void InjectDependencies(
      PathLinkRepository pathLinkPointService,
      IDayNightCycle dayNightCycle)
    {
      this._pathLinkRepository = pathLinkPointService;
      this._dayNightCycle = dayNightCycle;
    }

    public bool ShouldWait(
      IReadOnlyList<Vector3> pathCorners,
      ref int nextCornerIndex,
      MovementAnimator movementAnimator)
    {
      return this.IsWaiting || this.CheckForPathLink(ref pathCorners, ref nextCornerIndex, movementAnimator);
    }

    private bool CheckForPathLink(
      ref IReadOnlyList<Vector3> pathCorners,
      ref int nextCornerIndex,
      MovementAnimator movementAnimator)
    {
      PathLink pathLink = this._pathLinkRepository.GetPathLink(pathCorners[nextCornerIndex - 1], pathCorners[nextCornerIndex]);
      if (pathLink == null)
        return false;
      movementAnimator.StopAnimatingMovement();
      this.transform.position = pathLink.EndLinkPoint.Location;
      ++nextCornerIndex;
      this._finishTimestamp = this._dayNightCycle.DayNumberHoursFromNow(pathLink.WaitingTimeInHours);
      return true;
    }
  }
}
