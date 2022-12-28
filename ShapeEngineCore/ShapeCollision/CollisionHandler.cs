﻿using ShapeLib;
using System.Numerics;
using Raylib_CsLo;

namespace ShapeCollision
{
    public struct QueryInfo
    {
        public ICollidable collidable;
        public Intersection intersection;
        public QueryInfo(ICollidable collidable)
        {
            this.collidable = collidable;
            this.intersection = new();
        }
        public QueryInfo(ICollidable collidable, Intersection intersection)
        {
            this.collidable = collidable;
            this.intersection = intersection;
        }

    }

    //colliders have bounding sphere
    //check if bounding spheres would overlap and if the colliders relative velocity lets them move towards each other
    //only if both a true, check for final overlap
    public class CollisionHandler
    {
        protected List<ICollidable> collidables = new();
        protected List<ICollidable> tempHolding = new();
        protected List<ICollidable> tempRemoving = new();
        protected SpatialHash spatialHash;

        protected List<CollisionInfo> overlapInfos = new();
        public CollisionHandler(float x, float y, float w, float h, int rows, int cols)
        {
            spatialHash = new(x, y, w, h, rows, cols);
        }
        public void UpdateArea(Rectangle newArea)
        {
            int rows = spatialHash.GetRows();
            int cols = spatialHash.GetCols();
            spatialHash.Close();
            spatialHash = new(newArea.x, newArea.y, newArea.width, newArea.height, rows, cols);
        }
        public SpatialHash GetSpatialHash() { return spatialHash; }
        public void Add(ICollidable collider)
        {
            if (collidables.Contains(collider)) return;
            tempHolding.Add(collider);
        }
        public void AddRange(List<ICollidable> colliders)
        {
            foreach (ICollidable collider in colliders)
            {
                Add(collider);
            }
        }
        public void Remove(ICollidable collider)
        {
            tempRemoving.Add(collider);
        }
        public void RemoveRange(List<ICollidable> colliders)
        {
            tempRemoving.AddRange(colliders);
        }
        public void Clear()
        {
            collidables.Clear();
            tempHolding.Clear();
            tempRemoving.Clear();
            overlapInfos.Clear();
        }
        public void Close()
        {
            Clear();
            spatialHash.Close();
        }
        public virtual void Update(float dt)
        {
            spatialHash.Clear();

            for (int i = collidables.Count - 1; i >= 0; i--)
            {
                var collider = collidables[i];
                if (collider.GetCollider().IsEnabled())
                {
                    spatialHash.Add(collider);
                }
            }

            for (int i = 0; i < collidables.Count; i++)
            {
                ICollidable collider = collidables[i];
                if (!collider.GetCollider().IsEnabled() || !collider.GetCollider().CheckCollision) continue;
                string[] collisionMask = collider.GetCollisionMask();


                List<ICollidable> others = spatialHash.GetObjects(collider);
                foreach (ICollidable other in others)
                {
                    string otherLayer = other.GetCollisionLayer();
                    if (collisionMask.Length > 0)
                    {
                        if (!collisionMask.Contains(otherLayer)) continue;
                    }//else collide with everything

                    var selfC = collider.GetCollider();
                    var info = SGeometry.GetCollisionInfo(collider, other);
                    if (info.overlapping)
                        overlapInfos.Add(info);
                }
            }
            Resolve();
        }
        protected virtual void Resolve()
        {
            //collidables.AddRange(tempHolding);
            foreach (var collider in tempHolding)
            {
                collidables.Add(collider);
            }
            tempHolding.Clear();

            foreach (var collider in tempRemoving)
            {
                collidables.Remove(collider);
            }
            tempRemoving.Clear();


            foreach (CollisionInfo info in overlapInfos)
            {
                if (info.other == null || info.self == null) continue;
                //info.other.Overlap(info);
                info.self.Overlap(info);
            }
            overlapInfos.Clear();
            
        }

        private List<QueryInfo> GetQueryInfo(Collider caster, bool getIntersections, params string[] collisionMask)
        {
            List<QueryInfo> infos = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(caster, obj.GetCollider()))
                    {
                        Intersection intersection = getIntersections ? SGeometry.Intersection(caster, obj.GetCollider()) : new();
                        QueryInfo q = new(obj, intersection);
                        infos.Add(q);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(caster, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            Intersection intersection = getIntersections ? SGeometry.Intersection(caster, obj.GetCollider()) : new();
                            QueryInfo q = new(obj, intersection);
                            infos.Add(q);
                        }
                    }
                }
            }
            return infos;
        }
        public List<QueryInfo> QuerySpace(ICollidable caster)
        {
            return GetQueryInfo(caster.GetCollider(), caster.GetCollider().CheckIntersections, caster.GetCollisionMask());
            //List<QueryInfo> infos = new();
            //List<ICollidable> objects = spatialHash.GetObjects(caster);
            //foreach (ICollidable obj in objects)
            //{
            //    if (caster.GetCollisionMask().Length <= 0)
            //    {
            //        if (SGeometry.Overlap(caster.GetCollider(), obj.GetCollider()))
            //        {
            //            Intersection intersection = getIntersections ? SGeometry.Intersection(caster.GetCollider(), obj.GetCollider()) : new();
            //            QueryInfo q = new(obj, intersection);
            //            infos.Add(q);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(caster.GetCollider(), obj.GetCollider()))
            //        {
            //            if (caster.GetCollisionMask().Contains(obj.GetCollisionLayer()))
            //            {
            //                Intersection intersection = getIntersections ? SGeometry.Intersection(caster.GetCollider(), obj.GetCollider()) : new();
            //                QueryInfo q = new(obj, intersection);
            //                infos.Add(q);
            //            }
            //        }
            //    }
            //}
            //return infos;
        }
        public List<QueryInfo> QuerySpace(Collider collider, params string[] collisionMask)
        {
            return GetQueryInfo(collider, collider.CheckIntersections, collisionMask);
            //List<QueryInfo> infos = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //}
            //return infos;
        }
        public List<QueryInfo> QuerySpace(Rectangle rect, bool getIntersections,params string[] collisionMask)
        {
            RectCollider collider = new(rect);
            return GetQueryInfo(collider, getIntersections, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<QueryInfo> QuerySpace(Vector2 pos, float r, bool getIntersections, params string[] collisionMask)
        {
            CircleCollider collider = new(pos, r);
            return GetQueryInfo(collider, getIntersections, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<QueryInfo> QuerySpace(Vector2 pos, Vector2 size, Vector2 alignement, bool getIntersections, params string[] collisionMask)
        {
            RectCollider collider = new(pos, size, alignement);
            return GetQueryInfo(collider, getIntersections, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<QueryInfo> QuerySpace(Vector2 pos, Vector2 dir, float length, bool getIntersections, params string[] collisionMask)
        {
            SegmentCollider collider = new(pos, dir, length);
            return GetQueryInfo(collider, getIntersections, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<QueryInfo> QuerySpace(Vector2 start, Vector2 end, bool getIntersections, params string[] collisionMask)
        {
            SegmentCollider collider = new(start, end);
            return GetQueryInfo(collider, getIntersections, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }


        private List<ICollidable> GetCastBodies(Collider caster, params string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(caster, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(caster, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }
            }
            return bodies;
        }
        public List<ICollidable> CastSpace(ICollidable caster)
        {
            return GetCastBodies(caster.GetCollider(), caster.GetCollisionMask());
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(caster);
            //foreach (ICollidable obj in objects)
            //{
            //    if (caster.GetCollisionMask().Length <= 0)
            //    {
            //        if (SGeometry.Overlap(caster.GetCollider(), obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(caster.GetCollider(), obj.GetCollider()))
            //        {
            //            if (caster.GetCollisionMask().Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Collider collider, params string[] collisionMask)
        {
            return GetCastBodies(collider, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Rectangle rect, params string[] collisionMask)
        {
            RectCollider collider = new(rect);
            return GetCastBodies(collider, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, float r, params string[] collisionMask)
        {
            CircleCollider collider = new(pos, r);
            return GetCastBodies(collider, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 size, Vector2 alignement, params string[] collisionMask)
        {
            RectCollider collider = new(pos, size, alignement);
            return GetCastBodies(collider, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 dir, float length, params string[] collisionMask)
        {
            SegmentCollider collider = new(pos, dir, length);
            return GetCastBodies(collider, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 start, Vector2 end, params string[] collisionMask)
        {
            SegmentCollider collider = new(start, end);
            return GetCastBodies(collider, collisionMask);
            //List<ICollidable> bodies = new();
            //List<ICollidable> objects = spatialHash.GetObjects(collider);
            //foreach (ICollidable obj in objects)
            //{
            //    if (collisionMask.Length <= 0)
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            bodies.Add(obj);
            //        }
            //    }
            //    else
            //    {
            //        if (SGeometry.Overlap(collider, obj.GetCollider()))
            //        {
            //            if (collisionMask.Contains(obj.GetCollisionLayer()))
            //            {
            //                bodies.Add(obj);
            //            }
            //        }
            //    }
            //
            //}
            //return bodies;
        }


        public void DebugDrawGrid(Color border, Color fill)
        {
            spatialHash.DebugDrawGrid(border, fill);
        }
    }


}
