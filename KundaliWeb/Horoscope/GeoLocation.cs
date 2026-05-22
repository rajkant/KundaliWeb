using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VedAstro.Library;

public interface IToXml
{
    XElement ToXml();
    dynamic FromXml<T>(XElement personXml) where T : IToXml;
}

[Serializable]
public readonly struct GeoLocation : IToXml
{
    public static GeoLocation Empty = new GeoLocation("Empty", 101.0, 4.59);

    private readonly string _name;

    private readonly double _longitude;

    private readonly double _latitude;

    public GeoLocation(string name, double longitude, double latitude)
    {
        _name = name;
        _longitude = longitude;
        _latitude = latitude;
    }

    public string GetName()
    {
        return _name;
    }

    public double GetLongitude()
    {
        return _longitude;
    }

    public double GetLatitude()
    {
        return _latitude;
    }

    public override bool Equals(object value)
    {
        if (value.GetType() == typeof(GeoLocation))
        {
            GeoLocation geoLocation = (GeoLocation)value;
            return GetHashCode() == geoLocation.GetHashCode();
        }

        return false;
    }

    public override string ToString()
    {
        return $"{_name} {_longitude} {_latitude}";
    }

    public override int GetHashCode()
    {
        int stringHashCode = Tools.GetStringHashCode(_name);
        int hashCode = _longitude.GetHashCode();
        int hashCode2 = _latitude.GetHashCode();
        return stringHashCode + hashCode + hashCode2;
    }

    public XElement ToXml()
    {
        XElement xElement = new XElement("Location");
        XElement xElement2 = new XElement("Name", GetName());
        XElement xElement3 = new XElement("Longitude", GetLongitude());
        XElement xElement4 = new XElement("Latitude", GetLatitude());
        xElement.Add(xElement2, xElement3, xElement4);
        return xElement;
    }

    public dynamic FromXml<T>(XElement xml) where T : IToXml
    {
        return FromXml(xml);
    }

    public static GeoLocation FromXml(XElement locationXml)
    {
        try
        {
            string name = locationXml.Element("Name")?.Value;
            double longitude = double.Parse(locationXml.Element("Longitude")?.Value ?? "-1");
            double latitude = double.Parse(locationXml.Element("Latitude")?.Value ?? "-1");
            return new GeoLocation(name, longitude, latitude);
        }
        catch (Exception exception)
        {
            //LibLogger.Error(exception, $"GeoLocation.FromXml FAIL! : {locationXml}");
            return Empty;
        }
    }

    public static async Task<GeoLocation> FromName(string locationName)
    {
        WebResult<GeoLocation> results = await Tools.AddressToGeoLocation(locationName);
        if (!results.IsPass)
        {
            throw new Exception("Location failed to find!");
        }

        return results.Payload;
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
