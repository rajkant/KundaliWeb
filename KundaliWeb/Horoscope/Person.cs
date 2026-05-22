#region Assembly VedAstro.Library, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\singh\.nuget\packages\vedastro.library\1.2.0\lib\net7.0\VedAstro.Library.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace VedAstro.Library;


public struct Person : IToXml
{
    private static string[] DefaultUserId = new string[1] { "101" };

    public static Person Empty = new Person("0", "Empty", Time.Now(GeoLocation.Empty), Gender.Empty, DefaultUserId, "Empty", new List<LifeEvent>());

    private string _notes;

    public string Id { get; set; }

    public string Name { get; set; }

    public string[] UserId { get; set; }

    public string UserIdString => string.Join(",", UserId);

    public string Notes
    {
        get
        {
            return HttpUtility.HtmlDecode(_notes);
        }
        set
        {
            _notes = HttpUtility.HtmlEncode(value);
        }
    }

    public Gender Gender { get; set; }

    public Time BirthTime { get; set; }

    public List<LifeEvent> LifeEventList { get; set; }

    public int BirthYear => BirthTime.GetStdDateTimeOffset().Year;

    public string BirthTimeZone => BirthTime.GetStdDateTimeOffset().ToString("zzz");

    public string BirthHourMinute => BirthTime.GetStdDateTimeOffset().ToString("HH:mm");

    public string BirthDateMonthYear => BirthTime.GetStdDateTimeOffset().ToString("dd/MM/yyyy");

    public string GenderString => Gender.ToString();

    public int Hash => GetHashCode();

    public string BirthTimeString => BirthTime.GetStdDateTimeOffsetText();

    public DateTimeOffset StdTimeNowAtBirthLocation => DateTimeOffset.Now.ToOffset(BirthTime.GetStdDateTimeOffset().Offset);

    public Person(string id, string name, Time birthTime, Gender gender, string[] userId, string notes = "", List<LifeEvent> lifeEventList = null)
    {
        _notes = null;
        LifeEventList = new List<LifeEvent>();
        Id = id;
        Name = name;
        BirthTime = birthTime;
        Gender = gender;
        UserId = (userId.Any() ? userId : DefaultUserId);
        Notes = notes;
        LifeEventList = lifeEventList ?? new List<LifeEvent>();
    }

    public GeoLocation GetBirthLocation()
    {
        return BirthTime.GetGeoLocation();
    }

    public int GetAge(Time time)
    {
        return time.GetStdDateTimeOffset().Year - BirthYear;
    }

    public int GetAge(int year)
    {
        return year - BirthYear;
    }

    public override bool Equals(object value)
    {
        if (value.GetType() == typeof(Person))
        {
            Person person = (Person)value;
            return GetHashCode() == person.GetHashCode();
        }

        return false;
    }

    public override string ToString()
    {
        return Name ?? "";
    }

    public override int GetHashCode()
    {
        int stringHashCode = Tools.GetStringHashCode(Name);
        int hashCode = BirthTime.GetHashCode();
        int stringHashCode2 = Tools.GetStringHashCode(UserIdString);
        return Math.Abs(stringHashCode + hashCode + stringHashCode2);
    }

    public XElement ToXml()
    {
        XElement xElement = new XElement("Person");
        XElement xElement2 = new XElement("Name", Name);
        XElement xElement3 = new XElement("PersonId", Id);
        XElement xElement4 = new XElement("Notes", Notes);
        XElement xElement5 = new XElement("Gender", Gender.ToString());
        XElement xElement6 = new XElement("BirthTime", BirthTime.ToXml());
        XElement xElement7 = new XElement("UserId", UserIdString);
        XElement xElement8 = getLifeEventListXml(LifeEventList);
        xElement.Add(xElement3, xElement2, xElement5, xElement6, xElement7, xElement8, xElement4);
        return xElement;
        static XElement getLifeEventListXml(List<LifeEvent> lifeList)
        {
            XElement xElement9 = new XElement("LifeEventList");
            if (lifeList == null)
            {
                return xElement9;
            }

            foreach (LifeEvent life in lifeList)
            {
                XElement content = life.ToXml();
                xElement9.Add(content);
            }

            return xElement9;
        }
    }

    public dynamic FromXml<T>(XElement xml) where T : IToXml
    {
        return FromXml(xml);
    }

    public static Person FromXml(XElement personXml)
    {
        string name = personXml.Element("Name")?.Value;
        string id = personXml.Element("PersonId")?.Value ?? Tools.GenerateId();
        string notes = personXml.Element("Notes")?.Value;
        Time birthTime = Time.FromXml(personXml.Element("BirthTime")?.Element("Time"));
        Gender gender = Enum.Parse<Gender>(personXml.Element("Gender")?.Value);
        string text = personXml.Element("UserId")?.Value ?? "";
        text = text.Replace("\n", "");
        text = text.Replace(" ", "");
        string[] userId = text.Split(',');
        List<LifeEvent> lifeEventList = getLifeEventListFromXml();
        return new Person(id, name, birthTime, gender, userId, notes, lifeEventList);
        List<LifeEvent> getLifeEventListFromXml()
        {
            try
            {
                IEnumerable<XElement> enumerable = personXml.Element("LifeEventList")?.Elements();
                List<LifeEvent> list = new List<LifeEvent>();
                if (enumerable != null)
                {
                    foreach (XElement item in enumerable)
                    {
                        list.Add(LifeEvent.FromXml(item));
                    }
                }

                return list;
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
                Console.WriteLine("No Valid Life Events Found! Empty list used!");
                return new List<LifeEvent>();
            }
        }
    }

    public static List<Person> FromXml(IEnumerable<XElement> personXmlList)
    {
        return personXmlList.Select((XElement personXml) => FromXml(personXml)).ToList();
    }

    public Person SetNewLifeEvents(List<LifeEvent> updatedLifeEventList)
    {
        return new Person(Id, Name, BirthTime, Gender, UserId, Notes, updatedLifeEventList);
    }

    public Person ChangeBirthTime(Time newBirthTime)
    {
        return new Person(Id, Name, newBirthTime, Gender, UserId, Notes, LifeEventList);
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
