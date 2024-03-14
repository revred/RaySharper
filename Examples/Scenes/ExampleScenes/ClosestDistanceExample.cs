using System.Diagnostics;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Text;
using Color = System.Drawing.Color;
namespace Examples.Scenes.ExampleScenes;




public class ClosestDistanceExample : ExampleScene
{
    private const float LineThickness = 4f;
    
    private abstract class Shape
    {
        public abstract void Move(Vector2 newPosition);
        public abstract void Draw(ColorRgba color);
        public abstract ShapeType GetShapeType();
        public abstract ClosestDistance GetClosestDistanceTo(Shape shape);

        public string GetName()
        {
            switch (GetShapeType())
            {
                case ShapeType.None: return "Point";
                case ShapeType.Circle: return "Circle";
                case ShapeType.Segment: return "Segment";
                case ShapeType.Triangle: return "Triangle";
                case ShapeType.Quad: return "Quad";
                case ShapeType.Rect: return "Rect";
                case ShapeType.Poly: return "Poly";
                case ShapeType.PolyLine: return "Polyline";
            }

            return "Invalid Shape";
        }
    }

    private class PointShape : Shape
    {
        public Vector2 Position;
        private float size;
        public PointShape(Vector2 pos, float size)
        {
            this.Position = pos;
            this.size = size;
        }
        public override void Move(Vector2 newPosition)
        {
            Position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            Position.Draw(size, color, 16);
        }

        public override ShapeType GetShapeType() => ShapeType.None;
        
        public override ClosestDistance GetClosestDistanceTo(Shape shape)
        {
            return new();
        }
    }
    private class SegmentShape : Shape
    {
        public Segment Segment;
        private Vector2 position;
        public SegmentShape(Vector2 pos, float size)
        {
            position = pos;
            var randAngle = ShapeRandom.RandAngleRad();
            var offset = new Vector2(size, 0f).Rotate(randAngle);
            var start = pos - offset;
            var end = pos + offset;
            Segment = new(start, end);
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            Segment = Segment.Move(offset);
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            Segment.Draw(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Segment;
        
        public override ClosestDistance GetClosestDistanceTo(Shape shape)
        {
            if (shape is PointShape pointShape) return Segment.GetClosestDistanceTo(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Segment.GetClosestDistanceTo(segmentShape.Segment);
            if (shape is CircleShape circleShape) return Segment.GetClosestDistanceTo(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Segment.GetClosestDistanceTo(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Segment.GetClosestDistanceTo(quadShape.Quad);
            if (shape is RectShape rectShape) return Segment.GetClosestDistanceTo(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Segment.GetClosestDistanceTo(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Segment.GetClosestDistanceTo(polylineShape.Polyline);
            return new();
        }
    }
    private class CircleShape : Shape
    {
        public Circle Circle;
        public CircleShape(Vector2 pos, float size)
        {
            Circle = new(pos, size);
        }
        public override void Move(Vector2 newPosition)
        {
            Circle = new(newPosition, Circle.Radius);
        }

        public override void Draw(ColorRgba color)
        {
            Circle.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Circle;
        
        public override ClosestDistance GetClosestDistanceTo(Shape shape)
        {
            if (shape is PointShape pointShape) return Circle.GetClosestDistanceTo(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Circle.GetClosestDistanceTo(segmentShape.Segment);
            if (shape is CircleShape circleShape) return Circle.GetClosestDistanceTo(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Circle.GetClosestDistanceTo(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Circle.GetClosestDistanceTo(quadShape.Quad);
            if (shape is RectShape rectShape) return Circle.GetClosestDistanceTo(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Circle.GetClosestDistanceTo(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Circle.GetClosestDistanceTo(polylineShape.Polyline);
            return new();
        }
    }
    private class TriangleShape : Shape
    {
        private Vector2 position;
        public Triangle Triangle;

        public TriangleShape(Vector2 pos, float size)
        {
            position = pos;
            var randAngle = ShapeRandom.RandAngleRad();
            var a = pos + new Vector2(size * ShapeRandom.RandF(0.5f, 1f), size * ShapeRandom.RandF(-0.5f, 0.5f)).Rotate(randAngle);
            var b = pos + new Vector2(-size * ShapeRandom.RandF(0.5f, 1f), -size * ShapeRandom.RandF(0.5f, 1f)).Rotate(randAngle);
            var c = pos + new Vector2(-size * ShapeRandom.RandF(0.5f, 1f), size * ShapeRandom.RandF(0.5f, 1f)).Rotate(randAngle);
            Triangle = new(a, b, c);
        }

        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            Triangle = Triangle.Move(offset);
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            Triangle.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Triangle;
        
        public override ClosestDistance GetClosestDistanceTo(Shape shape)
        {
            if (shape is PointShape pointShape) return Triangle.GetClosestDistanceTo(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Triangle.GetClosestDistanceTo(segmentShape.Segment);
            if (shape is CircleShape circleShape) return Triangle.GetClosestDistanceTo(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Triangle.GetClosestDistanceTo(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Triangle.GetClosestDistanceTo(quadShape.Quad);
            if (shape is RectShape rectShape) return Triangle.GetClosestDistanceTo(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Triangle.GetClosestDistanceTo(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Triangle.GetClosestDistanceTo(polylineShape.Polyline);
            return new();
        }
    }
    private class QuadShape : Shape
    {
        public Quad Quad;
        public QuadShape(Vector2 pos, float size)
        {
            var randAngle = ShapeRandom.RandAngleRad();
            Quad = new(pos, new Vector2(size), randAngle, new Vector2(0.5f));
        }
        public override void Move(Vector2 newPosition)
        {
            Quad = Quad.MoveTo(newPosition, new Vector2(0.5f));
        }

        public override void Draw(ColorRgba color)
        {
           Quad.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Quad;
        
        public override ClosestDistance GetClosestDistanceTo(Shape shape)
        {
            // if (shape is PointShape pointShape) return Quad.GetClosestDistanceTo(pointShape.Position);
            // if (shape is SegmentShape segmentShape) return Quad.GetClosestDistanceTo(segmentShape.Segment);
            // if (shape is CircleShape circleShape) return Quad.GetClosestDistanceTo(circleShape.Circle);
            // if (shape is TriangleShape triangleShape) return Quad.GetClosestDistanceTo(triangleShape.Triangle);
            // if (shape is QuadShape quadShape) return Quad.GetClosestDistanceTo(quadShape.Quad);
            // if (shape is RectShape rectShape) return Quad.GetClosestDistanceTo(rectShape.Rect);
            // if (shape is PolygonShape polygonShape) return Quad.GetClosestDistanceTo(polygonShape.Polygon);
            // if (shape is PolylineShape polylineShape) return Quad.GetClosestDistanceTo(polylineShape.Polyline);
            return new();
        }
    }
    private class RectShape : Shape
    {
        public Rect Rect;

        public RectShape(Vector2 pos, float size)
        {
            Rect = new(pos, new(size, size), new Vector2(0.5f));
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - Rect.Center;
            Rect = Rect.Move(offset);
        }

        public override void Draw(ColorRgba color)
        {
            Rect.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Rect;
        
        public override ClosestDistance GetClosestDistanceTo(Shape shape)
        {
            // if (shape is PointShape pointShape) return Rect.GetClosestDistanceTo(pointShape.Position);
            // if (shape is SegmentShape segmentShape) return Rect.GetClosestDistanceTo(segmentShape.Segment);
            // if (shape is CircleShape circleShape) return Rect.GetClosestDistanceTo(circleShape.Circle);
            // if (shape is TriangleShape triangleShape) return Rect.GetClosestDistanceTo(triangleShape.Triangle);
            // if (shape is QuadShape quadShape) return Rect.GetClosestDistanceTo(quadShape.Quad);
            // if (shape is RectShape rectShape) return Rect.GetClosestDistanceTo(rectShape.Rect);
            // if (shape is PolygonShape polygonShape) return Rect.GetClosestDistanceTo(polygonShape.Polygon);
            // if (shape is PolylineShape polylineShape) return Rect.GetClosestDistanceTo(polylineShape.Polyline);
            return new();
        }
    }
    private class PolygonShape : Shape
    {
        private Vector2 position;
        public readonly Polygon Polygon;

        public PolygonShape(Vector2 pos, float size)
        {
            Polygon = Polygon.Generate(pos, ShapeRandom.RandI(8, 16), size / 2, size);
            position = pos;
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            Polygon.MoveSelf(offset);
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            Polygon.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Poly;
        
        public override ClosestDistance GetClosestDistanceTo(Shape shape)
        {
            // if (shape is PointShape pointShape) return Polygon.GetClosestDistanceTo(pointShape.Position);
            // if (shape is SegmentShape segmentShape) return Polygon.GetClosestDistanceTo(segmentShape.Segment);
            // if (shape is CircleShape circleShape) return Polygon.GetClosestDistanceTo(circleShape.Circle);
            // if (shape is TriangleShape triangleShape) return Polygon.GetClosestDistanceTo(triangleShape.Triangle);
            // if (shape is QuadShape quadShape) return Polygon.GetClosestDistanceTo(quadShape.Quad);
            // if (shape is RectShape rectShape) return Polygon.GetClosestDistanceTo(rectShape.Rect);
            // if (shape is PolygonShape polygonShape) return Polygon.GetClosestDistanceTo(polygonShape.Polygon);
            // if (shape is PolylineShape polylineShape) return Polygon.GetClosestDistanceTo(polylineShape.Polyline);
            return new();
        }
    }
    private class PolylineShape : Shape
    {
        private Vector2 position;
        public readonly Polyline Polyline;

        public PolylineShape(Vector2 pos, float size)
        {
            
            Polyline = Polygon.Generate(pos, ShapeRandom.RandI(8, 16), size / 2, size).ToPolyline();
            position = pos;
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            for (var i = 0; i < Polyline.Count; i++)
            {
                var p = Polyline[i];
                Polyline[i] = p + offset;
            }
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            Polyline.Draw(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.PolyLine;
        
        public override ClosestDistance GetClosestDistanceTo(Shape shape)
        {
            // if (shape is PointShape pointShape) return Polyline.GetClosestDistanceTo(pointShape.Position);
            // if (shape is SegmentShape segmentShape) return Polyline.GetClosestDistanceTo(segmentShape.Segment);
            // if (shape is CircleShape circleShape) return Polyline.GetClosestDistanceTo(circleShape.Circle);
            // if (shape is TriangleShape triangleShape) return Polyline.GetClosestDistanceTo(triangleShape.Triangle);
            // if (shape is QuadShape quadShape) return Polyline.GetClosestDistanceTo(quadShape.Quad);
            // if (shape is RectShape rectShape) return Polyline.GetClosestDistanceTo(rectShape.Rect);
            // if (shape is PolygonShape polygonShape) return Polyline.GetClosestDistanceTo(polygonShape.Polygon);
            // if (shape is PolylineShape polylineShape) return Polyline.GetClosestDistanceTo(polylineShape.Polyline);
            return new();
        }
    }
    
    
    private InputAction nextStaticShape;
    private InputAction nextMovingShape;
    // private InputAction changeOffset;

    private Shape staticShape;
    private Shape movingShape;

    
    public ClosestDistanceExample()
    {
        Title = "Closest Distance Example";

        var nextStaticShapeMb = new InputTypeMouseButton(ShapeMouseButton.LEFT);
        var nextStaticShapeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
        var nextStaticShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
        nextStaticShape = new(nextStaticShapeMb, nextStaticShapeGp, nextStaticShapeKb);
        
        var nextMovingShapeMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
        var nextMovingShapeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
        var nextMovingShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
        nextMovingShape = new(nextMovingShapeMb, nextMovingShapeGp, nextMovingShapeKb);
        
        // var offsetMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f);
        // var offsetKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.S, ShapeKeyboardButton.W);
        // var offsetGP = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_DOWN, ShapeGamepadButton.LEFT_FACE_UP);
        // changeOffset = new(offsetMW, offsetGP, offsetKB);
        
        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Light;

        staticShape = CreateShape(new(), 150, ShapeType.Triangle);
        movingShape = CreateShape(new(), 50, ShapeType.Triangle);

    }
    public override void Reset()
    {
        
    }
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
    {
        base.HandleInput(dt, mousePosGame, mousePosUI);
        var gamepad = GAMELOOP.CurGamepad;
        
        nextStaticShape.Gamepad = gamepad;
        nextStaticShape.Update(dt);
        
        nextMovingShape.Gamepad = gamepad;
        nextMovingShape.Update(dt);
        
        // changeOffset.Gamepad = gamepad;
        // changeOffset.Update(dt);

        
        if (nextStaticShape.State.Pressed)
        {
            NextStaticShape();   
        }
        if (nextMovingShape.State.Pressed)
        {
            NextMovingShape(mousePosGame);   
        }
        movingShape.Move(mousePosGame);
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        staticShape.Draw(Colors.Highlight.ChangeBrightness(-0.3f));
        movingShape.Draw(Colors.Warm.ChangeBrightness(-0.3f));

        var closestDistance = staticShape.GetClosestDistanceTo(movingShape);
        if (closestDistance.DistanceSquared > 0)
        {
            var seg = closestDistance.GetSegment();
            seg.Draw(LineThickness / 2, Colors.Light);
            closestDistance.A.Draw(12f, Colors.Highlight);
            closestDistance.B.Draw(12f, Colors.Warm);
            
        }
        
        
    }
    protected override void OnDrawGameUIExample(ScreenInfo ui)
    {
        
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        var curDevice = ShapeInput.CurrentInputDeviceType;
        var nextStatic = nextStaticShape. GetInputTypeDescription( curDevice, true, 1, false); 
        var nextMoving = nextMovingShape. GetInputTypeDescription( curDevice, true, 1, false); 
        // var offset = changeOffset.GetInputTypeDescription( curDevice , true, 1, false);
        var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
        var hSplit = bottomCenter.SplitH(0.45f, 0.1f, 0.45f);
        var margin = bottomCenter.Height * 0.05f;
        var leftRect = hSplit[0];
        var middleRect = hSplit[1];
        var rightRect = hSplit[2];
        
        leftRect.DrawLines(2f, Colors.Highlight);
        rightRect.DrawLines(2f, Colors.Warm);
        // string infoText =
            // $"Add Point {create} | Remove Point {delete} | Inflate {offset} {MathF.Round(offsetDelta * 100) / 100}";

        var textStatic = $"{nextStatic} {staticShape.GetName()}";
        var textMiddle = " vs ";
        var textMoving = $"{movingShape.GetName()} {nextMoving}";
        
        textFont.ColorRgba = Colors.Highlight;
        textFont.DrawTextWrapNone(textStatic, leftRect.ApplyMarginsAbsolute(margin, margin, margin, margin), new(0f, 0.5f));
        textFont.ColorRgba = Colors.Light;
        textFont.DrawTextWrapNone(textMiddle, middleRect, new(0.5f));
        textFont.ColorRgba = Colors.Warm;
        textFont.DrawTextWrapNone(textMoving, rightRect.ApplyMarginsAbsolute(margin, margin, margin, margin), new(1f, 0.5f));
    }

    private void NextStaticShape(float size = 150f)
    {
        switch (staticShape.GetShapeType())
        {
            case ShapeType.None: staticShape = CreateShape(new(), size, ShapeType.Segment); //point
                break;
            case ShapeType.Segment: staticShape = CreateShape(new(), size, ShapeType.Circle);
                break;
            case ShapeType.Circle: staticShape = CreateShape(new(), size, ShapeType.Triangle);
                break;
            case ShapeType.Triangle: staticShape = CreateShape(new(), size, ShapeType.Quad);
                break;
            case ShapeType.Quad: staticShape = CreateShape(new(), size, ShapeType.Rect);
                break;
            case ShapeType.Rect: staticShape = CreateShape(new(), size, ShapeType.Poly);
                break;
            case ShapeType.Poly: staticShape = CreateShape(new(), size, ShapeType.PolyLine);
                break;
            case ShapeType.PolyLine: staticShape = CreateShape(new(), size / 4, ShapeType.None);
                break;
        }
    }
    private void NextMovingShape(Vector2 pos, float size = 150f)
    {
        switch (movingShape.GetShapeType())
        {
            case ShapeType.None: movingShape = CreateShape(pos, size, ShapeType.Segment); //point
                break;
            case ShapeType.Segment: movingShape = CreateShape(pos, size, ShapeType.Circle);
                break;
            case ShapeType.Circle: movingShape = CreateShape(pos, size, ShapeType.Triangle);
                break;
            case ShapeType.Triangle: movingShape = CreateShape(pos, size, ShapeType.Quad);
                break;
            case ShapeType.Quad: movingShape = CreateShape(pos, size, ShapeType.Rect);
                break;
            case ShapeType.Rect: movingShape = CreateShape(pos, size, ShapeType.Poly);
                break;
            case ShapeType.Poly: movingShape = CreateShape(pos, size, ShapeType.PolyLine);
                break;
            case ShapeType.PolyLine: movingShape = CreateShape(pos, size / 4, ShapeType.None);
                break;
        }
    }
    private Shape CreateShape(Vector2 pos, float size, ShapeType type)
    {
        switch (type)
        {
            case ShapeType.None: return new PointShape(pos, size);
            case ShapeType.Circle: return new CircleShape(pos, size);
            case ShapeType.Segment: return new SegmentShape(pos, size);
            case ShapeType.Triangle: return new TriangleShape(pos, size);
            case ShapeType.Quad: return new QuadShape(pos, size);
            case ShapeType.Rect: return new RectShape(pos, size);
            case ShapeType.Poly: return new PolygonShape(pos, size);
            case ShapeType.PolyLine: return new PolylineShape(pos, size);
        }
        
        return new PointShape(pos, size);
    }
    
}


