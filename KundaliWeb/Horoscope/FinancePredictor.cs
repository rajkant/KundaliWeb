using AASharp;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Predicts financial gains, wealth accumulation strategies, and periods prone to losses.
/// Analyzes the 2nd House (Wealth), 11th House (Gains), and 12th House (Losses/Expenses).
/// </summary>
public static class FinancePredictor
{
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    public static void Predict(VedicChartData natal)
    {
        int ascSign = natal.AscendantSign;
        int h2Sign  = (ascSign + 1) % 12;  // 2nd House (Wealth)
        int h11Sign = (ascSign + 10) % 12; // 11th House (Gains)
        int h12Sign = (ascSign + 11) % 12; // 12th House (Losses)

        string h2Lord = SignRulers[h2Sign];
        string h11Lord = SignRulers[h11Sign];

        Console.WriteLine("  -- WEALTH & FINANCE PREDICTION --");
        Console.WriteLine($"  Lagna (1st House) : {SignNames[ascSign]}");
        Console.WriteLine($"  2nd House (Wealth & Savings) : {SignNames[h2Sign]} | Lord: {FullName(h2Lord)}");
        Console.WriteLine($"  11th House (Income & Gains)  : {SignNames[h11Sign]} | Lord: {FullName(h11Lord)}");
        Console.WriteLine($"  12th House (Expenses/Losses) : {SignNames[h12Sign]}");
        Console.WriteLine();

        Console.WriteLine("  How to Increase Finances:");
        Console.WriteLine($"    * Your 2nd Lord is {FullName(h2Lord)}. {GetFinanceAdvice(h2Lord)}");
        if (h2Lord != h11Lord)
        {
            Console.WriteLine($"    * Your 11th Lord is {FullName(h11Lord)}. {GetFinanceAdvice(h11Lord)}");
        }
        Console.WriteLine("    * Jupiter is the primary significator of wealth. Maintain a generous and ethical nature to attract Jupiter's grace.");
        Console.WriteLine();

        Console.WriteLine("  Timing for Significant Financial Gains vs. Losses:");
        Console.WriteLine("  --------------------------------------------------------------------------");

        DateTime start = DateTime.UtcNow.AddYears(-2);
        DateTime end   = start.AddYears(12);

        var gainWindows = new List<(DateTime start, DateTime end)>();
        var lossWindows = new List<(DateTime start, DateTime end)>();

        bool inGain = false; DateTime gainStart = DateTime.MinValue;
        bool inLoss = false; DateTime lossStart = DateTime.MinValue;

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

            // Jupiter Aspects: 1, 5, 7, 9
            bool juAspects(int t) => juSign == t || (juSign+4)%12 == t || (juSign+6)%12 == t || (juSign+8)%12 == t;

            // Saturn Aspects: 1, 3, 7, 10
            bool saAspects(int t) => saSign == t || (saSign+2)%12 == t || (saSign+6)%12 == t || (saSign+9)%12 == t;

            // Rahu Aspects: 1, 5, 7, 9
            bool raAspects(int t) => raSign == t || (raSign+4)%12 == t || (raSign+6)%12 == t || (raSign+8)%12 == t;

            // Gain: Jupiter blessing 2nd or 11th, not heavily afflicted by Saturn/Rahu
            bool juFav = juAspects(h2Sign) || juAspects(h11Sign);
            bool saAfflict = saAspects(h2Sign) || saAspects(h11Sign);
            bool raAfflict = raAspects(h2Sign) || raAspects(h11Sign);

            bool isGain = juFav && !saAfflict && !raAfflict;

            // Loss: Saturn or Rahu afflicting 2nd, 11th, or occupying 12th without Jupiter's grace
            bool isLoss = ((saAspects(h2Sign) || raAspects(h2Sign)) && (saAspects(h11Sign) || raAspects(h11Sign))) 
                        || saSign == h12Sign || raSign == h12Sign;

            if (juAspects(h2Sign) || juAspects(h11Sign) || juAspects(h12Sign)) isLoss = false; // Jupiter protects

            // Gain Tracker
            if (isGain && !inGain) { inGain = true; gainStart = dt; }
            else if (!isGain && inGain)
            {
                inGain = false;
                if ((dt - gainStart).TotalDays >= 40) gainWindows.Add((gainStart, dt));
            }

            // Loss Tracker
            if (isLoss && !inLoss) { inLoss = true; lossStart = dt; }
            else if (!isLoss && inLoss)
            {
                inLoss = false;
                if ((dt - lossStart).TotalDays >= 40) lossWindows.Add((lossStart, dt));
            }
        }

        if (inGain && (end - gainStart).TotalDays >= 40) gainWindows.Add((gainStart, end));
        if (inLoss && (end - lossStart).TotalDays >= 40) lossWindows.Add((lossStart, end));

        Console.WriteLine("  [1] Highly Favorable Times for Financial Growth, Investments, and Profits");
        Console.WriteLine("      (Transit Jupiter positively influencing Wealth houses without malefic affliction)");
        PrintWindows(gainWindows);

        Console.WriteLine("  [2] Periods Prone to High Expenses, Financial Losses, or Stagnation");
        Console.WriteLine("      (Malefic influences on Wealth houses or 12th House activation)");
        PrintWindows(lossWindows);

        Console.WriteLine("  --------------------------------------------------------------------------\n");
    }

    private static string GetFinanceAdvice(string planet) => planet switch
    {
        "Su" => "Invest in gold, government bonds, or businesses with high visibility. Deal ethically and maintain a strong reputation to secure your wealth.",
        "Mo" => "Look into real estate, agriculture, food/hospitality, or liquid assets. Avoid erratic emotional spending.",
        "Ma" => "Property, land, engineering tech, or heavy industries offer good returns. Prevent impulsive or aggressive investments.",
        "Me" => "Diversify your portfolios. Trade, commerce, writing, tech, and strategic communication will multiply your wealth.",
        "Ju" => "Education, law, gold, counseling, or traditional banking. Seek wealth through righteous (dharmic) paths; charity will amplify your income.",
        "Ve" => "Art, luxury items, vehicles, fashion, or women-oriented businesses offer great returns. Balance spending on comforts.",
        "Sa" => "Long-term, slow-yielding structural investments like real estate, agriculture, or traditional blue-chip stocks. Avoid get-rich-quick schemes.",
        _ => "Focus on disciplined savings."
    };

    private static void PrintWindows(List<(DateTime start, DateTime end)> windows)
    {
        if (windows.Count == 0)
        {
            Console.WriteLine("      - No significant continuous windows found.");
        }
        else
        {
            foreach (var w in windows)
            {
                Console.WriteLine($"      * {w.start:MMM yyyy}  to  {w.end:MMM yyyy}");
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