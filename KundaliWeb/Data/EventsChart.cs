//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;

//namespace VedAstro.Library
//{
//    /// <summary>
//    /// Represents an events report chart
//    /// Note: made so that a chart can be saved and accessed later
//    /// </summary>
//    public class EventsChart
//    {

//        public static EventsChart Empty = new EventsChart("Empty", "Empty", Person.Empty, TimeRange.Empty, 0, new List<EventTag>(), ChartOptions.Empty);

//        public string ChartId { get; set; }
//        public string ContentSvg { get; set; }
//        public Person Person { get; }
//        public double DaysPerPixel { get; }
//        public List<EventTag> EventTagList { get; set; }
//        public TimeRange TimeRange { get; set; }

//        public ChartOptions Options { get; set; }

//        /// <summary>
//        /// CTOR
//        /// </summary>
//        public EventsChart(string chartId, string contentSvg, Person person, TimeRange timeRange, double daysPerPixel, List<EventTag> eventTagList, ChartOptions options)
//        {
//            ChartId = chartId;
//            ContentSvg = contentSvg;
//            Person = person;
//            EventTagList = eventTagList;
//            TimeRange = timeRange;
//            DaysPerPixel = daysPerPixel;
//            Options = options;
//        }



//        /// <summary>
//        /// Gets a nice identifiable name for this chart to show user 
//        /// </summary>
//        public object GetFormattedName(string personName)
//        {
//            var startYear = TimeRange.start.StdYear();
//            var startMonth = TimeRange.start.StdMonth();
//            var endYear = TimeRange.end.StdYear();
//            var endMonth = TimeRange.end.StdMonth();
//            return $"{personName} - {startMonth}/{startYear} to {endMonth}/{endYear}";
//        }

//        /// <summary>
//        /// create a unique signature to identify all future calls that is exactly alike
//        /// name of data, designed for human eyes
//        /// </summary>
//        public string GetEventsChartSignature()
//        {
//            //use ticks because can revert back from there
//            var endTime = TimeRange.end.StdDateMonthYearText;
//            var startTime = TimeRange.start.StdDateMonthYearText;

//            //include all data into the name of the cache,
//            //so easy to ID en masse and genocide them when needed
//            //NOTE: person ID must be first, to be detected by "cache cleaner" when person is updated
//            var dataSignature = $"{this.Person.Id}-" +
//                                $"{nameof(EventsChart)}-" +
//                                $"{startTime}-{endTime}-" +
//                                $"{this.DaysPerPixel}-" +
//                                $"{(Ayanamsa)Calculate.Ayanamsa}-" +
//                                $"{Tools.ListToString(EventTagList, "-")}-" +
//                                $"{Options}";

//            //remove white space that might have gotten in
//            dataSignature = dataSignature.Replace(" ", "");

//            //clean name of invalid characters
//            var cleaned = new string(dataSignature
//                .Where(ch => !Path.GetInvalidFileNameChars().Contains(ch))
//                .ToArray());

//            return cleaned;
//        }

//        public static async Task<EventsChart> FromCacheName(string chartName)
//        {
//            //AngelinaJolie1975-EventsChart-04061975-16052050-29.122-RAMAN-PD1-PD2-PD3-PD4-PD5-PD6-PD7-General-StrongestPlanet-StrongestHouse-IshtaKashtaPhala

//            //var parsed = new EventsChart("","",)
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// From user inputed data make specs for event 
//        /// </summary>
//        public static EventsChart FromData(Person inputPerson, TimeRange timeRange, List<EventTag> inputedEventTags, double daysPerPixelRaw, ChartOptions options)
//        {

//            //if not defined, use input
//            double daysPerPixelInput = 30;
//            daysPerPixelInput = daysPerPixelRaw != 0 ? daysPerPixelRaw : daysPerPixelInput;

//            //a new chart is born
//            var newChartId = Tools.GenerateId();
//            var newChart = new EventsChart(newChartId, "", inputPerson, timeRange, daysPerPixelInput, inputedEventTags, options);

//            return newChart;
//        }

//        /// <summary>
//        /// Converts an instance to URL format for easy transport to API server, only specs here
//        /// EXP : 
//        /// </summary>
//        public string ToUrl()
//        {
//            var final = "";

//            final += $"/{this.Person.Id}";
//            var start = this.TimeRange.start;
//            final += $"/Start/{start.StdHourMinuteText}/{start.StdDateMonthYearText}"; // 00:00/01/01/2011
//            var end = this.TimeRange.end;
//            final += $"/End/{end.StdHourMinuteText}/{end.StdDateMonthYearText}/{end.StdTimezoneText}"; // 00:00/01/01/2011/+08:00
//            final += $"/{this.DaysPerPixel}";
//            final += $"/{string.Join(",", this.EventTagList)}"; // PD1,PD2,PD3,PD4,PD5
//            final += $"/{string.Join(",", this.Options.SelectedAlgorithm.Select(func => func.Method.Name))}"; // GetGeneralScore,GocharaAshtakvargaBindu

//            return final;
//        }

//        /// <summary>
//        /// Packages the data to send to API to generate the chart
//        /// </summary>
//        public JObject ToJson()
//        {
//            var returnPayload = new JObject();

//            returnPayload["PersonId"] = this.Person.Id;
//            returnPayload["StartTime"] = this.TimeRange.start.ToJson();
//            returnPayload["EndTime"] = this.TimeRange.end.ToJson();
//            returnPayload["DaysPerPixel"] = this.DaysPerPixel;
//            returnPayload["EventTagList"] = EventTagExtensions.ToJsonList(this.EventTagList);
//            returnPayload["ChartOptions"] = this.Options.ToJson();

//            return returnPayload;
//        }

//        /// <summary>
//        /// calculates the precision of the events to fit inside width
//        /// </summary>
//        public static double GetDayPerPixel(TimeRange timeRange, int maxWidth)
//        {
//            var daysPerPixel = Math.Round(timeRange.DaysBetween / maxWidth, 3); //small val = higher precision

//            return daysPerPixel;
//        }

//        /// <summary>
//        /// Use CHART ID instead if possible
//        /// Gets HASH of chart ID, reliable
//        /// </summary>
//        public override int GetHashCode() => Tools.GetStringHashCode(ChartId);

//    }
//}
