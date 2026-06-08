using AASharp;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Predicts progeny (children) details: number, gender tendency, and timing 
/// based on the 5th House, 5th Lord, 9th House, and Jupiter.
/// </summary>
public static class ChildrenPredictor
{
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    public static void Predict(VedicChartData natal)
    {
        int ascSign = natal.AscendantSign;
        int h5Sign  = (ascSign + 4) % 12;
        string h5Lord = SignRulers[h5Sign];

        int h9Sign = (ascSign + 8) % 12;

        int h5LordSign = -1;
        var lordPlanet = natal.Planets.FirstOrDefault(p => p.Name == h5Lord);
        if (lordPlanet != null) h5LordSign = lordPlanet.SignIndex;

        int juSign = -1;
        var juPlanet = natal.Planets.FirstOrDefault(p => p.Name == "Ju");
        if (juPlanet != null) juSign = juPlanet.SignIndex;

        Console.WriteLine("  -- PROGENY / CHILDREN PREDICTION --");
        Console.WriteLine($"  Lagna (1st House) : {SignNames[ascSign]}");
        Console.WriteLine($"  5th House         : {SignNames[h5Sign]}");
        Console.WriteLine($"  5th Lord          : {FullName(h5Lord)} in {SignNames[h5LordSign]}");
        Console.WriteLine($"  Jupiter (Karaka)  : in {SignNames[juSign]}");
        Console.WriteLine();

        // 1. Prediction of number of children (Simplified)
        var planetsIn5th = natal.Planets.Where(p => p.SignIndex == h5Sign).ToList();
        int baseCount = Math.Max(1, planetsIn5th.Count); // Base 1 child minimum usually

        // Add if Jupiter (Karaka) is well-placed (not in dusthanas 6, 8, 12)
        int juHouse = ((juSign - ascSign + 12) % 12) + 1;
        if (juHouse is not 6 and not 8 and not 12) baseCount++;

        // Add if 5th lord is strongly placed (Kendra/Trikona)
        int h5LordHouse = ((h5LordSign - ascSign + 12) % 12) + 1;
        if (h5LordHouse is 1 or 4 or 5 or 7 or 9 or 10 or 11) baseCount++;

        // Afflictions lower the count
        bool isRahuKetuIn5th = planetsIn5th.Any(p => p.Name is "Ra" or "Ke");
        if (isRahuKetuIn5th) baseCount--;

        // Clamp to a reasonable modern family size (0 to 4)
        int childCount = Math.Clamp(baseCount, 0, 4);

        // 2. Gender Indication
        // Saptamsha logic approach: odd signs = male, even signs = female
        int currentHouse = h5Sign;

        if (childCount == 0)
        {
            Console.WriteLine("  Predicted Number of Children : Focus required, potential delays seen.");
        }
        else
        {
            Console.WriteLine($"  Predicted Number of Children : ~{childCount} (Based on 5th house & Jupiter strength)");
            Console.WriteLine("  Gender Prediction per child (using traditional alternating house progression):");

            for (int i = 1; i <= childCount; i++)
            {
                // In Vedic astrology, 1st child is 5th house, 2nd is 7th, 3rd is 9th, etc.
                int childHouseSign = (ascSign + 4 + (i - 1) * 2) % 12;

                int childLordSign = -1;
                string childLord = SignRulers[childHouseSign];
                var cLordPlanet = natal.Planets.FirstOrDefault(p => p.Name == childLord);
                if (cLordPlanet != null) childLordSign = cLordPlanet.SignIndex;

                int mPoints = 0;
                int fPoints = 0;

                // Sign is odd (male) or even (female)
                if (childHouseSign % 2 == 0) mPoints++; else fPoints++;

                // Lord's sign is odd (male) or even (female)
                if (childLordSign != -1)
                {
                    if (childLordSign % 2 == 0) mPoints++; else fPoints++;
                }

                // Planets occupying this specific child's house
                var planetsInChildHouse = natal.Planets.Where(p => p.SignIndex == childHouseSign).ToList();
                foreach (var p in planetsInChildHouse)
                {
                    if (p.Name is "Su" or "Ma" or "Ju") mPoints += 2;
                    if (p.Name is "Mo" or "Ve" or "Ra") fPoints += 2;
                }

                string gender = mPoints > fPoints ? "Boy" :
                                fPoints > mPoints ? "Girl" :
                                (childHouseSign % 2 == 0 ? "Boy (Lean)" : "Girl (Lean)");

                Console.WriteLine($"    Child {i} (from House {((childHouseSign - ascSign + 12) % 12) + 1}): {gender}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("  Highly probable timing for childbirth occurs when BOTH transit Jupiter and");
        Console.WriteLine("  transit Saturn aspect or occupy the 5th House, 9th House, or the 5th Lord.");
        Console.WriteLine("  -----------------------------------------------------------------------");

        DateTime start = DateTime.UtcNow;
        //DateTime start = new DateTime(1981, 5, 4, 3, 45, 00);
        DateTime end   = start.AddYears(40);

        bool inWindow = false;
        DateTime windowStart = DateTime.MinValue;

        var windows = new List<(DateTime start, DateTime end, bool isMaleFavored)>();

        // Check every 10 days over next 10 years
        for (DateTime dt = start; dt <= end; dt = dt.AddDays(10))
        {
            double jd = ToJD(dt);
            double aya = LahiriAyanamsa(jd);

            double juT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude - aya);
            int transitJuSign = (int)(juT / 30.0) % 12;

            double saT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude - aya);
            int transitSaSign = (int)(saT / 30.0) % 12;

            bool juAspects(int targetSign) => 
                transitJuSign == targetSign || 
                (transitJuSign + 4) % 12 == targetSign || 
                (transitJuSign + 6) % 12 == targetSign || 
                (transitJuSign + 8) % 12 == targetSign;

            bool saAspects(int targetSign) => 
                transitSaSign == targetSign || 
                (transitSaSign + 2) % 12 == targetSign || 
                (transitSaSign + 6) % 12 == targetSign || 
                (transitSaSign + 9) % 12 == targetSign;

            bool juFav = juAspects(h5Sign) || juAspects(h9Sign) || juAspects(h5LordSign);
            bool saFav = saAspects(h5Sign) || saAspects(h9Sign) || saAspects(h5LordSign);

            // Also check for male potential (transit Jupiter occupying or aspecting an odd male sign during this window)
            bool juInOrAspectsMaleSign = (transitJuSign % 2 == 0) || 
                                         ((transitJuSign + 4) % 12 % 2 == 0) || 
                                         ((transitJuSign + 6) % 12 % 2 == 0) || 
                                         ((transitJuSign + 8) % 12 % 2 == 0);

            bool isFav = juFav && saFav;

            if (isFav && !inWindow)
            {
                inWindow = true;
                windowStart = dt;
            }
            else if (!isFav && inWindow)
            {
                inWindow = false;
                if ((dt - windowStart).TotalDays >= 45) // Minimum 1.5 month continuous window
                {
                    windows.Add((windowStart, dt, juInOrAspectsMaleSign));
                }
            }
        }

        if (inWindow && (end - windowStart).TotalDays >= 45)
        {
            // Compute the aspect again for the very end of the window (approximation)
            double jd = ToJD(end);
            double aya = LahiriAyanamsa(jd);
            double juT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude - aya);
            int transitJuSign = (int)(juT / 30.0) % 12;
            bool juInOrAspectsMaleSignEnd = (transitJuSign % 2 == 0) || 
                                         ((transitJuSign + 4) % 12 % 2 == 0) || 
                                         ((transitJuSign + 6) % 12 % 2 == 0) || 
                                         ((transitJuSign + 8) % 12 % 2 == 0);
            windows.Add((windowStart, end, juInOrAspectsMaleSignEnd));
        }

        if (windows.Count > 0)
        {
            foreach (var w in windows)
            {
                string maleNote = w.isMaleFavored ? " [Favourable for Male Child]" : "";
                Console.WriteLine($"    * {w.start:MMMM yyyy}  to  {w.end:MMMM yyyy}{maleNote}");
            }
        }
        else
        {
            Console.WriteLine("    No strong double-transit windows for childbirth found in the next 10 years.");
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