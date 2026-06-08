using AASharp;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Predicts the chances of relocation/foreign settlement and calculates
/// the financial impact (better or bad) during those relocation windows.
/// Uses 4th (home), 9th (travel), 12th (foreign lands) for relocation,
/// and 2nd, 11th, and 12th for financial conditions.
/// </summary>
public static class RelocationPredictor
{
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    public static void Predict(VedicChartData natal)
    {
        int ascSign = natal.AscendantSign;
        int h2Sign  = (ascSign + 1) % 12;  // Wealth
        int h4Sign  = (ascSign + 3) % 12;  // Home / Homeland
        int h9Sign  = (ascSign + 8) % 12;  // Long travel / Foreign
        int h11Sign = (ascSign + 10) % 12; // Gains
        int h12Sign = (ascSign + 11) % 12; // Settlement abroad / Losses

        string ascLord = SignRulers[ascSign];
        string h4Lord = SignRulers[h4Sign];

        var ascPlanet = natal.Planets.FirstOrDefault(p => p.Name == ascLord);
        var h4Planet = natal.Planets.FirstOrDefault(p => p.Name == h4Lord);
        var moon = natal.Planets.FirstOrDefault(p => p.Name == "Mo");
        var rahu = natal.Planets.FirstOrDefault(p => p.Name == "Ra");
        var ketu = natal.Planets.FirstOrDefault(p => p.Name == "Ke");

        Console.WriteLine("  -- RELOCATION & FOREIGN SETTLEMENT PREDICTION --");
        Console.WriteLine($"  Lagna (1st House)  : {SignNames[ascSign]}");
        Console.WriteLine($"  4th House (Home)   : {SignNames[h4Sign]} | Lord: {FullName(h4Lord)}");
        Console.WriteLine($"  9th House (Travel) : {SignNames[h9Sign]}");
        Console.WriteLine($"  12th House (Abroad): {SignNames[h12Sign]}");
        Console.WriteLine();

        int relocationScore = 0;
        List<string> factors = new();

        // 1. Ascendant Lord in 9th or 12th
        if (ascPlanet != null)
        {
            int ascHouse = ((ascPlanet.SignIndex - ascSign + 12) % 12) + 1;
            if (ascHouse is 9 or 12)
            {
                relocationScore += 2;
                factors.Add($"Lagna Lord {FullName(ascLord)} is in House {ascHouse} (favorable for foreign lands).");
            }
        }

        // 2. 4th Lord in 6, 8, 12 or 9
        if (h4Planet != null)
        {
            int h4House = ((h4Planet.SignIndex - ascSign + 12) % 12) + 1;
            if (h4House is 9 or 12)
            {
                relocationScore += 2;
                factors.Add($"4th Lord {FullName(h4Lord)} is in House {h4House} (tendency to leave homeland).");
            }
            if (h4House is 6 or 8)
            {
                relocationScore += 1;
                factors.Add($"4th Lord {FullName(h4Lord)} in House {h4House} (disruption in birthplace).");
            }
        }

        // 3. Moon in 9th or 12th
        if (moon != null)
        {
            int moHouse = ((moon.SignIndex - ascSign + 12) % 12) + 1;
            if (moHouse is 9 or 12)
            {
                relocationScore += 2;
                factors.Add($"Moon in House {moHouse} (mind inclined towards distant/foreign places).");
            }
        }

        // 4. Rahu or Ketu in 4th
        if (rahu?.SignIndex == h4Sign || ketu?.SignIndex == h4Sign)
        {
            relocationScore += 2;
            factors.Add($"Malefic ({FullName(rahu?.SignIndex == h4Sign ? "Ra" : "Ke")}) in the 4th House causes separation from birthplace.");
        }

        string chances = relocationScore switch
        {
            0 or 1 => "Low (Strong ties to homeland)",
            2 or 3 => "Moderate (Short-term foreign travel or relocation possible)",
            4 or 5 => "High (Long-term foreign settlement likely)",
            _ => "Very High (Destined for foreign lands / permanent settlement abroad)"
        };

        Console.WriteLine($"  Chances of Relocation : {chances} (Score: {relocationScore})");
        if (factors.Count > 0)
        {
            Console.WriteLine("  Key Planetary Indications:");
            foreach (var f in factors) Console.WriteLine($"    - {f}");
        }
        else
        {
            Console.WriteLine("  No major planetary combinations found pushing native away from birthplace.");
        }
        Console.WriteLine();

        Console.WriteLine("  Timing for Relocation & Financial Impact:");
        Console.WriteLine("  (Windows activated by Rahu/Saturn on travel houses, combined with Jupiter's wealth influence)");
        Console.WriteLine("  --------------------------------------------------------------------------");

        DateTime start = DateTime.UtcNow.AddYears(-2);
        DateTime end   = start.AddYears(12);

        var betterFinance = new List<(DateTime start, DateTime end)>();
        var badFinance = new List<(DateTime start, DateTime end)>();

        bool inBetter = false; DateTime betterStart = DateTime.MinValue;
        bool inBad = false; DateTime badStart = DateTime.MinValue;

        for (DateTime dt = start; dt <= end; dt = dt.AddDays(10))
        {
            double jd = ToJD(dt);
            double aya = LahiriAyanamsa(jd);

            double juT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude - aya);
            int juSign = (int)(juT / 30.0) % 12;

            double saT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude - aya);
            int saSign = (int)(saT / 30.0) % 12;

            double raT = Norm(AASMoon.MeanLongitudeAscendingNode(jd) - aya);
            int raSign = (int)(raT / 30.0) % 12;

            // Transits activating travel (9th, 12th) or disrupting home (4th)
            bool travelActive = raSign == h9Sign || raSign == h12Sign || raSign == h4Sign ||
                                saSign == h9Sign || saSign == h12Sign || saSign == h4Sign;

            // Jupiter aspects: 1, 5, 7, 9
            bool juAspects(int t) => juSign == t || (juSign+4)%12 == t || (juSign+6)%12 == t || (juSign+8)%12 == t;
            // Saturn Aspects: 1, 3, 7, 10
            bool saAspects(int t) => saSign == t || (saSign+2)%12 == t || (saSign+6)%12 == t || (saSign+9)%12 == t;
            // Rahu Aspects: 1, 5, 7, 9
            bool raAspects(int t) => raSign == t || (raSign+4)%12 == t || (raSign+6)%12 == t || (raSign+8)%12 == t;

            bool wealthFav = juAspects(h2Sign) || juAspects(h11Sign);
            bool wealthAfflict = saAspects(h2Sign) || saAspects(h11Sign) || raAspects(h2Sign) || raAspects(h11Sign) || saSign == h12Sign || raSign == h12Sign;

            bool isBetter = travelActive && wealthFav && !wealthAfflict;
            bool isBad = travelActive && wealthAfflict && !wealthFav;

            if (isBetter && !inBetter) { inBetter = true; betterStart = dt; }
            else if (!isBetter && inBetter)
            {
                inBetter = false;
                if ((dt - betterStart).TotalDays >= 40) betterFinance.Add((betterStart, dt));
            }

            if (isBad && !inBad) { inBad = true; badStart = dt; }
            else if (!isBad && inBad)
            {
                inBad = false;
                if ((dt - badStart).TotalDays >= 40) badFinance.Add((badStart, dt));
            }
        }

        if (inBetter && (end - betterStart).TotalDays >= 40) betterFinance.Add((betterStart, end));
        if (inBad && (end - badStart).TotalDays >= 40) badFinance.Add((badStart, end));

        Console.WriteLine("    [+] Relocation windows likely bringing Better Finances / Gains:");
        PrintWindows(betterFinance);

        Console.WriteLine("    [-] Relocation windows likely bringing Financial Losses / High Expenses:");
        PrintWindows(badFinance);

        Console.WriteLine("  --------------------------------------------------------------------------\n");
    }

    private static void PrintWindows(List<(DateTime start, DateTime end)> windows)
    {
        if (windows.Count == 0)
        {
            Console.WriteLine("        - No significant windows found in this period.");
        }
        else
        {
            foreach (var w in windows)
            {
                Console.WriteLine($"        * {w.start:MMM yyyy}  to  {w.end:MMM yyyy}");
            }
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