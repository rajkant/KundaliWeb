using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/// <summary>
/// Generates PNG images of an astrology natal chart with 12 houses and planet positions.
/// </summary>
public static class AstrologyChart
{
    // House cusp labels (Roman numerals I–XII)
    private static readonly string[] HouseLabels =
        ["","1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"];

    // Zodiac sign abbreviations in order (Aries to Pisces).
    // Unicode glyphs (e.g. ♈) are avoided: System.Drawing renders them as '?'
    // when the rasteriser font lacks those code points.
    private static readonly string[] ZodiacSymbols =
        ["","Ar", "Ta", "Ge", "Ca", "Le", "Vi", "Li", "Sc", "Sa", "Ca", "Aq", "Pi"];

    // Default colors for chart elements
    private static readonly Color WheelBackground   = Color.FromArgb(255, 245, 235, 220);
    private static readonly Color HouseLineColor    = Color.FromArgb(255, 100,  80,  60);
    private static readonly Color ZodiacBandColor   = Color.FromArgb(255, 210, 190, 160);
    private static readonly Color ZodiacTextColor   = Color.FromArgb(255,  60,  40,  20);
    private static readonly Color HouseNumberColor  = Color.FromArgb(255,  80,  60,  40);
    private static readonly Color PlanetColor       = Color.FromArgb(255,  20,  80, 180);
    private static readonly Color PlanetBgColor     = Color.FromArgb(200, 255, 255, 255);
    private static readonly Color CenterCircleColor = Color.FromArgb(255, 235, 220, 200);

    /// <summary>
    /// Generates a PNG of the astrology chart with the specified planet positions.
    /// </summary>
    /// <param name="outputPath">Path to save the PNG file.</param>
    /// <param name="planets">List of planets to draw (name, house 1–12, angle 0–360).</param>
    /// <param name="ascendantDegree">The Ascendant degree (0–360) used to rotate the chart wheel.</param>
    /// <param name="size">Image width and height in pixels.</param>
    public static void GenerateChart(
        string outputPath,
        IEnumerable<PlanetPosition> planets,
        float ascendantDegree = 0f,
        int size = 800)
    {
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode   = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        g.Clear(Color.White);

        float cx = size / 2f;
        float cy = size / 2f;

        float outerRadius  = size * 0.46f;   // outer zodiac wheel edge
        float zodiacInner  = size * 0.40f;   // inner edge of zodiac band
        float houseOuter   = zodiacInner;
        float houseInner   = size * 0.01f;   // inner house circle (center space)

        // --- Background circle ---
        using var bgBrush = new SolidBrush(WheelBackground);
        g.FillEllipse(bgBrush, cx - outerRadius, cy - outerRadius, outerRadius * 2, outerRadius * 2);

        // --- Zodiac band (outer ring) ---
        DrawZodiacBand(g, cx, cy, outerRadius, zodiacInner, ascendantDegree);

        // --- House divisions ---
        DrawHouseDivisions(g, cx, cy, houseOuter, houseInner, ascendantDegree);

        // --- Center circle ---
        using var centerBrush = new SolidBrush(CenterCircleColor);
        g.FillEllipse(centerBrush, cx - houseInner, cy - houseInner, houseInner * 2, houseInner * 2);
        using var centerPen = new Pen(HouseLineColor, 1.5f);
        g.DrawEllipse(centerPen, cx - houseInner, cy - houseInner, houseInner * 2, houseInner * 2);

        // --- Planets ---
        float planetRadius = houseOuter * 0.78f + houseInner * 0.22f;
        foreach (var planet in planets)
            DrawPlanet(g, cx, cy, planetRadius, planet, ascendantDegree);

        // --- Outer border ---
        using var borderPen = new Pen(HouseLineColor, 2.5f);
        g.DrawEllipse(borderPen, cx - outerRadius, cy - outerRadius, outerRadius * 2, outerRadius * 2);

        bitmap.Save(outputPath, ImageFormat.Png);
    }

    /// <summary>
    /// Places a single planet on an existing chart image by house number and angle.
    /// </summary>
    /// <param name="houseNumber">House number (1–12).</param>
    /// <param name="angle">
    ///   Degrees within the house (0 = house cusp, up to ~30 before next cusp).
    ///   Alternatively, supply the absolute ecliptic longitude (0–360) when
    ///   <paramref name="isAbsoluteAngle"/> is <c>true</c>.
    /// </param>
    /// <param name="planetName">Symbol or name for the planet (e.g. "?", "Moon").</param>
    /// <param name="isAbsoluteAngle">
    ///   When <c>true</c>, <paramref name="angle"/> is treated as ecliptic longitude
    ///   (0–360). When <c>false</c>, it is treated as offset within the house.
    /// </param>
    /// <returns>A <see cref="PlanetPosition"/> ready for use with <see cref="GenerateChart"/>.</returns>
    public static PlanetPosition CreatePlanetPosition(
        int houseNumber,
        float angle,
        string planetName,
        bool isAbsoluteAngle = false)
    {
        if (houseNumber < 1 || houseNumber > 12)
            throw new ArgumentOutOfRangeException(nameof(houseNumber), "House number must be between 1 and 12.");

        float absoluteAngle;
        if (isAbsoluteAngle)
        {
            absoluteAngle = ((angle % 360) + 360) % 360;
        }
        else
        {
            // Each house spans 30 degrees; house 1 starts at 0°
            float houseStartDegree = (houseNumber - 1) * 30f;
            absoluteAngle = ((houseStartDegree + angle) % 360 + 360) % 360;
        }

        return new PlanetPosition(planetName, houseNumber, absoluteAngle);
    }

    /// <summary>
    /// Creates a planet position using zodiac sign and degree within that sign.
    /// </summary>
    /// <param name="zodiacSign">Zodiac sign number (1=Aries, 2=Taurus, ..., 12=Pisces).</param>
    /// <param name="degreeInSign">Degree within the zodiac sign (0-30°).</param>
    /// <param name="planetName">Symbol or name for the planet.</param>
    /// <returns>A <see cref="PlanetPosition"/> ready for use with <see cref="GenerateChart"/>.</returns>
    public static PlanetPosition CreatePlanetPositionByZodiac(
        int zodiacSign,
        float degreeInSign,
        string planetName)
    {
        if (zodiacSign < 1 || zodiacSign > 12)
            throw new ArgumentOutOfRangeException(nameof(zodiacSign), "Zodiac sign must be between 1 (Aries) and 12 (Pisces).");

        if (degreeInSign < 0 || degreeInSign >= 30)
            throw new ArgumentOutOfRangeException(nameof(degreeInSign), "Degree in sign must be between 0 and 30.");

        // Calculate absolute ecliptic longitude
        float absoluteAngle = (zodiacSign - 1) * 30f + degreeInSign;
        absoluteAngle = ((absoluteAngle % 360) + 360) % 360;

        // Calculate house (assuming Equal House system aligned with zodiac)
        int house = (int)(absoluteAngle / 30.0) + 1;
        if (house > 12) house = 12;

        return new PlanetPosition(planetName, house, absoluteAngle);
    }

    // ?? Private drawing helpers ?????????????????????????????????????????????

    private static void DrawZodiacBand(
        Graphics g, float cx, float cy,
        float outerR, float innerR, float ascendantDegree)
    {
        using var bandBrush = new SolidBrush(ZodiacBandColor);
        using var linePen   = new Pen(HouseLineColor, 1f);
        using var font      = new Font("Arial", innerR * 0.04f, FontStyle.Bold);
        using var textBrush = new SolidBrush(ZodiacTextColor);

        for (int i = 1; i < 13; i++)
        {
            // Each sign spans 30 degrees; Aries (0°) aligns with ascendant
            float startAngle = ToScreenAngle(i * 30f - ascendantDegree);

            // Draw pie slice for the sign
            using var path = new GraphicsPath();
            path.AddArc(cx - outerR, cy - outerR, outerR * 2, outerR * 2, startAngle, 30);
            path.AddArc(cx - innerR, cy - innerR, innerR * 2, innerR * 2, startAngle + 30, -30);
            path.CloseFigure();
            g.FillPath(bandBrush, path);
            g.DrawPath(linePen, path);

            // Place zodiac symbol in the middle of the slice
            float midAngle = startAngle + 15f;
            float midR     = (outerR + innerR) / 2f;
            PointF pt      = AngleToPoint(cx, cy, midR, midAngle);
            SizeF  sz      = g.MeasureString(ZodiacSymbols[i], font);
            g.DrawString(ZodiacSymbols[i], font, textBrush,
                pt.X - sz.Width / 2f, pt.Y - sz.Height / 2f);
        }
    }

    private static void DrawHouseDivisions(
        Graphics g, float cx, float cy,
        float outerR, float innerR, float ascendantDegree)
    {
        using var linePen    = new Pen(HouseLineColor, 1.5f);
        using var numBrush   = new SolidBrush(HouseNumberColor);

        // Small font for house numbers near the inner circle
        float houseFontSize = Math.Max(6f, outerR * 0.04f);
        using var font = new Font("Segoe UI", houseFontSize, FontStyle.Bold);

        // House numbers sit just outside the innermost circle
        float labelR = innerR + (outerR - innerR) * 0.10f;

        for (int i = 1; i < 13; i++)
        {
            // House cusp lines: house 1 cusp at ascendant (left = 180° in screen coords)
            float cuspAngle = ToScreenAngle(i * 30f - ascendantDegree);

            PointF inner = AngleToPoint(cx, cy, innerR, cuspAngle);
            PointF outer = AngleToPoint(cx, cy, outerR, cuspAngle);
            g.DrawLine(linePen, inner, outer);

            // House number label close to inner circle, centred in the sector
            float midAngle = cuspAngle + 15f;
            PointF labelPt = AngleToPoint(cx, cy, labelR, midAngle);
            string label   = HouseLabels[i];
            SizeF  sz      = g.MeasureString(label, font);
            g.DrawString(label, font, numBrush,
                labelPt.X - sz.Width / 2f, labelPt.Y - sz.Height / 2f);
        }
    }

    private static void DrawPlanet(
        Graphics g, float cx, float cy,
        float orbitRadius, PlanetPosition planet, float ascendantDegree)
    {
        float screenAngle = ToScreenAngle(planet.AbsoluteAngle - ascendantDegree);
        PointF pt = AngleToPoint(cx, cy, orbitRadius, screenAngle);

        using var font  = new Font("Segoe UI Symbol", orbitRadius * 0.06f, FontStyle.Bold);
        SizeF sz = g.MeasureString(planet.Name, font);

        float padding = 2f;
        float boxW = sz.Width  + padding * 2;
        float boxH = sz.Height + padding * 2;

        // Background pill
        using var bgBrush   = new SolidBrush(PlanetBgColor);
        using var borderPen = new Pen(PlanetColor, 1.0f);
        var rect = new RectangleF(pt.X - boxW / 2f, pt.Y - boxH / 2f, boxW, boxH);
        g.FillEllipse(bgBrush, rect);
        g.DrawEllipse(borderPen, rect);

        // Planet symbol / name
        using var textBrush = new SolidBrush(PlanetColor);
        g.DrawString(planet.Name, font, textBrush,
            pt.X - sz.Width / 2f, pt.Y - sz.Height / 2f);
    }

    // Converts ecliptic longitude (0° = Aries, counter-clockwise) to
    // System.Drawing screen angle (0° = right, clockwise).
    // Astrology wheels: Ascendant (0° offset) appears on the LEFT (180° screen).
    private static float ToScreenAngle(float eclipticDegree)
        => 180f - eclipticDegree;

    private static PointF AngleToPoint(float cx, float cy, float r, float angleDeg)
    {
        double rad = angleDeg * Math.PI / 180.0;
        return new PointF(
            cx + r * (float)Math.Cos(rad),
            cy + r * (float)Math.Sin(rad));
    }
}

/// <summary>Represents a planet placed on the chart.</summary>
/// <param name="Name">Display name or symbol of the planet.</param>
/// <param name="House">House number (1–12).</param>
/// <param name="AbsoluteAngle">Ecliptic longitude in degrees (0–360).</param>
public record PlanetPosition(string Name, int House, float AbsoluteAngle);
