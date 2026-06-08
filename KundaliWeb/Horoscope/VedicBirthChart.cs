using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/// <summary>
/// Renders a South Indian style Vedic birth chart as a PNG.
/// 
/// Layout — the 4×4 grid has 12 active cells arranged around the perimeter;
/// the 4 corner cells are fixed signs, and the inner 2×2 is blank (chart title area).
///
///  Col:  0      1      2      3
///  Row0: Pi    Ar     Ta     Ge
///  Row1: Aq   [title area]   Ca
///  Row2: Cp   [title area]   Le
///  Row3: Sa    Sc     Li     Vi
///
/// Signs are fixed; houses rotate relative to the Ascendant sign.
/// </summary>
public static class VedicBirthChart
{
    // Fixed sign index (0=Aries) for each of the 12 perimeter cells, reading
    // top-left ? top-right ? right-col top?bottom ? bottom-right?left ? left-col bottom?top
    // South Indian grid — sign positions (0-based, 0=Aries)
    //   Row 0 (top):    Pi=11  Ar=0   Ta=1   Ge=2
    //   Row 1 (mid):    Aq=10               Ca=3
    //   Row 2 (mid):    Cp=9                Le=4
    //   Row 3 (bot):    Sa=8   Sc=7   Li=6   Vi=5
    private static readonly (int row, int col, int sign)[] CellMap =
    [
        (0, 0, 11), (0, 1,  0), (0, 2,  1), (0, 3,  2),  // Pisces  Aries  Taurus  Gemini
        (1, 0, 10),                          (1, 3,  3),  // Aquarius               Cancer
        (2, 0,  9),                          (2, 3,  4),  // Capricorn              Leo
        (3, 0,  8), (3, 1,  7), (3, 2,  6), (3, 3,  5),  // Sagittarius Scorpio Libra Virgo
    ];

    private static readonly string[] SignAbbr =
        ["Ar", "Ta", "Ge", "Ca", "Le", "Vi", "Li", "Sc", "Sa", "Cp", "Aq", "Pi"];

    private static readonly Color GridBackground = Color.FromArgb(255, 252, 248, 240);
    private static readonly Color CellBackground = Color.FromArgb(255, 245, 238, 222);
    private static readonly Color CenterBg       = Color.FromArgb(255, 235, 225, 205);
    private static readonly Color GridLineColor  = Color.FromArgb(255,  90,  70,  50);
    private static readonly Color SignColor      = Color.FromArgb(255, 120,  90,  60);
    private static readonly Color PlanetColor    = Color.FromArgb(255,  20,  80, 180);
    private static readonly Color RetroColor     = Color.FromArgb(255, 180,  40,  40);
    private static readonly Color AscColor       = Color.FromArgb(255, 200,  40,  40);
    private static readonly Color HouseNumColor  = Color.FromArgb(255, 140, 110,  80);

    /// <summary>
    /// Generates a South Indian Vedic chart PNG from calculated chart data.
    /// </summary>
    public static void GenerateChart(string outputPath, VedicChartData data, int size = 900)
    {
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode     = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        g.Clear(GridBackground);

        int margin  = size / 20;
        int gridW   = size - margin * 2;
        int cellW   = gridW / 4;
        int cellH   = gridW / 4;
        int originX = margin;
        int originY = margin;

        // Draw outer border
        using var outerPen = new Pen(GridLineColor, 3f);
        g.DrawRectangle(outerPen, originX, originY, gridW, gridW);

        // Fill all 12 active cells and the center
        using var cellBrush   = new SolidBrush(CellBackground);
        using var centerBrush = new SolidBrush(CenterBg);

        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                var rect = CellRect(originX, originY, cellW, cellH, r, c);
                bool isCenter = (r == 1 || r == 2) && (c == 1 || c == 2);
                g.FillRectangle(isCenter ? centerBrush : cellBrush, rect);
            }

        // Draw grid lines
        using var gridPen = new Pen(GridLineColor, 1.5f);
        for (int i = 1; i < 4; i++)
        {
            int x = originX + i * cellW;
            int y = originY + i * cellH;
            g.DrawLine(gridPen, x, originY, x, originY + gridW);
            g.DrawLine(gridPen, originX, y, originX + gridW, y);
        }

        // Draw diagonal lines in corner cells (South Indian style)
        using var diagPen = new Pen(GridLineColor, 1.0f);
        DrawCornerDiagonals(g, diagPen, originX, originY, cellW, cellH);

        // Group planets by sign
        var planetsBySign = new Dictionary<int, List<VedicPlanetPosition>>();
        foreach (var p in data.Planets)
        {
            if (!planetsBySign.ContainsKey(p.SignIndex))
                planetsBySign[p.SignIndex] = [];
            planetsBySign[p.SignIndex].Add(p);
        }

        // Draw each cell
        float signFontSize   = Math.Max(7f, cellW * 0.12f);
        float planetFontSize = Math.Max(6f, cellW * 0.10f);
        float houseFontSize  = Math.Max(6f, cellW * 0.09f);
        float degFontSize    = Math.Max(5f, cellW * 0.08f);

        using var signFont   = new Font("Arial", signFontSize,  FontStyle.Bold);
        using var planetFont = new Font("Arial", planetFontSize, FontStyle.Bold);
        using var houseFont  = new Font("Arial", houseFontSize,  FontStyle.Regular);
        using var degFont    = new Font("Arial", degFontSize,    FontStyle.Regular);
        using var signBrush  = new SolidBrush(SignColor);
        using var planetBrush= new SolidBrush(PlanetColor);
        using var retroBrush = new SolidBrush(RetroColor);
        using var ascBrush   = new SolidBrush(AscColor);
        using var houseNumBrush = new SolidBrush(HouseNumColor);

        foreach (var (row, col, sign) in CellMap)
        {
            var rect = CellRect(originX, originY, cellW, cellH, row, col);
            int house = ((sign - data.AscendantSign + 12) % 12) + 1;
            bool isAscCell = sign == data.AscendantSign;

            // Sign abbreviation — top-left of cell
            string signText = SignAbbr[sign];
            g.DrawString(signText, signFont, signBrush,
                rect.Left + 4, rect.Top + 2);

            // House number — top-right of cell
            string houseText = house.ToString();
            SizeF hSz = g.MeasureString(houseText, houseFont);
            g.DrawString(houseText, houseFont, houseNumBrush,
                rect.Right - hSz.Width - 3, rect.Top + 2);

            // "Asc" marker on the ascendant cell
            if (isAscCell)
            {
                string ascText = $"Asc {data.AscendantDegree:F0}°";
                SizeF aSz = g.MeasureString(ascText, degFont);
                g.DrawString(ascText, degFont, ascBrush,
                    rect.Left + 4, rect.Top + signFontSize + 4);
            }

            // Planets in this sign
            if (planetsBySign.TryGetValue(sign, out var here))
            {
                float py = rect.Top + signFontSize + (isAscCell ? degFontSize + 8 : 4);
                foreach (var p in here)
                {
                    string pText = p.IsRetrograde ? $"{p.Name}(R)" : p.Name;
                    string dText = $"{p.DegreeInSign:F0}°";
                    var brush = p.IsRetrograde ? retroBrush : planetBrush;

                    // Keep within cell vertically
                    if (py + planetFontSize > rect.Bottom - 2) break;

                    g.DrawString(pText, planetFont, brush, rect.Left + 4, py);
                    SizeF pSz = g.MeasureString(pText, planetFont);
                    g.DrawString(dText, degFont, brush, rect.Left + 4 + pSz.Width, py + (planetFontSize - degFontSize));
                    py += planetFontSize + 2;
                }
            }
        }

        // Center area — chart title
        DrawCenterTitle(g, data, originX, originY, cellW, cellH, centerBrush);

        // Final outer border on top of everything
        g.DrawRectangle(outerPen, originX, originY, gridW, gridW);

        bitmap.Save(outputPath, ImageFormat.Png);
    }

    // ?? Private Helpers ?????????????????????????????????????????????????????

    private static void DrawCenterTitle(
        Graphics g, VedicChartData data,
        int originX, int originY, int cellW, int cellH,
        SolidBrush bg)
    {
        var center = new Rectangle(originX + cellW, originY + cellH, cellW * 2, cellH * 2);

        float titleFontSize = Math.Max(8f, cellW * 0.11f);
        float infoFontSize  = Math.Max(6f, cellW * 0.08f);

        using var titleFont = new Font("Arial", titleFontSize, FontStyle.Bold);
        using var infoFont  = new Font("Arial", infoFontSize,  FontStyle.Regular);
        using var titleBrush= new SolidBrush(GridLineColor);
        using var infoColor = new SolidBrush(SignColor);

        string[] lines =
        [
            "Vedic Birth Chart",
            data.ChartDate.ToString("dd MMM yyyy"),
            data.ChartDate.ToString("HH:mm") + " UTC",
            $"Lat {data.Latitude:F2}  Lon {data.Longitude:F2}",
            $"Ayanamsa {data.Ayanamsa:F2}°",
        ];

        float y = center.Top + center.Height * 0.12f;
        foreach (var (line, idx) in lines.Select((l, i) => (l, i)))
        {
            var font  = idx == 0 ? titleFont : infoFont;
            var brush = idx == 0 ? titleBrush : infoColor;
            SizeF sz  = g.MeasureString(line, font);
            g.DrawString(line, font, brush,
                center.Left + (center.Width - sz.Width) / 2f, y);
            y += sz.Height + 2;
        }
    }

    private static Rectangle CellRect(int ox, int oy, int cw, int ch, int row, int col)
        => new(ox + col * cw, oy + row * ch, cw, ch);

    private static void DrawCornerDiagonals(
        Graphics g, Pen pen,
        int ox, int oy, int cw, int ch)
    {
        // Top-left corner (Pisces)
        g.DrawLine(pen, ox, oy, ox + cw, oy + ch);
        // Top-right corner (Gemini)
        g.DrawLine(pen, ox + 3 * cw, oy, ox + 4 * cw, oy + ch);
        // Bottom-left corner (Sagittarius)
        g.DrawLine(pen, ox, oy + 3 * ch, ox + cw, oy + 4 * ch);
        // Bottom-right corner (Virgo)
        g.DrawLine(pen, ox + 3 * cw, oy + 3 * ch, ox + 4 * cw, oy + 4 * ch);
    }
}
