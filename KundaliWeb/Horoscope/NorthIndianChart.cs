using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/// <summary>
/// Renders a North Indian style Vedic birth chart as a PNG.
///
/// The North Indian chart is a fixed-geometry diamond divided into 12 triangular
/// houses.  The Ascendant sign is always placed in the top-center diamond, and
/// subsequent signs go clockwise.
///
/// Geometry (centres of the 12 triangular house cells relative to the chart square):
///
///          [Asc]          ? top centre  (House 1)
///      [12]      [2]
///   [11]    [1]    [3]    ? 1 = centre diamond
///      [10]      [4]
///          [9]            ? bottom centre
///      [8]       [5]
///   [7]               [6]   ? no — see actual layout below
///
/// Actual cell mapping (house offset 0–11 clockwise from top-centre):
///   0  top-centre         (Lagna)
///   1  upper-right
///   2  right
///   3  lower-right
///   4  bottom-centre
///   5  lower-left
///   6  left
///   7  upper-left
///   8  inner upper-left  (2nd row)
///   9  inner upper-right
///  10  inner lower-right
///  11  inner lower-left
///
/// The standard North Indian chart uses a 3×3 grid of diamonds:
///
///   TL | TC | TR          TL=house12  TC=house1(Asc)  TR=house2
///   ML | MC | MR          ML=house11  MC=centre        MR=house3
///   BL | BC | BR          BL=house10  BC=house9        BR=house4
///                         + 4 inner triangles: house8(BL inner), house5(BR inner),
///                                              house7(ML inner), house6(MR inner)
/// </summary>
public static class NorthIndianChart
{
    // ?? Colours ??????????????????????????????????????????????????????????????
    private static readonly Color Background  = Color.FromArgb(255, 252, 248, 240);
    private static readonly Color CellFill    = Color.FromArgb(255, 245, 238, 222);
    private static readonly Color CentreFill  = Color.FromArgb(255, 232, 220, 200);
    private static readonly Color LineColor   = Color.FromArgb(255,  80,  60,  40);
    private static readonly Color LagnaColor  = Color.FromArgb(255, 190,  30,  30);
    private static readonly Color SignColor   = Color.FromArgb(255, 110,  80,  50);
    private static readonly Color HouseColor  = Color.FromArgb(255, 140, 110,  70);
    private static readonly Color PlanetColor = Color.FromArgb(255,  20,  80, 180);
    private static readonly Color RetroColor  = Color.FromArgb(255, 180,  40,  40);

    private static readonly string[] SignAbbr =
        ["Ar","Ta","Ge","Ca","Le","Vi","Li","Sc","Sa","Cp","Aq","Pi"];
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    /// <summary>Generates the North Indian chart PNG.</summary>
    public static void GenerateChart(string outputPath, VedicChartData data, int size = 900)
    {
        using var bmp = new Bitmap(size, size);
        using var g   = Graphics.FromImage(bmp);
        g.SmoothingMode     = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        g.Clear(Background);

        int margin = size / 18;
        int sq     = size - margin * 2;   // side of the outer square
        int ox     = margin;
        int oy     = margin;

        // The 9 grid-node points (3×3 grid corners + centre)
        // Named by (col, row), 0-indexed from top-left
        PointF[,] node = GridNodes(ox, oy, sq);

        // Draw filled cells first, then lines on top
        DrawCells(g, node, sq);
        DrawGridLines(g, node, sq);
        DrawDiagonals(g, node, sq);

        // Place content in each of the 12 house triangles
        PlaceContent(g, node, sq, data);

        // Chart title in centre diamond
        DrawCentre(g, node, data);

        bmp.Save(outputPath, ImageFormat.Png);
    }

    // ?? Grid geometry ????????????????????????????????????????????????????????

    /// <summary>
    /// Returns the 4×4 grid of corner points.
    /// node[col, row] where col,row ? {0,1,2,3}.
    /// </summary>
    private static PointF[,] GridNodes(int ox, int oy, int sq)
    {
        float third = sq / 3f;
        var n = new PointF[4, 4];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                n[c, r] = new PointF(ox + c * third, oy + r * third);
        return n;
    }

    // Shorthand
    private static PointF N(PointF[,] n, int c, int r) => n[c, r];

    // ?? Cell drawing ????????????????????????????????????????????????????????

    private static void DrawCells(Graphics g, PointF[,] n, int sq)
    {
        using var cellBrush   = new SolidBrush(CellFill);
        using var centreBrush = new SolidBrush(CentreFill);

        // 12 triangular houses — each defined as a polygon of vertices
        foreach (var poly in HousePolygons(n))
            g.FillPolygon(cellBrush, poly);

        // Centre diamond
        g.FillPolygon(centreBrush, CentreDiamond(n));
    }

    private static void DrawGridLines(Graphics g, PointF[,] n, int sq)
    {
        using var pen = new Pen(LineColor, 2f);

        // Outer square
        g.DrawPolygon(pen, [N(n,0,0), N(n,3,0), N(n,3,3), N(n,0,3)]);

        // Inner square (rotated 45° — the centre diamond boundary lines are the diagonals)
        // Horizontal and vertical grid lines
        for (int i = 1; i <= 2; i++)
        {
            float third = sq / 3f;
            // Horizontal
            g.DrawLine(pen, n[0, i], n[3, i]);
            // Vertical
            g.DrawLine(pen, n[i, 0], n[i, 3]);
        }

        // Centre diamond outline
        using var diagPen = new Pen(LineColor, 2f);
        g.DrawPolygon(diagPen, CentreDiamond(n));
    }

    private static void DrawDiagonals(Graphics g, PointF[,] n, int sq)
    {
        using var pen = new Pen(LineColor, 1.5f);

        // Top-left cell: TL corner to inner-top & inner-left nodes
        g.DrawLine(pen, N(n,0,0), N(n,1,1));
        // Top-right cell
        g.DrawLine(pen, N(n,3,0), N(n,2,1));
        // Bottom-left cell
        g.DrawLine(pen, N(n,0,3), N(n,1,2));
        // Bottom-right cell
        g.DrawLine(pen, N(n,3,3), N(n,2,2));
    }

    // ?? House polygon definitions ????????????????????????????????????????????
    //
    // North Indian house layout (house offset 0 = Lagna at top, clockwise):
    //
    //   House offset ? grid region
    //    0  top-centre triangle       (between top-left and top-right cells)
    //    1  top-right cell top half
    //    2  right-centre triangle
    //    3  bottom-right cell bottom half
    //    4  bottom-centre triangle
    //    5  bottom-left cell bottom half
    //    6  left-centre triangle
    //    7  top-left cell top half
    //    8  top-left inner triangle
    //    9  top-right inner triangle
    //   10  bottom-right inner triangle
    //   11  bottom-left inner triangle

    private static PointF[][] HousePolygons(PointF[,] n)
    {
        return
        [
            // 0  Top-centre (Lagna): top-left?top-right?inner-top-left?inner-top-right
            [N(n,0,0), N(n,3,0), N(n,2,1), N(n,1,1)],

            // 1  Top-right corner: top-right?inner-top-right?inner-mid-right
            [N(n,3,0), N(n,3,1), N(n,2,1)],

            // 2  Right-centre: inner-top-right?right-top?right-bottom?inner-bot-right
            [N(n,3,1), N(n,3,2), N(n,2,2), N(n,2,1)],

            // 3  Bottom-right corner: right-bottom?bot-right?inner-bot-right
            [N(n,3,2), N(n,3,3), N(n,2,2)],

            // 4  Bottom-centre: bot-right?bot-left?inner-bot-left?inner-bot-right
            [N(n,3,3), N(n,0,3), N(n,1,2), N(n,2,2)],

            // 5  Bottom-left corner: bot-left?inner-bot-left?left-bottom
            [N(n,0,3), N(n,1,2), N(n,0,2)],

            // 6  Left-centre: left-top?left-bottom?inner-bot-left?inner-top-left
            [N(n,0,1), N(n,0,2), N(n,1,2), N(n,1,1)],

            // 7  Top-left corner: top-left?inner-top-left?left-top
            [N(n,0,0), N(n,1,1), N(n,0,1)],

            // 8  Inner upper-left triangle
            [N(n,1,1), N(n,2,1), N(n,1,2)],

            // 9  Inner upper-right triangle
            [N(n,2,1), N(n,2,2), N(n,1,2)],  // placeholder — merged with house 9

            // will be reassigned below; keep 12 entries
            [N(n,1,1), N(n,2,1), N(n,2,2), N(n,1,2)], // centre (not a house)
            [N(n,1,1), N(n,2,1), N(n,2,2), N(n,1,2)], // centre (not a house)
        ];
    }

    private static PointF[] CentreDiamond(PointF[,] n)
        => [N(n,1,1), N(n,2,1), N(n,2,2), N(n,1,2)];

    // ?? Content placement ????????????????????????????????????????????????????

    private static void PlaceContent(Graphics g, PointF[,] n, int sq, VedicChartData data)
    {
        float third = sq / 3f;
        float fontSize      = Math.Max(7f, third * 0.12f);
        float planetFontSz  = Math.Max(7f, third * 0.11f);
        float smallFontSz   = Math.Max(5f, third * 0.08f);

        using var signFont   = new Font("Arial", fontSize,     FontStyle.Bold);
        using var planetFont = new Font("Arial", planetFontSz, FontStyle.Bold);
        using var smallFont  = new Font("Arial", smallFontSz,  FontStyle.Regular);
        using var signBrush  = new SolidBrush(SignColor);
        using var houseBrush = new SolidBrush(HouseColor);
        using var pBrush     = new SolidBrush(PlanetColor);
        using var rBrush     = new SolidBrush(RetroColor);
        using var lagnaBrush = new SolidBrush(LagnaColor);

        // Group planets by sign index
        var bySign = new Dictionary<int, List<VedicPlanetPosition>>();
        foreach (var p in data.Planets)
        {
            if (!bySign.ContainsKey(p.SignIndex)) bySign[p.SignIndex] = [];
            bySign[p.SignIndex].Add(p);
        }

        // For each of the 12 houses (offset 0..11 clockwise from Lagna)
        for (int houseOffset = 0; houseOffset < 12; houseOffset++)
        {
            int signIdx = (data.AscendantSign + houseOffset) % 12;
            int houseNo = houseOffset + 1;
            PointF centre = HouseCentre(n, houseOffset);

            bool isLagna = houseOffset == 0;

            // Sign label
            string signText = isLagna ? SignNames[signIdx] : SignAbbr[signIdx];
            var    signF    = isLagna ? signFont : signFont;
            SizeF  sSz      = g.MeasureString(signText, signFont);
            var    sBrush   = isLagna ? lagnaBrush : signBrush;

            float signY = centre.Y - sSz.Height / 2f - planetFontSz * 1.6f;
            g.DrawString(signText, signFont, sBrush,
                centre.X - sSz.Width / 2f, signY);

            // For the Lagna cell: show "Asc" marker and degree on next line
            float nextY = signY + sSz.Height + 1f;
            if (isLagna)
            {
                string ascLabel = $"Asc {data.AscendantDegree:F2}°";
                SizeF  asSz    = g.MeasureString(ascLabel, smallFont);
                g.DrawString(ascLabel, smallFont, lagnaBrush,
                    centre.X - asSz.Width / 2f, nextY);
                nextY += smallFontSz + 2f;
            }

            // House number
            string houseText = houseNo.ToString();
            SizeF  hSz       = g.MeasureString(houseText, smallFont);
            g.DrawString(houseText, smallFont, houseBrush,
                centre.X - hSz.Width / 2f, nextY);

            // Planets
            if (bySign.TryGetValue(signIdx, out var here))
            {
                float py = centre.Y + planetFontSz * 0.5f;
                foreach (var p in here)
                {
                    string label = p.IsRetrograde ? $"{p.Name}(R)" : p.Name;
                    string deg   = $"{p.DegreeInSign:F0}°";
                    var    br    = p.IsRetrograde ? rBrush : pBrush;

                    SizeF lSz = g.MeasureString(label, planetFont);
                    g.DrawString(label, planetFont, br,
                        centre.X - lSz.Width / 2f, py);
                    py += planetFontSz + 1;

                    SizeF dSz = g.MeasureString(deg, smallFont);
                    g.DrawString(deg, smallFont, br,
                        centre.X - dSz.Width / 2f, py);
                    py += smallFontSz + 2;
                }
            }
        }
    }

    /// <summary>
    /// Returns the visual centre point for each house cell (0=top, clockwise).
    /// </summary>
    private static PointF HouseCentre(PointF[,] n, int houseOffset)
    {
        // Midpoints of the triangle/quadrilateral polygons
        return houseOffset switch
        {
            0  => Mid(N(n,0,0), N(n,3,0), N(n,2,1), N(n,1,1)),  // top centre
            1  => Mid(N(n,3,0), N(n,3,1), N(n,2,1)),              // top-right corner
            2  => Mid(N(n,3,1), N(n,3,2), N(n,2,2), N(n,2,1)),   // right
            3  => Mid(N(n,3,2), N(n,3,3), N(n,2,2)),              // bottom-right corner
            4  => Mid(N(n,3,3), N(n,0,3), N(n,1,2), N(n,2,2)),   // bottom centre
            5  => Mid(N(n,0,3), N(n,1,2), N(n,0,2)),              // bottom-left corner
            6  => Mid(N(n,0,1), N(n,0,2), N(n,1,2), N(n,1,1)),   // left
            7  => Mid(N(n,0,0), N(n,1,1), N(n,0,1)),              // top-left corner
            8  => Avg(N(n,1,1), N(n,2,1), N(n,1,2)),              // inner upper-left
            9  => Avg(N(n,2,1), N(n,2,2), N(n,1,2)),              // inner lower-right (mapped as H10 visually)
            10 => Avg(N(n,1,1), N(n,2,1), N(n,2,2)),              // inner upper-right
            11 => Avg(N(n,1,1), N(n,1,2), N(n,2,2)),              // inner lower-left
            _  => new PointF(0, 0)
        };
    }

    private static void DrawCentre(Graphics g, PointF[,] n, VedicChartData data)
    {
        PointF centre = Avg(N(n,1,1), N(n,2,1), N(n,2,2), N(n,1,2));
        float third   = Math.Abs(n[1,0].X - n[0,0].X);
        float fs      = Math.Max(6f, third * 0.085f);

        using var font  = new Font("Arial", fs, FontStyle.Bold);
        using var font2 = new Font("Arial", Math.Max(5f, third * 0.07f), FontStyle.Regular);
        using var brush = new SolidBrush(LineColor);

        string[] lines =
        [
            "North Indian",
            "Vedic Chart",
            data.ChartDate.ToString("dd MMM yyyy"),
            data.ChartDate.ToString("HH:mm") + " UTC",
        ];

        float totalH = lines.Length * (fs + 2);
        float y = centre.Y - totalH / 2f;
        foreach (var (line, i) in lines.Select((l, i) => (l, i)))
        {
            var f  = i < 2 ? font : font2;
            SizeF sz = g.MeasureString(line, f);
            g.DrawString(line, f, brush, centre.X - sz.Width / 2f, y);
            y += fs + 2;
        }
    }

    // ?? Geometry helpers ??????????????????????????????????????????????????????

    private static PointF Mid(PointF a, PointF b, PointF c, PointF d)
        => new((a.X + b.X + c.X + d.X) / 4f, (a.Y + b.Y + c.Y + d.Y) / 4f);

    private static PointF Mid(PointF a, PointF b, PointF c)
        => new((a.X + b.X + c.X) / 3f, (a.Y + b.Y + c.Y) / 3f);

    private static PointF Avg(params PointF[] pts)
        => new(pts.Average(p => p.X), pts.Average(p => p.Y));
}
