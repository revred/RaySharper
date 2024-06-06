﻿using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using System.Text;
using ShapeEngine.Color;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using Color = System.Drawing.Color;
using ShapeEngine.Core.Collision;

namespace Examples.Scenes.ExampleScenes
{
    internal class Pillar
    {
        Rect outline = new();
        Rect center = new();
        public Pillar(Vector2 pos, float size)
        {
            outline = new(pos, new Size(size), new Vector2(0.5f));
            center = outline.ScaleSize(0.5f, new Vector2(0.5f));
        }

        public void Draw()
        {
            var c = Colors.Warm; // new ColorRgba(Color.IndianRed);
            outline.DrawLines(4f, c);
            center.Draw(c);
        }
    }
    public class CameraExample : ExampleScene
    {
        ShapeCamera camera;
        Rect universe = new(new Vector2(0f), new Size(10000f), new Vector2(0.5f));

        List<Pillar> pillars = new();
        
        private readonly InputAction iaMoveCameraH;
        private readonly InputAction iaMoveCameraV;
        private readonly InputAction iaRotateCamera;
        private readonly List<InputAction> inputActions;
        
        public CameraExample()
        {
            Title = "Camera Example";

            var nextStaticShapeMb = new InputTypeMouseButton(ShapeMouseButton.MIDDLE);
            var nextStaticShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
            nextStaticShape = new(nextStaticShapeKb);
            staticShape = CreateShape(new(), 150);

            var cameraHorizontalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var cameraHorizontalGP = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            // var cameraHorizontalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            // var cameraHorizontalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraH = new(cameraHorizontalKB, cameraHorizontalGP);
            
            var cameraVerticalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var cameraVerticalGP = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            // var cameraVerticalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            // var cameraVerticalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraV = new(cameraVerticalKB, cameraVerticalGP);

            var rotateCameraKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.Q, ShapeKeyboardButton.E);
            var rotateCameraGP = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            var rotateCameraMb = new InputTypeMouseButtonAxis(ShapeMouseButton.LEFT, ShapeMouseButton.RIGHT, 0f);
                //new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouse);
            iaRotateCamera = new(rotateCameraKB, rotateCameraGP, rotateCameraMb);

            inputActions = new() { iaMoveCameraH, iaMoveCameraV, iaRotateCamera };
            
            camera = GAMELOOP.Camera;
            //boundaryRect = new(new Vector2(0, -45), new Vector2(1800, 810), new Vector2(0.5f));

            for (int i = 0; i < 0; i++)
            {
                //Vector2 pos = SRNG.randVec2(0, 5000);
                Vector2 pos = universe.GetRandomPointInside();
                float size = ShapeRandom.RandF(25, 100);
                Pillar p = new(pos, size);
                pillars.Add(p);
            }
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
        }

        public override void Reset()
        {
            camera.Reset();
        }
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            GAMELOOP.MouseControlEnabled = gamepad?.IsDown(ShapeGamepadAxis.RIGHT_TRIGGER, 0.1f) ?? true;
            InputAction.UpdateActions(dt, gamepad, inputActions);
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            // foreach (var ia in inputActions)
            // {
            //     ia.Gamepad = gamepadIndex;
            //     ia.Update(dt);
            // }
            HandleCameraPosition(dt);
            HandleCameraRotation(dt);
            nextStaticShape.Gamepad = gamepad;
            nextStaticShape.Update(dt);
            if (nextStaticShape.State.Pressed)
            {
                NextStaticShape();
            }
        }
        
        private void HandleCameraRotation(float dt)
        {
            var rotSpeedDeg = 90f;
            float rotDir = iaRotateCamera.State.AxisRaw ;
            // if (IsKeyDown(KeyboardKey.KEY_Q)) rotDir = -1;
            // else if (IsKeyDown(KeyboardKey.KEY_E)) rotDir = 1;

            if (rotDir != 0)
            {
                camera.Rotate(rotDir * rotSpeedDeg * dt);
            }
        }
        private void HandleCameraPosition(float dt)
        {
            float speed = 500;
            if (ShapeInput.CurrentInputDeviceType == InputDeviceType.Mouse)
            {
                var dir = ExampleScene.CalculateMouseMovementDirection(GAMELOOP.GameScreenInfo.MousePos, GAMELOOP.Camera);
                if (dir.X != 0 || dir.Y != 0)
                {
                    var movement = dir * speed * dt * camera.ZoomFactor;
                    movement = movement.Rotate(-camera.RotationDeg * ShapeMath.DEGTORAD);
                    camera.BasePosition += movement;
                    //camera.Translation += movement;
                }
            }
            else
            {
                float dirX = iaMoveCameraH.State.AxisRaw;
                float dirY = iaMoveCameraV.State.AxisRaw;

                if (dirX != 0 || dirY != 0)
                {
                    var movement = new Vector2(dirX, dirY).Normalize() * speed * dt * camera.ZoomFactor;
                    movement = movement.Rotate(-camera.RotationDeg * ShapeMath.DEGTORAD);
                    camera.BasePosition += movement;
                    //camera.Translation += movement;
                }
            }
            
            
        }
        
        protected override void OnDrawGameExample(ScreenInfo game)
        {
            foreach (var pillar in pillars)
            {
                pillar.Draw();
            }

            staticShape.Draw(Colors.Highlight.ChangeBrightness(-0.3f));

            var c = Colors.Cold; // new ColorRgba(Color.CornflowerBlue);
            float f = camera.ZoomFactor;
            //ShapeDrawing.DrawCircle(camera.BasePosition, 8f * f, c);
            //ShapeDrawing.DrawCircleLines(camera.BasePosition, 64 * f, 2f * f, c);
            //Segment hor = new(camera.BasePosition - new Vector2(3000 * f, 0), camera.BasePosition + new Vector2(3000 * f, 0));
           // hor.Draw(2f * f, c);
            //Segment ver = new(camera.BasePosition - new Vector2(0, 3000 * f), camera.BasePosition + new Vector2(0, 3000 * f));
           // ver.Draw(2f * f, c);
        }
        private abstract class Shape
        {
            public abstract void Move(Vector2 newPosition);
            public abstract void Draw(ColorRgba color);
            public abstract ShapeType GetShapeType();
            public string GetName()
            {
                switch (GetShapeType())
                {
                    case ShapeType.Rect: return "Rect";
                }

                return "Invalid Shape";
            }
        }
        private const float LineThickness = 4f;
        private Shape staticShape;
        private InputAction nextStaticShape;
        private class RectShape : Shape
        {
            public Rect Rect;
            public RectShape(Vector2 pos, float size) =>
                Rect = new(pos, new(size * 2, size * 2), new Vector2(0.5f));
            public override void Move(Vector2 newPosition) =>
                Rect = Rect.ChangePosition(newPosition - Rect.Center);
            public override void Draw(ColorRgba color) =>
                Rect.DrawLines(LineThickness, color);
            public override ShapeType GetShapeType() => ShapeType.Rect;
        }

        protected override void OnDrawGameUIExample(ScreenInfo ui)
        {
            
        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
            DrawInputText(bottomCenter);
            var curDevice = ShapeInput.CurrentInputDeviceType;
            var nextStaticText = nextStaticShape.GetInputTypeDescription(curDevice, true, 1, false);
        }
        
        private void DrawInputText(Rect rect)
        {
            var top = rect.ApplyMargins(0, 0, 0, 0.5f);
            var bottom = rect.ApplyMargins(0, 0, 0.5f, 0f);
            
            var sbCamera = new StringBuilder();
            var sbInfo = new StringBuilder();
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            //var curInputDeviceNoMouse = ShapeLoop.Input.CurrentInputDeviceNoMouse;
            
            var pos = camera.BasePosition;
            var x = (int)pos.X;
            var y = (int)pos.Y;
            var rot = (int)camera.BaseRotationDeg;
            var zoom = (int)(ShapeMath.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);

            sbInfo.Append($"Pos {x}/{y} | ");
            sbInfo.Append($"Rot {rot} | ");
            sbInfo.Append($"Zoom {zoom}");
            string moveCameraH = curInputDeviceAll == InputDeviceType.Mouse ? "Mx" : iaMoveCameraH.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string moveCameraV = curInputDeviceAll == InputDeviceType.Mouse ? "My" : iaMoveCameraV.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoomCamera = GAMELOOP.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string rotateCamera = iaRotateCamera.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            sbCamera.Append($"Move {moveCameraH} {moveCameraV} | ");
            sbCamera.Append($"Zoom {zoomCamera} | ");
            sbCamera.Append($"Rotate {rotateCamera}");
            
            
            textFont.DrawTextWrapNone(sbInfo.ToString(), top, new(0.5f));
            textFont.DrawTextWrapNone(sbCamera.ToString(), bottom, new(0.5f));
            // font.DrawText(sbInfo.ToString(), top, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            // font.DrawText(sbCamera.ToString(), bottom, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
        private void NextStaticShape(float size = 300f) =>
            staticShape = CreateShape(new(), size);

        private Shape CreateShape(Vector2 pos, float size) =>
            new RectShape(pos, size);
    }

}
