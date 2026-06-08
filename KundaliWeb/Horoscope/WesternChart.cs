using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/// <summary>
/// Renders a Western tropical natal chart as a circular wheel PNG.
///
/// Layout (outer ? inner):
///   1. Zodiac band  — 12 × 30° segments with sign abbreviation
///   2. Degree ticks — every 5° on the zodiac inner edge
///   3. House ring   — 12 houses, numbered, with cusp lines to centre
///   4. Planet ring  — planet glyphs placed at their ecliptic longitude
///   5. Aspect lines — drawn inside the centre circle
/// </summary>
public static class WesternChart
{
    private static readonly string[] SignAbbr =
        ["Ar","Ta","Ge","Ca","Le","Vi","Li","Sc","Sa","Cp","Aq","Pi"];

    // Alternating element colours (Fire/Earth/Air/Water × 3)
    private static readonly Color[] SignColors =
    [
        Color.FromArgb(255, 255, 225, 210),  // Ar — Fire
        Color.FromArgb(255, 220, 240, 220),  // Ta — Earth
        Color.FromArgb(255, 220, 235, 255),  // Ge — Air
        Color.FromArgb(255, 200, 225, 245),  // Ca — Water
        Color.FromArgb(255, 255, 225, 210),  // Le — Fire
        Color.FromArgb(255, 220, 240, 220),  // Vi — Earth
        Color.FromArgb(255, 220, 235, 255),  // Li — Air
        Color.FromArgb(255, 200, 225, 245),  // Sc — Water
        Color.FromArgb(255, 255, 225, 210),  // Sa — Fire
        Color.FromArgb(255, 220, 240, 220),  // Cp — Earth
        Color.FromArgb(255, 220, 235, 255),  // Aq — Air
        Color.FromArgb(255, 200, 225, 245),  // Pi — Water
    ];

    private static readonly Color BorderColor    = Color.FromArgb(255,  60,  50,  40);
    private static readonly Color HouseLineColor = Color.FromArgb(255, 100,  80,  60);
    private static readonly Color SignTextColor  = Color.FromArgb(255,  50,  40,  30);
    private static readonly Color HouseNumColor  = Color.FromArgb(255, 110,  85,  55);
    private static readonly Color PlanetColor    = Color.FromArgb(255,  20,  70, 180);
    private static readonly Color RetroColor     = Color.FromArgb(255, 180,  30,  30);
    private static readonly Color AscColor       = Color.FromArgb(255, 200,  20,  20);
    private static readonly Color AspectColor    = Color.FromArgb(100,  80, 120, 200);
    private static readonly Color CentreColor    = Color.FromArgb(255, 248, 244, 238);
    private static readonly Color TickColor      = Color.FromArgb(255, 130, 110,  90);

    // Major aspect orbs (degrees)
    private static readonly (string name, float angle, float orb, Color color)[] Aspects =
    [
        ("Conjunction",  0f,  8f, Color.FromArgb(120,  80,  80, 200)),
        ("Opposition",  180f,  8f, Color.FromArgb(120, 200,  60,  60)),
        ("Trine",       120f,  6f, Color.FromArgb(120,  60, 160,  60)),
        ("Square",       90f,  6f, Color.FromArgb(120, 200,  60,  60)),
        ("Sextile",      60f,  4f, Color.FromArgb(120,  60, 160,  60)),
    ];

    /// <summary>Generates the Western natal chart PNG.</summary>
    public static void GenerateChart(string outputPath, WesternChartData data, int size = 900)
    {
        using var bmp = new Bitmap(size, size);
        using var g   = Graphics.FromImage(bmp);
        g.SmoothingMode     = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        g.Clear(Color.White);

        float cx = size / 2f;
        float cy = size / 2f;

        // Radii (as fraction of half-size)
        float rOuter    = size * 0.455f;   // outer edge of zodiac band
        float rZodiacIn = size * 0.365f;   // inner edge of zodiac band / outer edge of house ring
        float rHouseOut = rZodiacIn;
        float rPlanet   = size * 0.290f;   // planet orbit radius
        float rHouseIn  = size * 0.220f;   // inner edge of house ring
        float rCentre   = size * 0.200f;   // centre circle (aspects drawn inside)

        float ascDeg = data.AscendantDegree;   // tropical Ascendant longitude

        // Draw layers back ? front
        DrawZodiacBand(g, cx, cy, rOuter, rZodiacIn, ascDeg);
        DrawDegreeTicks(g, cx, cy, rZodiacIn, ascDeg);
        DrawHouseRing(g, cx, cy, rHouseOut, rHouseIn, ascDeg);
        DrawCentre(g, cx, cy, rCentre, data);
        DrawAspectLines(g, cx, cy, rCentre, data.Planets, ascDeg);
        DrawPlanets(g, cx, cy, rPlanet, rHouseIn, data.Planets, ascDeg);
        DrawAscendantMarker(g, cx, cy, rOuter, rHouseIn, ascDeg);

        // Outer border
        using var pen = new Pen(BorderColor, 2.5f);
        g.DrawEllipse(pen, cx - rOuter, cy - rOuter, rOuter * 2, rOuter * 2);

        bmp.Save(outputPath, ImageFormat.Png);
    }

    // ?? Zodiac Band ??????????????????????????????????????????????????????????

    private static void DrawZodiacBand(
        Graphics g, float cx, float cy,
        float outerR, float innerR, float ascDeg)
    {
        using var linePen = new Pen(BorderColor, 1.2f);
        float fontSize = Math.Max(7f, (outerR - innerR) * 0.28f);
        using var font  = new Font("Arial", fontSize, FontStyle.Bold);

        for (int i = 0; i < 12; i++)
        {
            // i=0 ? Aries starts at ascDeg on screen
            float startScreen = ToScreen(i * 30f, ascDeg);

            using var brush = new SolidBrush(SignColors[i]);
            using var path  = new GraphicsPath();
            path.AddArc(cx - outerR, cy - outerR, outerR * 2, outerR * 2, startScreen, 30f);
            path.AddArc(cx - innerR, cy - innerR, innerR * 2, innerR * 2, startScreen + 30f, -30f);
            path.CloseFigure();
            g.FillPath(brush, path);
            g.DrawPath(linePen, path);

            // Sign abbreviation centred in the slice
            float midScreen = startScreen + 15f;
            float midR      = (outerR + innerR) / 2f;
            PointF pt = Polar(cx, cy, midR, midScreen);
            using var textBrush = new SolidBrush(SignTextColor);
            SizeF sz = g.MeasureString(SignAbbr[i], font);
            g.DrawString(SignAbbr[i], font, textBrush, pt.X - sz.Width / 2f, pt.Y - sz.Height / 2f);
        }
    }

    // ?? Degree Ticks ?????????????????????????????????????????????????????????

    private static void DrawDegreeTicks(
        Graphics g, float cx, float cy, float r, float ascDeg)
    {
        using var majorPen = new Pen(TickColor, 1.2f);
        using var minorPen = new Pen(TickColor, 0.6f);

        for (int deg = 0; deg < 360; deg++)
        {
            float screen = ToScreen(deg, ascDeg);
            bool  major  = deg % 5 == 0;
            float len    = major ? r * 0.03f : r * 0.015f;
            PointF outer = Polar(cx, cy, r, screen);
            PointF inner = Polar(cx, cy, r - len, screen);
            g.DrawLine(major ? majorPen : minorPen, outer, inner);
        }
    }

    // ?? House Ring ???????????????????????????????????????????????????????????

    private static void DrawHouseRing(
        Graphics g, float cx, float cy,
        float outerR, float innerR, float ascDeg)
    {
        using var linePen  = new Pen(HouseLineColor, 1.5f);
        using var lightPen = new Pen(HouseLineColor, 0.8f);
        using var bgBrush  = new SolidBrush(Color.FromArgb(255, 245, 240, 232));
        float numR     = (outerR + innerR) / 2f;
        float fontSize = Math.Max(6f, (outerR - innerR) * 0.22f);
        using var font     = new Font("Arial", fontSize, FontStyle.Bold);
        using var numBrush = new SolidBrush(HouseNumColor);

        // Fill house ring background
        using var path = new GraphicsPath();
        path.AddEllipse(cx - outerR, cy - outerR, outerR * 2, outerR * 2);
        path.AddEllipse(cx - innerR, cy - innerR, innerR * 2, innerR * 2);
        g.FillPath(bgBrush, path);

        // Draw 12 equal house cusps (Equal House from Ascendant)
        for (int h = 0; h < 12; h++)
        {
            float cuspLon    = (ascDeg + h * 30f) % 360f;
            float cuspScreen = ToScreen(cuspLon, ascDeg);

            PointF outerPt = Polar(cx, cy, outerR, cuspScreen);
            PointF innerPt = Polar(cx, cy, innerR, cuspScreen);
            g.DrawLine(h % 3 == 0 ? linePen : lightPen, outerPt, innerPt);

            // House number in the middle of the sector
            float midScreen = cuspScreen + 15f;
            PointF numPt    = Polar(cx, cy, numR, midScreen);
            string label    = (h + 1).ToString();
            SizeF  sz       = g.MeasureString(label, font);
            g.DrawString(label, font, numBrush, numPt.X - sz.Width / 2f, numPt.Y - sz.Height / 2f);
        }

        // Ring borders
        using var borderPen = new Pen(BorderColor, 1.5f);
        g.DrawEllipse(borderPen, cx - outerR, cy - outerR, outerR * 2, outerR * 2);
        g.DrawEllipse(borderPen, cx - innerR, cy - innerR, innerR * 2, innerR * 2);
    }

    // ?? Centre Circle ????????????????????????????????????????????????????????

    private static void DrawCentre(
        Graphics g, float cx, float cy, float r, WesternChartData data)
    {
        using var brush = new SolidBrush(CentreColor);
        g.FillEllipse(brush, cx - r, cy - r, r * 2, r * 2);
        using var pen = new Pen(BorderColor, 1.5f);
        g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2);

        float fs  = Math.Max(6f, r * 0.11f);
        float fs2 = Math.Max(5f, r * 0.09f);
        using var titleFont = new Font("Arial", fs,  FontStyle.Bold);
        using var infoFont  = new Font("Arial", fs2, FontStyle.Regular);
        using var titleBrush= new SolidBrush(BorderColor);
        using var infoBrush = new SolidBrush(HouseLineColor);

        string[] lines =
        [
            "Western Chart",
            data.ChartDate.ToString("dd MMM yyyy"),
            data.ChartDate.ToString("HH:mm"),
            $"{data.Latitude:F2}N  {data.Longitude:F2}E",
        ];
        float totalH = lines.Length * (fs + 3);
        float y = cy - totalH / 2f;
        foreach (var (line, i) in lines.Select((l, i) => (l, i)))
        {
            var f = i == 0 ? titleFont : infoFont;
            var b = i == 0 ? titleBrush : infoBrush;
            SizeF sz = g.MeasureString(line, f);
            g.DrawString(line, f, b, cx - sz.Width / 2f, y);
            y += sz.Height + 2;
        }
    }

    // ?? Aspect Lines ?????????????????????????????????????????????????????????

    private static void DrawAspectLines(
        Graphics g, float cx, float cy, float r,
        List<WesternPlanetPosition> planets, float ascDeg)
    {
        var traditionalPlanets = planets
            .Where(p => p.Name is "Su" or "Mo" or "Me" or "Ve" or "Ma" or "Ju" or "Sa")
            .ToList();

        for (int i = 0; i < traditionalPlanets.Count - 1; i++)
        for (int j = i + 1; j < traditionalPlanets.Count; j++)
        {
            float lonA = traditionalPlanets[i].AbsoluteLongitude;
            float lonB = traditionalPlanets[j].AbsoluteLongitude;
            float diff = Math.Abs(lonA - lonB);
            if (diff > 180) diff = 360 - diff;

            foreach (var (_, angle, orb, color) in Aspects)
            {
                if (Math.Abs(diff - angle) <= orb)
                {
                    PointF ptA = Polar(cx, cy, r * 0.95f, ToScreen(lonA, ascDeg));
                    PointF ptB = Polar(cx, cy, r * 0.95f, ToScreen(lonB, ascDeg));
                    using var pen = new Pen(color, 1.0f);
                    g.DrawLine(pen, ptA, ptB);
                    break;
                }
            }
        }
    }

    // ?? Planets ???????????????????????????????????????????????????????????????

    private static void DrawPlanets(
        Graphics g, float cx, float cy,
        float orbitR, float innerR,
        List<WesternPlanetPosition> planets, float ascDeg)
    {
        float fontSize = Math.Max(6f, orbitR * 0.075f);
        using var font    = new Font("Arial", fontSize, FontStyle.Bold);
        using var pBrush  = new SolidBrush(PlanetColor);
        using var rBrush  = new SolidBrush(RetroColor);
        using var bgBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
        using var bPen    = new Pen(PlanetColor, 1f);

        foreach (var p in planets)
        {
            float screen = ToScreen(p.AbsoluteLongitude, ascDeg);
            PointF pt    = Polar(cx, cy, orbitR, screen);

            string label = p.IsRetrograde ? $"{p.Name}R" : p.Name;
            string degLbl = $"{p.DegreeInSign:F0}°";

            var brush = p.IsRetrograde ? rBrush : pBrush;
            SizeF sz  = g.MeasureString(label, font);
            float pad = 2f;
            var rect  = new RectangleF(pt.X - sz.Width / 2f - pad, pt.Y - sz.Height / 2f - pad,
                                       sz.Width + pad * 2, sz.Height + pad * 2);
            g.FillEllipse(bgBrush, rect);
            g.DrawEllipse(bPen, rect);
            g.DrawString(label, font, brush, pt.X - sz.Width / 2f, pt.Y - sz.Height / 2f);

            // Small degree label along the spoke toward centre
            PointF degPt  = Polar(cx, cy, orbitR - sz.Height - 4, screen);
            float  degFs  = Math.Max(5f, fontSize * 0.75f);
            using var degFont  = new Font("Arial", degFs, FontStyle.Regular);
            SizeF  degSz  = g.MeasureString(degLbl, degFont);
            g.DrawString(degLbl, degFont, brush, degPt.X - degSz.Width / 2f, degPt.Y - degSz.Height / 2f);

            // Tick line from inner house ring to planet orbit
            PointF tickOuter = Polar(cx, cy, innerR, screen);
            PointF tickInner = Polar(cx, cy, innerR - 8, screen);
            using var tickPen = new Pen(p.IsRetrograde ? RetroColor : HouseLineColor, 1f);
            g.DrawLine(tickPen, tickOuter, tickInner);
        }
    }

    // ?? Ascendant Marker ?????????????????????????????????????????????????????

    private static void DrawAscendantMarker(
        Graphics g, float cx, float cy,
        float outerR, float innerR, float ascDeg)
    {
        float screen = ToScreen(ascDeg, ascDeg);  // Ascendant is always at 180° screen = left
        PointF outerPt = Polar(cx, cy, outerR, screen);
        PointF innerPt = Polar(cx, cy, innerR, screen);

        using var pen = new Pen(AscColor, 2.5f);
        g.DrawLine(pen, outerPt, innerPt);

        float fs = Math.Max(7f, outerR * 0.055f);
        using var font  = new Font("Arial", fs, FontStyle.Bold);
        using var brush = new SolidBrush(AscColor);
        string label = "Asc";
        SizeF  sz    = g.MeasureString(label, font);
        PointF lpt   = Polar(cx, cy, outerR + sz.Height, screen);
        g.DrawString(label, font, brush, lpt.X - sz.Width / 2f, lpt.Y - sz.Height / 2f);

        // Opposite (Desc) marker
        float descScreen = ToScreen((ascDeg + 180f) % 360f, ascDeg);
        PointF dOuter = Polar(cx, cy, outerR, descScreen);
        PointF dInner = Polar(cx, cy, innerR, descScreen);
        g.DrawLine(pen, dOuter, dInner);
    }

    // ?? Geometry helpers ?????????????????????????????????????????????????????

    /// <summary>
    /// Converts ecliptic longitude to screen angle.
    /// Ascendant sits on the LEFT (180° in screen coords, counter-clockwise wheel).
    /// </summary>
    private static float ToScreen(float eclipticLon, float ascDeg)
        => 180f - (eclipticLon - ascDeg);

    private static PointF Polar(float cx, float cy, float r, float angleDeg)
    {
        double rad = angleDeg * Math.PI / 180.0;
        return new PointF(cx + r * (float)Math.Cos(rad), cy + r * (float)Math.Sin(rad));
    }
}
