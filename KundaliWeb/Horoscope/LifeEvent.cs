#region Assembly VedAstro.Library, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\singh\.nuget\packages\vedastro.library\1.2.0\lib\net7.0\VedAstro.Library.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace VedAstro.Library;

public class LifeEvent : IToXml
{
    private string _name;

    private string _description;

    [JsonPropertyName("Name")]
    public string Name
    {
        get
        {
            return HttpUtility.HtmlDecode(_name);
        }
        set
        {
            _name = HttpUtility.HtmlEncode(value);
        }
    }

    [JsonPropertyName("Description")]
    public string Description
    {
        get
        {
            return HttpUtility.HtmlDecode(_description);
        }
        set
        {
            _description = HttpUtility.HtmlEncode(value);
        }
    }

    [JsonPropertyName("StartTime")]
    public string StartTime { get; set; }

    [JsonPropertyName("Location")]
    public string Location { get; set; }

    [JsonPropertyName("Nature")]
    public string Nature { get; set; }

    [JsonPropertyName("Timezone")]
    public string Timezone { get; set; } = "";


    public XElement ToXml()
    {
        XElement xElement = new XElement("LifeEvent");
        XElement xElement2 = new XElement("Name", Name);
        XElement xElement3 = new XElement("StartTime", StartTime);
        XElement xElement4 = new XElement("Timezone", Timezone);
        XElement xElement5 = new XElement("Location", Location);
        XElement xElement6 = new XElement("Description", Description);
        XElement xElement7 = new XElement("Nature", Nature);
        xElement.Add(xElement2, xElement3, xElement4, xElement5, xElement6, xElement7);
        return xElement;
    }

    public dynamic FromXml<T>(XElement xml) where T : IToXml
    {
        return FromXml(xml);
    }

    public static LifeEvent FromXml(XElement lifeEventXml)
    {
        LifeEvent lifeEvent = new LifeEvent();
        lifeEvent.Name = (string.IsNullOrEmpty(lifeEventXml.Element("Name")?.Value) ? "" : lifeEventXml?.Element("Name")?.Value);
        lifeEvent.StartTime = (string.IsNullOrEmpty(lifeEventXml.Element("StartTime")?.Value) ? "" : lifeEventXml?.Element("StartTime")?.Value);
        lifeEvent.Timezone = (string.IsNullOrEmpty(lifeEventXml.Element("Timezone")?.Value) ? "" : lifeEventXml?.Element("Timezone")?.Value);
        lifeEvent.Location = (string.IsNullOrEmpty(lifeEventXml.Element("Location")?.Value) ? "Singapore" : lifeEventXml?.Element("Location")?.Value);
        lifeEvent.Description = (string.IsNullOrEmpty(lifeEventXml.Element("Description")?.Value) ? "" : lifeEventXml?.Element("Description")?.Value);
        lifeEvent.Nature = (string.IsNullOrEmpty(lifeEventXml.Element("Nature")?.Value) ? "" : lifeEventXml?.Element("Nature")?.Value);
        return lifeEvent;
    }

    public override bool Equals(object value)
    {
        if (value.GetType() == typeof(LifeEvent))
        {
            LifeEvent lifeEvent = (LifeEvent)value;
            return GetHashCode() == lifeEvent.GetHashCode();
        }

        return false;
    }

    public override string ToString()
    {
        return $"{Name} - {Nature} - {StartTime} - {Location} - {Description}";
    }

    public override int GetHashCode()
    {
        int stringHashCode = Tools.GetStringHashCode(Name);
        int stringHashCode2 = Tools.GetStringHashCode(StartTime);
        int stringHashCode3 = Tools.GetStringHashCode(Timezone);
        int stringHashCode4 = Tools.GetStringHashCode(Location);
        int stringHashCode5 = Tools.GetStringHashCode(Description);
        int stringHashCode6 = Tools.GetStringHashCode(Nature);
        return Math.Abs(stringHashCode + stringHashCode2 + stringHashCode3 + stringHashCode4 + stringHashCode5 + stringHashCode6);
    }

    public async Task<DateTimeOffset> GetDateTimeOffsetAsync()
    {
        string timezone = ((!string.IsNullOrEmpty(Timezone)) ? Timezone : (await Tools.GetTimezoneOffsetString(Location, StartTime)));
        Timezone = timezone;
        string lifeEvtTimeStr = StartTime + " " + Timezone;
        return DateTimeOffset.ParseExact(lifeEvtTimeStr, "HH:mm dd/MM/yyyy zzz", null);
    }

    public DateTimeOffset GetDateTimeOffset()
    {
        if (string.IsNullOrEmpty(Timezone))
        {
            throw new Exception("Timezone data for event \"" + Name + "\" missing!");
        }

        Timezone = Timezone;
        string input = StartTime + " " + Timezone;
        return DateTimeOffset.ParseExact(input, "HH:mm dd/MM/yyyy zzz", null);
    }

    public DateTimeOffset GetDateTimeOffsetLocal()
    {
        Timezone = (string.IsNullOrEmpty(Timezone) ? "+00:00" : Timezone);
        string input = StartTime + " " + Timezone;
        return DateTimeOffset.ParseExact(input, "HH:mm dd/MM/yyyy zzz", null);
    }

    public async Task<Time> GetTime()
    {
        return new Time(await GetDateTimeOffsetAsync(), await GetGeoLocation());
    }

    public async Task<GeoLocation> GetGeoLocation()
    {
        return await GeoLocation.FromName(Location);
    }

    public int CompareTo(LifeEvent lifeEvent)
    {
        DateTimeOffset dateTimeOffsetLocal = lifeEvent.GetDateTimeOffsetLocal();
        return GetDateTimeOffsetLocal().CompareTo(dateTimeOffsetLocal);
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
