using AASharp;
using System.Text;

/// <summary>
/// Generates a Vedic (Jyotish) daily horoscope for a given Lagna (Ascendant) sign
/// based on real sidereal transits calculated for today.
///
/// Jyotish rules applied:
///   - Transit of all 9 Grahas (Su/Mo/Ma/Me/Ju/Ve/Sa/Ra/Ke) from Lagna
///   - Moon nakshatra (27 nakshatras) and its daily quality (Tarabala)
///   - House lordship for each Lagna (who rules which house)
///   - Benefic/malefic graha classification
///   - Ashtakavarga-inspired scoring (simplified bindu count)
///   - Moon transit house strength (Chandra Bala)
///   - Favourable/unfavourable transit interpretations per graha+house
/// </summary>
public static class VedicHoroscope
{
    // ?? Vedic sign data ??????????????????????????????????????????????????????

    public static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] SignRulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    private static readonly string[] Elements =
        ["Fire","Earth","Air","Water","Fire","Earth",
         "Air","Water","Fire","Earth","Air","Water"];

    // ?? 27 Nakshatras ????????????????????????????????????????????????????????

    private static readonly string[] Nakshatras =
    [
        "Ashwini","Bharani","Krittika","Rohini","Mrigashira","Ardra",
        "Punarvasu","Pushya","Ashlesha","Magha","Purva Phalguni","Uttara Phalguni",
        "Hasta","Chitra","Swati","Vishakha","Anuradha","Jyeshtha",
        "Mula","Purva Ashadha","Uttara Ashadha","Shravana","Dhanishtha","Shatabhisha",
        "Purva Bhadrapada","Uttara Bhadrapada","Revati"
    ];

    // Nakshatra ruling planet (Vimshottari Dasha lords)
    private static readonly string[] NakshatraLords =
    [
        "Ke","Ve","Su","Mo","Ma","Ra",
        "Ju","Sa","Me","Ke","Ve","Su",
        "Mo","Ma","Ra","Ju","Sa","Me",
        "Ke","Ve","Su","Mo","Ma","Ra",
        "Ju","Sa","Me"
    ];

    // Nakshatra quality (Tarabala group: 1=Janma,2=Sampat,3=Vipat,4=Kshema,5=Pratyari,6=Sadhaka,7=Vadha,8=Mitra,9=Atimitra)
    // Groups of 3 nakshatras cycle through 9 types
    private static readonly string[] TarabalaMeaning =
        ["Janma (Birth)","Sampat (Wealth)","Vipat (Danger)","Kshema (Wellbeing)",
         "Pratyari (Opposition)","Sadhaka (Achievement)","Vadha (Obstacle)","Mitra (Friend)","Atimitra (Great Friend)"];

    // ?? Graha benefic/malefic nature ?????????????????????????????????????????

    // Natural benefics: Ju, Ve, Mo (waxing), Me (alone) — simplified
    private static readonly HashSet<string> NaturalBenefics = ["Ju","Ve","Mo"];
    private static readonly HashSet<string> NaturalMalefics = ["Sa","Ma","Ra","Ke","Su"];

    // ?? Functional benefic/malefic by Lagna (simplified standard rules) ??????
    // For each lagna (0=Aries..11=Pisces): list of functionally benefic planet symbols
    private static readonly string[][] FunctionalBenefics =
    [
        ["Su","Mo","Ju","Ma"],         // Aries
        ["Sa","Ve","Me"],              // Taurus
        ["Ve","Sa","Me","Ra"],         // Gemini
        ["Mo","Ma","Ju","Su"],         // Cancer
        ["Su","Ma","Ju"],              // Leo
        ["Ve","Me","Sa"],              // Virgo
        ["Sa","Me","Ve"],              // Libra
        ["Mo","Ju","Su","Ma"],         // Scorpio
        ["Su","Ma","Ju"],              // Sagittarius
        ["Ve","Me","Sa"],              // Capricorn
        ["Ve","Sa","Me"],              // Aquarius
        ["Mo","Ma","Ju"],              // Pisces
    ];

    // ?? Moon transit house strength (Chandra Bala) ???????????????????????????
    // Houses from Moon's natal position: 1,6,11=neutral; 3,6,10,11=good; 2,5,9=very good; 7=bad
    // But for daily transit Chandra Bala uses Moon from Lagna:
    // H1,6,11 = medium; H3,7,10 = poor; H2,5,9 = auspicious; H4,8,12 = inauspicious
    private static readonly Dictionary<int, string> ChandraBalaRating = new()
    {
        {1,"Moderate"},{2,"Auspicious"},{3,"Unfavourable"},{4,"Inauspicious"},
        {5,"Auspicious"},{6,"Moderate"},{7,"Unfavourable"},{8,"Inauspicious"},
        {9,"Auspicious"},{10,"Unfavourable"},{11,"Moderate"},{12,"Inauspicious"}
    };

    // ?? Public API ???????????????????????????????????????????????????????????

    /// <summary>
    /// Generates a Vedic daily horoscope for the given Lagna sign,
    /// using today's sidereal transit positions.
    /// </summary>
    /// <param name="lagnaSign">0-based Lagna sign index (0=Aries…11=Pisces).</param>
    /// <param name="utcNow">Current UTC date/time.</param>
    /// <param name="utcOffsetHours">UTC offset of the observer (for transit Lagna calc).</param>
    public static VedicDailyHoroscope Generate(int lagnaSign, DateTime utcNow, double utcOffsetHours = 5.5)
    {
        var snap      = CalculateTransits(utcNow);
        var yesterday = CalculateTransits(utcNow.AddDays(-1));

        // Moon nakshatra and Tarabala
        int    moonNakIdx  = (int)(snap.MoonSid / (360.0 / 27.0)) % 27;
        int    lagnaNakIdx = lagnaSign * 9 / 12; // approximate natal nakshatra start for lagna
        int    taraIndex   = ((moonNakIdx - lagnaNakIdx + 27) % 27) % 9;
        string moonNak     = Nakshatras[moonNakIdx];
        string nakLord     = NakshatraLords[moonNakIdx];
        string tarabala    = TarabalaMeaning[taraIndex];

        // Moon house from Lagna
        int moonHouse    = House(snap.MoonSign, lagnaSign);
        string chandraBala = ChandraBalaRating[moonHouse];

        // Scores
        int overall  = OverallScore(lagnaSign, snap);
        int love     = LoveScore(lagnaSign, snap);
        int career   = CareerScore(lagnaSign, snap);
        int finance  = FinanceScore(lagnaSign, snap);
        int health   = HealthScore(lagnaSign, snap);
        int spirit   = SpiritScore(lagnaSign, snap, moonNakIdx);

        // Build graha transit interpretations
        var transitDetails = BuildTransitDetails(lagnaSign, snap);

        return new VedicDailyHoroscope
        {
            LagnaSign      = SignNames[lagnaSign],
            LagnaElement   = Elements[lagnaSign],
            LagnaLord      = FullName(SignRulers[lagnaSign]),
            Date           = utcNow.Date,
            MoonNakshatra  = moonNak,
            NakshatraLord  = FullName(nakLord),
            Tarabala       = tarabala,
            ChandraBala    = chandraBala,
            MoonHouse      = moonHouse,
            Overall        = overall,
            Love           = love,
            Career         = career,
            Finance        = finance,
            Health         = health,
            Spirituality   = spirit,
            GrahaTransits  = transitDetails,
            Overview       = BuildOverview(lagnaSign, snap, moonNak, tarabala, chandraBala, overall),
            LoveParagraph  = BuildLoveParagraph(lagnaSign, snap),
            CareerParagraph = BuildCareerParagraph(lagnaSign, snap),
            FinanceParagraph = BuildFinanceParagraph(lagnaSign, snap),
            HealthParagraph = BuildHealthParagraph(lagnaSign, snap),
            SpiritParagraph = BuildSpiritParagraph(lagnaSign, snap, moonNak, moonNakIdx),
            Remedies       = BuildRemedies(lagnaSign, snap, overall),
            Affirmation    = Affirmation(lagnaSign, overall),
            LuckyColor     = LuckyColor(lagnaSign, snap),
            LuckyNumber    = LuckyNumber(lagnaSign, snap),
            LuckyGem       = LuckyGem(lagnaSign),
            FavourableTime = FavourableTime(snap),
            Tip            = Tip(lagnaSign, snap, overall),
        };
    }

    /// <summary>Prints the horoscope to the console.</summary>
    public static void Print(VedicDailyHoroscope h)
    {
        string line = new string('?', 62);
        Console.WriteLine($"?{line}?");
        Console.WriteLine($"  VEDIC DAILY HOROSCOPE  —  {h.LagnaSign.ToUpper()} LAGNA");
        Console.WriteLine($"  {h.Date:dddd, dd MMMM yyyy}");
        Console.WriteLine($"?{line}?");
        Console.WriteLine($"  Lagna      : {h.LagnaSign} ({h.LagnaElement}) | Lord: {h.LagnaLord}");
        Console.WriteLine($"  Moon in    : {SignNames[0]}  Nakshatra: {h.MoonNakshatra} (Lord: {h.NakshatraLord})");
        Console.WriteLine($"  Tarabala   : {h.Tarabala}");
        Console.WriteLine($"  Chandra Bala: {h.ChandraBala}  (Moon in House {h.MoonHouse})");
        Console.WriteLine();
        Console.WriteLine($"  Overall  {Stars(h.Overall)}   Love     {Stars(h.Love)}");
        Console.WriteLine($"  Career   {Stars(h.Career)}   Finance  {Stars(h.Finance)}");
        Console.WriteLine($"  Health   {Stars(h.Health)}   Spirit   {Stars(h.Spirituality)}");
        Console.WriteLine();

        Console.WriteLine("  GRAHA TRANSITS FROM LAGNA");
        Console.WriteLine($"  {"Graha",-10} {"Sign",-14} {"House",5}  {"Nature",-12} Summary");
        Console.WriteLine("  " + new string('?', 70));
        foreach (var t in h.GrahaTransits)
            Console.WriteLine($"  {t.Planet,-10} {t.Sign,-14} H{t.House,2}   {t.Nature,-12} {t.OneLiner}");

        Console.WriteLine();
        Section("OVERVIEW",    h.Overview);
        Section("LOVE & MARRIAGE", h.LoveParagraph);
        Section("CAREER & DHARMA", h.CareerParagraph);
        Section("FINANCE & WEALTH", h.FinanceParagraph);
        Section("HEALTH & VITALITY", h.HealthParagraph);
        Section("SPIRITUALITY & KARMA", h.SpiritParagraph);

        Console.WriteLine("  REMEDIES");
        foreach (var r in h.Remedies)
            Console.WriteLine($"    • {r}");
        Console.WriteLine();

        Console.WriteLine($"  Lucky Color   : {h.LuckyColor}");
        Console.WriteLine($"  Lucky Number  : {h.LuckyNumber}");
        Console.WriteLine($"  Lucky Gem     : {h.LuckyGem}");
        Console.WriteLine($"  Favourable Time: {h.FavourableTime}");
        Console.WriteLine();
        Console.WriteLine($"  AFFIRMATION: \"{h.Affirmation}\"");
        Console.WriteLine($"  TIP: {h.Tip}");
        Console.WriteLine(new string('?', 64));
        Console.WriteLine();
    }

    // ?? Transit calculation ??????????????????????????????????????????????????

    private static VedicTransitSnap CalculateTransits(DateTime utc)
    {
        double jd  = ToJD(utc);
        double aya = LahiriAyanamsa(jd);

        double SidOf(double tropL) => Norm(tropL - aya);

        double sunT  = Norm(AASSun.ApparentEclipticLongitude(jd, false));
        double monT  = Norm(AASMoon.EclipticLongitude(jd));
        double meT   = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.MERCURY, false).ApparentGeocentricLongitude);
        double veT   = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.VENUS,   false).ApparentGeocentricLongitude);
        double maT   = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.MARS,    false).ApparentGeocentricLongitude);
        double juT   = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude);
        double saT   = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN,  false).ApparentGeocentricLongitude);
        double raT   = Norm(AASMoon.MeanLongitudeAscendingNode(jd));

        double jp = jd - 1;
        bool meR = IsRetro(Norm(AASElliptical.Calculate(jp, AASEllipticalObject.MERCURY, false).ApparentGeocentricLongitude), meT);
        bool veR = IsRetro(Norm(AASElliptical.Calculate(jp, AASEllipticalObject.VENUS,   false).ApparentGeocentricLongitude), veT);
        bool maR = IsRetro(Norm(AASElliptical.Calculate(jp, AASEllipticalObject.MARS,    false).ApparentGeocentricLongitude), maT);
        bool juR = IsRetro(Norm(AASElliptical.Calculate(jp, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude), juT);
        bool saR = IsRetro(Norm(AASElliptical.Calculate(jp, AASEllipticalObject.SATURN,  false).ApparentGeocentricLongitude), saT);

        return new VedicTransitSnap
        {
            SunSid = SidOf(sunT),  SunSign = SO(SidOf(sunT)),
            MoonSid = SidOf(monT), MoonSign = SO(SidOf(monT)), MoonDeg = (float)(SidOf(monT) % 30),
            MeSid  = SidOf(meT),   MeSign  = SO(SidOf(meT)),   MeRetro = meR,
            VeSid  = SidOf(veT),   VeSign  = SO(SidOf(veT)),   VeRetro = veR,
            MaSid  = SidOf(maT),   MaSign  = SO(SidOf(maT)),   MaRetro = maR,
            JuSid  = SidOf(juT),   JuSign  = SO(SidOf(juT)),   JuRetro = juR,
            SaSid  = SidOf(saT),   SaSign  = SO(SidOf(saT)),   SaRetro = saR,
            RaSid  = SidOf(raT),   RaSign  = SO(SidOf(raT)),
            KeSid  = Norm(SidOf(raT) + 180), KeSign = SO(Norm(SidOf(raT) + 180)),
        };
    }

    // ?? Transit detail builder ????????????????????????????????????????????????

    private static List<GrahaTransit> BuildTransitDetails(int lagna, VedicTransitSnap s)
    {
        var list = new List<GrahaTransit>();
        var grahas = new (string sym, int sign, bool retro)[]
        {
            ("Su", s.SunSign, false), ("Mo", s.MoonSign, false),
            ("Ma", s.MaSign,  s.MaRetro), ("Me", s.MeSign, s.MeRetro),
            ("Ju", s.JuSign,  s.JuRetro), ("Ve", s.VeSign, s.VeRetro),
            ("Sa", s.SaSign,  s.SaRetro), ("Ra", s.RaSign, true), ("Ke", s.KeSign, true),
        };

        var funcBen = new HashSet<string>(FunctionalBenefics[lagna]);

        foreach (var (sym, sign, retro) in grahas)
        {
            int h = House(sign, lagna);
            string nature = funcBen.Contains(sym) ? "Func.Benefic"
                          : NaturalBenefics.Contains(sym) ? "Nat.Benefic"
                          : "Malefic";
            string retStr = retro ? "(R) " : "";
            list.Add(new GrahaTransit
            {
                Planet  = FullName(sym),
                Sign    = SignNames[sign],
                House   = h,
                Nature  = nature,
                OneLiner = retStr + TransitSummary(sym, h, retro, funcBen.Contains(sym)),
            });
        }
        return list;
    }

    // ?? Scores (1-5) ?????????????????????????????????????????????????????????

    private static int OverallScore(int lg, VedicTransitSnap s)
    {
        int sc = 3;
        // Jupiter in kendras/trikonas from lagna = +1
        if (!s.JuRetro && House(s.JuSign, lg) is 1 or 4 or 5 or 7 or 9 or 10) sc++;
        // Saturn in dusthanas = -1
        if (House(s.SaSign, lg) is 6 or 8 or 12) sc--;
        // Moon in kendras/trikonas = +1
        if (House(s.MoonSign, lg) is 1 or 4 or 5 or 7 or 9 or 10) sc++;
        // Rahu/Ketu on lagna = -1
        if (House(s.RaSign, lg) == 1 || House(s.KeSign, lg) == 1) sc--;
        return Math.Clamp(sc, 1, 5);
    }

    private static int LoveScore(int lg, VedicTransitSnap s)
    {
        int sc = 3;
        int vH = House(s.VeSign, lg);
        sc += vH is 5 or 7 ? 1 : vH is 6 or 8 or 12 ? -1 : 0;
        sc += s.VeRetro ? -1 : 0;
        sc += House(s.MoonSign, lg) is 5 or 7 ? 1 : 0;
        sc += House(s.JuSign, lg) is 5 or 7 && !s.JuRetro ? 1 : 0;
        sc += House(s.MaSign, lg) is 7 ? -1 : 0; // Mars on 7th = conflict
        return Math.Clamp(sc, 1, 5);
    }

    private static int CareerScore(int lg, VedicTransitSnap s)
    {
        int sc = 3;
        sc += House(s.SunSign, lg) is 10 or 11 ? 1 : House(s.SunSign, lg) is 6 or 12 ? -1 : 0;
        sc += House(s.JuSign,  lg) is 10 or 11 && !s.JuRetro ? 1 : 0;
        sc += s.SaRetro || House(s.SaSign, lg) is 10 ? -1 : 0;
        sc += s.MeRetro ? -1 : House(s.MeSign, lg) is 10 or 3 ? 1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    private static int FinanceScore(int lg, VedicTransitSnap s)
    {
        int sc = 3;
        sc += House(s.JuSign, lg) is 2 or 9 or 11 && !s.JuRetro ? 1 : 0;
        sc += House(s.VeSign, lg) is 2 or 11 ? 1 : House(s.VeSign, lg) is 12 ? -1 : 0;
        sc += s.VeRetro ? -1 : 0;
        sc += House(s.SaSign, lg) is 2 ? -1 : 0;
        sc += House(s.RaSign, lg) is 2 or 11 ? 1 : House(s.RaSign, lg) is 8 or 12 ? -1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    private static int HealthScore(int lg, VedicTransitSnap s)
    {
        int sc = 3;
        sc += House(s.MaSign, lg) is 6 or 8 or 12 ? -1 : House(s.MaSign, lg) is 1 ? -1 : 0;
        sc += s.MaRetro ? -1 : 0;
        sc += House(s.SaSign, lg) is 6 or 8 ? -1 : 0;
        sc += House(s.JuSign, lg) is 1 or 5 && !s.JuRetro ? 1 : 0;
        sc += House(s.RaSign, lg) is 6 || House(s.KeSign, lg) is 6 ? -1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    private static int SpiritScore(int lg, VedicTransitSnap s, int moonNakIdx)
    {
        int sc = 3;
        sc += House(s.JuSign, lg) is 9 or 12 or 1 && !s.JuRetro ? 1 : 0;
        sc += House(s.MoonSign, lg) is 9 or 12 or 4 ? 1 : 0;
        sc += House(s.KeSign, lg) is 9 or 12 ? 1 : 0; // Ketu = moksha
        // Spiritual nakshatras: Ashwini(0),Pushya(7),Hasta(12),Anuradha(16),Uttara Ashadha(20)
        sc += moonNakIdx is 0 or 7 or 12 or 16 or 20 ? 1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    // ?? Narrative paragraphs ??????????????????????????????????????????????????

    private static string BuildOverview(int lg, VedicTransitSnap s, string moonNak, string tara, string chandraBala, int overall)
    {
        string lagnaName = SignNames[lg];
        string sunSign   = SignNames[s.SunSign];
        string moonSign  = SignNames[s.MoonSign];
        int    sunH      = House(s.SunSign, lg);
        int    moonH     = House(s.MoonSign, lg);

        string tone = overall >= 4 ? "The planetary configuration is broadly favourable for " + lagnaName + " Lagna today — press forward with confidence."
                    : overall <= 2 ? "The cosmic energies demand caution. Avoid hasty decisions and focus on consolidation."
                    :                "Mixed transits call for balanced action — neither reckless boldness nor excessive hesitation.";

        string taraNote = tara.Contains("Danger") || tara.Contains("Obstacle") || tara.Contains("Opposition")
            ? $"Tarabala today is {tara}, suggesting increased obstacles — proceed with extra care. "
            : $"Tarabala is {tara}, lending a supportive background to your efforts. ";

        string chandraBalaNote = chandraBala == "Inauspicious"
            ? $"Chandra Bala is weak with the Moon transiting House {moonH} — emotional decisions may be clouded. "
            : chandraBala == "Auspicious"
            ? $"Chandra Bala is strong with the Moon in House {moonH} — emotions support clear action. "
            : $"Chandra Bala is moderate with the Moon in House {moonH}. ";

        return $"The Sun transits {sunSign} in House {sunH} while the Moon moves through {moonSign} ({moonNak} nakshatra) in House {moonH} from your Lagna. " +
               $"{taraNote}{chandraBalaNote}{tone}";
    }

    private static string BuildLoveParagraph(int lg, VedicTransitSnap s)
    {
        int vH = House(s.VeSign, lg);
        int juH = House(s.JuSign, lg);
        int moonH = House(s.MoonSign, lg);
        string vSign = SignNames[s.VeSign];

        string veNote = s.VeRetro
            ? $"Venus is retrograde in {vSign} — unresolved matters in relationships may resurface. Handle them with patience rather than urgency. "
            : vH is 5 ? $"Venus in House 5 from Lagna is a powerful position for romance, creativity, and joyful self-expression. Romantic opportunities are highlighted. "
            : vH is 7 ? $"Venus in the 7th House strongly activates partnership energy — an excellent day for meaningful relationship conversations. "
            : vH is 12 ? $"Venus in House 12 suggests private or secretive romantic matters. Spiritual love and compassion are favoured over worldly romance. "
            : vH is 6 or 8 ? $"Venus in a dusthana (House {vH}) creates friction in relationships. Practise patience and avoid heated arguments. "
            : $"Venus in House {vH} from Lagna brings gentle romantic energy — nurture existing bonds through simple gestures of care. ";

        string juNote = !s.JuRetro && juH is 5 or 7
            ? "Jupiter's benefic gaze on the house of relationships brings blessings, wisdom, and harmony to your love life. "
            : "";
        string moonNote = moonH is 5 or 7
            ? "The Moon's transit heightens emotional receptivity and desire for closeness — an ideal day to express your feelings. "
            : moonH is 8 or 12
            ? "The Moon in a sensitive house encourages emotional depth; avoid surface-level interactions and seek genuine connection. "
            : "";

        int lScore = LoveScore(lg, s);
        string closing = lScore >= 4 ? "Today is auspicious for love — let your heart speak freely."
                       : lScore <= 2 ? "Refrain from major relationship decisions today; let clarity return before committing."
                       :               "Communicate with warmth and honesty; small acts of love carry great weight.";
        return $"{veNote}{juNote}{moonNote}{closing}";
    }

    private static string BuildCareerParagraph(int lg, VedicTransitSnap s)
    {
        int sunH = House(s.SunSign, lg);
        int meH  = House(s.MeSign, lg);
        int juH  = House(s.JuSign, lg);
        int saH  = House(s.SaSign, lg);

        string sunNote = sunH is 10 ? "The Sun illuminates your 10th House of career — authority, recognition, and professional success are strongly favoured today. "
                       : sunH is 1  ? "The Sun in your Lagna brings personal power and the confidence to lead — use this for professional initiatives. "
                       : sunH is 6  ? "The Sun in the 6th House sharpens your drive to overcome obstacles and rivals in the workplace. "
                       :              $"The Sun transits House {sunH}, directing its light toward that area of life. ";

        string meNote = s.MeRetro
            ? "Mercury retrograde warns against signing agreements, launching projects, or making major announcements — review instead. "
            : meH is 10 ? "Mercury in the 10th sharpens strategic communication and professional articulation — an excellent day to present ideas. "
            : meH is 3  ? "Mercury in the 3rd favours short trips, writing, networking, and collaborative efforts. "
            : "";

        string saNote = saH is 10 ? "Saturn in the 10th demands patience — rewards for sustained effort will come, but not overnight. "
                      : s.SaRetro ? "Saturn retrograde calls for reviewing long-term plans and correcting structural weaknesses. "
                      : "";

        int cScore = CareerScore(lg, s);
        string closing = cScore >= 4 ? "Take decisive professional action — the planetary support is strong."
                       : cScore <= 2 ? "Conserve energy and refine existing plans rather than launching new ventures."
                       :               "Steady, focused effort will yield reliable professional progress.";
        return $"{sunNote}{meNote}{saNote}{closing}";
    }

    private static string BuildFinanceParagraph(int lg, VedicTransitSnap s)
    {
        int juH = House(s.JuSign, lg);
        int vH  = House(s.VeSign, lg);
        int raH = House(s.RaSign, lg);

        string juNote = !s.JuRetro && juH is 2 or 11
            ? $"Jupiter in House {juH} is highly auspicious for wealth accumulation — financial opportunities may present themselves. "
            : s.JuRetro && juH is 2 or 11
            ? "Jupiter retrograde in a wealth house asks you to reclaim money owed or re-examine investments rather than making new ones. "
            : "";
        string vNote = vH is 2 or 11 ? "Venus in a financial house supports income through creative pursuits, luxury trades, or beauty-related fields. "
                     : vH is 12 ? "Venus in the 12th warns of unnecessary expenses or overindulgence — practise financial restraint. "
                     : s.VeRetro ? "Venus retrograde advises against large purchases or financial commitments today. "
                     : "";
        string raNote = raH is 2 or 11 ? "Rahu in your wealth houses may bring sudden financial gain — remain alert for unexpected opportunities. "
                      : raH is 8 or 12 ? "Rahu in a challenging house could bring hidden financial losses — audit expenditures carefully. "
                      : "";

        int fScore = FinanceScore(lg, s);
        string closing = fScore >= 4 ? "Financial prospects are encouraging — invest wisely in value-creating assets."
                       : fScore <= 2 ? "Tread carefully with finances today; postpone major decisions."
                       :               "Moderate caution and consistent saving will serve you well.";
        return $"{juNote}{vNote}{raNote}{closing}";
    }

    private static string BuildHealthParagraph(int lg, VedicTransitSnap s)
    {
        int maH = House(s.MaSign, lg);
        int saH = House(s.SaSign, lg);
        int keH = House(s.KeSign, lg);

        string maNote = s.MaRetro ? "Mars retrograde may deplete physical vitality — avoid overexertion and choose restorative activities. "
                      : maH is 1  ? "Mars in your Lagna can produce excess heat, aggression, or accident-proneness — channel energy through disciplined exercise. "
                      : maH is 6  ? "Mars in the 6th house sharpens the immune system's fighting capacity but can also indicate inflammation — stay hydrated. "
                      : maH is 8  ? "Mars in the 8th warns of hidden health issues; attend to any symptoms promptly. "
                      :              "Mars provides moderate physical energy today — use it constructively. ";

        string saNote = saH is 1 or 6 ? "Saturn's influence on health-related houses may bring fatigue, joint issues, or chronic conditions — prioritise rest. "
                      : saH is 8 ? "Saturn in the 8th house calls for regular health check-ups and steady, preventive care. "
                      : "";

        string keNote = keH is 6 ? "Ketu in the 6th house may bring mysterious or hard-to-diagnose ailments — trust your body's signals. " : "";

        int hScore = HealthScore(lg, s);
        string closing = hScore >= 4 ? "Vitality is solid today — embrace physical activity and healthy nourishment."
                       : hScore <= 2 ? "Rest is medicine today — honour your body's need to recover."
                       :               "Moderate exercise, good sleep, and mindful eating will maintain your wellbeing.";
        return $"{maNote}{saNote}{keNote}{closing}";
    }

    private static string BuildSpiritParagraph(int lg, VedicTransitSnap s, string moonNak, int moonNakIdx)
    {
        int juH = House(s.JuSign, lg);
        int keH = House(s.KeSign, lg);
        int moonH = House(s.MoonSign, lg);

        string juNote = !s.JuRetro && juH is 9 ? "Jupiter in the 9th House is the most auspicious transit for dharma, spiritual study, and blessings from the Guru. Seek wisdom today. "
                      : !s.JuRetro && juH is 12 ? "Jupiter in the 12th supports moksha-oriented practices — meditation, charity, and selfless service carry special merit. "
                      : "";

        string keNote = keH is 9 or 12 ? "Ketu in a moksha house deepens your intuitive and spiritual faculties — this is a powerful time for inner inquiry and meditation. "
                      : keH is 4 ? "Ketu in the 4th may create detachment from worldly comforts, drawing you naturally toward spiritual or philosophical pursuits. "
                      : "";

        // Spiritually potent nakshatras
        string nakNote = moonNakIdx is 0  ? $"The Moon in Ashwini nakshatra (ruled by Ketu) ignites the spirit of new beginnings and healing. "
                       : moonNakIdx is 7  ? "The Moon in Pushya — the most auspicious nakshatra — greatly amplifies prayers, rituals, and devotional practice. "
                       : moonNakIdx is 12 ? "The Moon in Hasta nakshatra carries the blessings of Savitar, the Sun deity — ideal for healing arts and skilful action. "
                       : moonNakIdx is 16 ? "Anuradha nakshatra governs devotion and friendship with the divine — an auspicious Moon for worship and heart-opening practices. "
                       : moonNakIdx is 22 ? "Dhanishtha nakshatra resonates with abundance and musical/sound healing — mantra recitation carries special power today. "
                       : $"The Moon in {moonNak} adds its unique vibrational quality to your spiritual practice today. ";

        int sScore = SpiritScore(lg, s, moonNakIdx);
        string closing = sScore >= 4 ? "The inner channels are open — commit to sadhana with full intention."
                       : sScore <= 2 ? "Ground yourself first through pranayama or nature walks before deeper spiritual work."
                       :               "Even brief, sincere moments of stillness will nourish your soul today.";
        return $"{juNote}{keNote}{nakNote}{closing}";
    }

    // ?? Remedies ?????????????????????????????????????????????????????????????

    private static List<string> BuildRemedies(int lg, VedicTransitSnap s, int overall)
    {
        var r = new List<string>();
        int saH = House(s.SaSign, lg);
        int maH = House(s.MaSign, lg);
        int raH = House(s.RaSign, lg);

        if (saH is 6 or 8 or 12 || s.SaRetro)
            r.Add("Offer sesame seeds and mustard oil to a Shani shrine on Saturday.");
        if (maH is 1 or 6 or 8 || s.MaRetro)
            r.Add("Recite the Hanuman Chalisa to pacify Mars energy and enhance courage.");
        if (raH is 1 or 8 or 12)
            r.Add("Donate black sesame or black cloth on Saturday to reduce Rahu's shadow.");
        if (s.MeRetro)
            r.Add("Chant 'Om Budhaya Namah' 108 times to stabilise Mercury's retrograde effects.");
        if (s.VeRetro)
            r.Add("Offer white flowers to the Goddess on Friday for Venus retrograde peace.");
        if (overall <= 2)
            r.Add("Light a ghee lamp before the rising Sun daily for overall planetary strength.");
        if (House(s.JuSign, lg) is 6 or 8 or 12 || s.JuRetro)
            r.Add("Recite 'Om Gurave Namah' and offer yellow flowers on Thursday to strengthen Jupiter.");

        if (r.Count == 0)
            r.Add("Offer gratitude at sunrise — the planets are supporting your intentions today.");
        return r;
    }

    // ?? One-liner transit summaries ???????????????????????????????????????????

    private static string TransitSummary(string sym, int h, bool retro, bool funcBenefic)
    {
        string r = retro ? "(retrograde) " : "";
        return sym switch
        {
            "Su" => h is 1 or 10 ? $"{r}Strengthens authority and self-expression"
                   : h is 6      ? $"{r}Boosts vitality and ability to overcome rivals"
                   : h is 7      ? $"{r}May cause ego friction in partnerships"
                   : h is 8 or 12 ? $"{r}Hidden challenges; introspect and conserve energy"
                   :                $"{r}Solar energy activates House {h} affairs",

            "Mo" => h is 1 or 4 or 7 or 10 ? $"Strong Chandra Bala — emotions support action"
                   : h is 6 or 8 or 12      ? $"Weak Chandra Bala — guard emotional decisions"
                   :                           $"Moderate Moon influence on House {h}",

            "Ma" => retro ? $"Retro Mars: redirected drive, possible delays in action"
                   : h is 3 or 6 or 11      ? $"{r}Courage, competitive edge, physical strength"
                   : h is 1 or 8 or 12      ? $"{r}Excess fire — channel energy carefully"
                   :                           $"{r}Mars activates drive in House {h}",

            "Me" => retro ? $"Retro Mercury: review communications, avoid contracts"
                   : h is 1 or 3 or 10      ? $"{r}Sharp intellect, good communication"
                   :                           $"{r}Mercury facilitates learning in House {h}",

            "Ju" => retro ? $"Retro Jupiter: inner wisdom, review beliefs and finances"
                   : h is 1 or 5 or 9 or 10 ? $"{r}Powerful blessings and expansion"
                   : h is 6 or 8 or 12       ? $"{r}Hidden blessings; growth through challenges"
                   :                            $"{r}Jupiter expands affairs of House {h}",

            "Ve" => retro ? $"Retro Venus: past relationships resurface; avoid luxury spending"
                   : h is 5 or 7             ? $"{r}Romance, beauty, and harmony flourish"
                   : h is 6 or 8 or 12       ? $"{r}Friction in pleasures; practise restraint"
                   :                            $"{r}Venus graces House {h} with beauty and charm",

            "Sa" => retro ? $"Retro Saturn: revisit responsibilities and karmic lessons"
                   : h is 3 or 6 or 11      ? $"{r}Saturn rewards discipline and hard work"
                   : h is 1 or 4 or 8 or 12 ? $"{r}Saturn tests — endurance and patience needed"
                   :                           $"{r}Saturn structures affairs of House {h}",

            "Ra" => h is 3 or 6 or 10 or 11 ? "Rahu amplifies ambition and unconventional gains"
                   : h is 1 or 8 or 12       ? "Rahu creates confusion — maintain clarity of purpose"
                   :                            $"Rahu's shadow falls on House {h}",

            "Ke" => h is 9 or 12            ? "Ketu deepens spirituality and moksha tendency"
                   : h is 1 or 8             ? "Ketu brings detachment — meditate to find direction"
                   :                            $"Ketu's separating influence touches House {h}",

            _ => $"Graha activates House {h}"
        };
    }

    // ?? Lucky / Tip properties ????????????????????????????????????????????????

    private static string LuckyColor(int lg, VedicTransitSnap s)
    {
        // Based on Lagna lord's colour
        string lord = SignRulers[lg];
        return lord switch
        {
            "Su" => "Bright Red or Gold",  "Mo" => "Pearl White or Silver",
            "Ma" => "Blood Red or Coral",  "Me" => "Green or Emerald",
            "Ju" => "Yellow or Saffron",   "Ve" => "White or Sky Blue",
            "Sa" => "Dark Blue or Black",  _ => "White"
        };
    }

    private static int LuckyNumber(int lg, VedicTransitSnap s)
    {
        int raw = (lg + 1) + (int)s.MoonDeg + House(s.JuSign, lg);
        return (raw % 9) + 1;
    }

    private static string LuckyGem(int lg)
    {
        string lord = SignRulers[lg];
        return lord switch
        {
            "Su" => "Ruby",    "Mo" => "Pearl",      "Ma" => "Red Coral",
            "Me" => "Emerald", "Ju" => "Yellow Sapphire", "Ve" => "Diamond or White Sapphire",
            "Sa" => "Blue Sapphire (with caution)", _ => "Hessonite (Gomed)"
        };
    }

    private static string FavourableTime(VedicTransitSnap s)
    {
        // Moon nakshatra lord determines favourable period (simplified hora)
        string lord = NakshatraLords[(int)(s.MoonSid / (360.0 / 27.0)) % 27];
        return lord switch
        {
            "Su" => "Sunrise (6–7 AM) and midday (12–1 PM)",
            "Mo" => "Early morning (5–7 AM) and evening (6–7 PM)",
            "Ma" => "Afternoon (3–5 PM)",
            "Me" => "Mid-morning (9–11 AM)",
            "Ju" => "Morning (8–10 AM) and early evening (5–6 PM)",
            "Ve" => "Evening (6–8 PM)",
            "Sa" => "Dusk (5–7 PM) and Saturday hours",
            "Ra" => "Dusk transition (6–7 PM — Rahu Kaal, use cautiously)",
            "Ke" => "Early morning before sunrise",
            _ => "Morning hours are generally auspicious"
        };
    }

    private static string Affirmation(int lg, int overall)
    {
        string lagnaName = SignNames[lg];
        string[] aff =
        [
            $"I, {lagnaName} rising, release all that no longer serves my highest dharma.",
            $"I, {lagnaName} rising, trust in divine timing and find strength through patience.",
            $"I, {lagnaName} rising, move with clarity, purpose, and gratitude.",
            $"I, {lagnaName} rising, embrace this auspicious moment and act with wisdom.",
            $"I, {lagnaName} rising, radiate abundance, joy, and cosmic alignment.",
        ];
        return aff[overall - 1];
    }

    private static string Tip(int lg, VedicTransitSnap s, int overall)
    {
        if (s.MeRetro) return "Avoid signing agreements or launching new communication projects today.";
        if (s.VeRetro) return "Pause on relationship decisions and reflect on what you truly value.";
        if (s.MaRetro) return "Channel restless energy into creative work or physical exercise indoors.";
        if (House(s.SaSign, lg) is 8) return "Saturn in the 8th calls for a health check-up and financial audit.";
        if (overall >= 4) return "Act decisively — planetary energies are aligned with your intentions today.";
        if (overall <= 2) return "Rest, recite your Lagna's mantra, and conserve energy for a more favourable day.";
        if (House(s.JuSign, lg) is 9) return "Seek guidance from an elder, teacher, or guru — Jupiter blesses learning today.";
        return "Begin your day with the Sun salutation facing east — align body, mind, and cosmos.";
    }

    // ?? Utilities ?????????????????????????????????????????????????????????????

    private static int House(int planetSign, int lagna)
        => ((planetSign - lagna + 12) % 12) + 1;

    private static double LahiriAyanamsa(double jd)
    {
        double t = (jd - 2451545.0) / 365.25;
        return 23.8553 + t * 0.013956;
    }

    private static double ToJD(DateTime utc)
        => AASDate.DateToJD(utc.Year, utc.Month,
            utc.Day + (utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0) / 24.0, true);

    private static double Norm(double d) { d %= 360; if (d < 0) d += 360; return d; }
    private static int    SO(double lon) => (int)(lon / 30.0) % 12;
    private static bool   IsRetro(double prev, double now)
    { double d = now - prev; if (d > 180) d -= 360; if (d < -180) d += 360; return d < 0; }

    private static string FullName(string s) => s switch
    {
        "Su" => "Sun", "Mo" => "Moon", "Ma" => "Mars", "Me" => "Mercury",
        "Ju" => "Jupiter", "Ve" => "Venus", "Sa" => "Saturn",
        "Ra" => "Rahu", "Ke" => "Ketu", _ => s
    };

    private static string Stars(int n)
        => new string('?', n) + new string('?', 5 - n) + $"  ({n}/5)";

    private static void Section(string title, string body)
    {
        Console.WriteLine($"  {title}");
        int width = 60;
        var words = body.Split(' ');
        var line  = new StringBuilder("  ");
        foreach (var w in words)
        {
            if (line.Length + w.Length + 1 > width + 2)
            { Console.WriteLine(line.ToString().TrimEnd()); line.Clear().Append("  "); }
            line.Append(w).Append(' ');
        }
        if (line.Length > 2) Console.WriteLine(line.ToString().TrimEnd());
        Console.WriteLine();
    }
}

// ?? Transit snapshot ??????????????????????????????????????????????????????????

internal class VedicTransitSnap
{
    public double SunSid  { get; init; } public int SunSign  { get; init; }
    public double MoonSid { get; init; } public int MoonSign { get; init; } public float MoonDeg { get; init; }
    public double MeSid   { get; init; } public int MeSign   { get; init; } public bool MeRetro  { get; init; }
    public double VeSid   { get; init; } public int VeSign   { get; init; } public bool VeRetro  { get; init; }
    public double MaSid   { get; init; } public int MaSign   { get; init; } public bool MaRetro  { get; init; }
    public double JuSid   { get; init; } public int JuSign   { get; init; } public bool JuRetro  { get; init; }
    public double SaSid   { get; init; } public int SaSign   { get; init; } public bool SaRetro  { get; init; }
    public double RaSid   { get; init; } public int RaSign   { get; init; }
    public double KeSid   { get; init; } public int KeSign   { get; init; }
}

// ?? Output model ???????????????????????????????????????????????????????????????

public class VedicDailyHoroscope
{
    public string   LagnaSign        { get; set; } = "";
    public string   LagnaElement     { get; set; } = "";
    public string   LagnaLord        { get; set; } = "";
    public DateTime Date             { get; set; }
    public string   MoonNakshatra    { get; set; } = "";
    public string   NakshatraLord    { get; set; } = "";
    public string   Tarabala         { get; set; } = "";
    public string   ChandraBala      { get; set; } = "";
    public int      MoonHouse        { get; set; }
    public int      Overall          { get; set; }
    public int      Love             { get; set; }
    public int      Career           { get; set; }
    public int      Finance          { get; set; }
    public int      Health           { get; set; }
    public int      Spirituality     { get; set; }
    public List<GrahaTransit> GrahaTransits { get; set; } = [];
    public string   Overview         { get; set; } = "";
    public string   LoveParagraph    { get; set; } = "";
    public string   CareerParagraph  { get; set; } = "";
    public string   FinanceParagraph { get; set; } = "";
    public string   HealthParagraph  { get; set; } = "";
    public string   SpiritParagraph  { get; set; } = "";
    public List<string> Remedies     { get; set; } = [];
    public string   Affirmation      { get; set; } = "";
    public string   LuckyColor       { get; set; } = "";
    public int      LuckyNumber      { get; set; }
    public string   LuckyGem         { get; set; } = "";
    public string   FavourableTime   { get; set; } = "";
    public string   Tip              { get; set; } = "";
}

public class GrahaTransit
{
    public string Planet  { get; set; } = "";
    public string Sign    { get; set; } = "";
    public int    House   { get; set; }
    public string Nature  { get; set; } = "";
    public string OneLiner { get; set; } = "";
}
