using AASharp;

/// <summary>
/// Calculates sidereal (Vedic/Jyotish) planetary positions using the Lahiri ayanamsa.
/// </summary>
public static class VedicCalculator
{
    private static readonly string[] SignNames =
        ["Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo",
         "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"];

    private static readonly string[] SignAbbr =
        ["Ar", "Ta", "Ge", "Ca", "Le", "Vi", "Li", "Sc", "Sa", "Cp", "Aq", "Pi"];

    /// <summary>
    /// Calculates the Lahiri ayanamsa for a given Julian Day.
    /// Lahiri value at J2000.0 = 23.8553°, precessing at ~50.2388"/year.
    /// </summary>
    public static double LahiriAyanamsa(double jd)
    {
        double t = (jd - 2451545.0) / 365.25; // years since J2000.0
        return 23.8553 + t * 0.013956;
    }

    /// <summary>
    /// Converts a tropical longitude to sidereal by subtracting the Lahiri ayanamsa.
    /// </summary>
    public static double ToSidereal(double tropicalLongitude, double ayanamsa)
    {
        double sid = tropicalLongitude - ayanamsa;
        return ((sid % 360) + 360) % 360;
    }

    /// <summary>
    /// Calculates all Vedic planet positions for today (UTC) at the given location.
    /// </summary>
    /// <summary>
    /// Calculates all Vedic planet positions for a given local birth date/time.
    /// </summary>
    /// <param name="localDateTime">Local birth date and time (do NOT pre-convert to UTC).</param>
    /// <param name="latitude">Observer latitude in degrees.</param>
    /// <param name="longitude">Observer longitude in degrees.</param>
    /// <param name="utcOffsetHours">UTC offset of the birth location (e.g. +5.5 for IST, -5 for EST).</param>
    public static VedicChartData Calculate(DateTime localDateTime, double latitude, double longitude, double utcOffsetHours = 0.0)
    {
        // Convert local birth time to UTC by subtracting the offset
        DateTime utc = localDateTime.AddHours(-utcOffsetHours);

        double jd       = ToJulianDay(utc);
        double jdMinus1 = jd - 1.0;
        double ayanamsa = LahiriAyanamsa(jd);

        var planets = new List<VedicPlanetPosition>();

        // All planets use AASElliptical.Calculate which returns the apparent
        // GEOCENTRIC ecliptic longitude — the correct value for a birth chart.
        // AASMercury/AASVenus.EclipticLongitude return heliocentric values and
        // must NOT be used here.

        // Sun — AASSun gives geocentric apparent longitude directly
        double sunTrop  = Normalize(AASSun.ApparentEclipticLongitude(jd,       false));
        double sunTrop1 = Normalize(AASSun.ApparentEclipticLongitude(jdMinus1, false));
        planets.Add(MakePosition("Su", ToSidereal(sunTrop, ayanamsa), IsRetrograde(sunTrop1, sunTrop)));

        // Moon — AASMoon gives geocentric longitude directly
        double monTrop  = Normalize(AASMoon.EclipticLongitude(jd));
        double monTrop1 = Normalize(AASMoon.EclipticLongitude(jdMinus1));
        planets.Add(MakePosition("Mo", ToSidereal(monTrop, ayanamsa), IsRetrograde(monTrop1, monTrop)));

        // Mercury — geocentric apparent
        double meTrop  = Normalize(AASElliptical.Calculate(jd,       AASEllipticalObject.MERCURY, false).ApparentGeocentricLongitude);
        double meTrop1 = Normalize(AASElliptical.Calculate(jdMinus1, AASEllipticalObject.MERCURY, false).ApparentGeocentricLongitude);
        planets.Add(MakePosition("Me", ToSidereal(meTrop, ayanamsa), IsRetrograde(meTrop1, meTrop)));

        // Venus — geocentric apparent
        double veTrop  = Normalize(AASElliptical.Calculate(jd,       AASEllipticalObject.VENUS, false).ApparentGeocentricLongitude);
        double veTrop1 = Normalize(AASElliptical.Calculate(jdMinus1, AASEllipticalObject.VENUS, false).ApparentGeocentricLongitude);
        planets.Add(MakePosition("Ve", ToSidereal(veTrop, ayanamsa), IsRetrograde(veTrop1, veTrop)));

        // Mars — geocentric apparent
        double maTrop  = Normalize(AASElliptical.Calculate(jd,       AASEllipticalObject.MARS, false).ApparentGeocentricLongitude);
        double maTrop1 = Normalize(AASElliptical.Calculate(jdMinus1, AASEllipticalObject.MARS, false).ApparentGeocentricLongitude);
        planets.Add(MakePosition("Ma", ToSidereal(maTrop, ayanamsa), IsRetrograde(maTrop1, maTrop)));

        // Jupiter — geocentric apparent
        double juTrop  = Normalize(AASElliptical.Calculate(jd,       AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude);
        double juTrop1 = Normalize(AASElliptical.Calculate(jdMinus1, AASEllipticalObject.JUPITER, false).ApparentGeocentricLongitude);
        planets.Add(MakePosition("Ju", ToSidereal(juTrop, ayanamsa), IsRetrograde(juTrop1, juTrop)));

        // Saturn — geocentric apparent
        double saTrop  = Normalize(AASElliptical.Calculate(jd,       AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude);
        double saTrop1 = Normalize(AASElliptical.Calculate(jdMinus1, AASEllipticalObject.SATURN, false).ApparentGeocentricLongitude);
        planets.Add(MakePosition("Sa", ToSidereal(saTrop, ayanamsa), IsRetrograde(saTrop1, saTrop)));

        // Uranus — geocentric apparent
        double urTrop  = Normalize(AASElliptical.Calculate(jd,       AASEllipticalObject.URANUS, false).ApparentGeocentricLongitude);
        double urTrop1 = Normalize(AASElliptical.Calculate(jdMinus1, AASEllipticalObject.URANUS, false).ApparentGeocentricLongitude);
        planets.Add(MakePosition("Ur", ToSidereal(urTrop, ayanamsa), IsRetrograde(urTrop1, urTrop)));

        // Neptune — geocentric apparent
        double neTrop  = Normalize(AASElliptical.Calculate(jd,       AASEllipticalObject.NEPTUNE, false).ApparentGeocentricLongitude);
        double neTrop1 = Normalize(AASElliptical.Calculate(jdMinus1, AASEllipticalObject.NEPTUNE, false).ApparentGeocentricLongitude);
        planets.Add(MakePosition("Ne", ToSidereal(neTrop, ayanamsa), IsRetrograde(neTrop1, neTrop)));

        // Pluto — geocentric apparent
        double plTrop  = Normalize(AASElliptical.Calculate(jd,       AASEllipticalObject.PLUTO, false).ApparentGeocentricLongitude);
        double plTrop1 = Normalize(AASElliptical.Calculate(jdMinus1, AASEllipticalObject.PLUTO, false).ApparentGeocentricLongitude);
        planets.Add(MakePosition("Pl", ToSidereal(plTrop, ayanamsa), IsRetrograde(plTrop1, plTrop)));

        // Rahu (Mean North Node) — always retrograde in mean motion
        double rahuTrop = Normalize(AASMoon.MeanLongitudeAscendingNode(jd));
        double rahuSid  = ToSidereal(rahuTrop, ayanamsa);
        planets.Add(MakePosition("Ra", rahuSid, isRetrograde: true));

        // Ketu (South Node) = Rahu + 180°
        double ketuSid = (rahuSid + 180.0) % 360.0;
        planets.Add(MakePosition("Ke", ketuSid, isRetrograde: true));

        // Ascendant (Lagna)
        float ascTropical = CalculateAscendant(jd, latitude, longitude);
        double ascSid     = ToSidereal(ascTropical, ayanamsa);
        int    ascSign    = (int)(ascSid / 30.0) % 12; // 0-based sign index
        float  ascDeg     = (float)(ascSid % 30.0);

        return new VedicChartData
        {
            Planets         = planets,
            AscendantSign   = ascSign,
            AscendantDegree = ascDeg,
            AscendantSid    = (float)ascSid,
            Ayanamsa        = ayanamsa,
            ChartDate       = localDateTime,
            Latitude        = latitude,
            Longitude       = longitude,
        };
    }

    /// <summary>
    /// Prints a summary of Vedic planetary positions to the console.
    /// </summary>
    public static void PrintSummary(VedicChartData data)
    {
        Console.WriteLine($"Vedic Birth Chart — {data.ChartDate:yyyy-MM-dd HH:mm} UTC");
        Console.WriteLine($"Location : {data.Latitude:F4}N  {data.Longitude:F4}E");
        Console.WriteLine($"Lahiri Ayanamsa : {data.Ayanamsa:F4}°");
        Console.WriteLine($"Lagna (Ascendant): {SignNames[data.AscendantSign]} {data.AscendantDegree:F2}°");
        Console.WriteLine();
        Console.WriteLine($"{"Planet",-10} {"Sign",-14} {"Deg":>6}  {"House",5}  Retro");
        Console.WriteLine(new string('-', 48));
        foreach (var p in data.Planets)
        {
            string retro = p.IsRetrograde ? " (R)" : "";
            int house = ((p.SignIndex - data.AscendantSign + 12) % 12) + 1;
            Console.WriteLine($"{FullName(p.Name),-10} {SignNames[p.SignIndex],-14} {p.DegreeInSign,5:F2}°  H{house,2}  {retro}");
        }
        Console.WriteLine();
    }

    // ?? Helpers ?????????????????????????????????????????????????????????????

    private static VedicPlanetPosition MakePosition(string name, double siderealLon, bool isRetrograde)
    {
        int   signIndex = (int)(siderealLon / 30.0) % 12;
        float deg       = (float)(siderealLon % 30.0);
        return new VedicPlanetPosition(name, signIndex, deg, isRetrograde);
    }

    /// <summary>
    /// Returns true when the planet moved backward between jd-1 and jd (retrograde).
    /// Handles the 359°?1° wrap-around.
    /// </summary>
    private static bool IsRetrograde(double lonPrev, double lonNow)
    {
        double delta = lonNow - lonPrev;
        if (delta > 180)  delta -= 360;
        if (delta < -180) delta += 360;
        return delta < 0;
    }

    private static float CalculateAscendant(double jd, double latitude, double longitude)
    {
        // Get the sidereal time directly for the given JD
        double gst = AASSidereal.ApparentGreenwichSiderealTime(jd);

        // Convert to LST (Local Sidereal Time)
        double lst = ((gst + longitude / 15.0) % 24 + 24) % 24;
        double ramc = lst * 15.0;

        double eps = AASNutation.MeanObliquityOfEcliptic(jd);
        double ramcRad = ramc * Math.PI / 180.0;
        double epsRad = eps * Math.PI / 180.0;
        double latRad = latitude * Math.PI / 180.0;

        // Ascendant formula from Jean Meeus "Astronomical Algorithms"
        // tan(ASC) = cos(RAMC) / (-sin(RAMC)*cos(eps) - tan(lat)*sin(eps))
        double y = Math.Cos(ramcRad);
        double x = -Math.Sin(ramcRad) * Math.Cos(epsRad) - Math.Tan(latRad) * Math.Sin(epsRad);

        double asc = Math.Atan2(y, x) * 180.0 / Math.PI;
        asc = Normalize(asc);

        return (float)asc;
    }

    private static double ToJulianDay(DateTime dt)
    {
        // Use the date/time fields directly.
        // Do NOT call ToUniversalTime() — DateTimeKind.Unspecified would be treated
        // as the local machine timezone and silently shift the time, corrupting RAMC.
        return AASDate.DateToJD(
            dt.Year, dt.Month,
            dt.Day + (dt.Hour + dt.Minute / 60.0 + dt.Second / 3600.0) / 24.0,
            true); // true = Gregorian calendar
    }

    private static double Normalize(double deg)
    {
        deg %= 360;
        if (deg < 0) deg += 360;
        return deg;
    }

    private static string FullName(string sym) => sym switch
    {
        "Su" => "Sun",   "Mo" => "Moon",    "Me" => "Mercury",
        "Ve" => "Venus", "Ma" => "Mars",    "Ju" => "Jupiter",
        "Sa" => "Saturn","Ra" => "Rahu",    "Ke" => "Ketu",
        "Ur" => "Uranus","Ne" => "Neptune", "Pl" => "Pluto",
        _ => sym
    };
}

/// <summary>All data needed to render a Vedic chart.</summary>
public class VedicChartData
{
    public List<VedicPlanetPosition> Planets { get; set; } = [];
    /// <summary>0-based sign index of the Ascendant (0=Aries … 11=Pisces).</summary>
    public int    AscendantSign   { get; set; }
    public float  AscendantDegree { get; set; }
    public float  AscendantSid    { get; set; }
    public double Ayanamsa        { get; set; }
    public DateTime ChartDate     { get; set; }
    public double Latitude        { get; set; }
    public double Longitude       { get; set; }
}

/// <summary>A planet's sidereal position.</summary>
/// <param name="Name">Two-letter symbol (Su, Mo, Me, Ve, Ma, Ju, Sa, Ra, Ke).</param>
/// <param name="SignIndex">0-based sidereal sign index (0=Aries … 11=Pisces).</param>
/// <param name="DegreeInSign">Degree within that sign (0–30°).</param>
/// <param name="IsRetrograde">True when the planet is retrograde.</param>
public record VedicPlanetPosition(string Name, int SignIndex, float DegreeInSign, bool IsRetrograde = false);
