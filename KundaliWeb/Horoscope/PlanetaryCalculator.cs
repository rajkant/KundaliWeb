using AASharp;

/// <summary>
/// Calculates tropical (Western) geocentric planetary positions.
/// All longitudes are returned as apparent geocentric ecliptic longitude (0-360°).
/// </summary>
public static class PlanetaryCalculator
{
    private static readonly string[] SignNames =
        ["Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo",
         "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"];

    /// <summary>
    /// Calculates tropical planet positions for a given LOCAL date/time.
    /// </summary>
    /// <param name="localDateTime">Local birth/event date and time.</param>
    /// <param name="latitude">Observer latitude in degrees.</param>
    /// <param name="longitude">Observer longitude in degrees.</param>
    /// <param name="utcOffsetHours">UTC offset of the location (e.g. +5.5 for IST, -5 for EST).</param>
    public static WesternChartData Calculate(
        DateTime localDateTime,
        double latitude,
        double longitude,
        double utcOffsetHours = 0.0)
    {
        DateTime utc    = localDateTime.AddHours(-utcOffsetHours);
        double   jd     = ToJulianDay(utc);
        double   jdPrev = jd - 1.0;

        var planets = new List<WesternPlanetPosition>();

        // Sun — AASSun.ApparentEclipticLongitude is geocentric by definition
        double sunL  = Norm(AASSun.ApparentEclipticLongitude(jd,     false));
        double sunL1 = Norm(AASSun.ApparentEclipticLongitude(jdPrev, false));
        planets.Add(Make("Su", sunL, IsRetro(sunL1, sunL)));

        // Moon — AASMoon.EclipticLongitude is geocentric by definition
        double monL  = Norm(AASMoon.EclipticLongitude(jd));
        double monL1 = Norm(AASMoon.EclipticLongitude(jdPrev));
        planets.Add(Make("Mo", monL, IsRetro(monL1, monL)));

        // Inner & outer planets — use AASElliptical which returns ApparentGeocentricLongitude.
        // AASMercury/AASVenus/AASMars etc. return HELIOCENTRIC longitudes — do NOT use them.
        planets.Add(GeoPosition("Me", jd, jdPrev, AASEllipticalObject.MERCURY));
        planets.Add(GeoPosition("Ve", jd, jdPrev, AASEllipticalObject.VENUS));
        planets.Add(GeoPosition("Ma", jd, jdPrev, AASEllipticalObject.MARS));
        planets.Add(GeoPosition("Ju", jd, jdPrev, AASEllipticalObject.JUPITER));
        planets.Add(GeoPosition("Sa", jd, jdPrev, AASEllipticalObject.SATURN));
        planets.Add(GeoPosition("Ur", jd, jdPrev, AASEllipticalObject.URANUS));
        planets.Add(GeoPosition("Ne", jd, jdPrev, AASEllipticalObject.NEPTUNE));
        planets.Add(GeoPosition("Pl", jd, jdPrev, AASEllipticalObject.PLUTO));

        // North Node (Mean)
        double rahuL = Norm(AASMoon.MeanLongitudeAscendingNode(jd));
        planets.Add(Make("Ra", rahuL, isRetro: true));   // always retrograde

        // Tropical Ascendant
        float ascTrop = CalculateAscendant(jd, latitude, longitude);

        return new WesternChartData
        {
            Planets          = planets,
            AscendantDegree  = ascTrop,
            ChartDate        = localDateTime,
            Latitude         = latitude,
            Longitude        = longitude,
        };
    }

    /// <summary>Prints a console summary of tropical positions.</summary>
    public static void PrintSummary(WesternChartData data)
    {
        Console.WriteLine($"Western Natal Chart  —  {data.ChartDate:dd MMM yyyy  HH:mm}");
        Console.WriteLine($"Location : {data.Latitude:F4}°N  {data.Longitude:F4}°E");
        Console.WriteLine($"Ascendant: {SignNames[(int)(data.AscendantDegree / 30) % 12]}  {data.AscendantDegree % 30:F2}°");
        Console.WriteLine();
        Console.WriteLine($"{"Planet",-10} {"Sign",-14} {"Deg",6}  Retro");
        Console.WriteLine(new string('-', 42));
        foreach (var p in data.Planets)
        {
            string retro = p.IsRetrograde ? " (R)" : "";
            Console.WriteLine($"{FullName(p.Name),-10} {SignNames[p.SignIndex],-14} {p.DegreeInSign,5:F2}°{retro}");
        }
        Console.WriteLine();
    }

    // ?? Private helpers ??????????????????????????????????????????????????????

    private static WesternPlanetPosition GeoPosition(
        string name, double jd, double jdPrev, AASEllipticalObject obj)
    {
        double lon  = Norm(AASElliptical.Calculate(jd,     obj, false).ApparentGeocentricLongitude);
        double lon1 = Norm(AASElliptical.Calculate(jdPrev, obj, false).ApparentGeocentricLongitude);
        return Make(name, lon, IsRetro(lon1, lon));
    }

    private static WesternPlanetPosition Make(string name, double lon, bool isRetro)
    {
        int   signIdx = (int)(lon / 30.0) % 12;
        float deg     = (float)(lon % 30.0);
        return new WesternPlanetPosition(name, signIdx, deg, isRetro);
    }

    private static float CalculateAscendant(double jd, double latitude, double longitude)
    {
        double gst  = AASSidereal.ApparentGreenwichSiderealTime(jd);
        double lst  = ((gst + longitude / 15.0) % 24 + 24) % 24;
        double ramc = lst * 15.0;

        double eps     = AASNutation.MeanObliquityOfEcliptic(jd);
        double ramcRad = ramc    * Math.PI / 180.0;
        double epsRad  = eps     * Math.PI / 180.0;
        double latRad  = latitude * Math.PI / 180.0;

        // Meeus Astronomical Algorithms — ascendant formula
        double y   = -Math.Cos(ramcRad);
        double x   =  Math.Sin(epsRad) * Math.Tan(latRad) + Math.Cos(epsRad) * Math.Sin(ramcRad);
        double asc = Norm(Math.Atan2(y, x) * 180.0 / Math.PI);

        // MC for quadrant correction
        double mc = Norm(Math.Atan2(Math.Tan(ramcRad), Math.Cos(epsRad)) * 180.0 / Math.PI);
        if (Norm(asc - mc) < 90.0 || Norm(asc - mc) > 270.0)
            asc = Norm(asc + 180.0);

        return (float)asc;
    }

    private static double ToJulianDay(DateTime utc)
        => AASDate.DateToJD(
            utc.Year, utc.Month,
            utc.Day + (utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0) / 24.0,
            true);  // true = Gregorian

    private static bool IsRetro(double prev, double now)
    {
        double d = now - prev;
        if (d >  180) d -= 360;
        if (d < -180) d += 360;
        return d < 0;
    }

    private static double Norm(double deg)
    {
        deg %= 360;
        if (deg < 0) deg += 360;
        return deg;
    }

    private static string FullName(string s) => s switch
    {
        "Su" => "Sun",    "Mo" => "Moon",    "Me" => "Mercury",
        "Ve" => "Venus",  "Ma" => "Mars",    "Ju" => "Jupiter",
        "Sa" => "Saturn", "Ur" => "Uranus",  "Ne" => "Neptune",
        "Pl" => "Pluto",  "Ra" => "N.Node",
        _ => s
    };
}

// ?? Data models ??????????????????????????????????????????????????????????????

/// <summary>All data needed to render a Western tropical chart.</summary>
public class WesternChartData
{
    public List<WesternPlanetPosition> Planets { get; set; } = [];
    /// <summary>Tropical Ascendant in absolute degrees (0-360°).</summary>
    public float    AscendantDegree { get; set; }
    public DateTime ChartDate       { get; set; }
    public double   Latitude        { get; set; }
    public double   Longitude       { get; set; }
}

/// <summary>A planet's tropical position.</summary>
public record WesternPlanetPosition(
    string Name,
    int    SignIndex,
    float  DegreeInSign,
    bool   IsRetrograde = false)
{
    /// <summary>Absolute ecliptic longitude (0-360°).</summary>
    public float AbsoluteLongitude => SignIndex * 30f + DegreeInSign;
}
