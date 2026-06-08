using AASharp;
using System.Text;

/// <summary>
/// Interprets a Vedic birth chart against today's transits to produce a
/// detailed personal daily horoscope.
///
/// Layers of interpretation:
///   1. Natal chart analysis  — Lagna, planets in houses/signs, lordships
///   2. Natal planet strength — exaltation, debilitation, own sign, friendly
///   3. Transit analysis      — each of 9 grahas over natal positions
///   4. Transit-to-natal insights — planetary returns, Sade Sati, Jupiter aspects
///   5. Moon: nakshatra, Tarabala, Chandra Bala
///   6. Per-area paragraphs: Self, Love, Career, Finance, Health, Spirituality
///   7. Remedies, affirmation, timing
/// </summary>
public static class VedicBirthChartInterpreter
{
    // ?? Sign constants ???????????????????????????????????????????????????????

    public static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] Elements =
        ["Fire","Earth","Air","Water","Fire","Earth",
         "Air","Water","Fire","Earth","Air","Water"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    // ?? Strength tables ??????????????????????????????????????????????????????

    private static readonly Dictionary<string, (int exalt, int debil)> ExaltDebil = new()
    {
        {"Su",(0,6)},  {"Mo",(1,7)},  {"Ma",(9,3)},  {"Me",(5,11)},
        {"Ju",(3,9)},  {"Ve",(11,5)}, {"Sa",(6,0)},
    };

    private static readonly Dictionary<string, int[]> OwnSigns = new()
    {
        {"Su",new[]{4}},    {"Mo",new[]{3}},     {"Ma",new[]{0,7}},
        {"Me",new[]{2,5}},  {"Ju",new[]{8,11}},  {"Ve",new[]{1,6}},
        {"Sa",new[]{9,10}},
    };

    private static readonly Dictionary<string, int[]> FriendSigns = new()
    {
        {"Su",new[]{0,3,7,8}},  {"Mo",new[]{0,3,6,8}},
        {"Ma",new[]{0,3,8,11}}, {"Me",new[]{1,4,5,6}},
        {"Ju",new[]{0,3,8,11}}, {"Ve",new[]{1,2,6,9}},
        {"Sa",new[]{1,2,6,9}},
    };

    // ?? 27 Nakshatras ????????????????????????????????????????????????????????

    private static readonly string[] Nakshatras =
    [
        "Ashwini","Bharani","Krittika","Rohini","Mrigashira","Ardra",
        "Punarvasu","Pushya","Ashlesha","Magha","Purva Phalguni","Uttara Phalguni",
        "Hasta","Chitra","Swati","Vishakha","Anuradha","Jyeshtha",
        "Mula","Purva Ashadha","Uttara Ashadha","Shravana","Dhanishtha","Shatabhisha",
        "Purva Bhadrapada","Uttara Bhadrapada","Revati"
    ];

    private static readonly string[] NakshatraLords =
    [
        "Ke","Ve","Su","Mo","Ma","Ra",
        "Ju","Sa","Me","Ke","Ve","Su",
        "Mo","Ma","Ra","Ju","Sa","Me",
        "Ke","Ve","Su","Mo","Ma","Ra",
        "Ju","Sa","Me"
    ];

    private static readonly string[] TarabalaMeaning =
    [
        "Janma (challenging — caution)","Sampat (wealth — favourable)","Vipat (danger — be careful)",
        "Kshema (wellbeing — auspicious)","Pratyari (opposition — avoid conflict)",
        "Sadhaka (achievement — push forward)","Vadha (obstacle — delays likely)",
        "Mitra (friend — support comes)","Atimitra (great friend — excellent day)"
    ];

    private static readonly string[] HouseNames =
    [
        "","Self & Personality","Wealth & Speech","Courage & Siblings",
        "Home & Mother","Children & Creativity","Health & Enemies",
        "Marriage & Partnerships","Longevity & Transformation","Dharma & Higher Learning",
        "Career & Public Life","Gains & Friendships","Losses & Moksha"
    ];

    // ?? Main API ?????????????????????????????????????????????????????????????

    /// <summary>
    /// Generates a full personal daily horoscope by combining natal chart analysis
    /// with today's sidereal transits.
    /// </summary>
    public static VedicPersonalHoroscope Interpret(VedicChartData natal, DateTime utcNow)
    {
        var transits = CalculateTransits(utcNow, natal.Ayanamsa);
        int lagna    = natal.AscendantSign;
        var natalMap = natal.Planets.ToDictionary(p => p.Name, p => p);

        // Moon nakshatra and Tarabala (from natal Moon nakshatra)
        int moonNakIdx   = (int)(transits["Mo"].Sid / (360.0 / 27.0)) % 27;
        int natalMoonNak = natalMap.TryGetValue("Mo", out var nMoon)
            ? (int)((nMoon.SignIndex * 30.0 + nMoon.DegreeInSign) / (360.0 / 27.0)) % 27 : 0;
        int taraIdx = ((moonNakIdx - natalMoonNak + 27) % 27) % 9;

        int overall = OverallScore(lagna, transits, natalMap);
        int love    = LoveScore(lagna, transits, natalMap);
        int career  = CareerScore(lagna, transits, natalMap);
        int finance = FinanceScore(lagna, transits, natalMap);
        int health  = HealthScore(lagna, transits, natalMap);
        int spirit  = SpiritScore(lagna, transits, natalMap, moonNakIdx);

        return new VedicPersonalHoroscope
        {
            Date             = utcNow.Date,
            LagnaSign        = SignNames[lagna],
            LagnaElement     = Elements[lagna],
            LagnaLord        = FullName(SignRulers[lagna]),
            LagnaDegree      = natal.AscendantDegree,
            Ayanamsa         = natal.Ayanamsa,
            MoonNakshatra    = Nakshatras[moonNakIdx],
            NakshatraLord    = FullName(NakshatraLords[moonNakIdx]),
            Tarabala         = TarabalaMeaning[taraIdx],
            ChandraBala      = ChandraBalaRating(House(transits["Mo"].Sign, lagna)),
            MoonTransitHouse = House(transits["Mo"].Sign, lagna),
            Overall          = overall,
            Love             = love,
            Career           = career,
            Finance          = finance,
            Health           = health,
            Spirituality     = spirit,
            NatalSummary     = BuildNatalSummary(lagna, natalMap),
            NatalHouseTable  = BuildHouseTable(lagna, natalMap),
            TransitTable     = BuildTransitTable(lagna, transits, natalMap),
            Overview         = BuildOverview(lagna, transits, natalMap, moonNakIdx, taraIdx, overall),
            SelfParagraph    = BuildSelfParagraph(lagna, transits, natalMap),
            LoveParagraph    = BuildLoveParagraph(lagna, transits, natalMap),
            CareerParagraph  = BuildCareerParagraph(lagna, transits, natalMap),
            FinanceParagraph = BuildFinanceParagraph(lagna, transits, natalMap),
            HealthParagraph  = BuildHealthParagraph(lagna, transits, natalMap),
            SpiritParagraph  = BuildSpiritParagraph(lagna, transits, natalMap, moonNakIdx),
            KeyTransits      = BuildKeyTransitInsights(lagna, transits, natalMap),
            Remedies         = BuildRemedies(lagna, transits, overall),
            FavourableTime   = FavourableTime(moonNakIdx),
            LuckyColor       = LuckyColor(lagna),
            LuckyNumber      = LuckyNumber(lagna, transits),
            LuckyGem         = LuckyGem(lagna),
            Affirmation      = Affirmation(lagna, overall),
            Tip              = Tip(lagna, transits, overall),
        };
    }

    /// <summary>Prints the full horoscope to the console.</summary>
    public static void Print(VedicPersonalHoroscope h)
    {
        string sep = new string('=', 64);
        Console.WriteLine($"+{sep}+");
        Console.WriteLine($"  VEDIC PERSONAL HOROSCOPE  --  {h.LagnaSign.ToUpper()} LAGNA");
        Console.WriteLine($"  {h.Date:dddd, dd MMMM yyyy}");
        Console.WriteLine($"+{sep}+");
        Console.WriteLine();
        Console.WriteLine($"  Lagna       : {h.LagnaSign} {h.LagnaDegree:F2} ({h.LagnaElement}) | Lord: {h.LagnaLord}");
        Console.WriteLine($"  Ayanamsa    : Lahiri {h.Ayanamsa:F4}");
        Console.WriteLine($"  Transit Moon: Nakshatra: {h.MoonNakshatra} (Lord: {h.NakshatraLord})");
        Console.WriteLine($"  Tarabala    : {h.Tarabala}");
        Console.WriteLine($"  Chandra Bala: {h.ChandraBala}  (Transit Moon in House {h.MoonTransitHouse})");
        Console.WriteLine();
        Console.WriteLine($"  Overall  {Stars(h.Overall)}   Love     {Stars(h.Love)}");
        Console.WriteLine($"  Career   {Stars(h.Career)}   Finance  {Stars(h.Finance)}");
        Console.WriteLine($"  Health   {Stars(h.Health)}   Spirit   {Stars(h.Spirituality)}");
        Console.WriteLine();

        Console.WriteLine("  -- NATAL CHART SUMMARY --");
        Console.WriteLine(h.NatalSummary);

        Console.WriteLine("  -- NATAL HOUSE OCCUPANTS --");
        Console.WriteLine(h.NatalHouseTable);

        Console.WriteLine("  -- TODAY'S TRANSIT TABLE --");
        Console.WriteLine($"  {"Graha",-10} {"Transit Sign",-14} {"TH",-4} {"Natal Sign",-14} {"NH",-4} {"Strength",-16} Status");
        Console.WriteLine("  " + new string('-', 76));
        foreach (var t in h.TransitTable)
            Console.WriteLine($"  {t.Planet,-10} {t.TransitSign,-14} H{t.TransitHouse,-3} {t.NatalSign,-14} H{t.NatalHouse,-3} {t.Strength,-16} {t.Status}");
        Console.WriteLine();

        Section("OVERVIEW",             h.Overview);
        Section("SELF & PERSONALITY",   h.SelfParagraph);
        Section("LOVE & MARRIAGE",      h.LoveParagraph);
        Section("CAREER & DHARMA",      h.CareerParagraph);
        Section("FINANCE & WEALTH",     h.FinanceParagraph);
        Section("HEALTH & VITALITY",    h.HealthParagraph);
        Section("SPIRITUALITY & KARMA", h.SpiritParagraph);

        if (h.KeyTransits.Count > 0)
        {
            Console.WriteLine("  KEY TRANSIT INSIGHTS");
            foreach (var k in h.KeyTransits)
                Console.WriteLine($"    * {k}");
            Console.WriteLine();
        }

        Console.WriteLine("  REMEDIES FOR TODAY");
        foreach (var r in h.Remedies)
            Console.WriteLine($"    * {r}");
        Console.WriteLine();

        Console.WriteLine($"  Lucky Color    : {h.LuckyColor}");
        Console.WriteLine($"  Lucky Number   : {h.LuckyNumber}");
        Console.WriteLine($"  Lucky Gem      : {h.LuckyGem}");
        Console.WriteLine($"  Favourable Time: {h.FavourableTime}");
        Console.WriteLine();
        Console.WriteLine($"  AFFIRMATION : \"{h.Affirmation}\"");
        Console.WriteLine($"  TODAY'S TIP : {h.Tip}");
        Console.WriteLine(new string('-', 66));
        Console.WriteLine();
    }

    // ?? Natal builders ????????????????????????????????????????????????????????

    private static string BuildNatalSummary(int lagna, Dictionary<string, VedicPlanetPosition> n)
    {
        var sb = new StringBuilder();
        string lagnaLord = SignRulers[lagna];
        if (n.TryGetValue(lagnaLord, out var ll))
        {
            int h     = House(ll.SignIndex, lagna);
            string str = PlanetStrength(lagnaLord, ll.SignIndex);
            sb.AppendLine($"  {SignNames[lagna]} Lagna -- ruled by {FullName(lagnaLord)} in House {h} ({SignNames[ll.SignIndex]}, {str}).");
        }
        foreach (var sym in new[]{"Su","Mo","Ma","Ju","Ve","Sa","Ra","Ke"})
        {
            if (!n.TryGetValue(sym, out var p)) continue;
            int h     = House(p.SignIndex, lagna);
            string str = PlanetStrength(sym, p.SignIndex);
            string r   = p.IsRetrograde ? " (R)" : "";
            sb.AppendLine($"  {FullName(sym),-10} in {SignNames[p.SignIndex],-13} H{h,-3} {str}{r}");
        }
        return sb.ToString();
    }

    private static string BuildHouseTable(int lagna, Dictionary<string, VedicPlanetPosition> n)
    {
        var map = Enumerable.Range(1, 12).ToDictionary(i => i, _ => new List<string>());
        foreach (var p in n.Values)
        {
            int h = House(p.SignIndex, lagna);
            map[h].Add(FullName(p.Name) + (p.IsRetrograde ? "(R)" : ""));
        }
        var sb = new StringBuilder();
        for (int h = 1; h <= 12; h++)
        {
            string sign = SignNames[(lagna + h - 1) % 12];
            string lord = SignRulers[(lagna + h - 1) % 12];
            string occ  = map[h].Count > 0 ? string.Join(", ", map[h]) : "Empty";
            sb.AppendLine($"  H{h,-3} {sign,-13} Lord:{FullName(lord),-10} | {HouseNames[h],-22} | {occ}");
        }
        return sb.ToString();
    }

    private static List<TransitRow> BuildTransitTable(int lagna,
        Dictionary<string, TransitPlanet> transits,
        Dictionary<string, VedicPlanetPosition> natal)
    {
        var list  = new List<TransitRow>();
        string[] order = ["Su","Mo","Ma","Me","Ju","Ve","Sa","Ra","Ke"];
        foreach (var sym in order)
        {
            if (!transits.TryGetValue(sym, out var tr)) continue;
            int tH = House(tr.Sign, lagna);
            natal.TryGetValue(sym, out var n);
            int    nH    = n != null ? House(n.SignIndex, lagna) : 0;
            string nSign = n != null ? SignNames[n.SignIndex] : "--";
            string str   = PlanetStrength(sym, tr.Sign) + (tr.Retro ? "(R)" : "");
            list.Add(new TransitRow
            {
                Planet       = FullName(sym),
                TransitSign  = SignNames[tr.Sign],
                TransitHouse = tH,
                NatalSign    = nSign,
                NatalHouse   = nH,
                Strength     = str,
                Status       = TransitStatus(sym, tH, tr.Retro),
            });
        }
        return list;
    }

    // ?? Scores (1-5) ?????????????????????????????????????????????????????????

    private static int OverallScore(int lg, Dictionary<string, TransitPlanet> t,
                                    Dictionary<string, VedicPlanetPosition> n)
    {
        int sc = 3;
        if (t.TryGetValue("Ju", out var ju) && !ju.Retro)
            sc += House(ju.Sign, lg) is 1 or 4 or 5 or 7 or 9 or 10 ? 1 : 0;
        if (t.TryGetValue("Sa", out var sa))
            sc += House(sa.Sign, lg) is 6 or 8 or 12 ? -1 : 0;
        if (t.TryGetValue("Mo", out var mo))
            sc += House(mo.Sign, lg) is 1 or 4 or 7 or 9 or 10 ? 1
                : House(mo.Sign, lg) is 6 or 8 or 12 ? -1 : 0;
        if (t.TryGetValue("Ra", out var ra))
            sc += House(ra.Sign, lg) is 1 or 8 or 12 ? -1 : 0;
        if (n.TryGetValue(SignRulers[lg], out var ll))
            sc += PlanetStrength(SignRulers[lg], ll.SignIndex) is "Exalted" or "Own Sign" ? 1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    private static int LoveScore(int lg, Dictionary<string, TransitPlanet> t,
                                  Dictionary<string, VedicPlanetPosition> n)
    {
        int sc = 3;
        if (t.TryGetValue("Ve", out var ve))
        {
            sc += House(ve.Sign, lg) is 5 or 7 ? 1 : House(ve.Sign, lg) is 6 or 8 or 12 ? -1 : 0;
            sc += ve.Retro ? -1 : 0;
        }
        if (t.TryGetValue("Mo", out var mo)) sc += House(mo.Sign, lg) is 5 or 7 ? 1 : 0;
        if (t.TryGetValue("Ma", out var ma)) sc += House(ma.Sign, lg) is 7 ? -1 : 0;
        if (n.TryGetValue("Ve", out var nVe)) sc += PlanetStrength("Ve", nVe.SignIndex) is "Exalted" ? 1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    private static int CareerScore(int lg, Dictionary<string, TransitPlanet> t,
                                    Dictionary<string, VedicPlanetPosition> n)
    {
        int sc = 3;
        if (t.TryGetValue("Su", out var su))
            sc += House(su.Sign, lg) is 10 or 1 ? 1 : House(su.Sign, lg) is 12 ? -1 : 0;
        if (t.TryGetValue("Ju", out var ju) && !ju.Retro)
            sc += House(ju.Sign, lg) is 10 or 11 ? 1 : 0;
        if (t.TryGetValue("Sa", out var sa))
            sc += sa.Retro || House(sa.Sign, lg) is 10 ? -1 : 0;
        if (t.TryGetValue("Me", out var me))
            sc += me.Retro ? -1 : House(me.Sign, lg) is 10 or 3 ? 1 : 0;
        if (n.TryGetValue("Su", out var nSu))
            sc += PlanetStrength("Su", nSu.SignIndex) is "Exalted" or "Own Sign" ? 1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    private static int FinanceScore(int lg, Dictionary<string, TransitPlanet> t,
                                     Dictionary<string, VedicPlanetPosition> n)
    {
        int sc = 3;
        if (t.TryGetValue("Ju", out var ju))
            sc += !ju.Retro && House(ju.Sign, lg) is 2 or 9 or 11 ? 1 : 0;
        if (t.TryGetValue("Ve", out var ve))
            sc += House(ve.Sign, lg) is 2 or 11 ? 1 : House(ve.Sign, lg) is 12 ? -1 : 0;
        if (t.TryGetValue("Ra", out var ra))
            sc += House(ra.Sign, lg) is 2 or 11 ? 1 : House(ra.Sign, lg) is 8 or 12 ? -1 : 0;
        if (t.TryGetValue("Sa", out var sa))
            sc += House(sa.Sign, lg) is 2 ? -1 : 0;
        if (n.TryGetValue("Ju", out var nJu))
            sc += PlanetStrength("Ju", nJu.SignIndex) is "Exalted" ? 1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    private static int HealthScore(int lg, Dictionary<string, TransitPlanet> t,
                                    Dictionary<string, VedicPlanetPosition> n)
    {
        int sc = 3;
        if (t.TryGetValue("Ma", out var ma))
            sc += House(ma.Sign, lg) is 1 or 6 or 8 ? -1 : ma.Retro ? -1 : 0;
        if (t.TryGetValue("Sa", out var sa))
            sc += House(sa.Sign, lg) is 6 or 8 ? -1 : 0;
        if (t.TryGetValue("Ke", out var ke))
            sc += House(ke.Sign, lg) is 6 ? -1 : 0;
        if (t.TryGetValue("Ju", out var ju))
            sc += !ju.Retro && House(ju.Sign, lg) is 1 or 5 ? 1 : 0;
        if (n.TryGetValue("Ma", out var nMa))
            sc += House(nMa.SignIndex, lg) is 6 or 8 ? -1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    private static int SpiritScore(int lg, Dictionary<string, TransitPlanet> t,
                                    Dictionary<string, VedicPlanetPosition> n, int moonNakIdx)
    {
        int sc = 3;
        if (t.TryGetValue("Ju", out var ju))
            sc += !ju.Retro && House(ju.Sign, lg) is 9 or 12 or 1 ? 1 : 0;
        if (t.TryGetValue("Ke", out var ke))
            sc += House(ke.Sign, lg) is 9 or 12 ? 1 : 0;
        if (t.TryGetValue("Mo", out var mo))
            sc += House(mo.Sign, lg) is 9 or 12 or 4 ? 1 : 0;
        sc += moonNakIdx is 0 or 7 or 12 or 16 or 20 ? 1 : 0;
        if (n.TryGetValue("Ju", out var nJu))
            sc += House(nJu.SignIndex, lg) is 9 or 12 ? 1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    // ?? Narrative builders ????????????????????????????????????????????????????

    private static string BuildOverview(int lg, Dictionary<string, TransitPlanet> t,
        Dictionary<string, VedicPlanetPosition> n, int moonNakIdx, int taraIdx, int overall)
    {
        var sb = new StringBuilder();
        string lagnaLord = SignRulers[lg];

        if (t.TryGetValue(lagnaLord, out var llT))
        {
            int h = House(llT.Sign, lg);
            string str = PlanetStrength(lagnaLord, llT.Sign);
            sb.Append($"Your Lagna lord {FullName(lagnaLord)} transits {SignNames[llT.Sign]} (House {h}, {str.ToLower()}) today. ");
        }
        if (n.TryGetValue(lagnaLord, out var llN))
        {
            int h = House(llN.SignIndex, lg);
            string str = PlanetStrength(lagnaLord, llN.SignIndex);
            sb.Append($"Natally placed in House {h} ({str.ToLower()}), ");
            sb.Append(str is "Exalted" or "Own Sign"
                ? "your inherent strength supports you through challenges. "
                : str is "Debilitated"
                ? "conscious effort is needed to overcome innate limitations. "
                : "it channels its energy into that house's affairs. ");
        }
        string taraNote = taraIdx is 2 or 4 or 6
            ? $"Today's Tarabala is {TarabalaMeaning[taraIdx]} — exercise extra caution. "
            : $"Tarabala is {TarabalaMeaning[taraIdx]} — a {(taraIdx is 1 or 3 or 5 or 7 or 8 ? "supportive" : "neutral")} backdrop. ";
        sb.Append(taraNote);
        sb.Append($"The Moon occupies {Nakshatras[moonNakIdx]} nakshatra ruled by {FullName(NakshatraLords[moonNakIdx])}, shaping today's emotional tone. ");
        sb.Append(overall >= 4 ? "Overall the configuration is auspicious — act with confidence and purpose."
                : overall <= 2 ? "The cosmic weather calls for patience, prayer, and conservation of energy."
                : "Mixed transits call for balanced, measured action.");
        return sb.ToString();
    }

    private static string BuildSelfParagraph(int lg, Dictionary<string, TransitPlanet> t,
                                              Dictionary<string, VedicPlanetPosition> n)
    {
        var sb = new StringBuilder();
        string lagnaLord = SignRulers[lg];
        if (t.TryGetValue(lagnaLord, out var llT))
        {
            int h = House(llT.Sign, lg);
            sb.Append(h is 1
                ? $"Your Lagna lord {FullName(lagnaLord)} graces your Lagna today — personal magnetism, vitality, and first impressions are heightened. Lead from the front. "
                : h is 10 ? "Your Lagna lord in the 10th directs personal energy toward career and public achievement. "
                : h is 6  ? "Your Lagna lord in the 6th sharpens competitive drive though physical vitality needs care. "
                : h is 8 or 12 ? $"Your Lagna lord in House {h} favours inner work over outward push today. "
                : $"Your Lagna lord {FullName(lagnaLord)} in House {h} focuses energy on {HouseNames[h].ToLower()}. ");
        }
        if (t.TryGetValue("Su", out var su))
        {
            int h = House(su.Sign, lg);
            sb.Append(h is 1  ? "The transit Sun in your Lagna brings a surge of authority and self-awareness. "
                    : h is 10 ? "The Sun illuminates your 10th house — leadership and recognition are strongly favoured. "
                    : $"The Sun transits House {h}, lighting up {HouseNames[h].ToLower()}. ");
        }
        if (n.TryGetValue("Su", out var nSu))
            sb.Append($"Natally your Sun in {SignNames[nSu.SignIndex]} (H{House(nSu.SignIndex, lg)}, {PlanetStrength("Su", nSu.SignIndex).ToLower()}) defines the core of your identity and purpose. ");
        return sb.ToString().Trim();
    }

    private static string BuildLoveParagraph(int lg, Dictionary<string, TransitPlanet> t,
                                              Dictionary<string, VedicPlanetPosition> n)
    {
        var sb = new StringBuilder();
        t.TryGetValue("Ve", out var ve); t.TryGetValue("Mo", out var mo);
        t.TryGetValue("Ma", out var ma); t.TryGetValue("Ju", out var ju);
        int h7sign  = (lg + 6) % 12;
        n.TryGetValue(SignRulers[h7sign], out var n7lord);

        if (ve != null)
        {
            int vH = House(ve.Sign, lg);
            sb.Append(ve.Retro
                ? "Venus retrograde resurfaces unresolved relationship matters — heal and clarify rather than initiating new romantic connections. "
                : vH is 7 ? "Transit Venus activates your 7th house — deeply auspicious for partnership discussions and romantic harmony. "
                : vH is 5 ? "Venus in your 5th sparks romance and creative joy — express love openly and spontaneously. "
                : vH is 2 ? "Venus in the 2nd favours love expressed through gifts, shared meals, and building material foundations together. "
                : vH is 12 ? "Venus in the 12th turns love inward — spiritual bonds and karmic connections are highlighted. "
                : vH is 6 or 8 ? $"Venus in House {vH} may create friction in relationships — patience and non-reactivity are your best tools. "
                : $"Venus transits House {vH}, gracing {HouseNames[vH].ToLower()} with charm and harmony. ");
        }
        if (n7lord != null)
        {
            int h = House(n7lord.SignIndex, lg);
            string str = PlanetStrength(SignRulers[h7sign], n7lord.SignIndex);
            sb.Append($"Natally your 7th lord {FullName(SignRulers[h7sign])} in House {h} ({str.ToLower()}) — ");
            sb.Append(str is "Exalted" or "Own Sign" ? "indicates strong, fulfilling partnerships. "
                    : str is "Debilitated" ? "relationship patterns require conscious healing and communication. "
                    : "brings steady, evolving partnership dynamics. ");
        }
        if (ma != null && House(ma.Sign, lg) == 7)
            sb.Append("Mars in your 7th house may spark arguments — choose words carefully and avoid confrontations today. ");
        if (ju != null && !ju.Retro && House(ju.Sign, lg) is 5 or 7)
            sb.Append("Jupiter's benefic blessing on your relationship houses brings wisdom, generosity, and mutual growth. ");
        int lScore = LoveScore(lg, t, n);
        sb.Append(lScore >= 4 ? "Express your feelings with confidence — the stars favour love today."
                : lScore <= 2 ? "Give relationships space to breathe; patience now prevents lasting friction."
                : "Honest, warm communication will strengthen your most important bonds.");
        return sb.ToString();
    }

    private static string BuildCareerParagraph(int lg, Dictionary<string, TransitPlanet> t,
                                                Dictionary<string, VedicPlanetPosition> n)
    {
        var sb = new StringBuilder();
        t.TryGetValue("Su", out var su); t.TryGetValue("Me", out var me);
        t.TryGetValue("Ju", out var ju); t.TryGetValue("Sa", out var sa);
        int h10sign = (lg + 9) % 12;
        n.TryGetValue(SignRulers[h10sign], out var n10lord);

        if (su != null)
        {
            int h = House(su.Sign, lg);
            sb.Append(h is 10 ? "The Sun blazing through your 10th spotlights career — authority figures notice your work and leadership opportunities arise. "
                    : h is 1  ? "The Sun in your Lagna gives a surge of personal drive that fuels professional initiatives. "
                    : h is 6  ? "The Sun in the 6th sharpens your competitive instincts and capacity to overcome workplace obstacles. "
                    : $"The Sun transits House {h}, illuminating {HouseNames[h].ToLower()}. ");
        }
        if (me != null)
            sb.Append(me.Retro
                ? "Mercury retrograde: avoid signing contracts, launching projects, or making major announcements — review and refine instead. "
                : House(me.Sign, lg) is 10 ? "Mercury in the 10th enhances strategic thinking and professional communication. "
                : House(me.Sign, lg) is 3  ? "Mercury in the 3rd favours networking, writing, and collaborative work. " : "");
        if (n10lord != null)
        {
            string str = PlanetStrength(SignRulers[h10sign], n10lord.SignIndex);
            int h = House(n10lord.SignIndex, lg);
            sb.Append($"Your natal 10th lord {FullName(SignRulers[h10sign])} in House {h} ({str.ToLower()}) — ");
            sb.Append(str is "Exalted" or "Own Sign" ? "strong professional potential and natural recognition. "
                    : str is "Debilitated" ? "career requires extra effort and resilience to achieve recognition. "
                    : "steady, developing professional trajectory. ");
        }
        if (sa != null)
            sb.Append(sa.Retro ? "Saturn retrograde: review long-term career structures and correct strategic weaknesses. "
                    : House(sa.Sign, lg) is 10 ? "Saturn in the 10th demands patient discipline — steady progress, not shortcuts. " : "");
        int cScore = CareerScore(lg, t, n);
        sb.Append(cScore >= 4 ? "Decisive professional action is strongly supported today."
                : cScore <= 2 ? "Focus on preparation and inner clarity rather than launching major career moves."
                : "Consistent, focused effort will advance your professional goals.");
        return sb.ToString();
    }

    private static string BuildFinanceParagraph(int lg, Dictionary<string, TransitPlanet> t,
                                                  Dictionary<string, VedicPlanetPosition> n)
    {
        var sb = new StringBuilder();
        t.TryGetValue("Ju", out var ju); t.TryGetValue("Ve", out var ve);
        t.TryGetValue("Ra", out var ra); t.TryGetValue("Sa", out var sa);
        int h2sign = (lg + 1) % 12;
        n.TryGetValue(SignRulers[h2sign], out var n2lord);

        if (ju != null)
        {
            int h = House(ju.Sign, lg);
            sb.Append(!ju.Retro && h is 2 or 11
                ? "Jupiter in your wealth houses is the most auspicious transit for financial growth — calculated investments are strongly favoured. "
                : ju.Retro && h is 2 or 11
                ? "Jupiter retrograde in your wealth houses: reclaim owed money and review investments rather than initiating new ones. "
                : !ju.Retro && h is 9
                ? "Jupiter in the 9th brings fortune through righteous action and support from mentors. " : "");
        }
        if (n2lord != null)
        {
            string str = PlanetStrength(SignRulers[h2sign], n2lord.SignIndex);
            int h = House(n2lord.SignIndex, lg);
            sb.Append($"Your natal 2nd lord {FullName(SignRulers[h2sign])} in House {h} ({str.ToLower()}) — ");
            sb.Append(str is "Exalted" or "Own Sign" ? "strong natural capacity to accumulate and protect wealth. "
                    : str is "Debilitated" ? "financial discipline must be actively cultivated. "
                    : "steady, manageable financial conditions. ");
        }
        if (ra != null)
        {
            int h = House(ra.Sign, lg);
            sb.Append(h is 2 or 11 ? "Rahu in your gain houses may bring sudden or unconventional financial gains — stay alert. "
                    : h is 8 or 12 ? "Rahu in a difficult house warns of hidden losses — scrutinise financial dealings carefully. " : "");
        }
        if (sa != null && House(sa.Sign, lg) is 2)
            sb.Append("Saturn in the 2nd demands fiscal discipline — review budgets and plan for the long term. ");
        int fScore = FinanceScore(lg, t, n);
        sb.Append(fScore >= 4 ? "Financial momentum is building — invest wisely in value-adding assets."
                : fScore <= 2 ? "Postpone major financial commitments; conservative money management is your best strategy."
                : "Steady, disciplined financial habits will accumulate meaningful results.");
        return sb.ToString();
    }

    private static string BuildHealthParagraph(int lg, Dictionary<string, TransitPlanet> t,
                                                Dictionary<string, VedicPlanetPosition> n)
    {
        var sb = new StringBuilder();
        t.TryGetValue("Ma", out var ma); t.TryGetValue("Sa", out var sa);
        t.TryGetValue("Ju", out var ju);

        if (ma != null)
        {
            int h = House(ma.Sign, lg);
            sb.Append(ma.Retro ? "Mars retrograde drains physical reserves — prioritise restorative practices over intense exertion. "
                    : h is 1  ? "Transit Mars in your Lagna brings excess heat and injury-proneness — channel energy through disciplined exercise. "
                    : h is 6  ? "Mars in the 6th sharpens immunity but warns of overwork-related fatigue — pace yourself. "
                    : h is 8  ? "Mars in the 8th: act promptly on any health symptoms and avoid risky activities. "
                    : h is 3 or 11 ? "Mars in an upachaya house builds physical strength and endurance — channel energy positively. "
                    : $"Mars activates vitality through House {h}. ");
        }
        if (sa != null && House(sa.Sign, lg) is 1 or 6 or 8)
            sb.Append("Saturn's influence on health-sensitive houses may bring fatigue or chronic conditions — prioritise sleep and preventive care. ");
        if (n.TryGetValue("Ma", out var nMa) && House(nMa.SignIndex, lg) is 6 or 8)
            sb.Append($"Natally, Mars in your H{House(nMa.SignIndex, lg)} indicates a constitution prone to inflammation — maintain consistent exercise and hydration. ");
        if (ju != null && !ju.Retro && House(ju.Sign, lg) is 1 or 5)
            sb.Append("Jupiter's benefic gaze supports vitality and recovery today. ");
        int hScore = HealthScore(lg, t, n);
        sb.Append(hScore >= 4 ? "Vitality is strong — embrace physical activity and nourishing food."
                : hScore <= 2 ? "Rest is medicine today — listen to your body's signals and avoid overexertion."
                : "Moderate exercise, quality sleep, and mindful nutrition will keep your energy balanced.");
        return sb.ToString();
    }

    private static string BuildSpiritParagraph(int lg, Dictionary<string, TransitPlanet> t,
                                                Dictionary<string, VedicPlanetPosition> n, int moonNakIdx)
    {
        var sb = new StringBuilder();
        t.TryGetValue("Ju", out var ju); t.TryGetValue("Ke", out var ke);
        int h9sign = (lg + 8) % 12;
        n.TryGetValue(SignRulers[h9sign], out var n9lord);

        if (ju != null)
        {
            int h = House(ju.Sign, lg);
            sb.Append(!ju.Retro && h is 9 ? "Jupiter in the 9th — the most powerful transit for dharma, devotion, and Guru's blessings. Seek wisdom from teachers or scripture today. "
                    : !ju.Retro && h is 12 ? "Jupiter in the 12th supports moksha-oriented practices — charity, meditation, and selfless service carry exceptional merit. "
                    : ju.Retro ? "Jupiter retrograde invites deep philosophical enquiry — revisit your beliefs and deepen your dharmic understanding. " : "");
        }
        if (ke != null && House(ke.Sign, lg) is 9 or 12)
            sb.Append("Ketu in a moksha house deepens spiritual sensitivity — meditation, mantras, and solitude are especially rewarding. ");
        if (n9lord != null)
        {
            string str = PlanetStrength(SignRulers[h9sign], n9lord.SignIndex);
            int h = House(n9lord.SignIndex, lg);
            sb.Append($"Natally your 9th lord {FullName(SignRulers[h9sign])} in House {h} ({str.ToLower()}) — ");
            sb.Append(str is "Exalted" or "Own Sign" ? "strong natural faith and access to spiritual blessings. "
                    : str is "Debilitated" ? "the spiritual path is meaningful but may feel difficult; persistence is transformative. "
                    : "steady spiritual growth through consistent practice. ");
        }
        sb.Append(moonNakIdx is 7  ? "The Moon in Pushya — most auspicious of all nakshatras — multiplies the merit of every prayer and charitable act. "
                : moonNakIdx is 0  ? "Ashwini nakshatra carries healing energy — excellent for new beginnings and inner renewal. "
                : moonNakIdx is 16 ? "Anuradha nakshatra opens the heart to devotion — bhakti, chanting, and prayer are richly supported. "
                : $"The Moon in {Nakshatras[moonNakIdx]} adds its unique vibrational essence to your inner life today. ");
        int sScore = SpiritScore(lg, t, n, moonNakIdx);
        sb.Append(sScore >= 4 ? "The inner channels are wide open — commit fully to sadhana and let grace do the rest."
                : sScore <= 2 ? "Ground yourself through pranayama or nature walks before deeper spiritual practices."
                : "Even ten sincere minutes of stillness will nourish your soul and clarify your vision.");
        return sb.ToString();
    }

    // ?? Key transit insights ??????????????????????????????????????????????????

    private static List<string> BuildKeyTransitInsights(int lg,
        Dictionary<string, TransitPlanet> t, Dictionary<string, VedicPlanetPosition> n)
    {
        var list = new List<string>();

        // Planetary return (transit within 10 degrees of natal longitude)
        foreach (var (sym, tr) in t)
        {
            if (!n.TryGetValue(sym, out var nat)) continue;
            double natLon = nat.SignIndex * 30.0 + nat.DegreeInSign;
            double diff   = Math.Abs(tr.Sid - natLon) % 360;
            if (diff > 180) diff = 360 - diff;
            if (diff < 10)
                list.Add($"Planetary return: Transit {FullName(sym)} is within {diff:F0} degrees of its natal position (H{House(nat.SignIndex, lg)}) — themes of that house come to the foreground.");
        }

        // Jupiter's 5th/9th aspect over natal Sun or Moon
        if (t.TryGetValue("Ju", out var ju) && !ju.Retro)
        {
            if (n.TryGetValue("Su", out var nSu))
            {
                int rel = ((House(nSu.SignIndex, lg) - House(ju.Sign, lg) + 12) % 12) + 1;
                if (rel is 5 or 9)
                    list.Add($"Jupiter's {(rel == 5 ? "5th" : "9th")} aspect graces your natal Sun — expanded confidence, recognition, and creative power.");
            }
            if (n.TryGetValue("Mo", out var nMo))
            {
                int rel = ((House(nMo.SignIndex, lg) - House(ju.Sign, lg) + 12) % 12) + 1;
                if (rel is 5 or 9)
                    list.Add($"Jupiter's {(rel == 5 ? "5th" : "9th")} aspect blesses your natal Moon — emotional wisdom, inner peace, and fortunate encounters.");
            }
        }

        // Sade Sati / Ashtama Sani detection
        if (t.TryGetValue("Sa", out var sa) && n.TryGetValue("Mo", out var nMo2))
        {
            int diff = Math.Abs(sa.Sign - nMo2.SignIndex);
            if (diff > 6) diff = 12 - diff;
            if (diff <= 1)
                list.Add("SADE SATI / ASHTAMA SANI: Saturn is within one sign of your natal Moon — a period of deep karmic restructuring. Practise patience, perseverance, and increased sadhana.");
        }

        // Rahu/Ketu crossing natal Lagna
        if (t.TryGetValue("Ra", out var ra) && ra.Sign == lg)
            list.Add("Transit Rahu crossing your natal Lagna: intense ambition, identity shifts, and unconventional experiences. Stay grounded and maintain your sadhana.");
        if (t.TryGetValue("Ke", out var ke) && ke.Sign == lg)
            list.Add("Transit Ketu crossing your Lagna: themes of detachment, spiritual awakening, and past-life karmas surface powerfully. Meditation and self-inquiry are especially productive.");

        // Mars on natal 7th
        if (t.TryGetValue("Ma", out var ma) && !ma.Retro && ma.Sign == (lg + 6) % 12)
            list.Add("Transit Mars occupies your natal 7th house sign — guard against conflict in partnerships. Choose words with care and avoid power struggles.");

        if (list.Count == 0)
            list.Add("No exceptional transit-to-natal conjunctions today — a stable day for focused, routine progress.");

        return list;
    }

    // ?? Remedies ??????????????????????????????????????????????????????????????

    private static List<string> BuildRemedies(int lg, Dictionary<string, TransitPlanet> t, int overall)
    {
        var list = new List<string>();
        list.Add(SignRulers[lg] switch
        {
            "Su" => "Offer water to the rising Sun and recite the Aditya Hridayam or Gayatri Mantra daily.",
            "Mo" => "Wear white on Mondays, offer milk to Shiva, chant 'Om Chandraya Namah' 108 times.",
            "Ma" => "Recite the Hanuman Chalisa and offer red flowers to Hanuman on Tuesdays.",
            "Me" => "Chant 'Om Budhaya Namah' 108 times; donate green mung dal on Wednesdays.",
            "Ju" => "Offer yellow flowers and turmeric to Lord Vishnu on Thursdays; recite Vishnu Sahasranama.",
            "Ve" => "Offer white flowers to Goddess Lakshmi on Fridays; wear white or light colours.",
            "Sa" => "Light a sesame oil lamp on Saturdays; donate black sesame seeds to the needy.",
            _    => "Recite your Ishta Devata's mantra at sunrise for overall planetary strength."
        });
        if (t.TryGetValue("Sa", out var sa) && (sa.Retro || House(sa.Sign, lg) is 6 or 8 or 12))
            list.Add("Donate mustard oil and sesame on Saturday to pacify Saturn's challenging transit.");
        if (t.TryGetValue("Ma", out var ma) && (ma.Retro || House(ma.Sign, lg) is 1 or 8))
            list.Add("Chant 'Om Mangalaya Namah'; donate red lentils on Tuesday to pacify Mars.");
        if (t.TryGetValue("Ra", out var ra) && House(ra.Sign, lg) is 1 or 8 or 12)
            list.Add("Donate black cloth or coconut to a Rahu shrine; chant Durga Saptashati for protection.");
        if (t.TryGetValue("Me", out var me) && me.Retro)
            list.Add("Mercury retrograde: chant 'Om Budhaya Namah'; keep all communications precise and documented.");
        if (t.TryGetValue("Ve", out var ve) && ve.Retro)
            list.Add("Venus retrograde: offer white flowers to the Goddess on Friday; postpone major relationship decisions.");
        if (overall <= 2)
            list.Add("Light 11 ghee lamps before the Sun on any morning for overall planetary pacification.");
        return list;
    }

    // ?? Transit snapshot (sidereal) ???????????????????????????????????????????

    private static Dictionary<string, TransitPlanet> CalculateTransits(DateTime utc, double ayanamsa)
    {
        double jd = AASDate.DateToJD(utc.Year, utc.Month,
            utc.Day + (utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0) / 24.0, true);
        double jp = jd - 1;

        double Sid(double trop) { double s = (trop - ayanamsa) % 360; if (s < 0) s += 360; return s; }
        double Norm(double d)   { d %= 360; if (d < 0) d += 360; return d; }
        bool   Ret(double p, double c) { double d = c - p; if (d > 180) d -= 360; if (d < -180) d += 360; return d < 0; }

        double sunT = Norm(AASSun.ApparentEclipticLongitude(jd, false));
        double monT = Norm(AASMoon.EclipticLongitude(jd));
        double meT  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.MERCURY, false).ApparentGeocentricLongitude);
        double veT  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.VENUS,   false).ApparentGeocentricLongitude);
        double maT  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.MARS,    false).ApparentGeocentricLongitude);
        double juT  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude);
        double saT  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN,  false).ApparentGeocentricLongitude);
        double raT  = Norm(AASMoon.MeanLongitudeAscendingNode(jd));

        double meP = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.MERCURY, false).ApparentGeocentricLongitude);
        double veP = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.VENUS,   false).ApparentGeocentricLongitude);
        double maP = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.MARS,    false).ApparentGeocentricLongitude);
        double juP = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude);
        double saP = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.SATURN,  false).ApparentGeocentricLongitude);

        TransitPlanet P(string sym, double trop, bool retro = false) => new()
        {
            Sym = sym, Sid = Sid(trop), Sign = SO(Sid(trop)), Retro = retro
        };

        return new Dictionary<string, TransitPlanet>
        {
            ["Su"] = P("Su", sunT),
            ["Mo"] = P("Mo", monT),
            ["Me"] = P("Me", meT, Ret(meP, meT)),
            ["Ve"] = P("Ve", veT, Ret(veP, veT)),
            ["Ma"] = P("Ma", maT, Ret(maP, maT)),
            ["Ju"] = P("Ju", juT, Ret(juP, juT)),
            ["Sa"] = P("Sa", saT, Ret(saP, saT)),
            ["Ra"] = new() { Sym = "Ra", Sid = Sid(raT),              Sign = SO(Sid(raT)),              Retro = true },
            ["Ke"] = new() { Sym = "Ke", Sid = Sid(Norm(raT + 180)), Sign = SO(Sid(Norm(raT + 180))), Retro = true },
        };
    }

    // ?? Utilities ?????????????????????????????????????????????????????????????

    private static int House(int planetSign, int lagna) => ((planetSign - lagna + 12) % 12) + 1;
    private static int SO(double lon) => (int)(lon / 30.0) % 12;

    private static string PlanetStrength(string sym, int sign)
    {
        if (ExaltDebil.TryGetValue(sym, out var ed))
        {
            if (sign == ed.exalt) return "Exalted";
            if (sign == ed.debil) return "Debilitated";
        }
        if (OwnSigns.TryGetValue(sym, out var own) && own.Contains(sign)) return "Own Sign";
        if (FriendSigns.TryGetValue(sym, out var fri) && fri.Contains(sign)) return "Friendly Sign";
        return "Neutral";
    }

    private static string TransitStatus(string sym, int h, bool retro)
    {
        string r = retro ? "(R) " : "";
        return sym switch
        {
            "Ju" => !retro && h is 1 or 5 or 9 or 10 ? $"{r}Very favourable"
                   : retro ? $"{r}Inner growth"
                   : h is 6 or 8 or 12 ? $"{r}Challenging" : $"{r}Moderate",
            "Sa" => !retro && h is 3 or 6 or 11 ? $"{r}Upachaya strength"
                   : h is 1 or 4 or 8 or 12 ? $"{r}Difficult" : $"{r}Moderate",
            "Ma" => !retro && h is 3 or 6 or 11 ? $"{r}Competitive edge"
                   : !retro && h is 1 or 8 ? $"{r}Aggressive" : $"{r}Moderate",
            "Ve" => !retro && h is 5 or 7 ? $"{r}Auspicious"
                   : retro ? $"{r}Past resurfaces"
                   : h is 6 or 8 or 12 ? $"{r}Restricted" : $"{r}Moderate",
            "Me" => retro ? $"{r}Review needed" : h is 10 or 3 ? $"{r}Sharp intellect" : $"{r}Moderate",
            "Ra" => h is 3 or 6 or 10 or 11 ? "Ambitious drive" : h is 1 or 8 or 12 ? "Disruptive" : "Mixed",
            "Ke" => h is 9 or 12 ? "Spiritual depth" : h is 1 or 8 ? "Detachment" : "Mixed",
            _    => h is 1 or 10 ? $"{r}Strong" : h is 6 or 8 or 12 ? $"{r}Weak" : $"{r}Moderate"
        };
    }

    private static string ChandraBalaRating(int moonHouse) => moonHouse switch
    {
        2 or 5 or 9  => "Strong (Auspicious)",
        1 or 6 or 11 => "Moderate",
        3 or 7 or 10 => "Unfavourable",
        _            => "Inauspicious"
    };

    private static string FavourableTime(int moonNakIdx) => NakshatraLords[moonNakIdx] switch
    {
        "Su" => "Sunrise to 7 AM and midday (12-1 PM) — Sun hora",
        "Mo" => "5-7 AM and 6-7 PM — Moon hora; Mondays especially",
        "Ma" => "Afternoon 3-5 PM — Mars hora",
        "Me" => "Mid-morning 9-11 AM — Mercury hora; ideal for communication",
        "Ju" => "Morning 8-10 AM and evening 5-6 PM — Jupiter hora",
        "Ve" => "Late morning 10 AM-12 PM and evening — Venus hora",
        "Sa" => "Dusk 5-7 PM — Saturn hora; Saturday hours most intense",
        "Ra" => "Avoid Rahu Kaal; dusk hours are sensitive",
        "Ke" => "Pre-dawn before sunrise — Ketu's mystical window",
        _    => "Morning hours generally auspicious"
    };

    private static string LuckyColor(int lg) => SignRulers[lg] switch
    {
        "Su" => "Bright Red or Gold",    "Mo" => "Pearl White or Silver",
        "Ma" => "Blood Red or Coral",    "Me" => "Green or Emerald",
        "Ju" => "Yellow or Saffron",     "Ve" => "White or Sky Blue",
        "Sa" => "Dark Blue or Black",    _    => "White"
    };

    private static int LuckyNumber(int lg, Dictionary<string, TransitPlanet> t)
    {
        int deg = t.TryGetValue("Mo", out var mo) ? (int)(mo.Sid % 30) : 0;
        int juH = t.TryGetValue("Ju", out var ju) ? House(ju.Sign, lg) : 1;
        return ((lg + 1 + deg + juH) % 9) + 1;
    }

    private static string LuckyGem(int lg) => SignRulers[lg] switch
    {
        "Su" => "Ruby (Manik)",
        "Mo" => "Natural Pearl (Moti)",
        "Ma" => "Red Coral (Moonga)",
        "Me" => "Emerald (Panna)",
        "Ju" => "Yellow Sapphire (Pukhraj)",
        "Ve" => "Diamond or White Sapphire",
        "Sa" => "Blue Sapphire (consult an astrologer before wearing)",
        _    => "Hessonite (Gomed)"
    };

    private static string Affirmation(int lg, int overall) => overall switch
    {
        1 => $"I, {SignNames[lg]} Lagna, trust divine timing and find strength through surrender.",
        2 => $"I, {SignNames[lg]} Lagna, persevere with grace — every challenge deepens my dharma.",
        3 => $"I, {SignNames[lg]} Lagna, walk in balance, meeting each moment with clarity.",
        4 => $"I, {SignNames[lg]} Lagna, act with wisdom and confidence — the stars align with my purpose.",
        _ => $"I, {SignNames[lg]} Lagna, radiate abundance, dharma, and cosmic grace.",
    };

    private static string Tip(int lg, Dictionary<string, TransitPlanet> t, int overall)
    {
        if (t.TryGetValue("Me", out var me) && me.Retro)
            return "Mercury retrograde: back up all work, avoid contracts, verify communications twice.";
        if (t.TryGetValue("Ve", out var ve) && ve.Retro)
            return "Venus retrograde: pause on relationship and financial decisions; reflect on core values.";
        if (t.TryGetValue("Ma", out var ma) && ma.Retro)
            return "Mars retrograde: redirect energy into creative outlets; avoid impulsive confrontations.";
        if (t.TryGetValue("Sa", out var sa) && House(sa.Sign, lg) is 8)
            return "Saturn in the 8th: schedule a health check-up and conduct a thorough financial audit.";
        if (overall >= 4)
            return "The planetary alignment strongly favours you — initiate, lead, and act with full conviction.";
        if (overall <= 2)
            return "Rest, chant your Lagna's mantra, and conserve energy for a more auspicious window.";
        if (t.TryGetValue("Ju", out var ju) && !ju.Retro && House(ju.Sign, lg) is 9)
            return "Jupiter blesses the 9th — seek a guru or elder's guidance; learning carries special merit today.";
        return "Begin your day with 12 Sun salutations facing east — align body, mind, and cosmos.";
    }

    private static string FullName(string s) => s switch
    {
        "Su" => "Sun",  "Mo" => "Moon",    "Ma" => "Mars",    "Me" => "Mercury",
        "Ju" => "Jupiter","Ve" => "Venus", "Sa" => "Saturn",  "Ra" => "Rahu",
        "Ke" => "Ketu", _ => s
    };

    private static string Stars(int n) => new string('*', n) + new string('.', 5 - n) + $"  ({n}/5)";

    private static void Section(string title, string body)
    {
        Console.WriteLine($"  {title}");
        var words = body.Split(' ');
        var line  = new StringBuilder("  ");
        foreach (var w in words)
        {
            if (line.Length + w.Length + 1 > 66)
            {
                Console.WriteLine(line.ToString().TrimEnd());
                line.Clear().Append("  ");
            }
            line.Append(w).Append(' ');
        }
        if (line.Length > 2) Console.WriteLine(line.ToString().TrimEnd());
        Console.WriteLine();
    }
}

// ?? Internal transit model ????????????????????????????????????????????????????

internal class TransitPlanet
{
    public string Sym   { get; set; } = "";
    public double Sid   { get; set; }
    public int    Sign  { get; set; }
    public bool   Retro { get; set; }
}

// ?? Output models ?????????????????????????????????????????????????????????????

public class VedicPersonalHoroscope
{
    public DateTime Date             { get; set; }
    public string   LagnaSign        { get; set; } = "";
    public string   LagnaElement     { get; set; } = "";
    public string   LagnaLord        { get; set; } = "";
    public float    LagnaDegree      { get; set; }
    public double   Ayanamsa         { get; set; }
    public string   MoonNakshatra    { get; set; } = "";
    public string   NakshatraLord    { get; set; } = "";
    public string   Tarabala         { get; set; } = "";
    public string   ChandraBala      { get; set; } = "";
    public int      MoonTransitHouse { get; set; }
    public int      Overall          { get; set; }
    public int      Love             { get; set; }
    public int      Career           { get; set; }
    public int      Finance          { get; set; }
    public int      Health           { get; set; }
    public int      Spirituality     { get; set; }
    public string   NatalSummary     { get; set; } = "";
    public string   NatalHouseTable  { get; set; } = "";
    public List<TransitRow> TransitTable  { get; set; } = [];
    public string   Overview         { get; set; } = "";
    public string   SelfParagraph    { get; set; } = "";
    public string   LoveParagraph    { get; set; } = "";
    public string   CareerParagraph  { get; set; } = "";
    public string   FinanceParagraph { get; set; } = "";
    public string   HealthParagraph  { get; set; } = "";
    public string   SpiritParagraph  { get; set; } = "";
    public List<string> KeyTransits  { get; set; } = [];
    public List<string> Remedies     { get; set; } = [];
    public string   FavourableTime   { get; set; } = "";
    public string   LuckyColor       { get; set; } = "";
    public int      LuckyNumber      { get; set; }
    public string   LuckyGem         { get; set; } = "";
    public string   Affirmation      { get; set; } = "";
    public string   Tip              { get; set; } = "";
}

public class TransitRow
{
    public string Planet       { get; set; } = "";
    public string TransitSign  { get; set; } = "";
    public int    TransitHouse { get; set; }
    public string NatalSign    { get; set; } = "";
    public int    NatalHouse   { get; set; }
    public string Strength     { get; set; } = "";
    public string Status       { get; set; } = "";
}
