using AASharp;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Predicts longevity (lifespan) and potential year of passing based on
/// Lagna, 8th House, 8th Lord, and Saturn (Ayush Karaka).
/// </summary>
public static class LongevityPredictor
{
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    public static void Predict(VedicChartData natal)
    {
        int ascSign = natal.AscendantSign;
        int h8Sign  = (ascSign + 7) % 12;
        string h8Lord = SignRulers[h8Sign];
        string ascLord = SignRulers[ascSign];

        var ascLordPlanet = natal.Planets.FirstOrDefault(p => p.Name == ascLord);
        var h8LordPlanet = natal.Planets.FirstOrDefault(p => p.Name == h8Lord);
        var saturn = natal.Planets.FirstOrDefault(p => p.Name == "Sa");
        var jupiter = natal.Planets.FirstOrDefault(p => p.Name == "Ju");

        Console.WriteLine("  -- LONGEVITY & LIFESPAN PREDICTION --");
        Console.WriteLine($"  Lagna (1st House) : {SignNames[ascSign]}");
        Console.WriteLine($"  Lagna Lord        : {FullName(ascLord)}");
        Console.WriteLine($"  8th House         : {SignNames[h8Sign]}");
        Console.WriteLine($"  8th Lord          : {FullName(h8Lord)}");
        Console.WriteLine($"  Saturn (Ayush Karaka) : in {SignNames[saturn?.SignIndex ?? 0]}");
        Console.WriteLine();

        int lifespan = 70; // Base longevity

        // Lagna Lord strength
        if (ascLordPlanet != null)
        {
            int ascLordHouse = ((ascLordPlanet.SignIndex - ascSign + 12) % 12) + 1;
            if (ascLordHouse is 1 or 4 or 5 or 7 or 9 or 10) lifespan += 5; // Kendra/Trikona
            if (ascLordHouse is 6 or 8 or 12) lifespan -= 5;
        }

        // 8th Lord strength
        if (h8LordPlanet != null)
        {
            int h8LordHouse = ((h8LordPlanet.SignIndex - ascSign + 12) % 12) + 1;
            if (h8LordHouse is 8 or 11) lifespan += 5;
            if (h8LordHouse is 2 or 7) lifespan -= 5; // Maraka houses
        }

        // Saturn
        if (saturn != null)
        {
            int saturnHouse = ((saturn.SignIndex - ascSign + 12) % 12) + 1;
            if (saturnHouse == 8) lifespan += 8; // Saturn in 8th is excellent for longevity
            else if (saturnHouse is 2 or 7) lifespan -= 4; // Maraka placement
        }

        // Jupiter's grace
        if (jupiter != null)
        {
            int juHouse = ((jupiter.SignIndex - ascSign + 12) % 12) + 1;
            // Jupiter aspects 5th, 7th, 9th from its position
            int a1 = (juHouse + 4 - 1) % 12 + 1;
            int a2 = (juHouse + 6 - 1) % 12 + 1;
            int a3 = (juHouse + 8 - 1) % 12 + 1;

            if (juHouse == 1 || a1 == 1 || a2 == 1 || a3 == 1) lifespan += 5; // Jupiter aspects Lagna
            if (juHouse == 8 || a1 == 8 || a2 == 8 || a3 == 8) lifespan += 5; // Jupiter aspects 8th
        }

        // Afflictions in 8th house
        var planetsIn8th = natal.Planets.Where(p => p.SignIndex == h8Sign).ToList();
        foreach (var p in planetsIn8th)
        {
            if (p.Name is "Ra" or "Ke" or "Ma") lifespan -= 4;
            if (p.Name is "Ve" or "Me" or "Mo") lifespan += 2;
        }

        lifespan = Math.Clamp(lifespan, 30, 110); // Cap it

        string category = lifespan < 32 ? "Alpayu (Short Life)" : 
                          lifespan < 64 ? "Madhyayu (Average Life)" : 
                          lifespan >= 90 ? "Dirghayu (Exceptionally Long Life - 90+ years)" : "Purnayu (Long Life)";

        int birthYear = natal.ChartDate.Year;
        int deathYear = birthYear + lifespan;

        Console.WriteLine($"  Predicted Lifespan Base   : {lifespan} years");
        Console.WriteLine($"  Longevity Category        : {category}");

        string beyond90 = lifespan >= 90 ? "Yes, charts show excellent indications of crossing 90 years." : "No, base planetary strengths indicate a lifespan under 90.";
        Console.WriteLine($"  Will Person Live Beyond 90?: {beyond90}");
        Console.WriteLine($"  Predicted Year of Passing : Around {deathYear}");
        Console.WriteLine();
        Console.WriteLine("  Highly probable timing for major health crisis or passing occurs when transiting");
        Console.WriteLine("  Saturn and Rahu mutually afflict the 8th House or Maraka Houses (2nd, 7th).");
        Console.WriteLine("  --------------------------------------------------------------------------");

        // Predict windows using Saturn and Rahu afflicting 2nd, 7th, 8th houses around the predicted death year.
        // We'll search around deathYear - 3 to deathYear + 3
        DateTime start = new DateTime(Math.Max(1900, deathYear - 3), 1, 1);
        DateTime end   = new DateTime(Math.Min(2100, deathYear + 4), 1, 1);

        bool inWindow = false;
        DateTime windowStart = DateTime.MinValue;
        var windows = new List<(DateTime start, DateTime end)>();

        int h2Sign = (ascSign + 1) % 12;
        int h7Sign = (ascSign + 6) % 12;

        for (DateTime dt = start; dt <= end; dt = dt.AddDays(10))
        {
            double jd = ToJD(dt);
            double aya = LahiriAyanamsa(jd);

            double saT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude - aya);
            int saSign = (int)(saT / 30.0) % 12;

            double raT = Norm(AASMoon.MeanLongitudeAscendingNode(jd) - aya);
            int raSign = (int)(raT / 30.0) % 12;

            // Saturn Aspects: 1, 3, 7, 10
            bool saAs(int t) => saSign == t || (saSign+2)%12 == t || (saSign+6)%12 == t || (saSign+9)%12 == t;
            // Rahu Aspects: 1, 5, 7, 9
            bool raAs(int t) => raSign == t || (raSign+4)%12 == t || (raSign+6)%12 == t || (raSign+8)%12 == t;

            bool saBad = saAs(h8Sign) || saAs(h2Sign) || saAs(h7Sign);
            bool raBad = raAs(h8Sign) || raAs(h2Sign) || raAs(h7Sign);

            bool isRisk = saBad && raBad;

            if (isRisk && !inWindow)
            {
                inWindow = true;
                windowStart = dt;
            }
            else if (!isRisk && inWindow)
            {
                inWindow = false;
                if ((dt - windowStart).TotalDays >= 45) // Minimum 1.5 month continuous window
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
            Console.WriteLine("    Periods of increased vulnerability to life-threatening events:");
            foreach (var w in windows)
            {
                Console.WriteLine($"      * {w.start:MMMM yyyy}  to  {w.end:MMMM yyyy}");
            }
        }
        else
        {
            Console.WriteLine("    No specific double-malefic transits forming Maraka influences found in this range.");
        }
        Console.WriteLine();
    }

    private static string FullName(string s) => s switch
    {
        "Su" => "Sun", "Mo" => "Moon", "Ma" => "Mars", "Me" => "Mercury",
        "Ju" => "Jupiter", "Ve" => "Venus", "Sa" => "Saturn", "Ra" => "Rahu", "Ke" => "Ketu", _ => s
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