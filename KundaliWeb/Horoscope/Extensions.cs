using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace KundaliWeb;

public static class Extensions
{
    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static void SetS<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetS<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    public static T? GetC<T>(this IMemoryCache cache, string key)
    {
        byte[]? bytes = (byte[]?)cache.Get(key);                                    
        if (bytes is { Length: > 0 })
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json);
        }
        else return default;
    }

    public static void SetC<T>(this IMemoryCache cache, string key, T value)
    {         
        string json = JsonSerializer.Serialize(value);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        cache.Set(key, bytes);
    }

    public static string LatitudeToGPS(Double UnformattedLatitude)
    {
        string Direction = "";
        if (UnformattedLatitude > 0)
        {
            Direction = "N";
        }
        else
        {
            UnformattedLatitude = UnformattedLatitude * -1;
            Direction = "S";
        }
        //string GPSString = UnformattedLatitude.ToString("0.0000") + Direction;
        return Direction;
    }

    public static string LongitudeToGPS(Double UnformattedLongitude)
    {
        string Direction = "";            
        if (UnformattedLongitude > 0)
        {
            Direction = "E";
        }
        else
        {
            UnformattedLongitude = UnformattedLongitude * -1;
            Direction = "W";
        }
        //string GPSString = UnformattedLongitude.ToString("0.0000") + Direction;
        return Direction;
    }

    public static uint GetMinutes(Double cord)
    {            
        int sec = (int)Math.Round(cord * 3600);
        int deg = sec / 3600;
        sec = Math.Abs(sec % 3600);
        int min = sec / 60;
        return (uint)min;
    }

    public static uint GetSeconds(Double cord)
    {
        int sec = (int)Math.Round(cord * 3600);
        int deg = sec / 3600;
        sec = Math.Abs(sec % 3600);
        int min = sec / 60;
        sec %= 60;
        return (uint)sec;
    }

    public static IEnumerable<string> SplitByLength(this string str, int maxLength)
    {
        int index = 0;
        while (index + maxLength < str.Length)
        {
            yield return str.Substring(index, maxLength);
            index += maxLength;
        }

        yield return str.Substring(index);
    }

    private static double UniversalTimeFromCalendar(int year, int month, int day, int hour, int minute, double second)
    {
        // This formula is adapted from NOVAS C 3.1 function julian_date(),
        // which in turn comes from Henry F. Fliegel & Thomas C. Van Flendern:
        // Communications of the ACM, Vol 11, No 10, October 1968, p. 657.
        // See: https://dl.acm.org/doi/pdf/10.1145/364096.364097
        //
        // [Don Cross - 2023-02-25] I modified the formula so that it will
        // work correctly with years as far back as -999999.

        long y = (long)year;
        long m = (long)month;
        long d = (long)day;
        long f = (14 - m) / 12;

        long y2000 = (
            (d - 365972956)
            + (1461 * (y + 1000000 - f)) / 4
            + (367 * (m - 2 + 12 * f)) / 12
            - (3 * ((y + 1000100 - f) / 100)) / 4
        );

        double ut = (y2000 - 0.5) + (hour / 24.0) + (minute / 1440.0) + (second / 86400.0);
        return ut;
    }
}
