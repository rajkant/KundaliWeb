using AASharp;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Predicts potential marriage timing using the "Double Transit" (Gochar) principle
/// of Jupiter and Saturn over the 7th house, 7th lord, or Lagna.
/// </summary>
public static class MarriageTimingPredictor
{
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    public static void Predict(VedicChartData natal)
    {
        // Identify key marriage significators
        int ascSign = natal.AscendantSign;
        int h7Sign  = (ascSign + 6) % 12;
        string h7Lord = SignRulers[h7Sign];

        int h7LordSign = -1;
        var lordPlanet = natal.Planets.FirstOrDefault(p => p.Name == h7Lord);
        if (lordPlanet != null) h7LordSign = lordPlanet.SignIndex;

        int veSign = -1;
        var vePlanet = natal.Planets.FirstOrDefault(p => p.Name == "Ve");
        if (vePlanet != null) veSign = vePlanet.SignIndex;

        Console.WriteLine("  -- MARRIAGE TIMING PREDICTION (Double Transit Method over next 10 Years) --");
        Console.WriteLine($"  Lagna (1st House) : {SignNames[ascSign]}");
        Console.WriteLine($"  7th House         : {SignNames[h7Sign]}");
        Console.WriteLine($"  7th Lord          : {FullName(h7Lord)} in {SignNames[h7LordSign]}");
        Console.WriteLine($"  Venus (Karaka)    : in {SignNames[veSign]}");
        Console.WriteLine();
        Console.WriteLine("  Highly probable marriage windows typically occur when BOTH transit Jupiter");
        Console.WriteLine("  and Saturn aspect or occupy the 7th House, 7th Lord, Venus, or Lagna.");
        Console.WriteLine("  -----------------------------------------------------------------------");

        DateTime start = DateTime.UtcNow; 
        DateTime end   = start.AddYears(30);

        bool inWindow = false;
        DateTime windowStart = DateTime.MinValue;

        var windows = new List<(DateTime start, DateTime end)>();

        // Check every 10 days for transits
        for (DateTime dt = start; dt <= end; dt = dt.AddDays(10))
        {
            double jd = ToJD(dt);
            double aya = LahiriAyanamsa(jd);

            double juT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude - aya);
            int juSign = (int)(juT / 30.0) % 12;

            double saT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude - aya);
            int saSign = (int)(saT / 30.0) % 12;

            // Jupiter aspects: 1 (occupies), 5, 7, 9
            bool juAspects(int targetSign) => 
                juSign == targetSign || 
                (juSign + 4) % 12 == targetSign || 
                (juSign + 6) % 12 == targetSign || 
                (juSign + 8) % 12 == targetSign;

            // Saturn aspects: 1 (occupies), 3, 7, 10
            bool saAspects(int targetSign) => 
                saSign == targetSign || 
                (saSign + 2) % 12 == targetSign || 
                (saSign + 6) % 12 == targetSign || 
                (saSign + 9) % 12 == targetSign;

            bool juFav = juAspects(h7Sign) || juAspects(h7LordSign) || juAspects(ascSign) || juAspects(veSign);
            bool saFav = saAspects(h7Sign) || saAspects(h7LordSign) || saAspects(veSign);

            bool isFav = juFav && saFav;

            if (isFav && !inWindow)
            {
                inWindow = true;
                windowStart = dt;
            }
            else if (!isFav && inWindow)
            {
                inWindow = false;
                if ((dt - windowStart).TotalDays >= 45) // Minimum 1.5 month continuous window to be significant
                {
                    windows.Add((windowStart, dt));
                }
            }
        }

        if (inWindow && (end - windowStart).TotalDays >= 45)
        {
            windows.Add((windowStart, end));
        }

        if (windows.Count > 0)
        {
            foreach (var w in windows)
            {
                Console.WriteLine($"    * {w.start:MMMM yyyy}  to  {w.end:MMMM yyyy}");
            }
        }
        else
        {
            Console.WriteLine("    No strong double-transit windows found in the next 10 years.");
            Console.WriteLine("    (Note: Vimshottari Dasha or other methods may still indicate marriage).");
        }
        Console.WriteLine();
    }

    private static string FullName(string s) => s switch
    {
        "Su" => "Sun", "Mo" => "Moon", "Ma" => "Mars", "Me" => "Mercury",
        "Ju" => "Jupiter", "Ve" => "Venus", "Sa" => "Saturn", _ => s
    };

    private static double ToJD(DateTime utc) =>
        AASDate.DateToJD(utc.Year, utc.Month, utc.Day + (utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0) / 24.0, true);

    private static double LahiriAyanamsa(double jd) =>
        23.8553 + ((jd - 2451545.0) / 365.25) * 0.013956;

    private static double Norm(double d)
    {
        d %= 360;
        if (d < 0) d += 360;
        return d;
    }
}
