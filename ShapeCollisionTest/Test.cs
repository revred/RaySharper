﻿using ShapeEngineCore;
using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.UI;
using ShapeCollision;
using ShapeLib;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;
using System.ComponentModel;

namespace ShapeCollisionTest
{
    public class Test
    {
        public virtual void Update(float dt, Vector2 mousePos) { }
        public virtual void Draw(Vector2 mousePos) 
        { 
        }
        public virtual void Close() { }
        public virtual void Start() { }
    }

    public class TestStart : Test
    {
        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_SPACE))
            {
                Program.ChangeTest(new LaserTest());
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            UIHandler.DrawTextAligned("Press Space", new(1920/2, 1080/2, 1000, 500), 15, Raylib.WHITE, Alignement.CENTER);
        }
    }

    //closest point test
    public class Test1 : Test
    {
        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, 100);
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(150, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(8, new(0f), 50, 100));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, 100);
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 100, 200));
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test1()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);

            staticColliders[staIndex].DebugDrawShape(Raylib.YELLOW);
            dynamicColliders[dynIndex].DebugDrawShape(Raylib.GREEN);
            //Vector2 p = SClosestPoint.ClosestPoint(staticColliders[staIndex], dynamicColliders[dynIndex]);
            //Vector2 p2 = SClosestPoint.ClosestPoint(dynamicColliders[dynIndex], staticColliders[staIndex]);
            //Raylib.DrawCircleV(p, 10f, new Color(255, 0, 0, 150));
            //Raylib.DrawCircleV(p2, 10f, new Color(0, 0, 255, 150));
        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }
    
    //intersection test
    public class Test2 : Test
    {
        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, SRNG.randF(50, 150));
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 50, 250));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, SRNG.randF(200, 300));
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, new() { new Vector2(1, 0) * 100f, new Vector2(-0.5f, -0.5f) * 100f, new Vector2(-0.5f, 0.5f) * 100f });
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test2()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);

            staticColliders[staIndex].DebugDrawShape(Raylib.YELLOW);
            dynamicColliders[dynIndex].DebugDrawShape(Raylib.GREEN);
            //var points = SGeometry.Intersect(staticColliders[staIndex], dynamicColliders[dynIndex]);
            //foreach (var p in points)
            //{
            //    Raylib.DrawCircleV(p, 10f, new Color(255, 0, 0, 150));
            //}
            
        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }
    
    //contains test
    public class Test3 : Test
    {
        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, SRNG.randF(50, 100));
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 50, 150));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, SRNG.randF(200, 300));
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(500, 500), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 250, 400));
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test3()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);

            bool contains = false;// SContains.Contains(staticColliders[staIndex], dynamicColliders[dynIndex]);
            staticColliders[staIndex].DebugDrawShape(Raylib.YELLOW);
            dynamicColliders[dynIndex].DebugDrawShape(contains ? Raylib.RED : Raylib.GREEN);

        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }

    //overlap test
    public class Test4 : Test
    {
        internal class Collidable : ICollidable
        {
            Collider collider;
            public Collidable(Collider collider)
            {
                this.collider = collider;
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "all";
            }

            public string[] GetCollisionMask()
            {
                return new string[] { "all" };
            }

            public string GetID()
            {
                return "test";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }
            public void Overlap(CollisionInfo info)
            {
                return;
            }
        }

        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, SRNG.randF(50, 100));
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 50, 150));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, SRNG.randF(200, 300));
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(500, 500), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 250, 400));
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test4()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);
            Collidable a = new(staticColliders[staIndex]);
            Collidable b = new(dynamicColliders[dynIndex]);
            var info = SGeometry.GetCollisionInfo(a, b);
            staticColliders[staIndex].DebugDrawShape(Raylib.BLUE);

            Color color = Raylib.GREEN;
            if(info.overlapping) color = Raylib.ORANGE;
            dynamicColliders[dynIndex].DebugDrawShape(color);

            //foreach (var p in info.intersectionPoints)
            //{
            //    Raylib.DrawCircleV(p, 5f, Raylib.RED);
            //}

        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }
    public class Test4a : Test
    {
        internal class Collidable : ICollidable
        {
            Collider collider;
            public Collidable(Collider collider)
            {
                this.collider = collider;
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "all";
            }

            public string[] GetCollisionMask()
            {
                return new string[] { "all" };
            }

            public string GetID()
            {
                return "test";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }
            public void Overlap(CollisionInfo info)
            {
                return;
            }
        }

        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, SRNG.randF(50, 100));
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 50, 150));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, SRNG.randF(200, 300));
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(500, 500), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 250, 400));
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test4a()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);
            Collidable a = new(staticColliders[staIndex]);
            Collidable b = new(dynamicColliders[dynIndex]);
            var info = SGeometry.GetCollisionInfo(b, a);
            staticColliders[staIndex].DebugDrawShape(Raylib.BLUE);

            Color color = Raylib.GREEN;
            if (info.overlapping) color = Raylib.ORANGE;
            dynamicColliders[dynIndex].DebugDrawShape(color);

            if (info.intersection.valid)
            {
                Raylib.DrawCircleV(info.intersection.p, 5f, Raylib.RED);
                Raylib.DrawLineEx(info.intersection.p, info.intersection.p + info.intersection.n * 300, 2f, Raylib.RED);

                foreach (var intersections in info.intersection.points)
                {
                    Raylib.DrawCircleV(intersections.p, 5f, new(200, 0, 0, 200));
                    Raylib.DrawLineEx(intersections.p, intersections.p + intersections.n * 300, 2f, new(200, 0, 0, 200));
                }
            }

        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }
    //stress test
    public class Test5 : Test
    {
        internal class Collidable : ICollidable
        {
            Collider collider;
            public Collidable(Collider collider)
            {
                this.collider = collider;
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "all";
            }

            public string[] GetCollisionMask()
            {
                return new string[] { "all" };
            }

            public string GetID()
            {
                return "test";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }
            public void Overlap(CollisionInfo info)
            {
                return;
            }
        }

        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, SRNG.randF(50, 100));
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 50, 150));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        List<Collidable> collidables = new();
        int staIndex = 0;

        Vector2 lastSpawnPos = new(0f);

        public Test5()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                if (SpawnCollidable(mousePos))
                {
                    lastSpawnPos = mousePos;
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                SpawnCollidable(mousePos, 1);
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= 5) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            UIHandler.DrawTextAligned(String.Format("Objs: {0}", collidables.Count), new(5, 55, 150, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            Collidable dyn = new(dynamicColliders[dynIndex]);
            dynamicColliders[dynIndex].DebugDrawShape(Raylib.BLUE);
            foreach (var col in collidables)
            {
                var info = SGeometry.GetCollisionInfo(dyn, col);
                Color color = Raylib.GREEN;
                if (info.overlapping) color = Raylib.ORANGE;
                col.GetCollider().DebugDrawShape(color);
                //foreach (var p in info.intersectionPoints)
                //{
                //    Raylib.DrawCircleV(p, 7f, Raylib.RED);
                //}
            }

            DrawSign(staIndex, new Vector2(100, 150), 50, Raylib.WHITE);
        }

        private bool SpawnCollidable(Vector2 pos, int count = 1)
        {
            float minSpawnDis = 25;
            if((pos - lastSpawnPos).LengthSquared() > minSpawnDis * minSpawnDis)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector2 randPos = pos + SRNG.randVec2(50);
                    Collidable c = new(GetCollider(staIndex, randPos));
                    collidables.Add(c);
                }
                return true;
            }
            return false;
        }

        private Collider GetCollider(int index, Vector2 pos)
        {
            if (index == 0) return new Collider(pos.X, pos.Y);
            else if (index == 1) return new CircleCollider(pos, SRNG.randF(50, 100));
            else if (index == 2) return new SegmentCollider(pos, pos + SRNG.randVec2(100, 500));
            else if (index == 3) return new RectCollider(pos, new Vector2(SRNG.randF(50, 100), SRNG.randF(50, 100)), new(0.5f, 0.5f));
            else if (index == 4) return new PolyCollider(pos, SPoly.GeneratePolygon(12, new(0f), 50, 100));
            else return new Collider(pos.X, pos.Y);
        }

        private void DrawSign(int index, Vector2 pos, float size, Color color)
        {
            if (index == 0)
            {
                Raylib.DrawCircleV(pos, size / 5, color);
            }
            else if (index == 1)
            {
                Raylib.DrawCircleV(pos, size, color);
            }
            else if (index == 2)
            {
                Raylib.DrawLineEx(pos - new Vector2(size/2,0f), pos + new Vector2(size/2, 0), size / 10, color);
            }
            else if (index == 3)
            {
                Raylib.DrawRectangleRec(new(pos.X - size / 2, pos.Y - size /2, size, size), color);
            }
            else if (index == 4)
            {
                Drawing.DrawPolygon(new() { pos + new Vector2(size, 0), pos + new Vector2(-size, -size / 2), pos + new Vector2(-size, size/ 2) }, size / 10f, color);
            }
            
        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }

    //hash test
    public class Test6 : Test
    {
        internal interface GameObject : ICollidable
        {
            public bool IsDead();
            public void Kill();
            public void Draw(Vector2 mousePos);
            public void Update(float dt, Vector2 mousePos);
        }
        internal class Wall : GameObject
        {
            public SegmentCollider collider;
            string[] collisionMask = new string[0];
            
            public Wall(Vector2 start, Vector2 end, params string[] collisionMask)
            {
                this.collider = new(start, end);
                this.collisionMask = collisionMask;
                this.collider.CheckCollision = false;
            }

            public void Draw(Vector2 mousePos)
            {
                collider.DebugDrawShape(Raylib.WHITE);
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "walls";
            }

            public string[] GetCollisionMask()
            {
                return collisionMask;
            }

            public string GetID()
            {
                return "wall";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }

            public bool IsDead()
            {
                return false;
            }

            public void Kill()
            {
                return;
            }

            public void Overlap(CollisionInfo info)
            {
                return;
            }

            public void Update(float dt, Vector2 mousePos)
            {
                return;
            }
        }
        internal class Ball : GameObject
        {
            public CircleCollider collider;
            string[] collisionMask = new string[0];

            public Ball(Vector2 pos, float r, Vector2 vel, params string[] collisionMask)
            {
                this.collider = new(pos, vel, r);
                this.collisionMask = collisionMask;
                this.collider.CheckCollision = true;
                this.collider.CheckIntersections = true;
            }

            public void Draw(Vector2 mousePos)
            {
                collider.DebugDrawShape(Raylib.WHITE);
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "balls";
            }

            public string[] GetCollisionMask()
            {
                return collisionMask;
            }

            public string GetID()
            {
                return "ball";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }

            public bool IsDead()
            {
                return false;
            }

            public void Kill()
            {
                return;
            }

            public void Overlap(CollisionInfo info)
            {
               if (info.overlapping && info.other != null && info.intersection.valid)
               {
                   collider.Vel = SVec.Reflect(collider.Vel, info.intersection.n);
               }
            }

            public void Update(float dt, Vector2 mousePos)
            {
                collider.ApplyAccumulatedForce(dt);
                collider.Pos += collider.Vel * dt;
            }
        }
        internal class Box : GameObject
        {
            RectCollider collider;
            string[] collisionMask = new string[0];
            Color baseColor = Raylib.WHITE;
            Color overlapColor = Raylib.ORANGE;
            Color curColor = Raylib.WHITE;
            public Box(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 vel, params string[] collisionMask)
            {
                this.collider = new(pos, vel, size, alignement);
                this.collisionMask = collisionMask;
            }

            public void Draw(Vector2 mousePos)
            {
                collider.DebugDrawShape(curColor);
                curColor = baseColor;
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "boxes";
            }

            public string[] GetCollisionMask()
            {
                return collisionMask;
            }

            public string GetID()
            {
                return "box";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }

            public bool IsDead()
            {
                return false;
            }

            public void Kill()
            {
                return;
            }

            public void Overlap(CollisionInfo info)
            {
                if(info.overlapping && info.other != null)
                {
                    curColor = overlapColor;
                }
            }

            public void Update(float dt, Vector2 mousePos)
            {
                collider.ApplyAccumulatedForce(dt);
                collider.Pos += collider.Vel * dt;
            }
        }
        internal class Poly : GameObject
        {
            PolyCollider collider;
            string[] collisionMask = new string[0];
            //Vector2 prevPos = new(0f);
            //float prevRot = 0f;
            
            public Poly(Vector2 pos, List<Vector2> points, Vector2 vel, params string[] collisionMask)
            {
                this.collider = new(pos, points, SRNG.randAngleRad());
                this.collisionMask = collisionMask;
                this.collider.Vel = vel;
                this.collider.CheckIntersections = true;
            }

            public void Draw(Vector2 mousePos)
            {
                collider.DebugDrawShape(Raylib.BLUE);
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "polies";
            }

            public string[] GetCollisionMask()
            {
                return collisionMask;
            }

            public string GetID()
            {
                return "poly";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }

            public bool IsDead()
            {
                return false;
            }

            public void Kill()
            {
                return;
            }

            public void Overlap(CollisionInfo info)
            {
                if (info.overlapping && info.other != null && info.intersection.valid)
                {
                    //collider.Vel = SVec.RotateDeg(collider.Vel, 180 + SRNG.randF(-25, 25));
                    //collider.Vel = new(0f);
                    collider.Vel = SVec.Reflect(collider.Vel, info.intersection.n);
                    //collider.Pos = prevPos;
                    //collider.RotRad = prevRot;
                }
            }

            public void Update(float dt, Vector2 mousePos)
            {
                //prevPos = collider.Pos;
                //prevRot = collider.RotRad;
                collider.ApplyAccumulatedForce(dt);
                collider.Pos += collider.Vel * dt;
                
                if(collider.Vel.X > 0 || collider.Vel.Y > 0)
                    collider.RotRad += 0.5f * dt;
            }
        }

        CollisionHandler ch = new(0, 0, 1920, 1080, 10, 10);
        List<GameObject> gameObjects = new();
        List<GameObject> persistent = new();
        public Test6()
        {
            float offset = 100;
            Wall top = new(new Vector2(offset, offset), new Vector2(1920 - offset, offset));
            Wall bottom = new(new Vector2(offset, 1080 - offset), new Vector2(1920 - offset, 1080 - offset));
            Wall left = new(new Vector2(offset, offset), new Vector2(offset, 1080 - offset));
            Wall right = new(new Vector2(1920 - offset, offset), new Vector2(1920 - offset, 1080 - offset));
            
            persistent.Add(top);
            persistent.Add(bottom);
            persistent.Add(left);
            persistent.Add(right);
            
            ch.AddRange(persistent.ToList<ICollidable>());
            gameObjects.AddRange(persistent);
        }
        public void Restart()
        {
            ch.Clear();
            gameObjects.Clear();

            ch.AddRange(persistent.ToList<ICollidable>());
            gameObjects.AddRange(persistent);
        }
        public override void Update(float dt, Vector2 mousePos)
        {
            if(Raylib.IsKeyReleased(KeyboardKey.KEY_R))
            {
                Restart();
                return;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_ONE))
            {
                Ball b = new(mousePos, SRNG.randF(15, 15), SRNG.randVec2(1000, 1100), "walls", "balls", "polies");
                gameObjects.Add(b);
                ch.Add(b);
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO))
            {
                Box b = new(mousePos, new Vector2(SRNG.randF(50, 150), SRNG.randF(50, 150)), new(0.5f, 0.5f), new(0f), "balls", "polies");
                gameObjects.Add(b);
                ch.Add(b);
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE))
            {
                var points = SPoly.GeneratePolygon(SRNG.randI(5, 12), new(0f), 25, 100);
                Poly p = new(mousePos, points, SRNG.randVec2(25, 100), "balls", "walls", "polies");
                gameObjects.Add(p);
                ch.Add(p);
            }

            
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                var go = gameObjects[i];
                go.Update(dt, mousePos);
                if(go.IsDead()) gameObjects.RemoveAt(i);
            }
            ch.Update(dt);
        }

        public override void Draw(Vector2 mousePos)
        {
            ch.DebugDrawGrid(new(200, 0, 0, 200), new(100, 100, 100, 100));
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                var go = gameObjects[i];
                go.Draw(mousePos);
            }
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            UIHandler.DrawTextAligned(String.Format("Objs: {0}", gameObjects.Count), new(5, 55, 150, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
        }

        public override void Close()
        {
            ch.Close();
        }
    }

    public class LaserTest : Test
    {
        internal interface GameObject : ICollidable
        {
            public bool IsDead();
            public void Kill();
            public void Draw(Vector2 mousePos);
            public void Update(float dt, Vector2 mousePos);
        }
        internal class Wall : GameObject
        {
            public SegmentCollider collider;
            string[] collisionMask = new string[0];

            public Wall(Vector2 start, Vector2 end, params string[] collisionMask)
            {
                this.collider = new(start, end);
                this.collisionMask = collisionMask;
                this.collider.CheckCollision = false;
            }

            public void Draw(Vector2 mousePos)
            {
                collider.DebugDrawShape(Raylib.WHITE);
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "walls";
            }

            public string[] GetCollisionMask()
            {
                return collisionMask;
            }

            public string GetID()
            {
                return "wall";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }

            public bool IsDead()
            {
                return false;
            }

            public void Kill()
            {
                return;
            }

            public void Overlap(CollisionInfo info)
            {
                return;
            }

            public void Update(float dt, Vector2 mousePos)
            {
                return;
            }
        }
        
        internal class Enemy : GameObject
        {
            string[] collisionMask = new string[0];
            bool dead = false;
            protected float slowResistance = 1f;
            public Enemy(params string[] collisionMask)
            {
                this.collisionMask = collisionMask;
            }

            public virtual void Draw(Vector2 mousePos)
            {
                //GetCollider().DebugDrawShape(Raylib.WHITE);
            }

            public virtual Collider GetCollider()
            {
                return new();
            }

            public string GetCollisionLayer()
            {
                return "enemies";
            }

            public string[] GetCollisionMask()
            {
                return collisionMask;
            }

            public string GetID()
            {
                return "enemy";
            }

            public Vector2 GetPos()
            {
                return GetCollider().Pos;
            }

            public bool IsDead()
            {
                return dead;
            }

            public void Kill()
            {
                if (dead) return;
                dead = true;
            }

            public void Damage(float amount)
            {
                var col = GetCollider();
                float speed = col.Vel.Length();
                speed -= amount;
                if(amount <= 0)
                {
                    col.Vel = new(0f);
                    Kill();
                }
                else col.Vel = SVec.Normalize(col.Vel) * speed;
            }

            public void Overlap(CollisionInfo info)
            {
                if (dead || GetCollider().Vel.LengthSquared() <= 0f) return;
                if (info.overlapping)
                {
                    if(info.other != null)
                    {
                        if (info.other is Wall) Kill();
                        //else
                        //{
                        //    if (info.intersection.valid)
                        //    {
                        //        GetCollider().Vel = SVec.Reflect(GetCollider().Vel, info.intersection.n);
                        //    }
                        //}
                    }
                }
                
            }

            public void Update(float dt, Vector2 mousePos)
            {
                GetCollider().ApplyAccumulatedForce(dt);
                GetCollider().Pos += GetCollider().Vel * dt;
            }
        }
        internal class Enemy1 : Enemy
        {
            public CircleCollider collider;

            public Enemy1(Vector2 pos, Vector2 vel, params string[] collisionMask) : base(collisionMask)
            {
                this.collider = new(pos, vel, SRNG.randF(10, 30));
                this.collider.CheckCollision = true;
                this.collider.CheckIntersections = true;
                this.slowResistance = 2f;
            }

            public override void Draw(Vector2 mousePos)
            {
                collider.DebugDrawShape(Raylib.WHITE);
            }

            public override Collider GetCollider()
            {
                return collider;
            }
        }
        internal class Enemy2 : Enemy
        {
            PolyCollider collider;

            public Enemy2(Vector2 pos, Vector2 vel, params string[] collisionMask) : base(collisionMask)
            {
                var points = SPoly.GeneratePolygon(SRNG.randI(5, 10), new(0f), 20, 40);
                this.collider = new(pos, points, SRNG.randAngleRad());
                this.collider.Vel = vel;
                this.collider.CheckIntersections = true;
                this.collider.CheckCollision = true;
                this.slowResistance = 0.25f;
            }

            public override void Draw(Vector2 mousePos)
            {
                collider.DebugDrawShape(Raylib.BLUE);
            }

            public override Collider GetCollider()
            {
                return collider;
            }

        }


        CollisionHandler ch = new(0, 0, 1920, 1080, 10, 10);
        List<GameObject> gameObjects = new();
        List<GameObject> persistent = new();
        float spawnTime = 1f;
        float timer = 0f;
        Vector2 laserStart = new(150, 540);
        Vector2 laserDir = new(1, 0);
        float laserLength = 1000f;
        Vector2 laserEnd = new(0f);
        Vector2 laserOff = new();
        public LaserTest()
        {
            float offset = 100;
            Wall left = new(new Vector2(offset, -offset), new Vector2(offset, 1080 + offset));
            //Wall top = new(new Vector2(offset, offset), new Vector2(1920 - offset, offset));
            //Wall bottom = new(new Vector2(offset, 1080 - offset), new Vector2(1920 - offset, 1080 - offset));
            //Wall right = new(new Vector2(1920 - offset, offset), new Vector2(1920 - offset, 1080 - offset));

            persistent.Add(left);

            ch.AddRange(persistent.ToList<ICollidable>());
            gameObjects.AddRange(persistent);

            timer = spawnTime;
        }
        public void Restart()
        {
            ch.Clear();
            gameObjects.Clear();

            ch.AddRange(persistent.ToList<ICollidable>());
            gameObjects.AddRange(persistent);
        }
        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_R))
            {
                Restart();
                return;
            }

            if(timer > 0f)
            {
                timer -= dt;
                if(timer <= 0f)
                {
                    timer = spawnTime - MathF.Abs(timer);
                    SpawnEnemy();
                }
            }

            laserDir = SVec.Normalize(mousePos - laserStart);
            var query = ch.QuerySpace(laserStart, laserDir, laserLength, true, "enemies");
            if(query.Count <= 0)
            {
                laserEnd = laserStart + laserDir * laserLength;
                laserOff = new(0f);
            }
            else
            {
                QueryInfo closest = query[0];
                float closestDisSq = float.PositiveInfinity;
                foreach (var q in query)
                {
                    float disSq = (q.collidable.GetPos() - laserStart).LengthSquared();
                    if (disSq < closestDisSq)
                    {
                        closestDisSq = disSq;
                        closest = q;
                    }
                }

                if (closest.intersection.valid)
                {
                    laserEnd = closest.intersection.p;
                    float lengthSq = (laserEnd - laserStart).LengthSquared();

                    float p = 1f - (lengthSq / (laserLength * laserLength));
                    float remaining = laserLength * p;
                    laserOff = SVec.Reflect(laserDir, closest.intersection.n) * remaining;
                }
                else
                {
                    laserEnd = laserStart + laserDir * laserLength;
                    laserOff = new(0f);
                }
            }
            

            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                var go = gameObjects[i];
                go.Update(dt, mousePos);
                if (go.IsDead()) gameObjects.RemoveAt(i);
            }
            ch.Update(dt);
        }

        private void SpawnEnemy()
        {
            Rectangle rect = new(1600, 100, 220, 980);
            Vector2 spawnPoint = SRNG.randPoint(rect);
            
            Enemy e;
            if (SRNG.randF() < 0.2f)
            {
                Vector2 randVel = new Vector2(-SRNG.randF(10, 20), 0f);
                e = new Enemy2(spawnPoint, randVel, "walls");
            }
            else
            {
                Vector2 randVel = new Vector2(-SRNG.randF(40, 100), 0f);
                e = new Enemy1(spawnPoint, randVel, "walls");
                
            }
            gameObjects.Add(e);
            ch.Add(e);
        }
        public override void Draw(Vector2 mousePos)
        {
            ch.DebugDrawGrid(new(200, 0, 0, 200), new(100, 100, 100, 100));
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                var go = gameObjects[i];
                go.Draw(mousePos);
            }
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            UIHandler.DrawTextAligned(String.Format("Objs: {0}", gameObjects.Count), new(5, 55, 150, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);

            Raylib.DrawLineEx(laserStart, laserEnd, 5f, Raylib.RED);
            if (laserOff.LengthSquared() > 0f) Raylib.DrawLineEx(laserEnd, laserEnd + laserOff, 3f, Raylib.RED);
        }

        public override void Close()
        {
            ch.Close();
        }
    }

}
