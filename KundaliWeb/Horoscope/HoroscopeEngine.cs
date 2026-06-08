using AASharp;
using System.Text;

/// <summary>
/// Generates detailed daily and weekly horoscope predictions for all 12 zodiac signs
/// using real geocentric planetary positions.
///
/// Extended detail includes:
///   - Moon phase and void-of-course warning
///   - Planetary aspects with exact orb degrees
///   - Per-area paragraphs: Love, Career, Finances, Health, Spirituality
///   - Weekly best day / caution day
///   - Daily affirmation
///   - Compatibility hint
///   - Ruling planet focus sentence
/// </summary>
public static class HoroscopeEngine
{
    // ?? Sign metadata ????????????????????????????????????????????????????????

    public static readonly string[] SignNames =
        ["Aries","Taurus","Gemini","Cancer","Leo","Virgo",
         "Libra","Scorpio","Sagittarius","Capricorn","Aquarius","Pisces"];

    private static readonly string[] Elements =
        ["Fire","Earth","Air","Water","Fire","Earth",
         "Air","Water","Fire","Earth","Air","Water"];

    private static readonly string[] Modalities =
        ["Cardinal","Fixed","Mutable","Cardinal","Fixed","Mutable",
         "Cardinal","Fixed","Mutable","Cardinal","Fixed","Mutable"];

    // Compatible signs (element-based: same + complementary element)
    private static readonly int[][] CompatibleSigns =
    [
        [0,4,8,2,6,10],  // Aries  : Fire + Air
        [1,5,9,3,7,11],  // Taurus : Earth + Water
        [2,6,10,0,4,8],  // Gemini : Air + Fire
        [3,7,11,1,5,9],  // Cancer : Water + Earth
        [4,8,0,2,6,10],  // Leo    : Fire + Air
        [5,9,1,3,7,11],  // Virgo  : Earth + Water
        [6,10,2,0,4,8],  // Libra  : Air + Fire
        [7,11,3,1,5,9],  // Scorpio: Water + Earth
        [8,0,4,2,6,10],  // Sagittarius
        [9,1,5,3,7,11],  // Capricorn
        [10,6,2,0,4,8],  // Aquarius
        [11,3,7,1,5,9],  // Pisces
    ];

    internal static readonly string[] DayNames =
        ["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"];

    internal static readonly string[] Rulers =
        ["Ma","Ve","Me","Mo","Su","Me","Ve","Ma","Ju","Sa","Sa","Ju"];

    internal static string RulerFullName(string sym) => sym switch
    {
        "Su" => "Sun",  "Mo" => "Moon",   "Me" => "Mercury",
        "Ve" => "Venus","Ma" => "Mars",   "Ju" => "Jupiter",
        "Sa" => "Saturn", _ => sym
    };

    // ?? Public API ???????????????????????????????????????????????????????????

    public static List<Horoscope> GenerateDaily(DateTime utcDate)
    {
        var snap = CalculateForDate(utcDate);
        var next = CalculateForDate(utcDate.AddDays(1));
        var prev = CalculateForDate(utcDate.AddDays(-1));
        var result = new List<Horoscope>();
        for (int s = 0; s < 12; s++)
            result.Add(Build(s, snap, next, prev, utcDate, "Daily", null, null));
        return result;
    }

    public static List<Horoscope> GenerateWeekly(DateTime utcMonday)
    {
        var snaps = Enumerable.Range(0, 7)
            .Select(d => CalculateForDate(utcMonday.AddDays(d)))
            .ToArray();
        var result = new List<Horoscope>();
        for (int s = 0; s < 12; s++)
            result.Add(Build(s, snaps[0], snaps[6], snaps[0], utcMonday, "Weekly", snaps, utcMonday));
        return result;
    }

    public static void Print(List<Horoscope> horoscopes)
    {
        foreach (var h in horoscopes)
        {
            string border = new string('?', 58);
            Console.WriteLine($"?{border}?");
            Console.WriteLine($"?  {h.SignName.ToUpper()} ({h.Element} / {h.Modality})  —  {h.Period}  {h.Date:dd MMM yyyy}  ?");
            Console.WriteLine($"?{border}?");
            Console.WriteLine($"  Ruling Planet : {h.RulingPlanet}   Moon Phase: {h.MoonPhase}");
            Console.WriteLine();
            Console.WriteLine($"  Overall  {Stars(h.Overall)}  Love     {Stars(h.Love)}");
            Console.WriteLine($"  Career   {Stars(h.Career)}  Finances {Stars(h.Finances)}");
            Console.WriteLine($"  Health   {Stars(h.Health)}  Spirit   {Stars(h.Spirituality)}");
            Console.WriteLine();

            if (h.KeyAspects.Count > 0)
            {
                Console.WriteLine("  KEY PLANETARY ASPECTS:");
                foreach (var a in h.KeyAspects)
                    Console.WriteLine($"    • {a}");
                Console.WriteLine();
            }

            Console.WriteLine("  OVERVIEW");
            Console.WriteLine(Wrap(h.Overview, 58, "  "));
            Console.WriteLine();
            Console.WriteLine("  LOVE & RELATIONSHIPS");
            Console.WriteLine(Wrap(h.LoveParagraph, 58, "  "));
            Console.WriteLine();
            Console.WriteLine("  CAREER & AMBITION");
            Console.WriteLine(Wrap(h.CareerParagraph, 58, "  "));
            Console.WriteLine();
            Console.WriteLine("  FINANCES");
            Console.WriteLine(Wrap(h.FinanceParagraph, 58, "  "));
            Console.WriteLine();
            Console.WriteLine("  HEALTH & ENERGY");
            Console.WriteLine(Wrap(h.HealthParagraph, 58, "  "));
            Console.WriteLine();
            Console.WriteLine("  SPIRITUALITY & INTUITION");
            Console.WriteLine(Wrap(h.SpiritParagraph, 58, "  "));
            Console.WriteLine();

            if (h.BestDay != null)
                Console.WriteLine($"  Best Day    : {h.BestDay}   Caution Day: {h.CautionDay}");

            Console.WriteLine($"  Compatible  : {string.Join(", ", h.CompatibleSigns)}");
            Console.WriteLine($"  Lucky Color : {h.LuckyColor}   Lucky Number: {h.LuckyNumber}   Lucky Stone: {h.LuckyStone}");
            Console.WriteLine();
            Console.WriteLine($"  AFFIRMATION: \"{h.Affirmation}\"");
            Console.WriteLine($"  TIP: {h.Tip}");
            Console.WriteLine(new string('?', 60));
            Console.WriteLine();
        }
    }

    // ?? Builder ??????????????????????????????????????????????????????????????

    private static Horoscope Build(
        int sign,
        SnapshotData start,
        SnapshotData end,
        SnapshotData prev,
        DateTime date,
        string period,
        SnapshotData[]? allDays,
        DateTime? weekStart)
    {
        var ctx = new PredictionContext(sign, start, end, prev, period == "Weekly" ? 7 : 1, allDays);
        return new Horoscope
        {
            SignName      = SignNames[sign],
            Element       = Elements[sign],
            Modality      = Modalities[sign],
            RulingPlanet  = RulerFullName(Rulers[sign]),
            Period        = period,
            Date          = date,
            MoonPhase     = MoonPhaseName(start),
            Overall       = ctx.OverallScore(),
            Love          = ctx.LoveScore(),
            Career        = ctx.CareerScore(),
            Finances      = ctx.FinanceScore(),
            Health        = ctx.HealthScore(),
            Spirituality  = ctx.SpiritScore(),
            KeyAspects    = ctx.KeyAspects(),
            Overview      = ctx.Overview(period),
            LoveParagraph = ctx.LoveParagraph(period),
            CareerParagraph = ctx.CareerParagraph(period),
            FinanceParagraph = ctx.FinanceParagraph(period),
            HealthParagraph = ctx.HealthParagraph(period),
            SpiritParagraph = ctx.SpiritParagraph(period),
            BestDay       = period == "Weekly" ? ctx.BestDay(weekStart!.Value) : null,
            CautionDay    = period == "Weekly" ? ctx.CautionDay(weekStart!.Value) : null,
            CompatibleSigns = CompatibleSigns[sign].Take(3).Select(i => SignNames[i]).ToList(),
            LuckyColor    = ctx.LuckyColor(),
            LuckyNumber   = ctx.LuckyNumber(),
            LuckyStone    = LuckyStone(sign, start),
            Affirmation   = ctx.Affirmation(),
            Tip           = ctx.Tip(),
        };
    }

    // ?? Snapshot calculation ?????????????????????????????????????????????????

    internal static SnapshotData CalculateForDate(DateTime utc)
    {
        double jd  = ToJD(utc);
        double jp  = jd - 1.0;

        double sunL = Norm(AASSun.ApparentEclipticLongitude(jd, false));
        double monL = Norm(AASMoon.EclipticLongitude(jd));
        double meL  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.MERCURY, false).ApparentGeocentricLongitude);
        double veL  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.VENUS,   false).ApparentGeocentricLongitude);
        double maL  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.MARS,    false).ApparentGeocentricLongitude);
        double juL  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude);
        double saL  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.SATURN,  false).ApparentGeocentricLongitude);
        double urL  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.URANUS,  false).ApparentGeocentricLongitude);
        double neL  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.NEPTUNE, false).ApparentGeocentricLongitude);
        double plL  = Norm(AASElliptical.Calculate(jd, AASEllipticalObject.PLUTO,   false).ApparentGeocentricLongitude);

        double meP  = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.MERCURY, false).ApparentGeocentricLongitude);
        double veP  = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.VENUS,   false).ApparentGeocentricLongitude);
        double maP  = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.MARS,    false).ApparentGeocentricLongitude);
        double juP  = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude);
        double saP  = Norm(AASElliptical.Calculate(jp, AASEllipticalObject.SATURN,  false).ApparentGeocentricLongitude);

        double sunP = Norm(AASSun.ApparentEclipticLongitude(jp, false));
        double monP = Norm(AASMoon.EclipticLongitude(jp));

        // Moon phase angle (Sun-Moon elongation)
        double moonPhaseAngle = Norm(monL - sunL);

        return new SnapshotData
        {
            Date = utc,
            SunL = sunL, SunSign = SO(sunL), SunDeg = (float)(sunL % 30),
            MonL = monL, MoonSign = SO(monL), MoonDeg = (float)(monL % 30),
            MoonPhaseAngle = moonPhaseAngle,
            MeL = meL,  MeSign = SO(meL),  MeDeg = (float)(meL % 30),  MeRetro = Retro(meP, meL),
            VeL = veL,  VeSign = SO(veL),  VeDeg = (float)(veL % 30),  VeRetro = Retro(veP, veL),
            MaL = maL,  MaSign = SO(maL),  MaDeg = (float)(maL % 30),  MaRetro = Retro(maP, maL),
            JuL = juL,  JuSign = SO(juL),  JuDeg = (float)(juL % 30),  JuRetro = Retro(juP, juL),
            SaL = saL,  SaSign = SO(saL),  SaDeg = (float)(saL % 30),  SaRetro = Retro(saP, saL),
            UrL = urL,  UrSign = SO(urL),
            NeL = neL,  NeSign = SO(neL),
            PlL = plL,  PlSign = SO(plL),
        };
    }

    // ?? Utility ???????????????????????????????????????????????????????????????

    private static string MoonPhaseName(SnapshotData s)
    {
        double a = s.MoonPhaseAngle;
        return a switch
        {
            < 15 or >= 345 => "New Moon",
            < 45           => "Waxing Crescent",
            < 90           => "First Quarter",
            < 135          => "Waxing Gibbous",
            < 180          => "Full Moon Approaching",
            < 195          => "Full Moon",
            < 225          => "Waning Gibbous",
            < 270          => "Last Quarter",
            < 315          => "Waning Crescent",
            _              => "Balsamic Moon",
        };
    }

    private static string LuckyStone(int sign, SnapshotData s)
    {
        string[] stones = ["Diamond","Emerald","Agate","Pearl","Ruby","Sapphire",
                           "Opal","Topaz","Turquoise","Garnet","Amethyst","Aquamarine"];
        return stones[(sign + s.JuSign) % 12];
    }

    internal static double ToJD(DateTime utc)
        => AASDate.DateToJD(utc.Year, utc.Month,
            utc.Day + (utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0) / 24.0, true);

    internal static double Norm(double d) { d %= 360; if (d < 0) d += 360; return d; }
    internal static int    SO(double lon) => (int)(lon / 30.0) % 12;
    internal static bool   Retro(double prev, double now)
    { double d = now - prev; if (d > 180) d -= 360; if (d < -180) d += 360; return d < 0; }

    private static string Stars(int n)
        => new string('?', n) + new string('?', 5 - n) + $"  ({n}/5)";

    private static string Wrap(string text, int width, string indent)
    {
        var sb  = new StringBuilder();
        var words = text.Split(' ');
        var line  = new StringBuilder(indent);
        foreach (var w in words)
        {
            if (line.Length + w.Length + 1 > width + indent.Length)
            {
                sb.AppendLine(line.ToString().TrimEnd());
                line.Clear().Append(indent);
            }
            line.Append(w).Append(' ');
        }
        if (line.Length > indent.Length) sb.AppendLine(line.ToString().TrimEnd());
        return sb.ToString().TrimEnd();
    }
}

// ?? Snapshot ?????????????????????????????????????????????????????????????????

internal class SnapshotData
{
    public DateTime Date { get; init; }
    public double SunL { get; init; } public int SunSign { get; init; } public float SunDeg { get; init; }
    public double MonL { get; init; } public int MoonSign { get; init; } public float MoonDeg { get; init; }
    public double MoonPhaseAngle { get; init; }
    public double MeL  { get; init; } public int MeSign  { get; init; } public float MeDeg  { get; init; } public bool MeRetro { get; init; }
    public double VeL  { get; init; } public int VeSign  { get; init; } public float VeDeg  { get; init; } public bool VeRetro { get; init; }
    public double MaL  { get; init; } public int MaSign  { get; init; } public float MaDeg  { get; init; } public bool MaRetro { get; init; }
    public double JuL  { get; init; } public int JuSign  { get; init; } public float JuDeg  { get; init; } public bool JuRetro { get; init; }
    public double SaL  { get; init; } public int SaSign  { get; init; } public float SaDeg  { get; init; } public bool SaRetro { get; init; }
    public double UrL  { get; init; } public int UrSign  { get; init; }
    public double NeL  { get; init; } public int NeSign  { get; init; }
    public double PlL  { get; init; } public int PlSign  { get; init; }
}

// ?? Prediction context ????????????????????????????????????????????????????????

internal class PredictionContext
{
    private readonly int             _sign;
    private readonly SnapshotData    _s;    // start snapshot
    private readonly SnapshotData    _e;    // end snapshot
    private readonly SnapshotData    _p;    // previous day
    private readonly int             _days;
    private readonly SnapshotData[]? _all;

    private int H(int transitSign) => ((transitSign - _sign + 12) % 12) + 1;

    // Aspect between two absolute longitudes; returns (name, orb) or null
    private static (string name, double orb)? Aspect(double a, double b)
    {
        double diff = Math.Abs(a - b) % 360;
        if (diff > 180) diff = 360 - diff;
        (string, double, double)[] defs =
        [
            ("Conjunction", 0,   8),
            ("Sextile",     60,  5),
            ("Square",      90,  7),
            ("Trine",       120, 7),
            ("Quincunx",    150, 3),
            ("Opposition",  180, 8),
        ];
        foreach (var (name, angle, orb) in defs)
            if (Math.Abs(diff - angle) <= orb)
                return (name, Math.Round(Math.Abs(diff - angle), 1));
        return null;
    }

    private static bool IsHarmonious(string aspect)
        => aspect is "Trine" or "Sextile" or "Conjunction";

    public PredictionContext(int sign, SnapshotData start, SnapshotData end,
                             SnapshotData prev, int days, SnapshotData[]? allDays)
    {
        _sign = sign; _s = start; _e = end; _p = prev; _days = days; _all = allDays;
    }

    // ?? Scores ????????????????????????????????????????????????????????????????

    public int OverallScore()
    {
        int sc = 3;
        sc += H(_s.JuSign) is 1 or 5 or 9 or 10 && !_s.JuRetro ? 1 : 0;
        sc += H(_s.SaSign) is 6 or 8 or 12 || _s.SaRetro ? -1 : 0;
        sc += SunMoonHarmony();
        sc += AspectMod(_s.SunL, _s.JuL);
        return Math.Clamp(sc, 1, 5);
    }

    public int LoveScore()
    {
        int sc = 3;
        int vH = H(_s.VeSign);
        sc += vH is 1 or 5 or 7 ? 1 : vH is 6 or 12 ? -1 : 0;
        sc += _s.VeRetro ? -1 : 0;
        sc += H(_s.MoonSign) is 5 or 7 ? 1 : 0;
        sc += AspectMod(_s.VeL, _s.JuL);
        sc += AspectMod(_s.VeL, _s.MaL) / 2;
        return Math.Clamp(sc, 1, 5);
    }

    public int CareerScore()
    {
        int sc = 3;
        sc += H(_s.SunSign)  is 10 or 1 ? 1 : H(_s.SunSign)  is 12 or 6 ? -1 : 0;
        sc += H(_s.JuSign)   is 10 or 2 ? 1 : H(_s.JuSign)   is 6  or 12 ? -1 : 0;
        sc += _s.SaRetro || H(_s.SaSign) is 6 or 12 ? -1 : 0;
        sc += _s.MeRetro ? -1 : H(_s.MeSign) is 3 or 10 ? 1 : 0;
        return Math.Clamp(sc, 1, 5);
    }

    public int FinanceScore()
    {
        int sc = 3;
        sc += H(_s.VeSign) is 2 or 8 ? 1 : H(_s.VeSign) is 12 ? -1 : 0;
        sc += H(_s.JuSign) is 2 or 8 or 11 ? 1 : 0;
        sc += _s.VeRetro ? -1 : 0;
        sc += H(_s.SaSign) is 2 ? -1 : 0;
        sc += AspectMod(_s.VeL, _s.JuL);
        return Math.Clamp(sc, 1, 5);
    }

    public int HealthScore()
    {
        int sc = 3;
        sc += H(_s.MaSign) is 1 or 6 ? -1 : 1;
        sc += _s.MaRetro ? -1 : 0;
        sc += H(_s.SaSign) is 1 or 6 ? -1 : 0;
        sc += H(_s.JuSign) is 1 or 5 ? 1 : 0;
        sc += AspectMod(_s.SunL, _s.MaL) / 2;
        return Math.Clamp(sc, 1, 5);
    }

    public int SpiritScore()
    {
        int sc = 3;
        sc += H(_s.NeSign) is 12 or 9 or 1 ? 1 : 0;
        sc += H(_s.MoonSign) is 12 or 8 or 4 ? 1 : 0;
        double moonPhase = _s.MoonPhaseAngle;
        sc += moonPhase is > 165 and < 195 ? 1 :    // Full Moon
              moonPhase is < 15 or > 345 ? 1 : 0;   // New Moon
        return Math.Clamp(sc, 1, 5);
    }

    // ?? Key Aspects ???????????????????????????????????????????????????????????

    public List<string> KeyAspects()
    {
        var list = new List<string>();
        var pairs = new (double a, double b, string nameA, string nameB)[]
        {
            (_s.SunL, _s.MonL, "Sun",     "Moon"),
            (_s.SunL, _s.JuL,    "Sun",     "Jupiter"),
            (_s.SunL, _s.SaL,    "Sun",     "Saturn"),
            (_s.MonL, _s.VeL, "Moon",    "Venus"),
            (_s.MonL, _s.MaL, "Moon",    "Mars"),
            (_s.VeL,  _s.MaL,   "Venus",   "Mars"),
            (_s.VeL,  _s.JuL,   "Venus",   "Jupiter"),
            (_s.MeL,  _s.MaL,   "Mercury", "Mars"),
            (_s.MeL,  _s.JuL,   "Mercury", "Jupiter"),
            (_s.MaL,  _s.JuL,   "Mars",    "Jupiter"),
            (_s.JuL,  _s.SaL,   "Jupiter", "Saturn"),
            (_s.SaL,  _s.UrL,   "Saturn",  "Uranus"),
        };
        foreach (var (a, b, nA, nB) in pairs)
        {
            var asp = Aspect(a, b);
            if (asp is not null)
            {
                string harmony = IsHarmonious(asp.Value.name) ? "?" : "?";
                string signA = HoroscopeEngine.SignNames[HoroscopeEngine.SO(a)];
                string signB = HoroscopeEngine.SignNames[HoroscopeEngine.SO(b)];
                list.Add($"{harmony} {nA} in {signA} {asp.Value.name} {nB} in {signB} (orb {asp.Value.orb:F1}°)");
            }
        }
        return list;
    }

    // ?? Narrative paragraphs ??????????????????????????????????????????????????

    public string Overview(string period)
    {
        string tempo    = period == "Daily" ? "Today" : "This week";
        string sunSign  = HoroscopeEngine.SignNames[_s.SunSign];
        string moonSign = HoroscopeEngine.SignNames[_s.MoonSign];
        int    moonH    = H(_s.MoonSign);
        string moonDesc = MoonHouseDesc(moonH);
        string phaseNote = MoonPhaseNote(_s.MoonPhaseAngle);
        string rulerNote = RulerNote();
        int    overall  = OverallScore();
        string tone     = overall >= 4 ? "The stars are broadly favourable — this is an excellent time to move forward with confidence."
                        : overall <= 2 ? "The cosmic weather calls for caution; focus on consolidation rather than new initiatives."
                        :                "Mixed energies are at play; success comes to those who remain adaptable and clear-headed.";

        return $"{tempo} the Sun illuminates {sunSign} while the transiting Moon moves through {moonSign}, {moonDesc}. " +
               $"{phaseNote} {rulerNote} {tone}";
    }

    public string LoveParagraph(string period)
    {
        string veSign  = HoroscopeEngine.SignNames[_s.VeSign];
        int    vH      = H(_s.VeSign);
        string tempo   = period == "Daily" ? "today" : "this week";
        string veDesc  = VenusHouseDesc(vH);
        string retroNote = _s.VeRetro
            ? "With Venus retrograde, unresolved feelings from past relationships may surface — approach them with compassion rather than urgency. "
            : "";
        string moonLove = H(_s.MoonSign) is 5 or 7
            ? "The Moon's transit heightens emotional sensitivity and desire for connection. "
            : H(_s.MoonSign) is 12 or 8
            ? "The Moon's placement encourages emotional depth over surface-level interactions. "
            : "";
        var veJuAsp = Aspect(_s.VeL, _s.JuL);
        string aspNote = veJuAsp is not null
            ? $"A {veJuAsp.Value.name} between Venus and Jupiter {(IsHarmonious(veJuAsp.Value.name) ? "blesses romantic encounters with warmth and generosity" : "may create tension between idealism and reality in relationships")}. "
            : "";
        int lScore = LoveScore();
        string closing = lScore >= 4 ? "Open your heart — meaningful connections are within reach."
                       : lScore <= 2 ? "Give relationships space to breathe; forced situations will not thrive now."
                       :               "Honest, patient communication will strengthen bonds.";

        return $"Venus in {veSign} {veDesc} {tempo}. {retroNote}{moonLove}{aspNote}{closing}";
    }

    public string CareerParagraph(string period)
    {
        string sunSign = HoroscopeEngine.SignNames[_s.SunSign];
        string meSign  = HoroscopeEngine.SignNames[_s.MeSign];
        int    sunH    = H(_s.SunSign);
        int    meH     = H(_s.MeSign);
        string tempo   = period == "Daily" ? "today" : "this week";
        string sunArea = sunH is 10 ? "career and public reputation are spotlighted"
                       : sunH is 1  ? "personal initiative drives professional success"
                       : sunH is 6  ? "diligence in daily duties is rewarded"
                       :              $"energy flows through the {sunH}{Ord(sunH)} house of your chart";
        string meNote  = _s.MeRetro
            ? "Mercury retrograde warns against signing contracts or launching new projects without careful review. "
            : meH is 10 ? "Mercury sharpens strategic thinking and strengthens your professional voice. "
            : meH is 3  ? "Mercury encourages collaboration, networking, and short-distance travel for work. "
            : "";
        string juNote = H(_s.JuSign) is 10 or 2
            ? "Jupiter's expansive energy supports career advancement and financial growth. "
            : H(_s.JuSign) is 6 or 12 && !_s.JuRetro
            ? "Jupiter challenges you to look beyond routine and consider bolder opportunities. "
            : "";
        int cScore = CareerScore();
        string closing = cScore >= 4 ? "Seize leadership opportunities — you are operating near peak professional effectiveness."
                       : cScore <= 2 ? "Avoid hasty decisions; methodical preparation now yields better outcomes later."
                       :               "Steady effort and clear communication will carry you forward.";

        return $"With the Sun in {sunSign}, {sunArea} {tempo}. {meNote}{juNote}{closing}";
    }

    public string FinanceParagraph(string period)
    {
        int    vH     = H(_s.VeSign);
        int    juH    = H(_s.JuSign);
        string tempo  = period == "Daily" ? "today" : "this week";
        string veNote = vH is 2 ? "Venus activates your income sector, making this a positive time for earnings and purchases. "
                      : vH is 8 ? "Venus in your shared-resources house highlights joint finances, investments, and inheritances. "
                      : vH is 12 ? "Venus in the hidden sector suggests unexpected expenses — review your budget carefully. "
                      : _s.VeRetro ? "Venus retrograde advises postponing major purchases or financial commitments. "
                      : "Venus supports balanced spending and thoughtful financial decisions. ";
        string juNote = juH is 2 or 11 ? "Jupiter in your wealth sector expands financial possibility — calculated risks may pay off. "
                      : juH is 6 or 12 ? "Jupiter warns against over-spending or ill-considered generosity right now. "
                      : "";
        string saNote = H(_s.SaSign) is 2 ? "Saturn in your money house demands fiscal discipline and long-term planning. " : "";
        int fScore = FinanceScore();
        string closing = fScore >= 4 ? "Financial momentum is building — invest wisely in what genuinely adds value."
                       : fScore <= 2 ? "Conserve resources and avoid speculative ventures until planetary support improves."
                       :               "Modest, consistent financial habits will serve you best now.";

        return $"{veNote}{juNote}{saNote}{closing}";
    }

    public string HealthParagraph(string period)
    {
        int    maH    = H(_s.MaSign);
        string maSign = HoroscopeEngine.SignNames[_s.MaSign];
        string tempo  = period == "Daily" ? "today" : "this week";
        string maNote = _s.MaRetro ? "Mars retrograde can drain physical reserves — prioritise restorative practices over intense exertion. "
                      : maH is 1   ? $"Mars energises your body and vitality — channel this drive into healthy physical activity. "
                      : maH is 6   ? $"Mars in your health house flags potential for overwork-related fatigue; pace yourself. "
                      : maH is 12  ? "Mars in the subconscious house may produce restless sleep or low-grade anxiety — meditation helps. "
                      :              "Physical energy is present but benefits from structured release. ";
        string juNote = H(_s.JuSign) is 1 or 5 ? "Jupiter's positive influence supports vitality and recovery. " : "";
        int hScore = HealthScore();
        string closing = hScore >= 4 ? "Vitality is high — make the most of this energetic window."
                       : hScore <= 2 ? "Listen to your body's signals; rest is not a luxury but a necessity now."
                       :               "Moderate activity, good sleep, and mindful nutrition will keep you balanced.";

        return $"{maNote}{juNote}{closing}";
    }

    public string SpiritParagraph(string period)
    {
        string moonSign = HoroscopeEngine.SignNames[_s.MoonSign];
        string neSign   = HoroscopeEngine.SignNames[_s.NeSign];
        int    moonH    = H(_s.MoonSign);
        string phaseNote = SpiritPhaseNote(_s.MoonPhaseAngle);
        string neNote = H(_s.NeSign) is 12 or 9
            ? $"Neptune in {neSign} deepens your access to intuition and mystical insight — journalling or meditation is highly rewarding now. "
            : $"Neptune in {neSign} subtly dissolves old boundaries between the ego and the wider world. ";
        string moonNote = moonH is 12 ? "The Moon in your house of the unconscious opens a portal to vivid dreams and spiritual revelation. "
                        : moonH is 8  ? "Deep emotional excavation under this Moon transit can be profoundly healing. "
                        : moonH is 9  ? "The Moon calls you toward philosophical inquiry and spiritual journeys, literal or metaphorical. "
                        :               $"The Moon in {moonSign} grounds your spiritual practice in everyday awareness. ";

        int sScore = SpiritScore();
        string closing = sScore >= 4 ? "The veil between the seen and unseen is thin — trust the guidance that arrives."
                       : sScore <= 2 ? "Ground yourself through nature, breathwork, or physical ritual before seeking mystical insight."
                       :               "Small, consistent spiritual practices will accumulate into meaningful inner growth.";

        return $"{phaseNote}{moonNote}{neNote}{closing}";
    }

    // ?? Weekly best/caution days ??????????????????????????????????????????????

    public string BestDay(DateTime weekStart)
    {
        // Highest-scoring day = day with Moon in a harmonious house and no retrograde starts
        int bestDow = 0; int bestScore = -99;
        for (int d = 0; d < 7; d++)
        {
            var sn = _all![d];
            int sc = H(sn.MoonSign) switch { 1 or 5 or 9 or 10 => 2, 6 or 12 => -2, _ => 0 };
            sc += sn.JuRetro || sn.SaRetro ? -1 : 1;
            if (sc > bestScore) { bestScore = sc; bestDow = d; }
        }
        return $"{HoroscopeEngine.DayNames[(int)weekStart.AddDays(bestDow).DayOfWeek]} ({weekStart.AddDays(bestDow):dd MMM})";
    }

    public string CautionDay(DateTime weekStart)
    {
        int cautionDow = 0; int worstScore = 99;
        for (int d = 0; d < 7; d++)
        {
            var sn = _all![d];
            int sc = H(sn.MoonSign) switch { 6 or 12 or 8 => -2, 1 or 5 => 2, _ => 0 };
            sc += sn.MeRetro ? -2 : 0;
            if (sc < worstScore) { worstScore = sc; cautionDow = d; }
        }
        return $"{HoroscopeEngine.DayNames[(int)weekStart.AddDays(cautionDow).DayOfWeek]} ({weekStart.AddDays(cautionDow):dd MMM})";
    }

    // ?? Lucky properties ??????????????????????????????????????????????????????

    public string LuckyColor()
    {
        string[] colors = ["Red","Green","Yellow","Silver","Gold","Navy",
                           "Pink","Black","Purple","Brown","Sky Blue","Sea Green"];
        return colors[(_sign + _s.VeSign) % 12];
    }

    public int LuckyNumber()
    {
        int raw = (_sign + 1) + (int)_s.MoonDeg + H(_s.JuSign);
        return (raw % 9) + 1;
    }

    public string Affirmation()
    {
        int overall = OverallScore();
        string[] aff =
        [
            "I release what no longer serves me and welcome new beginnings.",         // 1
            "I trust the process and find strength in patient perseverance.",          // 2
            "I am open, balanced, and ready to receive what the universe offers.",     // 3
            "I move forward with confidence; my actions create positive momentum.",    // 4
            "I am aligned with abundance and share my gifts generously with the world.", // 5
        ];
        return aff[overall - 1];
    }

    public string Tip()
    {
        if (_s.MeRetro) return "Back up data, re-read before sending, and avoid signing contracts.";
        if (_s.VeRetro) return "Reconnect with existing relationships before pursuing new ones.";
        if (_s.MaRetro) return "Redirect frustrated energy into creative or spiritual outlets.";
        if (HealthScore() <= 2) return "Prioritise rest; your long-term wellbeing outweighs short-term urgency.";
        if (CareerScore() >= 4) return "Lead decisively — colleagues and superiors are receptive to your ideas.";
        if (LoveScore() >= 4)   return "Express affection openly; vulnerability deepens intimacy.";
        if (SpiritScore() >= 4) return "Meditate or journal at dawn; insights received now carry lasting value.";
        return "Stay present, breathe deliberately, and let clarity guide each decision.";
    }

    // ?? Private helpers ???????????????????????????????????????????????????????

    private int SunMoonHarmony()
    {
        double diff = Math.Abs(_s.SunL - _s.MonL) % 360;
        if (diff > 180) diff = 360 - diff;
        return diff < 30 || Math.Abs(diff - 120) < 15 || Math.Abs(diff - 60) < 10 ? 1
             : Math.Abs(diff - 180) < 15 || Math.Abs(diff - 90) < 10 ? -1 : 0;
    }

    // Returns -1, 0, or +1 based on aspect between two longitudes
    private static int AspectMod(double a, double b)
    {
        var asp = Aspect(a, b);
        if (asp is null) return 0;
        return IsHarmonious(asp.Value.name) ? 1 : -1;
    }

    private string MoonHouseDesc(int h) => h switch
    {
        1  => "heightening self-awareness and personal magnetism",
        2  => "drawing attention to material security and self-worth",
        3  => "stimulating communication, learning, and short trips",
        4  => "calling you home to family, roots, and emotional foundations",
        5  => "sparking creativity, romance, and playful self-expression",
        6  => "focusing on health routines, service, and daily discipline",
        7  => "illuminating partnerships and the need for balance",
        8  => "stirring deep emotional currents and transformative insight",
        9  => "opening your mind to philosophy, travel, and higher learning",
        10 => "shining a spotlight on career ambitions and public image",
        11 => "connecting you with community, ideals, and future visions",
        12 => "encouraging rest, solitude, and inner reflection",
        _  => $"activating the {h}{Ord(h)} house of your natal chart"
    };

    private string VenusHouseDesc(int h) => h switch
    {
        1  => "radiates charm and personal attractiveness",
        2  => "supports financial gain and sensory pleasure",
        3  => "sweetens conversations and neighbourly bonds",
        4  => "beautifies home life and family harmony",
        5  => "elevates romance, creativity, and joyful self-expression",
        6  => "finds love through shared tasks and acts of service",
        7  => "blesses partnerships and committed relationships",
        8  => "deepens intimacy and may bring financial gifts through others",
        9  => "encourages romance across cultures and philosophical connections",
        10 => "may bring professional recognition or a public relationship",
        11 => "favours social connections and group harmony",
        12 => "creates a private, introspective love life",
        _  => "influences your relationships in subtle ways"
    };

    private string MoonPhaseNote(double angle) => angle switch
    {
        < 15 or >= 345 => "The New Moon invites fresh intentions — plant seeds you wish to see bloom. ",
        < 90           => "The waxing Moon builds momentum — take action on what you have initiated. ",
        < 195          => "The Full Moon amplifies emotions and illuminates hidden truths — stay centred. ",
        < 270          => "The waning Moon is ideal for releasing, reflecting, and completing unfinished cycles. ",
        _              => "The Balsamic Moon is a sacred pause before the next lunar chapter — rest and integrate. ",
    };

    private string SpiritPhaseNote(double angle) => angle switch
    {
        < 15 or >= 345 => "The New Moon opens a powerful window for intention-setting and fresh spiritual commitments. ",
        < 90           => "The growing Moon amplifies your prayers and affirmations with forward-moving energy. ",
        < 195          => "The Full Moon heightens psychic sensitivity — dreams and synchronicities carry important messages. ",
        _              => "The waning Moon helps release energetic blocks and old spiritual patterns that no longer serve. ",
    };

    private string RulerNote()
    {
        string ruler = HoroscopeEngine.Rulers[_sign];
        double rulerLon = ruler switch
        {
            "Su" => _s.SunL, "Mo" => _s.MonL, "Me" => _s.MeL,
            "Ve" => _s.VeL,  "Ma" => _s.MaL,    "Ju" => _s.JuL,
            "Sa" => _s.SaL,  _ => _s.SunL
        };
        bool retro = ruler switch
        {
            "Me" => _s.MeRetro, "Ve" => _s.VeRetro, "Ma" => _s.MaRetro,
            "Ju" => _s.JuRetro, "Sa" => _s.SaRetro, _ => false
        };
        string rulerName = HoroscopeEngine.RulerFullName(ruler);
        string rulerSign = HoroscopeEngine.SignNames[HoroscopeEngine.SO(rulerLon)];
        string retroStr  = retro ? " (retrograde)" : "";
        int rulerH = H(HoroscopeEngine.SO(rulerLon));
        return $"Your ruling planet {rulerName} is transiting {rulerSign}{retroStr}, activating your {rulerH}{Ord(rulerH)} house. ";
    }

    private static string Ord(int n) => n switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
}

// ?? Output model ??????????????????????????????????????????????????????????????

public class Horoscope
{
    public string   SignName         { get; set; } = "";
    public string   Element          { get; set; } = "";
    public string   Modality         { get; set; } = "";
    public string   RulingPlanet     { get; set; } = "";
    public string   Period           { get; set; } = "";
    public DateTime Date             { get; set; }
    public string   MoonPhase        { get; set; } = "";
    public int      Overall          { get; set; }
    public int      Love             { get; set; }
    public int      Career           { get; set; }
    public int      Finances         { get; set; }
    public int      Health           { get; set; }
    public int      Spirituality     { get; set; }
    public List<string> KeyAspects   { get; set; } = [];
    public string   Overview         { get; set; } = "";
    public string   LoveParagraph    { get; set; } = "";
    public string   CareerParagraph  { get; set; } = "";
    public string   FinanceParagraph { get; set; } = "";
    public string   HealthParagraph  { get; set; } = "";
    public string   SpiritParagraph  { get; set; } = "";
    public string?  BestDay          { get; set; }
    public string?  CautionDay       { get; set; }
    public List<string> CompatibleSigns { get; set; } = [];
    public string   LuckyColor       { get; set; } = "";
    public int      LuckyNumber      { get; set; }
    public string   LuckyStone       { get; set; } = "";
    public string   Affirmation      { get; set; } = "";
    public string   Tip              { get; set; } = "";
}
