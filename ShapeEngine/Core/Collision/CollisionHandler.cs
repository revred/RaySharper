﻿using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision
{
    
    public class CollisionHandler : IBounds
    {
        private class OverlapRegister
        {
            private HashSet<OverlapEntry> entries;

            public OverlapRegister(int capacity)
            {
                entries = new(capacity);
            }
            
            public OverlapEntry? FindEntry(Collider self, Collider other)
            {
                foreach (var entry in entries)
                {
                    if (self == entry.Self && other == entry.Other)
                    {
                        return entry;
                    }
                }

                return null;
            }

            public bool AddEntry(OverlapEntry entry) => entries.Add(entry);

            public bool RemoveEntry(OverlapEntry entry) => entries.Remove(entry);

            public void ProcessEntries()
            {
                foreach (var entry in entries)
                {
                    entry.Self.ResolveCollisionEnded(entry.Other);
                }
            }

            public void Clear() => entries.Clear();
            public void Swap(OverlapRegister other) => (entries, other.entries) = (other.entries, entries);
        }
        private class OverlapEntry
        {
            public readonly Collider Self;
            public readonly Collider Other;

            public OverlapEntry(Collider self, Collider other)
            {
                this.Self = self;
                this.Other = other;
            }
        }
        private class ObjectRegister<T>
        {
            public readonly HashSet<T> AllObjects;
            private readonly List<T> tempHolding;
            private readonly List<T> tempRemoving;

            public ObjectRegister(int capacity)
            {
                AllObjects = new(capacity);
                tempHolding = new(capacity / 4);
                tempRemoving = new(capacity / 4);
            }
            
            public void Add(T obj) => tempHolding.Add(obj);

            public void AddRange(IEnumerable<T> objs) => tempHolding.AddRange(objs);

            public void AddRange(params T[] objs) => tempHolding.AddRange(objs);

            public void Remove(T obj) => tempRemoving.Add(obj);

            public void RemoveRange(IEnumerable<T> objs) => tempRemoving.AddRange(objs);

            public void RemoveRange(params T[] objs) => tempRemoving.AddRange(objs);

            public void Process()
            {
                foreach (var obj in tempRemoving)
                {
                    AllObjects.Remove(obj);
                }
                tempRemoving.Clear();
                
                foreach (var obj in tempHolding)
                {
                    AllObjects.Add(obj);
                }
                tempHolding.Clear();

                
            }
            protected virtual void ObjectAdded(T obj) { }
            protected virtual void ObjectRemoved(T obj) { }
            public void Clear()
            {
                foreach (var obj in AllObjects)
                {
                    ObjectRemoved(obj);
                }
                AllObjects.Clear();
                tempHolding.Clear();
                tempRemoving.Clear();
            }
        }

        private class CollisionObjectRegister : ObjectRegister<CollisionObject>
        {
            private readonly CollisionHandler handler;
            public CollisionObjectRegister(int capacity, CollisionHandler handler) : base(capacity)
            {
                this.handler = handler;
            }

            protected override void ObjectAdded(CollisionObject obj)
            {
                obj.OnCollisionSystemEntered(handler);
            }

            protected override void ObjectRemoved(CollisionObject obj)
            {
                obj.OnCollisionSystemLeft(handler);
            }
        }
        
        
        private readonly CollisionObjectRegister collisionBodyRegister;
        private readonly ObjectRegister<Collider> colliderRegister;
        
        private readonly SpatialHash spatialHash;
        private readonly Dictionary<Collider, List<Collision>> collisionStack;
        
        //use for detecting when overlap has ended
        private readonly OverlapRegister activeRegister;
        private readonly OverlapRegister oldRegister;

        private readonly HashSet<Collider> collisionCandidateCheckRegister = new();
        private List<SpatialHash.Bucket> collisionCandidateBuckets = new();

        public int Count => collisionBodyRegister.AllObjects.Count; // collisionBodies.Count;

        public Rect Bounds => spatialHash.Bounds;

        public CollisionHandler(float x, float y, float w, float h, int rows, int cols, int startCapacity = 1024)
        {
            spatialHash = new(x, y, w, h, rows, cols);
            collisionBodyRegister = new(startCapacity, this);
            colliderRegister = new(startCapacity);
            collisionStack = new(startCapacity / 4);
            activeRegister = new(startCapacity / 4);
            oldRegister = new(startCapacity / 4);
        }

        public CollisionHandler(Rect bounds, int rows, int cols, int startCapacity = 1024)
        {
            spatialHash = new(bounds.X, bounds.Y, bounds.Width, bounds.Height, rows, cols);
            
            collisionBodyRegister = new(startCapacity, this);
            colliderRegister = new(startCapacity);
            collisionStack = new(startCapacity / 4);
            activeRegister = new(startCapacity / 4);
            oldRegister = new(startCapacity / 4);
        }
        
        
        public void ResizeBounds(Rect newBounds) { spatialHash.ResizeBounds(newBounds); }

        public void Add(CollisionObject collisionObject) => collisionBodyRegister.Add(collisionObject);
        public void AddRange(IEnumerable<CollisionObject> collisionObjects) => collisionBodyRegister.AddRange(collisionObjects);
        public void AddRange(params CollisionObject[] collisionObjects)=> collisionBodyRegister.AddRange(collisionObjects);
        public void Remove(CollisionObject collisionObject)=> collisionBodyRegister.Remove(collisionObject);
        public void RemoveRange(IEnumerable<CollisionObject> collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);
        public void RemoveRange(params CollisionObject[] collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);
        
        public void Add(Collider collider) => colliderRegister.Add(collider);
        public void AddRange(IEnumerable<Collider> colliders) => colliderRegister.AddRange(colliders);
        public void AddRange(params Collider[] colliders)=> colliderRegister.AddRange(colliders);
        public void Remove(Collider collider)=> colliderRegister.Remove(collider);
        public void RemoveRange(IEnumerable<Collider> colliders)  => colliderRegister.RemoveRange(colliders);
        public void RemoveRange(params Collider[] colliders)  => colliderRegister.RemoveRange(colliders);
        
        public void Clear()
        {
            collisionBodyRegister.Clear();
            colliderRegister.Clear();
            collisionStack.Clear();
        }
        public void Close()
        {
            Clear();
            spatialHash.Close();
            
        }

        public void Update(float dt)
        {
            spatialHash.Fill(collisionBodyRegister.AllObjects, colliderRegister.AllObjects);

            ProcessCollisions(dt);
            
            Resolve();
        }
        private void ProcessCollisions(float dt)
        {
            foreach (var collisionBody in collisionBodyRegister.AllObjects)
            {
                if (!collisionBody.Enabled || !collisionBody.HasColliders) continue;

                if (collisionBody.ProjectShape)
                {
                    foreach (var collider in collisionBody.Colliders)
                    {
                        if (!collider.Enabled) continue;

                        var projected = collider.Project(collisionBody.Velocity * dt);
                        if(projected == null) continue;
                        collisionCandidateBuckets.Clear();
                        collisionCandidateCheckRegister.Clear();
                        spatialHash.GetCandidateBuckets(projected, ref collisionCandidateBuckets);
                        
                        if(collisionCandidateBuckets.Count <= 0) continue;     
                        
                        var mask = collider.CollisionMask;
                        bool computeIntersections = collider.ComputeIntersections;
                        List<Collision>? cols = null;
                        
                        foreach (var bucket in collisionCandidateBuckets)
                        {
                            foreach (var candidate in bucket)
                            {
                                if (candidate == collider) continue;
                                if (candidate.Parent != null && collider.Parent != null && candidate.Parent == collider.Parent) continue;
                                if (!mask.Has(candidate.CollisionLayer)) continue;
                                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                                bool overlap = projected.Overlap(candidate); // ShapeGeometry.Overlap(collider, candidate);
                                if (overlap)
                                {
                                    var oldEntry = oldRegister.FindEntry(collider, candidate);
                                    bool firstContact;
                                    if (oldEntry != null)
                                    {
                                        firstContact = false;
                                        oldRegister.RemoveEntry(oldEntry);
                                        activeRegister.AddEntry(oldEntry);

                                    }
                                    else
                                    {
                                        firstContact = true;
                                        activeRegister.AddEntry(new OverlapEntry(collider, candidate));
                                    }
                                    
                                    if (computeIntersections)
                                    {
                                        //CollisionChecksPerFrame++;
                                        var collisionPoints = projected.Intersect(candidate); // ShapeGeometry.Intersect(collider, candidate);

                                        //shapes overlap but no collision points means collidable is completely inside other
                                        //closest point on bounds of other are now used for collision point
                                        if (collisionPoints == null || collisionPoints.Count <= 0)
                                        {
                                            var refPoint = collider.PrevTransform.Position;// PrevPosition;
                                            if (!candidate.ContainsPoint(refPoint))
                                            {
                                                var closest = candidate.GetClosestCollisionPoint(refPoint);
                                                collisionPoints ??= new();
                                                collisionPoints.Add(closest);
                                            }
                                        }

                                        Collision c = new(collider, candidate, firstContact, collisionPoints);
                                        cols ??= new();
                                        cols.Add(c);
                                    }
                                    else
                                    {
                                        Collision c = new(collider, candidate, firstContact);
                                        cols ??= new();
                                        cols.Add(c);
                                    }
                                }
                            }
                        }

                        if (cols is { Count: > 0 })
                        {
                            if (!collisionStack.TryAdd(collider, cols))
                            {
                                collisionStack[collider].AddRange(cols);
                            }
                        }
                    }
                
                }
                else
                {
                    foreach (var collider in collisionBody.Colliders)
                    {
                        if (!collider.Enabled) continue;
                    
                        collisionCandidateBuckets.Clear();
                        collisionCandidateCheckRegister.Clear();
                        spatialHash.GetRegisteredCollisionCandidateBuckets(collider, ref collisionCandidateBuckets);
                        
                        if(collisionCandidateBuckets.Count <= 0) continue;     
                        
                        var mask = collider.CollisionMask;
                        bool computeIntersections = collider.ComputeIntersections;
                        List<Collision>? cols = null;
                        
                        foreach (var bucket in collisionCandidateBuckets)
                        {
                            foreach (var candidate in bucket)
                            {
                                if(candidate == collider) continue;
                                if (candidate.Parent != null && collider.Parent != null && candidate.Parent == collider.Parent) continue;
                                if (!mask.Has(candidate.CollisionLayer)) continue;
                                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                                bool overlap = collider.Overlap(candidate); // ShapeGeometry.Overlap(collider, candidate);
                                if (overlap)
                                {
                                    var oldEntry = oldRegister.FindEntry(collider, candidate);
                                    bool firstContact;
                                    if (oldEntry != null)
                                    {
                                        firstContact = false;
                                        oldRegister.RemoveEntry(oldEntry);
                                        activeRegister.AddEntry(oldEntry);

                                    }
                                    else
                                    {
                                        firstContact = true;
                                        activeRegister.AddEntry(new OverlapEntry(collider, candidate));
                                    }
                                    
                                    if (computeIntersections)
                                    {
                                        //CollisionChecksPerFrame++;
                                        var collisionPoints = collider.Intersect(candidate); // ShapeGeometry.Intersect(collider, candidate);

                                        //shapes overlap but no collision points means collidable is completely inside other
                                        //closest point on bounds of other are now used for collision point
                                        if (collisionPoints == null || collisionPoints.Count <= 0)
                                        {
                                            var refPoint = collider.PrevTransform.Position;// PrevPosition;
                                            if (!candidate.ContainsPoint(refPoint))
                                            {
                                                var closest = candidate.GetClosestCollisionPoint(refPoint);
                                                collisionPoints ??= new();
                                                collisionPoints.Add(closest);
                                            }
                                            //CollisionPoint closest = shape.GetClosestPoint(refPoint);
                                            //collisionPoints.Add(closest);

                                        }

                                        Collision c = new(collider, candidate, firstContact, collisionPoints);
                                        cols ??= new();
                                        cols.Add(c);
                                    }
                                    else
                                    {
                                        Collision c = new(collider, candidate, firstContact);
                                        cols ??= new();
                                        cols.Add(c);
                                    }
                                }
                            }
                        }

                        if (cols is { Count: > 0 })
                        {
                            if (!collisionStack.TryAdd(collider, cols))
                            {
                                collisionStack[collider].AddRange(cols);
                            }
                        }
                    }
                
                }
                
            }
        }
        private void Resolve()
        {
            collisionBodyRegister.Process();
            colliderRegister.Process();


            foreach (var kvp in collisionStack)
            {
                var collider = kvp.Key;
                var cols = kvp.Value;
                if(cols.Count > 0)
                {
                    CollisionInformation collisionInfo = new(cols, collider.ComputeIntersections);
                    collider.ResolveCollision(collisionInfo);
                }
            }
            collisionStack.Clear();

            oldRegister.ProcessEntries();
            oldRegister.Swap(activeRegister);
            activeRegister.Clear();
        }
        
        
        // public QueryInfos? QuerySpace(CollisionBody collisionBody, Vector2 origin, bool sorted = true)
        // {
        //     if (!collisionBody.HasColliders) return null;
        //     QueryInfos? infos = null;
        //     
        //     foreach (var collider in collisionBody.Colliders)
        //     {
        //         collisionCandidateBuckets.Clear();
        //         collisionCandidateCheckRegister.Clear();
        //         spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
        //         if (collisionCandidateBuckets.Count <= 0) return null;
        //         
        //         var mask = collider.CollisionMask;
        //         foreach (var bucket in collisionCandidateBuckets)
        //         {
        //             foreach (var candidate in bucket)
        //             {
        //                 if (candidate == collider) continue;
        //                 if (!mask.Has(candidate.CollisionLayer)) continue;
        //                 if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        //
        //                 var collisionPoints = collider.Intersect(candidate);
        //         
        //                 if (collisionPoints == null || !collisionPoints.Valid) continue;
        //                 
        //                 infos ??= new();
        //                 infos.Add(new(collider, candidate, origin, collisionPoints));
        //             }
        //         }
        //     }
        //     
        //     if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
        //     return infos;
        // }
        //
        public Dictionary<Collider, QueryInfos>? QuerySpace(CollisionObject collisionObject, Vector2 origin, bool sorted = true)
        {
            if (!collisionObject.Enabled || !collisionObject.HasColliders) return null;

            Dictionary<Collider, QueryInfos>? result = null;
            
            foreach (var collider in collisionObject.Colliders)
            {
                var info = QuerySpace(collider, origin, sorted);
                if (info != null && info.Count > 0)
                {
                    result ??= new();
                    result.Add(collider, info);
                }
            }

            return result;
        }
        public QueryInfos? QuerySpace(Collider collider, Vector2 origin, bool sorted = true)
        {
            QueryInfos? infos = null;
            
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            
            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    var collisionPoints = collider.Intersect(candidate);
            
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Segment shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            QueryInfos? infos = null;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Triangle shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            QueryInfos? infos = null;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Circle shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            QueryInfos? infos = null;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Rect shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            QueryInfos? infos = null;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Polygon shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            QueryInfos? infos = null;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }
        public QueryInfos? QuerySpace(Polyline shape, Vector2 origin, BitFlag collisionMask, bool sorted = true)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            QueryInfos? infos = null;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
        
                    var collisionPoints = shape.Intersect(candidate); //  ShapeGeometry.Intersect(shape, candidate.GetCollider().GetShape());
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    infos ??= new();
                    infos.Add(new(candidate, origin, collisionPoints));
                }
            }
            if(sorted && infos is { Count: > 1 }) infos.SortClosest(origin);
            return infos;
        }

        
        public void CastSpace(CollisionObject collisionBody, ref List<Collider> result, bool sorted = false)
        {
            if (!collisionBody.HasColliders) return;

            foreach (var collider in collisionBody.Colliders)
            {
                collisionCandidateBuckets.Clear();
                collisionCandidateCheckRegister.Clear();
                spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
                if (collisionCandidateBuckets.Count <= 0) return;

                var mask = collider.CollisionMask;
                foreach (var bucket in collisionCandidateBuckets)
                {
                    foreach (var candidate in bucket)
                    {
                        if (candidate == collider) continue;
                        if (!mask.Has(candidate.CollisionLayer)) continue;
                        if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                        if (collider.Overlap(candidate))
                        {
                            result.Add(candidate);
                        }
                    }
                }
            }
            if (sorted && result.Count > 1)
            {
                var origin = collisionBody.Transform.Position; // collidable.GetCollider().Pos;
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (origin - a.CurTransform.Position).LengthSquared();
                        float lb = (origin - b.CurTransform.Position).LengthSquared();

                        if (la > lb) return 1;
                        if (ShapeMath.EqualsF(la, lb)) return 0;
                        return -1;
                    }
                );
            }
        }
        public void CastSpace(Collider collider,  ref List<Collider> result, bool sorted = false)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        result.Add(candidate);
                    }
                }
            }
            if (sorted && result.Count > 1)
            {
                var origin = collider.CurTransform.Position;
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (origin - a.CurTransform.Position).LengthSquared();
                        float lb = (origin - b.CurTransform.Position).LengthSquared();

                        if (la > lb) return 1;
                        if (ShapeMath.EqualsF(la, lb)) return 0;
                        return -1;
                    }
                );
            }
        }
        public void CastSpace(Segment shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Triangle shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Circle shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Rect shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Polygon shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        public void CastSpace(Polyline shape, BitFlag collisionMask, ref List<Collider> result)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
        
        
        public int CastSpace(CollisionObject collisionBody)
        {
            if (!collisionBody.HasColliders) return 0;

            int count = 0;
            foreach (var collider in collisionBody.Colliders)
            {
                collisionCandidateBuckets.Clear();
                collisionCandidateCheckRegister.Clear();
                spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
                if (collisionCandidateBuckets.Count <= 0) return 0;

                var mask = collider.CollisionMask;
                foreach (var bucket in collisionCandidateBuckets)
                {
                    foreach (var candidate in bucket)
                    {
                        if (candidate == collider) continue;
                        if (!mask.Has(candidate.CollisionLayer)) continue;
                        if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                        if (collider.Overlap(candidate))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }
        public int CastSpace(Collider collider)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            int count = 0;
            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        public int CastSpace(Segment shape, BitFlag collisionMask)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            int count = 0;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        public int CastSpace(Triangle shape, BitFlag collisionMask)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            int count = 0;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        public int CastSpace(Circle shape, BitFlag collisionMask)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            int count = 0;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        public int CastSpace(Rect shape, BitFlag collisionMask)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            int count = 0;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        public int CastSpace(Polygon shape, BitFlag collisionMask)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            int count = 0;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        public int CastSpace(Polyline shape, BitFlag collisionMask)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            int count = 0;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    
                    if (candidate.Overlap(shape))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        
        
        
        public void SortCastResult(ref List<Collider> result, Vector2 sortOrigin)
        {
            if (result.Count > 1)
            {
                result.Sort
                (
                    (a, b) =>
                    {
                        float la = (sortOrigin - a.CurTransform.Position).LengthSquared();
                        float lb = (sortOrigin - b.CurTransform.Position).LengthSquared();

                        if (la > lb) return 1;
                        if (ShapeMath.EqualsF(la, lb)) return 0;
                        return -1;
                    }
                );
            }
        }

        
        
        public void DebugDraw(ColorRgba border, ColorRgba fill)
        {
            spatialHash.DebugDraw(border, fill);
        }

        public static HashSet<CollisionObject>? GetParents(List<Collider> colliders)
        {
            if (colliders.Count <= 0) return null;
            
            var parents = new HashSet<CollisionObject>();
            
            foreach (var collider in colliders)
            {
                var parent = collider.Parent;
                if (parent != null)
                {
                    parents.Add(parent);
                }
            }

            return parents;
        }
        
    }

}

  
        /*
        private void ProcessCollisionsBucketFirst()
        {
            int bucketCount = spatialHash.GetBucketCount();
            for (int i = 0; i < bucketCount; i++)
            {
                var bucketInfo = spatialHash.GetBucketInfo(i);
                if (!bucketInfo.Valid) continue;

                foreach (var collidable in bucketInfo.Active)
                {
                    var others = bucketInfo.GetOthers(collidable);

                    bool computeIntersections = collidable.GetCollider().ComputeIntersections;
                    List<Collision<TCollidable>> cols = new();
                    foreach (var other in others)
                    {
                        //IterationsPerFrame++;
                        if (activeRegister.HasEntry(collidable, other)) continue;

                        bool overlap = SGeometry.Overlap(collidable, other);
                        if (overlap)
                        {
                            bool firstContact = !oldRegister.RemoveEntry(collidable, other);
                            activeRegister.AddEntry(collidable, other);
                            if (computeIntersections)
                            {
                                //CollisionChecksPerFrame++;
                                var collisionPoints = SGeometry.Intersect(collidable, other);

                                //shapes overlap but no collision points means collidable is completely inside other
                                //closest point on bounds of other are now used for collision point
                                if (collisionPoints.Count <= 0)
                                {
                                    Vector2 refPoint = collidable.GetCollider().GetPreviousPosition();
                                    var shape = other.GetCollider().GetShape();
                                    if (!shape.IsPointInside(refPoint))
                                    {
                                        CollisionPoint closest = shape.GetClosestPoint(refPoint);
                                        collisionPoints.Add(closest);
                                        //ClosestPointChecksPerFrame++;
                                    }

                                }

                                Collision<TCollidable> c = new(collidable, other, firstContact, collisionPoints);
                                cols.Add(c);
                            }
                            else
                            {
                                Collision<TCollidable> c = new(collidable, other, firstContact);
                                cols.Add(c);
                            }
                        }
                    }

                    if (cols.Count > 0)
                    {
                        if (collisionStack.ContainsKey(collidable))
                        {
                            collisionStack[collidable].AddRange(cols);
                        }
                        else collisionStack.Add(collidable, cols);
                    }
                }
                //IterationsPerFrame++;
            }

        }
        */
        
