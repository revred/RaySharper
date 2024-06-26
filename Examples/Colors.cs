using ShapeEngine.Color;

namespace Examples;

public static class Colors
{
    internal class Palette : IColorPalette
    {
        private readonly List<PaletteColor> colors;

        public Palette(params PaletteColor[] colors)
        {
            this.colors = colors.ToList();
        }
        public List<PaletteColor> GetColors() => colors;
    }
       
       
    public static ColorRgba Background      => PcBackground.ColorRgba;
    public static ColorRgba Dark       => PcDark.ColorRgba; 
    public static ColorRgba Medium     => PcMedium.ColorRgba; 
    public static ColorRgba Light      => PcLight.ColorRgba; 
    public static ColorRgba Text => PcText.ColorRgba; 
    public static ColorRgba Highlight => PcHighlight.ColorRgba; 
    public static ColorRgba Special => PcSpecial.ColorRgba;
    public static ColorRgba Special2 => PcSpecial2.ColorRgba;
    public static ColorRgba Warm   => PcWarm.ColorRgba;
    public static ColorRgba Cold   => PcCold.ColorRgba;

    //public static readonly PaletteColor BackgroundP = new();
       
    public static readonly PaletteColor PcBackground = new(0, new ColorRgba(System.Drawing.Color.DarkSlateGray).ToHSL().ChangeLightness(-0.15f).ToRGB());
    public static readonly PaletteColor PcDark = new(1, new ColorRgba(System.Drawing.Color.DarkSlateGray).ToHSL().ChangeLightness(-0.1f).ToRGB());
    public static readonly PaletteColor PcMedium = new(2, new(System.Drawing.Color.DarkGray));
    public static readonly PaletteColor PcLight = new(3, new(System.Drawing.Color.LightGray));
    public static readonly PaletteColor PcText = new(4, new(System.Drawing.Color.AntiqueWhite));
    public static readonly PaletteColor PcHighlight = new(5, new(System.Drawing.Color.Aquamarine));
    public static readonly PaletteColor PcSpecial = new(6, new(System.Drawing.Color.Coral));
    public static readonly PaletteColor PcSpecial2 = new(7, new(System.Drawing.Color.Goldenrod));
    public static readonly PaletteColor PcWarm = new(8, new(System.Drawing.Color.IndianRed));
    public static readonly PaletteColor PcCold = new(9, new(System.Drawing.Color.CornflowerBlue));
       
    private static readonly Palette colorPalette = new
    (
        PcBackground, 
        PcDark, PcMedium, PcLight, 
        PcText,
        PcHighlight, PcSpecial, PcSpecial2,
        PcWarm, PcCold
    );

    public static readonly Colorscheme DefaultColorscheme = new
    (
        new ColorRgba(System.Drawing.Color.DarkSlateGray).ToHSL().ChangeLightness(-0.15f).ToRGB(),
        new ColorRgba(System.Drawing.Color.DimGray).ToHSL().ChangeLightness(-0.1f).ToRGB(),
        new ColorRgba(System.Drawing.Color.DarkGray),
        new ColorRgba(System.Drawing.Color.LightGray),
        new ColorRgba(System.Drawing.Color.AntiqueWhite),
        new ColorRgba(System.Drawing.Color.Aquamarine),
        new ColorRgba(System.Drawing.Color.Coral),
        new ColorRgba(System.Drawing.Color.Goldenrod),
        new ColorRgba(System.Drawing.Color.IndianRed),
        new ColorRgba(System.Drawing.Color.CornflowerBlue)
    );
        
    public static readonly Colorscheme WarmColorscheme = new
    (
        new ColorRgba(System.Drawing.Color.DarkRed).ToHSL().ChangeLightness(-0.2f).ToRGB(),
        new ColorRgba(System.Drawing.Color.SaddleBrown).ToHSL().ChangeLightness(-0.15f).ToRGB(),
        new ColorRgba(System.Drawing.Color.Sienna),
        new ColorRgba(System.Drawing.Color.Salmon),
        new ColorRgba(System.Drawing.Color.Tomato),
        new ColorRgba(System.Drawing.Color.OrangeRed),
        new ColorRgba(System.Drawing.Color.HotPink),
        new ColorRgba(System.Drawing.Color.PaleVioletRed),
        new ColorRgba(System.Drawing.Color.Crimson),
        new ColorRgba(System.Drawing.Color.Orchid)
    );
       
    public static readonly Colorscheme ColdColorscheme = new
    (
        new ColorRgba(System.Drawing.Color.Navy).ToHSL().ChangeLightness(-0.2f).ToRGB(),
        new ColorRgba(System.Drawing.Color.DarkSlateBlue).ToHSL().ChangeLightness(-0.15f).ToRGB(),
        new ColorRgba(System.Drawing.Color.SlateBlue),
        new ColorRgba(System.Drawing.Color.LightSteelBlue),
        new ColorRgba(System.Drawing.Color.AliceBlue),
        new ColorRgba(System.Drawing.Color.Aqua),
        new ColorRgba(System.Drawing.Color.Aquamarine),
        new ColorRgba(System.Drawing.Color.DeepSkyBlue),
        new ColorRgba(System.Drawing.Color.GreenYellow),
        new ColorRgba(System.Drawing.Color.RoyalBlue)
    );
    
    
    private static readonly Colorscheme[] Colorschemes = new[] { DefaultColorscheme, WarmColorscheme, ColdColorscheme };
    private static readonly string[] ColorschemeNames = new[] { "Default", "Warm", "Cold" };
    private static int curColorschemeIndex = 0;
    public static int CurColorschemeIndex
    {
        get => curColorschemeIndex;
        private set
        {
            if (value < 0) curColorschemeIndex = Colorschemes.Length - 1;
            else if (value >= Colorschemes.Length) curColorschemeIndex = 0;
            else curColorschemeIndex = value;
        }
    }

    public static string CurColorschemeName => ColorschemeNames[curColorschemeIndex];
    private static void NextColorschemeIndex() => CurColorschemeIndex += 1;
    private static void PreviousColorschemeIndex() => CurColorschemeIndex -= 1;

    public static void PreviousColorscheme()
    {
        PreviousColorschemeIndex();
        ApplyColorscheme(Colorschemes[curColorschemeIndex]);
    }
    public static void NextColorscheme()
    {
        NextColorschemeIndex();
        ApplyColorscheme(Colorschemes[curColorschemeIndex]);
    }
    public static void ApplyColorscheme(Colorscheme cc) => cc.Apply(colorPalette);
}