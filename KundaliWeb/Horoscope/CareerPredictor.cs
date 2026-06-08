using AASharp;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Predicts career-related timing: getting a job, promotion, salary increase, 
/// and warns against bad times for job changes.
/// Analyzes the 2nd, 6th, 10th, and 11th Houses and their lords.
/// </summary>
public static class CareerPredictor
{
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    public static void Predict(VedicChartData natal)
    {
        int ascSign = natal.AscendantSign;
        int h2Sign  = (ascSign + 1) % 12;  // Wealth, savings
        int h6Sign  = (ascSign + 5) % 12;  // Daily work, service
        int h10Sign = (ascSign + 9) % 12;  // Career, reputation, promotion
        int h11Sign = (ascSign + 10) % 12; // Income, gains

        string h10Lord = SignRulers[h10Sign];
        int h10LordSign = natal.Planets.FirstOrDefault(p => p.Name == h10Lord)?.SignIndex ?? -1;

        Console.WriteLine("  -- CAREER & FINANCIAL TIMING PREDICTIONS --");
        Console.WriteLine($"  Lagna (1st House) : {SignNames[ascSign]}");
        Console.WriteLine($"  10th House (Career)        : {SignNames[h10Sign]}  | Lord: {FullName(h10Lord)}");
        Console.WriteLine($"  6th House (Service/Jobs)   : {SignNames[h6Sign]}");
        Console.WriteLine($"  11th House (Income/Gains)  : {SignNames[h11Sign]}");
        Console.WriteLine($"  2nd House (Wealth)         : {SignNames[h2Sign]}");
        Console.WriteLine();

        // Predict Suitable Career Paths
        Console.WriteLine("  Suitable Career Paths (Based on 10th Lord):");
        string careerPath = h10Lord switch
        {
            "Su" => "Government roles, leadership, management, administration, medicine, or politics.",
            "Mo" => "Public relations, HR, caregiving, nursing, hospitality, food, or psychology.",
            "Ma" => "Engineering, military, police, sports, surgery, real estate, or construction.",
            "Me" => "IT, software, writing, accounting, finance, journalism, teaching, or business.",
            "Ju" => "Education, law, academia, banking, consulting, advisory, or philosophy.",
            "Ve" => "Arts, entertainment, fashion, beauty, luxury, design, media, or architecture.",
            "Sa" => "Agriculture, mining, manufacturing, heavy industry, administration, or routine systematic work.",
            _ => "Varied career paths depending on other planetary influences."
        };
        Console.WriteLine($"    * {FullName(h10Lord)} rules the 10th House: {careerPath}");

        var traditionalPlanets = new HashSet<string> { "Su", "Mo", "Ma", "Me", "Ju", "Ve", "Sa", "Ra", "Ke" };
        var planetsIn10th = natal.Planets.Where(p => p.SignIndex == h10Sign && traditionalPlanets.Contains(p.Name)).ToList();
        if (planetsIn10th.Count > 0)
        {
            Console.WriteLine("  Additional Influences (Planets in 10th House):");
            foreach (var p in planetsIn10th)
            {
                string influence = p.Name switch
                {
                    "Su" => "Brings authority and favor from leadership.",
                    "Mo" => "Indicates a career involving public interaction or fluctuating paths.",
                    "Ma" => "Adds drive, competitiveness, and technical/engineering skills.",
                    "Me" => "Emphasizes communication, intellect, and analytical skills in career.",
                    "Ju" => "Brings wisdom, teaching roles, and ethical/legal professions.",
                    "Ve" => "Brings creativity, diplomacy, and success in arts/luxury sectors.",
                    "Sa" => "Requires hard work and patience, often leads to stable long-term success.",
                    "Ra" => "Indicates unconventional career paths, ambition, and overseas connections.",
                    "Ke" => "Indicates tech fields, research, or frequent changes without attachment.",
                    _ => ""
                };
                Console.WriteLine($"    * {FullName(p.Name)}: {influence}");
            }
        }
        Console.WriteLine();

        DateTime start = DateTime.UtcNow.AddYears(-10);
        DateTime end   = start.AddYears(20); // Look ahead 10 years

        var jobWindows = new List<(DateTime start, DateTime end)>();
        var promotionWindows = new List<(DateTime start, DateTime end)>();
        var badChangeWindows = new List<(DateTime start, DateTime end)>();

        bool inJob = false; DateTime jobStart = DateTime.MinValue;
        bool inProm = false; DateTime promStart = DateTime.MinValue;
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

            // Jupiter Aspects: 1, 5, 7, 9
            bool juAspects(int t) => juSign == t || (juSign+4)%12 == t || (juSign+6)%12 == t || (juSign+8)%12 == t;

            // Saturn Aspects: 1, 3, 7, 10
            bool saAspects(int t) => saSign == t || (saSign+2)%12 == t || (saSign+6)%12 == t || (saSign+9)%12 == t;

            // Rahu Aspects: 1, 5, 7, 9
            bool raAspects(int t) => raSign == t || (raSign+4)%12 == t || (raSign+6)%12 == t || (raSign+8)%12 == t;

            // 1. Best time to get a job or change job: Jupiter blesses 6th or 10th
            bool isJobFav = juAspects(h6Sign) || juAspects(h10Sign) || (h10LordSign != -1 && juAspects(h10LordSign));

            // 2. Best time for promotion / salary increase: Jupiter blesses 10th and (11th or 2nd)
            bool isPromFav = juAspects(h10Sign) && (juAspects(h11Sign) || juAspects(h2Sign));

            // 3. Bad time for job changes: Saturn AND Rahu afflict 10th or 10th Lord, without Jupiter's grace
            bool saBad = saAspects(h10Sign) || (h10LordSign != -1 && saAspects(h10LordSign));
            bool raBad = raAspects(h10Sign) || (h10LordSign != -1 && raAspects(h10LordSign));
            bool isBadChange = saBad && raBad && !juAspects(h10Sign);

            // Job Tracker
            if (isJobFav && !inJob) { inJob = true; jobStart = dt; }
            else if (!isJobFav && inJob)
            {
                inJob = false;
                if ((dt - jobStart).TotalDays >= 30) jobWindows.Add((jobStart, dt));
            }

            // Promotion Tracker
            if (isPromFav && !inProm) { inProm = true; promStart = dt; }
            else if (!isPromFav && inProm)
            {
                inProm = false;
                if ((dt - promStart).TotalDays >= 30) promotionWindows.Add((promStart, dt));
            }

            // Bad Change Tracker
            if (isBadChange && !inBad) { inBad = true; badStart = dt; }
            else if (!isBadChange && inBad)
            {
                inBad = false;
                if ((dt - badStart).TotalDays >= 45) badChangeWindows.Add((badStart, dt));
            }
        }

        // Close open windows
        if (inJob && (end - jobStart).TotalDays >= 30) jobWindows.Add((jobStart, end));
        if (inProm && (end - promStart).TotalDays >= 30) promotionWindows.Add((promStart, end));
        if (inBad && (end - badStart).TotalDays >= 45) badChangeWindows.Add((badStart, end));

        Console.WriteLine("  [1] Best Timing to Get a New Job or Favorable Job Change");
        Console.WriteLine("      (Transit Jupiter positively influencing 6th or 10th House)");
        PrintWindows(jobWindows);

        Console.WriteLine("  [2] Best Timing for Promotion or Salary Increase");
        Console.WriteLine("      (Transit Jupiter simultaneously blessing 10th House + 2nd/11th House of Gains)");
        PrintWindows(promotionWindows);

        Console.WriteLine("  [3] Highly Unfavorable Timing for Quitting or Risky Job Changes");
        Console.WriteLine("      (Saturn and Rahu afflicting the 10th House without Jupiter's protection)");
        PrintWindows(badChangeWindows);

        Console.WriteLine("  --------------------------------------------------------------------------\n");
    }

    private static void PrintWindows(List<(DateTime start, DateTime end)> windows)
    {
        if (windows.Count == 0)
        {
            Console.WriteLine("      - No significant continuous windows found in the next 10 years.");
        }
        else
        {
            foreach (var w in windows)
            {
                Console.WriteLine($"      * {w.start:MMMM yyyy}  to  {w.end:MMMM yyyy}");
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