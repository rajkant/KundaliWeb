using AASharp;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Predicts the risk of divorce/separation based on the 7th house, 7th lord, Venus, and Mars (Mangal Dosha).
/// Calculates potential vulnerable periods looking at Saturn and Rahu transits.
/// </summary>
public static class DivorcePredictor
{
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    public static void Predict(VedicChartData natal)
    {
        int ascSign = natal.AscendantSign;
        int h7Sign  = (ascSign + 6) % 12;
        string h7Lord = SignRulers[h7Sign];

        int h7LordSign = -1;
        var lordPlanet = natal.Planets.FirstOrDefault(p => p.Name == h7Lord);
        if (lordPlanet != null) h7LordSign = lordPlanet.SignIndex;

        int veSign = -1;
        var vePlanet = natal.Planets.FirstOrDefault(p => p.Name == "Ve");
        if (vePlanet != null) veSign = vePlanet.SignIndex;

        int maSign = -1;
        var maPlanet = natal.Planets.FirstOrDefault(p => p.Name == "Ma");
        if (maPlanet != null) maSign = maPlanet.SignIndex;

        Console.WriteLine("  -- MARRIAGE STABILITY / DIVORCE PREDICTION --");
        Console.WriteLine($"  Lagna (1st House) : {SignNames[ascSign]}");
        Console.WriteLine($"  7th House         : {SignNames[h7Sign]}");
        Console.WriteLine($"  7th Lord          : {FullName(h7Lord)} in {SignNames[h7LordSign]}");
        Console.WriteLine($"  Venus (Karaka)    : in {SignNames[veSign]}");
        Console.WriteLine();

        // 1. Calculate Risk Score
        int riskScore = 0;
        List<string> riskFactors = new List<string>();

        // Mangal Dosha (Mars in 1, 4, 7, 8, 12)
        int maHouse = ((maSign - ascSign + 12) % 12) + 1;
        if (maHouse is 1 or 4 or 7 or 8 or 12)
        {
            riskScore += 2;
            riskFactors.Add($"Kuja/Mangal Dosha present (Mars in House {maHouse}).");
        }

        // 7th Lord in Dusthanas (6, 8, 12)
        int h7LordHouse = ((h7LordSign - ascSign + 12) % 12) + 1;
        if (h7LordHouse is 6 or 8 or 12)
        {
            riskScore += 2;
            riskFactors.Add($"7th Lord {FullName(h7Lord)} is in a challenging House ({h7LordHouse}).");
        }

        // Malefics in 7th House
        var planetsIn7th = natal.Planets.Where(p => p.SignIndex == h7Sign).ToList();
        foreach (var p in planetsIn7th)
        {
            if (p.Name is "Sa" or "Ra" or "Ke" or "Su" or "Ma")
            {
                riskScore += 1;
                riskFactors.Add($"Malefic {FullName(p.Name)} occupies the 7th House.");
            }
        }

        // Venus in Dusthanas
        int veHouse = ((veSign - ascSign + 12) % 12) + 1;
        if (veHouse is 6 or 8 or 12)
        {
            riskScore += 1;
            riskFactors.Add($"Venus is in a challenging House ({veHouse}).");
        }

        // Evaluate risk level
        string riskLevel = riskScore switch
        {
            0 => "Low (Strong marriage indicators)",
            1 or 2 => "Moderate (Normal relationship challenges)",
            3 or 4 => "High (Significant relationship friction)",
            _ => "Very High (Prone to separation without careful management)"
        };

        Console.WriteLine($"  Divorce/Separation Risk : {riskLevel} (Score: {riskScore})");
        if (riskFactors.Count > 0)
        {
            Console.WriteLine("  Contributing Factors:");
            foreach (var f in riskFactors) Console.WriteLine($"    - {f}");
        }
        else
        {
            Console.WriteLine("  No major malefic afflictions to marriage indicators in native chart.");
        }
        Console.WriteLine();

        if (riskScore >= 1)
        {
            Console.WriteLine("  Highly probable timing for relationship friction or separation occurs when");
            Console.WriteLine("  transit Saturn AND transit Rahu simultaneously afflict the 7th House or 7th Lord.");
            Console.WriteLine("  -----------------------------------------------------------------------------");

            // Look over the next 20 years
            DateTime start = DateTime.UtcNow;
            DateTime end   = start.AddYears(20);

            bool inWindow = false;
            DateTime windowStart = DateTime.MinValue;
            var windows = new List<(DateTime start, DateTime end)>();

            for (DateTime dt = start; dt <= end; dt = dt.AddDays(10))
            {
                double jd = ToJD(dt);
                double aya = LahiriAyanamsa(jd);

                double saT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude - aya);
                int saSign = (int)(saT / 30.0) % 12;

                double raT = Norm(AASMoon.MeanLongitudeAscendingNode(jd) - aya);
                int raSign = (int)(raT / 30.0) % 12;

                // Saturn aspects: 1 (occupies), 3, 7, 10
                bool saAfflicts(int targetSign) => 
                    saSign == targetSign || 
                    (saSign + 2) % 12 == targetSign || 
                    (saSign + 6) % 12 == targetSign || 
                    (saSign + 9) % 12 == targetSign;

                // Rahu aspects: 1 (occupies), 5, 7, 9
                bool raAfflicts(int targetSign) => 
                    raSign == targetSign || 
                    (raSign + 4) % 12 == targetSign || 
                    (raSign + 6) % 12 == targetSign || 
                    (raSign + 8) % 12 == targetSign;

                bool saBad = saAfflicts(h7Sign) || saAfflicts(h7LordSign);
                bool raBad = raAfflicts(h7Sign) || raAfflicts(h7LordSign);

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
                Console.WriteLine("    Periods of increased vulnerability to separation / disputes:");
                foreach (var w in windows)
                {
                    Console.WriteLine($"      * {w.start:MMMM yyyy}  to  {w.end:MMMM yyyy}");
                }
            }
            else
            {
                Console.WriteLine("    No severe double-malefic transits on marriage indicators in the next 20 years.");
            }
        }
        else
        {
            Console.WriteLine("  Because the native risk is low, separation periods are not calculated.");
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