using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class CollisionInformation
{
    public readonly List<Collision> Collisions;
    public readonly CollisionSurface CollisionSurface;
    public CollisionInformation(List<Collision> collisions, bool computesIntersections)
    {
        this.Collisions = collisions;
        if (!computesIntersections) this.CollisionSurface = new();
        else
        {
            Vector2 avgPoint = new();
            Vector2 avgNormal = new();
            var count = 0;
            foreach (var col in collisions)
            {
                if (col.Intersection.Valid)
                {
                    count++;
                    var surface = col.Intersection.CollisionSurface;
                    avgPoint += surface.Point;
                    avgNormal += surface.Normal;
                }
            }

            if (count > 0)
            {
                avgPoint /= count;
                avgNormal = avgNormal.Normalize();
                this.CollisionSurface = new(avgPoint, avgNormal);
            }
            else
            {
                this.CollisionSurface = new();
            }
        }
    }

    //public bool ContainsCollidable(TCollidable other)
    //{
    //    foreach (var c in Collisions)
    //    {
    //        if (c.Other == other) return true;
    //    }
    //    return false;
    //}
    public List<Collision> FilterCollisions(Predicate<Collision> match)
    {
        List<Collision> filtered = new();
        foreach (var c in Collisions)
        {
            if (match(c)) filtered.Add(c);
        }
        return filtered;
    }
    public HashSet<Collider> FilterColliders(Predicate<Collider> match)
    {
        HashSet<Collider> filtered = new();
        foreach (var c in Collisions)
        {
            if (match(c.Other)) filtered.Add(c.Other);
        }
        return filtered;
    }
    public HashSet<Collider> GetAllColliders()
    {
        HashSet<Collider> others = new();
        foreach (var c in Collisions)
        {
            others.Add(c.Other);

        }
        return others;
    }
    public List<Collision> GetFirstContactCollisions()
    {
        return FilterCollisions((c) => c.FirstContact);
    }
    public HashSet<Collider> GetFirstContactColliders()
    {
        var filtered = GetFirstContactCollisions();
        HashSet<Collider> others = new();
        foreach (var c in filtered)
        {
            others.Add(c.Other);
        }
        return others;
    }
}