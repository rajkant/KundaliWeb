#region Assembly VedAstro.Library, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\singh\.nuget\packages\vedastro.library\1.2.0\lib\net7.0\VedAstro.Library.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace VedAstro.Library;

public static class Tools
{
    public static XElement TimeStampSystemXml => new XElement("TimeStamp", GetNowSystemTimeSecondsText());

    public static XElement TimeStampServerXml => new XElement("TimeStampServer", GetNowServerTimeSecondsText());

    public static List<string> SplitAlpha(string input)
    {
        List<string> list = new List<string> { string.Empty };
        for (int i = 0; i < input.Length; i++)
        {
            list[list.Count - 1] += input[i];
            if (i + 1 < input.Length && char.IsLetter(input[i]) != char.IsLetter(input[i + 1]))
            {
                list.Add(string.Empty);
            }
        }

        return list;
    }

    public static string XmlToString(XElement xml)
    {
        return xml.ToString(SaveOptions.DisableFormatting);
    }

    public static async Task<List<XElement>> GetXmlFileHttp(string url)
    {
        using HttpClient client = new HttpClient();
        XDocument document = XDocument.Load(await client.GetStreamAsync(url));
        return document.Root.Elements().ToList();
    }

    public static XElement AnyTypeToXml<T>(T value)
    {
        if ((object)value is IToXml toXml)
        {
            return toXml.ToXml();
        }

        string content = value?.ToString();
        string fullName = typeof(T).FullName;
        return new XElement(fullName, content);
    }

    public static XElement AnyTypeToXmlList<T>(List<T> xmlList, string rootElementName = "Root") where T : IToXml
    {
        XElement xElement = new XElement(rootElementName);
        foreach (T xml in xmlList)
        {
            xElement.Add(AnyTypeToXml(xml));
        }

        return xElement;
    }

    public static XElement AnyTypeToXmlList(List<XElement> xmlList, string rootElementName = "Root")
    {
        XElement xElement = new XElement(rootElementName);
        foreach (XElement xml in xmlList)
        {
            xElement.Add(xml);
        }

        return xElement;
    }

    public static async Task<List<T>> ConvertXmlListFileToInstanceList<T>(string httpUrl) where T : IToXml, new()
    {
        List<XElement> eventDataListXml = await GetXmlFileHttp(httpUrl);
        List<T> eventDataList = new List<T>();
        foreach (XElement eventDataXml in eventDataListXml)
        {
            eventDataList.Add(new T().FromXml<T>(eventDataXml));
        }

        return eventDataList;
    }

    public static XElement ExceptionToXml(Exception e)
    {
        XElement xElement = new XElement("Exception");
        xElement.Add("#Message#\n" + e.Message + "\n");
        xElement.Add($"#Data#\n{e.Data}\n");
        xElement.Add($"#InnerException#\n{e.InnerException}\n");
        xElement.Add("#Source#\n" + e.Source + "\n");
        xElement.Add("#Source#\n" + e.Source + "\n");
        xElement.Add("#StackTrace#\n" + e.StackTrace + "\n");
        xElement.Add($"#StackTrace#\n{e.TargetSite}\n");
        return xElement;
    }

    public static dynamic XmlToAnyType<T>(XElement xml)
    {
        string fullName = typeof(T).FullName;
        string fullName2 = typeof(T).FullName;
        Console.WriteLine(xml.ToString());
        XName xName = xml?.Name;
        string value = xml.Value;
        if (!(xName == fullName) && !(xName == typeof(T).GetShortTypeName()))
        {
            throw new Exception($"Can't parse XML {xName} to {fullName}");
        }

        if (typeof(T).GetInterfaces().Any((Type x) => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IToXml)))
        {
            dynamic instance = GetInstance(typeof(T).FullName);
            return instance.FromXml(xml);
        }

        if (typeof(T).IsEnum)
        {
            T val = (T)Enum.Parse(typeof(T), value);
            return val;
        }

        if (typeof(T) == typeof(string))
        {
            return value;
        }

        if (typeof(T) == typeof(double))
        {
            return double.Parse(value);
        }

        if (typeof(T) == typeof(int))
        {
            return int.Parse(value);
        }

        throw new NotImplementedException("XML converter for " + fullName + ", not implemented!");
    }

    public static string GetShortTypeName(this Type type)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string name = type.Name;
        if (!type.IsGenericType)
        {
            return name;
        }

        stringBuilder.Append(name.Substring(0, name.IndexOf('`')));
        stringBuilder.Append("<");
        stringBuilder.Append(string.Join(", ", from t in type.GetGenericArguments()
                                               select t.GetShortTypeName()));
        stringBuilder.Append(">");
        return stringBuilder.ToString();
    }

    public static bool Implements<I>(this Type type, I @interface) where I : class
    {
        if (@interface as Type == null || !(@interface as Type).IsInterface)
        {
            throw new ArgumentException("Only interfaces can be 'implemented'.");
        }

        return (@interface as Type).IsAssignableFrom(type);
    }

    public static object GetInstance(string strFullyQualifiedName)
    {
        Type type = Type.GetType(strFullyQualifiedName);
        if (type != null)
        {
            return Activator.CreateInstance(type);
        }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            type = assembly.GetType(strFullyQualifiedName);
            if (type != null)
            {
                return Activator.CreateInstance(type);
            }
        }

        return null;
    }

    public static double DaysToHours(double days)
    {
        return days * 24.0;
    }

    public static double MinutesToHours(double minutes)
    {
        return minutes / 60.0;
    }

    public static double MinutesToYears(double minutes)
    {
        return minutes / 525600.0;
    }

    public static double MinutesToDays(double minutes)
    {
        return minutes / 1440.0;
    }

    public static double GetDaysToNextYear(Time getBirthDateTime)
    {
        DateTimeOffset stdDateTimeOffset = getBirthDateTime.GetStdDateTimeOffset();
        int year = stdDateTimeOffset.Year + 1;
        DateTimeOffset dateTimeOffset = new DateTimeOffset(year, 1, 1, 0, 0, 0, 0, stdDateTimeOffset.Offset);
        return (dateTimeOffset - stdDateTimeOffset).TotalDays;
    }

    public static string GetNowSystemTimeText()
    {
        return DateTimeOffset.Now.ToString("HH:mm dd/MM/yyyy zzz");
    }

    public static string GetNowSystemTimeSecondsText()
    {
        return DateTimeOffset.Now.ToString("HH:mm:ss dd/MM/yyyy zzz");
    }

    public static string GetNowServerTimeSecondsText()
    {
        return DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8.0)).ToString("HH:mm:ss dd/MM/yyyy zzz");
    }

    public static int GetStringHashCode(string stringToHash)
    {
        if (stringToHash == null)
        {
            return 0;
        }

        int num = 352654597;
        int num2 = num;
        for (int i = 0; i < stringToHash.Length; i += 2)
        {
            num = ((num << 5) + num) ^ stringToHash[i];
            if (i == stringToHash.Length - 1)
            {
                break;
            }

            num2 = ((num2 << 5) + num2) ^ stringToHash[i + 1];
        }

        return num + num2 * 1566083941;
    }

    public static string GenerateId()
    {
        return Guid.NewGuid().ToString("N");
    }

    public static string ListToString<T>(List<T> list)
    {
        string text = "";
        foreach (T item in list)
        {
            text = text + item.ToString() + ", ";
        }

        return text;
    }

    public static bool FindCluster(this string haystack, string needle)
    {
        if (haystack == null)
        {
            return false;
        }

        if (needle == null)
        {
            return false;
        }

        if (haystack.Length < needle.Length)
        {
            return false;
        }

        long num = needle.ToCharArray().Sum((char c) => c);
        long num2 = haystack.ToCharArray().Take(needle.Length).Sum((char c) => c);
        int num3 = 0;
        int num4 = needle.Length;
        while (num2 != num)
        {
            if (num4 >= haystack.Length)
            {
                return false;
            }

            num2 -= haystack[num3];
            num2 += haystack[num4];
            num4++;
            num3++;
        }

        return true;
    }

    public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        float num = from - fromMin;
        float num2 = fromMax - fromMin;
        float num3 = num / num2;
        float num4 = toMax - toMin;
        float num5 = num4 * num3;
        return num5 + toMin;
    }

    public static double Remap(this double from, double fromMin, double fromMax, double toMin, double toMax)
    {
        double num = from - fromMin;
        double num2 = fromMax - fromMin;
        double num3 = num / num2;
        double num4 = toMax - toMin;
        double num5 = num4 * num3;
        return num5 + toMin;
    }

    public static string StreamToString(Stream stream)
    {
        StreamReader streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }

    public static TimeSpan StringToTimezone(string timezoneRaw)
    {
        return DateTimeOffset.ParseExact(timezoneRaw, "zzz", CultureInfo.InvariantCulture).Offset;
    }

    public static string GetSystemTimezoneStr()
    {
        return DateTimeOffset.Now.ToString("zzz");
    }

    public static TimeSpan GetSystemTimezone()
    {
        return DateTimeOffset.Now.Offset;
    }

    public static async Task<WebResult<GeoLocation>> AddressToGeoLocation(string address)
    {
        string apiKey = "AIzaSyDqBWCqzU1BJenneravNabDUGIHotMBsgE";
        string url = $"https://maps.googleapis.com/maps/api/geocode/xml?key={apiKey}&address={Uri.EscapeDataString(address)}&sensor=false";
        WebResult<XElement> webResult = await ReadFromServerXmlReply(url);
        if (!webResult.IsPass)
        {
            return new WebResult<GeoLocation>(result: false, GeoLocation.Empty);
        }

        XElement geocodeResponseXml = webResult.Payload;
        XElement resultXml = geocodeResponseXml.Element("result");
        XElement statusXml = geocodeResponseXml.Element("status");
        if (statusXml == null || statusXml.Value == "ZERO_RESULTS")
        {
            return new WebResult<GeoLocation>(result: false, GeoLocation.Empty);
        }

        XElement locationElement = resultXml?.Element("geometry")?.Element("location");
        double lat2 = double.Parse(locationElement?.Element("lat")?.Value ?? "0");
        double lng2 = double.Parse(locationElement?.Element("lng")?.Value ?? "0");
        lat2 = Math.Round(lat2, 3);
        lng2 = Math.Round(lng2, 3);
        string fullName = resultXml?.Element("formatted_address")?.Value;
        return new WebResult<GeoLocation>(result: true, new GeoLocation(fullName, lng2, lat2));
    }

    public static async Task<GeoLocation> CoordinateToGeoLocation(double longitude, double latitude, string apiKey)
    {
        string url = string.Format($"https://maps.googleapis.com/maps/api/geocode/xml?latlng={latitude},{longitude}&key={apiKey}");
        XElement rawReplyXml = (await ReadFromServerXmlReply(url)).Payload;
        XDocument locationData = new XDocument(rawReplyXml);
        string locationName = (locationData.Element("GeocodeResponse")?.Elements("result").FirstOrDefault((XElement result) => result.Element("type")?.Value == "locality"))?.Element("formatted_address")?.Value;
        return new GeoLocation(locationName, longitude, latitude);
    }

    public static async Task<TimeSpan> GetTimezoneOffset(string locationName, DateTimeOffset timeAtLocation, string apiKey)
    {
        return StringToTimezone(await GetTimezoneOffsetApi(await GeoLocation.FromName(locationName), timeAtLocation, apiKey));
    }

    public static async Task<string> GetTimezoneOffsetString(string locationName, DateTime timeAtLocation, string apiKey)
    {
        return await GetTimezoneOffsetApi(await GeoLocation.FromName(locationName), timeAtLocation, apiKey);
    }

    public static async Task<string> GetTimezoneOffsetString(string location, string dateTime)
    {
        DateTime lifeEvtTimeNoTimezone = DateTime.ParseExact(dateTime, "HH:mm dd/MM/yyyy", null);
        return await GetTimezoneOffsetString(location, lifeEvtTimeNoTimezone, "AIzaSyDqBWCqzU1BJenneravNabDUGIHotMBsgE");
    }

    public static async Task<WebResult<string>> GetTimezoneOffsetApi(GeoLocation geoLocation, DateTimeOffset timeAtLocation, string apiKey)
    {
        WebResult<string> returnResult = new WebResult<string>();
        long locationTimeUnix = timeAtLocation.ToUnixTimeSeconds();
        double longitude = geoLocation.GetLongitude();
        double latitude = geoLocation.GetLatitude();
        string url = string.Format($"https://maps.googleapis.com/maps/api/timezone/xml?location={latitude},{longitude}&timestamp={locationTimeUnix}&key={apiKey}");
        WebResult<XElement> apiResult = await ReadFromServerXmlReply(url);
        TimeSpan offsetMinutes2;
        if (apiResult.IsPass)
        {
            XElement timeZoneResponseXml = apiResult.Payload;
            if (TryParseGoogleTimeZoneResponse(timeZoneResponseXml, out offsetMinutes2))
            {
                string parsedOffsetString = TimeSpanToUTCTimezoneString(offsetMinutes2);
                returnResult.Payload = parsedOffsetString;
                returnResult.IsPass = true;
                return returnResult;
            }
        }

        returnResult.IsPass = false;
        offsetMinutes2 = GetSystemTimezone();
        returnResult.Payload = TimeSpanToUTCTimezoneString(offsetMinutes2);
        return returnResult;
    }

    private static string TimeSpanToUTCTimezoneString(TimeSpan offsetMinutes)
    {
        return DateTimeOffset.UtcNow.ToOffset(offsetMinutes).ToString("zzz");
    }

    public static bool TryParseGoogleTimeZoneResponse(XElement timeZoneResponseXml, out TimeSpan offsetMinutes)
    {
        string text = timeZoneResponseXml?.Element("status")?.Value ?? "";
        if (!text.Contains("INVALID_REQUEST"))
        {
            string text2 = timeZoneResponseXml?.Element("raw_offset")?.Value;
            if (!string.IsNullOrEmpty(text2) && double.TryParse(text2, out var result))
            {
                double totalMinutes = TimeSpan.FromSeconds(result).TotalMinutes;
                offsetMinutes = TimeSpan.FromMinutes((int)Math.Round(totalMinutes));
                return true;
            }
        }

        //LibLogger.Error(timeZoneResponseXml);
        offsetMinutes = TimeSpan.Zero;
        return false;
    }

    public static async Task<WebResult<XElement>> ReadFromServerXmlReply(string apiUrl, string rootElementName = "Root")
    {
        WebResult<XElement> returnResult = new WebResult<XElement>();
        string rawMessage = "";
        try
        {
            rawMessage = (await RequestServerPost(apiUrl)).Content.ReadAsStringAsync().Result;
            XElement readFromServerXmlReply2 = XElement.Parse(rawMessage);
            returnResult.Payload = readFromServerXmlReply2;
            returnResult.IsPass = true;
        }
        catch (Exception)
        {
            try
            {
                XElement readFromServerXmlReply = XElement.Parse(JsonConvert.DeserializeXmlNode(rawMessage, rootElementName)?.InnerXml ?? "<Empty/>");
                returnResult.Payload = readFromServerXmlReply;
                returnResult.IsPass = true;
            }
            catch (Exception)
            {
                _ = "ReadFromServerXmlReply()\n" + rawMessage;
                returnResult.IsPass = false;
            }
        }

        return returnResult;
        static async Task<HttpResponseMessage> RequestServerPost(string receiverAddress)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, receiverAddress);
            using HttpClient client = new HttpClient
            {
                Timeout = new TimeSpan(0, 0, 0, 0, -1)
            };
            HttpCompletionOption waitForContent = HttpCompletionOption.ResponseContentRead;
            return await client.SendAsync(httpRequestMessage, waitForContent);
        }
    }

    public static string RandomSelect(string[] msgList)
    {
        Random random = new Random();
        int num = random.Next(msgList.Length);
        return msgList[num];
    }

    public static IEnumerable<string> SplitByCharCount(string str, int maxChunkSize)
    {
        for (int i = 0; i < str.Length; i += maxChunkSize)
        {
            yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
    }

    public static List<PlanetName> GetPlanetFromName(string eventName)
    {
        List<PlanetName> list = new List<PlanetName>();
        string text = eventName.ToLower();
        string[] array = text.Split(' ');
        string[] array2 = array;
        foreach (string possiblePlanetName in array2)
        {
            if (PlanetName.TryParse(possiblePlanetName, out var parsed))
            {
                list.Add(parsed);
            }
        }

        return list;
    }

    public static StringContent XmLtoHttpContent(XElement data)
    {
        string content = XmlToString(data);
        Encoding uTF = Encoding.UTF8;
        string mediaType = "plain/text";
        return new StringContent(content, uTF, mediaType);
    }

    public static XElement ExtractDataFromException(Exception e)
    {
        Exception baseException = e.GetBaseException();
        StackTrace stackTrace = new StackTrace(e, fNeedFileInfo: true);
        StackFrame frame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        string content = frame?.GetFileName();
        string content2 = frame.GetMethod()?.Name;
        int fileLineNumber = frame.GetFileLineNumber();
        int fileColumnNumber = frame.GetFileColumnNumber();
        string content3 = baseException.ToString();
        string source = baseException.Source;
        string stackTrace2 = baseException.StackTrace;
        return new XElement("Error", new XElement("Message", content3), new XElement("Source", source), new XElement("FileName", content), new XElement("SourceLineNumber", fileLineNumber), new XElement("SourceColNumber", fileColumnNumber), new XElement("MethodName", content2), new XElement("MethodName", content2));
    }

    public static string GetNow()
    {
        TimeSpan offset = new TimeSpan(8, 0, 0);
        return DateTimeOffset.Now.ToUniversalTime().ToOffset(offset).ToString("HH:mm:ss dd/MM/yyyy zzz");
    }

    public static string CleanNameText(string nameInput)
    {
        try
        {
            return Regex.Replace(nameInput, "[^\\w\\.\\s*-]", "", RegexOptions.None, TimeSpan.FromSeconds(2.0));
        }
        catch (RegexMatchTimeoutException)
        {
            return string.Empty;
        }
    }
}
#if false // Decompilation log
'197' items in cache
------------------
Resolve: 'System.Runtime, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Runtime.dll'
------------------
Resolve: 'System.Collections, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Collections.dll'
------------------
Resolve: 'SwissEphNet, Version=2.8.0.2, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'SwissEphNet, Version=2.8.0.2, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\singh\.nuget\packages\swissephnet\2.8.0.2\lib\netstandard1.0\SwissEphNet.dll'
------------------
Resolve: 'System.Linq.Expressions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq.Expressions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Linq.Expressions.dll'
------------------
Resolve: 'System.Collections.Concurrent, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Concurrent, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Collections.Concurrent.dll'
------------------
Resolve: 'Microsoft.Extensions.Caching.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.Extensions.Caching.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\singh\.nuget\packages\microsoft.extensions.caching.memory\6.0.1\lib\netstandard2.0\Microsoft.Extensions.Caching.Memory.dll'
------------------
Resolve: 'System.Threading.Thread, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Thread, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Threading.Thread.dll'
------------------
Resolve: 'Microsoft.Extensions.Caching.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.Extensions.Caching.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\singh\.nuget\packages\microsoft.extensions.caching.abstractions\6.0.0\lib\netstandard2.0\Microsoft.Extensions.Caching.Abstractions.dll'
------------------
Resolve: 'System.Runtime.Serialization.Formatters, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Serialization.Formatters, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Runtime.Serialization.Formatters.dll'
------------------
Resolve: 'Google.Apis.Calendar.v3, Version=1.51.0.2237, Culture=neutral, PublicKeyToken=4b01fa6e34db77ab'
Found single assembly: 'Google.Apis.Calendar.v3, Version=1.51.0.2237, Culture=neutral, PublicKeyToken=4b01fa6e34db77ab'
Load from: 'C:\Users\singh\.nuget\packages\google.apis.calendar.v3\1.51.0.2237\lib\netstandard2.0\Google.Apis.Calendar.v3.dll'
------------------
Resolve: 'Google.Apis.Auth, Version=1.51.0.0, Culture=neutral, PublicKeyToken=4b01fa6e34db77ab'
Found single assembly: 'Google.Apis.Auth, Version=1.51.0.0, Culture=neutral, PublicKeyToken=4b01fa6e34db77ab'
Load from: 'C:\Users\singh\.nuget\packages\google.apis.auth\1.51.0\lib\netstandard2.0\Google.Apis.Auth.dll'
------------------
Resolve: 'System.Xml.XDocument, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.XDocument, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Xml.XDocument.dll'
------------------
Resolve: 'System.Linq, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Linq.dll'
------------------
Resolve: 'System.Threading.Tasks.Parallel, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Tasks.Parallel, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Threading.Tasks.Parallel.dll'
------------------
Resolve: 'System.Net.Http, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Http, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Net.Http.dll'
------------------
Resolve: 'System.Net.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Primitives, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Net.Primitives.dll'
------------------
Resolve: 'System.Diagnostics.StackTrace, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.StackTrace, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Diagnostics.StackTrace.dll'
------------------
Resolve: 'System.Net.Requests, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Requests, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Net.Requests.dll'
------------------
Resolve: 'System.Text.RegularExpressions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Text.RegularExpressions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Text.RegularExpressions.dll'
------------------
Resolve: 'System.Xml.ReaderWriter, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.ReaderWriter, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Xml.ReaderWriter.dll'
------------------
Resolve: 'System.Text.Json, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Text.Json, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Text.Json.dll'
------------------
Resolve: 'System.Net.Mail, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Net.Mail, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Net.Mail.dll'
------------------
Resolve: 'Microsoft.CSharp, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.CSharp, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\Microsoft.CSharp.dll'
------------------
Resolve: 'Google.Apis, Version=1.51.0.0, Culture=neutral, PublicKeyToken=4b01fa6e34db77ab'
Found single assembly: 'Google.Apis, Version=1.51.0.0, Culture=neutral, PublicKeyToken=4b01fa6e34db77ab'
Load from: 'C:\Users\singh\.nuget\packages\google.apis\1.51.0\lib\netstandard2.0\Google.Apis.dll'
------------------
Resolve: 'Google.Apis.Core, Version=1.51.0.0, Culture=neutral, PublicKeyToken=4b01fa6e34db77ab'
Found single assembly: 'Google.Apis.Core, Version=1.51.0.0, Culture=neutral, PublicKeyToken=4b01fa6e34db77ab'
Load from: 'C:\Users\singh\.nuget\packages\google.apis.core\1.51.0\lib\netstandard2.0\Google.Apis.Core.dll'
------------------
Resolve: 'System.Threading, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Threading.dll'
------------------
Resolve: 'System.Console, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Console, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Console.dll'
------------------
Resolve: 'System.Web.HttpUtility, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Web.HttpUtility, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Web.HttpUtility.dll'
------------------
Resolve: 'Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed'
Found single assembly: 'Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed'
Load from: 'C:\Users\singh\.nuget\packages\newtonsoft.json\12.0.3\lib\netstandard2.0\Newtonsoft.Json.dll'
------------------
Resolve: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Runtime.dll'
------------------
Resolve: 'System.Net.WebHeaderCollection, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.WebHeaderCollection, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Net.WebHeaderCollection.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.InteropServices, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Runtime.CompilerServices.Unsafe.dll'
#endif
