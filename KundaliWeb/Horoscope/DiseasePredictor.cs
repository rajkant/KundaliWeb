using AASharp;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Predicts potential diseases and health vulnerabilities based on the 6th House 
/// (disease), its ruler, and planets situated within it. Computes vulnerable 
/// timing periods dynamically.
/// </summary>
public static class DiseasePredictor
{
    private static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    private static readonly Dictionary<string, string> PlanetDiseases = new()
    {
        { "Su", "Heart issues, bone weakness, eye problems, excessive body heat/fevers" },
        { "Mo", "Mental stress, fluid imbalances, lung/chest issues, digestion sensitivity" },
        { "Ma", "Inflammations, blood pressure irregularities, prone to injuries or surgeries" },
        { "Me", "Nervous disorders, skin problems, speech or respiratory vulnerabilities" },
        { "Ju", "Liver problems, weight gain, sugar/diabetes issues, allergies" },
        { "Ve", "Kidney stress, reproductive system issues, hormonal imbalances" },
        { "Sa", "Chronic joint pain, bone issues, fatigue, rheumatism, slow-healing diseases" },
        { "Ra", "Hard-to-diagnose illnesses, sudden infections, toxicity, psychological fears" },
        { "Ke", "Mysterious infections, viral diseases, strange localized ailments" }
    };

    private static readonly Dictionary<int, string> SignDiseases = new()
    {
        { 0, "Headaches, brain-related fatigue, fevers, eye strain (Aries)" },
        { 1, "Throat infections, thyroid issues, neck/shoulder stiffness (Taurus)" },
        { 2, "Nervous system stress, arm/hand pain, lung vulnerability (Gemini)" },
        { 3, "Digestive issues, stomach sensitivity, chest/water retention (Cancer)" },
        { 4, "Heart vulnerability, upper spine/back pain, acidity (Leo)" },
        { 5, "Intestinal issues, digestion tracking, frequent nervous stomach (Virgo)" },
        { 6, "Kidney stress, lower back pain, urinary tract issues (Libra)" },
        { 7, "Reproductive organ issues, bowel/colon problems, hidden inflammation (Scorpio)" },
        { 8, "Hip/thigh pain, liver sluggishness, blood circulation issues (Sagittarius)" },
        { 9, "Knee problems, bone and joint stiffness, skeletal vulnerabilities (Capricorn)" },
        { 10, "Calf pain, ankle weakness, nervous system spasms (Aquarius)" },
        { 11, "Foot/toe problems, weakened immune system, lymphatic issues (Pisces)" }
    };

    private static readonly Dictionary<string, string> PlanetPrevention = new()
    {
        { "Su", "Maintain hydration, avoid excessive spicy/hot foods, practice cooling pranayama." },
        { "Mo", "Prioritize emotional peace, avoid cold/damp environments, practice meditation." },
        { "Ma", "Avoid reckless physical activities, limit red meat/spices, channel energy into regular exercise." },
        { "Me", "Take tech-breaks to rest the nervous system, prioritize sleep, practice deep breathing." },
        { "Ju", "Avoid overeating sweets/fats, maintain an active physical routine, support liver health." },
        { "Ve", "Maintain kidney health with adequate water, avoid excessive sugars/carbs, practice moderation." },
        { "Sa", "Keep joints warm, maintain flexibility through stretching/yoga, avoid extremely cold/stale food." },
        { "Ra", "Avoid intoxicants or unhygienic environments, practice grounding, maintain a clean diet." },
        { "Ke", "Boost immunity with natural herbs, keep surroundings hygienic, avoid mysterious alternative meds." }
    };

    private static readonly Dictionary<int, string> SignPrevention = new()
    {
        { 0, "Avoid extreme stress/anger; stay hydrated." },
        { 1, "Protect the throat, avoid overly cold drinks." },
        { 2, "Do breathing exercises, rest the arms/shoulders." },
        { 3, "Eat easily digestible food, monitor emotional health." },
        { 4, "Do cardiovascular exercises, reduce dietary cholesterol." },
        { 5, "Eat high-fiber meals, avoid eating when anxious." },
        { 6, "Drink plenty of water, ensure proper lower back support." },
        { 7, "Avoid highly processed foods, practice safe detox routines." },
        { 8, "Avoid heavy fatty foods, stay active and walk often." },
        { 9, "Take calcium/bone supplements, do low-impact joint exercises." },
        { 10, "Avoid prolonged standing/sitting, ensure good circulation." },
        { 11, "Protect feet, maintain strong immunity with vitamin C." }
    };

    public static void Predict(VedicChartData natal)
    {
        int ascSign = natal.AscendantSign;
        int h6Sign  = (ascSign + 5) % 12; // 6th House
        int h8Sign  = (ascSign + 7) % 12; // 8th House
        string h6Lord = SignRulers[h6Sign];

        Console.WriteLine("  -- HEALTH & DISEASE SUSCEPTIBILITY PREDICTION --");
        Console.WriteLine($"  Lagna (1st House) : {SignNames[ascSign]} (Overall Vitality)");
        Console.WriteLine($"  6th House         : {SignNames[h6Sign]} (Seat of Disease)");
        Console.WriteLine($"  6th Lord          : {FullName(h6Lord)}");
        Console.WriteLine();

        string ascLord = SignRulers[ascSign];
        var ascLordPlanet = natal.Planets.FirstOrDefault(p => p.Name == ascLord);
        var sunPlanet = natal.Planets.FirstOrDefault(p => p.Name == "Su");

        int vitalityScore = 10; // Base score

        if (ascLordPlanet != null)
        {
            int ascLordHouse = ((ascLordPlanet.SignIndex - ascSign + 12) % 12) + 1;
            if (ascLordHouse is 1 or 4 or 5 or 7 or 9 or 10 or 11) vitalityScore += 3;
            else if (ascLordHouse is 6 or 8 or 12) vitalityScore -= 3;
        }

        if (sunPlanet != null)
        {
            int sunHouse = ((sunPlanet.SignIndex - ascSign + 12) % 12) + 1;
            if (sunHouse is 1 or 3 or 6 or 9 or 10 or 11) vitalityScore += 2;
            else if (sunHouse is 8 or 12) vitalityScore -= 2;
        }

        var planetsInLagna = natal.Planets.Where(p => p.SignIndex == ascSign).ToList();
        foreach (var p in planetsInLagna)
        {
            if (p.Name is "Sa" or "Ra" or "Ke" or "Ma") vitalityScore -= 2;
            if (p.Name is "Ju" or "Ve" or "Me" or "Mo") vitalityScore += 2;
        }

        string overallHealth;
        if (vitalityScore >= 13)
        {
            overallHealth = "Strong constitution. Less physical chance of getting sick; quick recovery from illnesses.";
        }
        else if (vitalityScore >= 8)
        {
            overallHealth = "Moderate constitution. Average chance of getting sick. Can maintain health with proper balance.";
        }
        else
        {
            overallHealth = "Delicate constitution. More physical chance of getting sick; vulnerability is higher. Needs cautious diet and lifestyle.";
        }

        Console.WriteLine($"  Overall Health Profile    : {overallHealth}");
        Console.WriteLine();

        var planetsIn6th = natal.Planets
            .Where(p => p.SignIndex == h6Sign && PlanetDiseases.ContainsKey(p.Name))
            .ToList();

        Console.WriteLine("  Potential Health Vulnerabilities (Based on Natal Chart):");
        Console.WriteLine($"    * From 6th House Sign: {SignDiseases[h6Sign]}");

        foreach (var p in planetsIn6th)
        {
            Console.WriteLine($"    * From {FullName(p.Name)} in 6th House: {PlanetDiseases[p.Name]}");
        }

        var h6LordPlanet = natal.Planets.FirstOrDefault(p => p.Name == h6Lord);
        if (h6LordPlanet != null && !planetsIn6th.Any(p => p.Name == h6Lord))
        {
            Console.WriteLine($"    * From 6th Lord ({FullName(h6Lord)}): {PlanetDiseases[h6Lord]}");
        }

        Console.WriteLine();
        Console.WriteLine("  Advice to Avoid Frequent Illnesses (Preventative Measures):");
        Console.WriteLine($"    * For 6th House Sign: {SignPrevention[h6Sign]}");

        foreach (var p in planetsIn6th)
        {
            Console.WriteLine($"    * To balance {FullName(p.Name)} in 6th House: {PlanetPrevention[p.Name]}");
        }

        if (h6LordPlanet != null && !planetsIn6th.Any(p => p.Name == h6Lord))
        {
            Console.WriteLine($"    * To balance 6th Lord ({FullName(h6Lord)}): {PlanetPrevention[h6Lord]}");
        }

        Console.WriteLine();
        Console.WriteLine("  Highly probable timing for health vulnerability occurs when BOTH:");
        Console.WriteLine("  Transit Saturn AND Transit Rahu heavily afflict the 6th or 8th Houses.");
        Console.WriteLine("  --------------------------------------------------------------------------");

        DateTime start = DateTime.UtcNow;
        DateTime end   = start.AddYears(20);

        bool inWindow = false;
        DateTime windowStart = DateTime.MinValue;
        var windows = new List<(DateTime start, DateTime end)>();

        // Check every 10 days over next 20 years
        for (DateTime dt = start; dt <= end; dt = dt.AddDays(10))
        {
            double jd = ToJD(dt);
            double aya = LahiriAyanamsa(jd);

            double saT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude - aya);
            int saSign = (int)(saT / 30.0) % 12;

            double raT = Norm(AASMoon.MeanLongitudeAscendingNode(jd) - aya);
            int raSign = (int)(raT / 30.0) % 12;

            // Saturn Aspects: 1 (occupies), 3, 7, 10
            bool saAs(int t) => saSign == t || (saSign+2)%12 == t || (saSign+6)%12 == t || (saSign+9)%12 == t;

            // Rahu Aspects (using Jupiter-like 5, 7, 9 + occupies): 1, 5, 7, 9
            bool raAs(int t) => raSign == t || (raSign+4)%12 == t || (raSign+6)%12 == t || (raSign+8)%12 == t;

            bool saBad = saAs(h6Sign) || saAs(h8Sign);
            bool raBad = raAs(h6Sign) || raAs(h8Sign);

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
            Console.WriteLine("    Periods of increased vulnerability to health issues / disease:");
            foreach (var w in windows)
            {
                Console.WriteLine($"      * {w.start:MMMM yyyy}  to  {w.end:MMMM yyyy}");
            }
        }
        else
        {
            Console.WriteLine("    No severe double-malefic transits on health houses found in the next 20 years.");
        }
        Console.WriteLine();

        Console.WriteLine("  -- BEST TIMING FOR MEDICAL TREATMENTS OR SURGERY --");
        Console.WriteLine("  Favorable periods for treatment occur when Transit Jupiter protects/heals the Lagna");
        Console.WriteLine("  or 6th House, and simultaneously Transit Mars and Saturn do NOT afflict the 8th House.");
        Console.WriteLine("  --------------------------------------------------------------------------");

        DateTime treatStart = DateTime.UtcNow;
        DateTime treatEnd   = treatStart.AddYears(5); // Usually people look for surgery timing in near future

        bool inTreatWindow = false;
        DateTime treatWindowStart = DateTime.MinValue;
        var treatWindows = new List<(DateTime start, DateTime end)>();

        for (DateTime dt = treatStart; dt <= treatEnd; dt = dt.AddDays(10))
        {
            double jd = ToJD(dt);
            double aya = LahiriAyanamsa(jd);

            double juT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude - aya);
            int juSign = (int)(juT / 30.0) % 12;

            double maT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.MARS, false).ApparentGeocentricLongitude - aya);
            int maSign = (int)(maT / 30.0) % 12;

            double saT = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude - aya);
            int saSign = (int)(saT / 30.0) % 12;

            // Jupiter Aspects: 1, 5, 7, 9
            bool juHeals(int t) => juSign == t || (juSign+4)%12 == t || (juSign+6)%12 == t || (juSign+8)%12 == t;

            // Mars Aspects: 1, 4, 7, 8
            bool maAfflicts(int t) => maSign == t || (maSign+3)%12 == t || (maSign+6)%12 == t || (maSign+7)%12 == t;

            // Saturn Aspects: 1, 3, 7, 10
            bool saAfflicts(int t) => saSign == t || (saSign+2)%12 == t || (saSign+6)%12 == t || (saSign+9)%12 == t;

            bool isHealing = juHeals(ascSign) || juHeals(h6Sign);
            bool isSurgerySafe = !maAfflicts(h8Sign) && !saAfflicts(h8Sign);

            bool isFav = isHealing && isSurgerySafe;

            if (isFav && !inTreatWindow)
            {
                inTreatWindow = true;
                treatWindowStart = dt;
            }
            else if (!isFav && inTreatWindow)
            {
                inTreatWindow = false;
                if ((dt - treatWindowStart).TotalDays >= 20) // Minimum 3 week continuous window
                {
                    treatWindows.Add((treatWindowStart, dt));
                }
            }
        }

        if (inTreatWindow && (treatEnd - treatWindowStart).TotalDays >= 20)
        {
            treatWindows.Add((treatWindowStart, treatEnd));
        }

        if (treatWindows.Count > 0)
        {
            Console.WriteLine("    Highly favourable upcoming windows for treatment, procedures or surgeries:");
            foreach (var w in treatWindows)
            {
                Console.WriteLine($"      * {w.start:dd MMM yyyy}  to  {w.end:dd MMM yyyy}");
            }
        }
        else
        {
            Console.WriteLine("    No exceptionally strong continuous healing transits found in the next 5 years.");
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