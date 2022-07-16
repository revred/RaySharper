﻿global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static ShapeEngineCore.ShapeEngine;
using ShapeEngineCore;


namespace ShapeEngineTemplate
{
    static class Program
    {
        public static void Main(params string[] launchParams)
        {

            ScreenInitInfo screenInitInfo = new ScreenInitInfo(1920, 1080, 1f, 1.0f, "Shape Engine Template", 60, true, false, 0, false);
            DataInitInfo dataInitInfo = new DataInitInfo("data/test-properties.json", new ShapeEngineCore.Globals.Persistent.DataResolver());
            ShapeEngine.Start(new GameLoop(), screenInitInfo, dataInitInfo);
        }
    }
}