using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathLinkUtilities
{
  public class PathLinkRepository
  {
    private readonly HashSet<PathLink> _pathLinks = new();

    public void AddNew(PathLink pathLink) => _pathLinks.Add(pathLink);

    public PathLink GetPathLink(Vector3 startBeaverPosition, Vector3 endBeaverPosition)
    {
      foreach (PathLink pathLink in _pathLinks)
      {
        Vector3 location1 = pathLink.StartLinkPoint.Location;
        Vector3 location2 = pathLink.EndLinkPoint.Location;
        if (Vector3Int.CeilToInt(location1) == Vector3Int.CeilToInt(startBeaverPosition) && Vector3Int.FloorToInt(location2) == Vector3Int.FloorToInt(endBeaverPosition) || Vector3Int.FloorToInt(location1) == Vector3Int.FloorToInt(startBeaverPosition) && Vector3Int.CeilToInt(location2) == Vector3Int.CeilToInt(endBeaverPosition))
          return pathLink;
      }
      return null;
    }

    public PathLink GetPathLink(
      PathLinkPoint startPathLinkPoint,
      PathLinkPoint endPathLinkPoint)
    {
      foreach (PathLink pathLink in _pathLinks)
      {
        if (startPathLinkPoint == pathLink.StartLinkPoint && endPathLinkPoint == pathLink.EndLinkPoint)
          return pathLink;
      }
      return null;
    }

    public void ValidateLinks() => _pathLinks.RemoveWhere(link => !link.ValidLink());

    public IEnumerable<PathLink> PathLinks(PathLinkPoint a) => _pathLinks.Where(link => link.StartLinkPoint == a);

    public void RemoveLinks(PathLinkPoint a) => _pathLinks.RemoveWhere(link => link.StartLinkPoint == a || link.EndLinkPoint == a);
  }
}
