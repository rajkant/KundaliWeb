//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Web;
//using System.Xml.Linq;
//using Newtonsoft.Json.Linq;

//namespace VedAstro.Library
//{

//    /// <summary>
//    /// Data structure to encapsulate an event before it's calculated
//    /// In other words an object instance of the event data as stored in file
//    /// </summary>
//    public struct EventData : IToJson
//    {
//        /** FIELDS **/

//        public static EventData Empty = new();

//        private string _description = "";


//        /** CTOR **/
//        public EventData(EventName name, EventNature nature, SpecializedSummary specializedNature, string description, List<EventTag> eventTags, EventCalculatorDelegate eventCalculator)
//        {
//            Name = name;
//            Nature = nature;
//            SpecializedSummary = specializedNature;
//            EventCalculator = eventCalculator;
//            Description = description;
//            EventTags = eventTags;
//        }


//        /** PROPERTIES **/
//        //mainly created for access from WPF binding
//        public EventName Name { get; private set; } = EventName.Empty;

//        /// <summary>
//        /// Gets human readable Event Name, removes camel case
//        /// </summary>
//        public string FormattedName => Format.FormatName(this.Name);

//        public EventNature Nature { get; private set; }

//        public SpecializedSummary SpecializedSummary { get; private set; }

//        public EventCalculatorDelegate EventCalculator { get; private set; }

//        /// <summary>
//        /// Auto encoding to HTML safe on get & set
//        /// </summary>
//        public string Description
//        {
//            get => HttpUtility.HtmlDecode(_description);
//            set => _description = HttpUtility.HtmlEncode(value);
//        }

//        /// <summary>
//        /// Contains data about planets, houses, and signs related to a calculation
//        /// Note: filled when IsEventOccuring is called
//        /// </summary>
//        public RelatedBody RelatedBody { get; set; } = new RelatedBody(); //default empty

//        public List<EventTag> EventTags { get; }

//        /// <summary>
//        /// Handles nulls nicely
//        /// </summary>
//        public static List<EventTag> GetEventTags(string rawTags)
//        {
//            if (string.IsNullOrEmpty(rawTags))
//            {
//                return new List<EventTag>();
//            }
//            // Split the string by comma and parse each tag
//            var returnTags = rawTags.Split(',')
//                                    .Select(rawTag =>
//                                    {
//                                        if (!Enum.TryParse(rawTag, out EventTag eventTag))
//                                        {
//                                            throw new Exception($"Event tag '{rawTag}' not found!");
//                                        }
//                                        return eventTag;
//                                    })
//                                    .ToList();
//            return returnTags;
//        }

//        private static string CleanText(string text)
//        {
//            // Remove all special characters and decode HTML
//            var cleaned = Regex.Replace(text, @"\s+", " ");
//            cleaned = HttpUtility.HtmlDecode(cleaned);
//            return cleaned;
//        }

//        /// <summary>
//        /// Searches all text in prediction for input
//        /// </summary>
//        public bool Contains(string searchText)
//        {
//            //place all text together
//            var compiledText = $"{FormattedName} {Description} {Nature} {string.Join(",", EventTags)}";

//            //do the searching
//            string pattern = @"\b" + Regex.Escape(searchText) + @"\b"; //searches only words
//            var searchResult = Regex.Match(compiledText, pattern, RegexOptions.IgnoreCase).Success;
//            return searchResult;

//        }



//        /** METHOD OVERRIDES **/
//        public override bool Equals(object value)
//        {

//            if (value.GetType() == typeof(EventData))
//            {
//                //cast to type
//                var parsedValue = (EventData)value;

//                //check equality
//                bool returnValue = (this.GetHashCode() == parsedValue.GetHashCode());

//                return returnValue;
//            }
//            else
//            {
//                //Return false if value is null
//                return false;
//            }


//        }

//        public override int GetHashCode()
//        {
//            //get hash of all the fields & combine them
//            var hash1 = Name.GetHashCode();
//            var hash2 = Nature.GetHashCode();
//            var hash3 = Tools.GetStringHashCode(Description);

//            return hash1 + hash2 + hash3;
//        }

//        public override string ToString()
//        {
//            return $"{Name} - {Nature} - {Description}";
//        }

//        public static bool operator ==(EventData left, EventData right)
//        {
//            return left.Equals(right);
//        }

//        public static bool operator !=(EventData left, EventData right)
//        {
//            return !(left == right);
//        }
              

//        JObject IToJson.ToJson() => (JObject)this.ToJson();

//        public JObject ToJson()
//        {
//            // Check if eventData is null
//            if (this == null) { return new JObject(); }

//            // Create a new JObject
//            var json = new JObject();

//            // Convert EventName and EventNature to string and add to JObject
//            json["Name"] = this.Name.ToString();
//            json["Nature"] = this.Nature.ToString();

//            // Convert SpecializedSummary to JObject and add to JObject
//            json["SpecializedSummary"] = this.SpecializedSummary.ToJson();

//            // Add the description text to JObject
//            json["Description"] = this.Description;

//            // Convert the list of tags to a comma-separated string and add to JObject
//            var tagString = string.Join(",", this.EventTags);
//            json["Tag"] = tagString;

//            // Convert the calculator method to string (if possible) and add to JObject
//            // Note: This assumes that the calculator method can be represented as a string. 
//            // If it can't, you might need to remove this line or handle it differently.
//            json["CalculatorMethod"] = this.EventCalculator.Method.Name;

//            // Return the JObject
//            return json;
//        }

//        /// <summary>
//        /// Given a parsed list of EventData will convert to JSON
//        /// </summary>
//        public static JArray ListToJson(List<EventData> eventDataList)
//        {
//            var returnValue = new JArray();
//            foreach (var eventData in eventDataList)
//            {
//                returnValue.Add(eventData.ToJson());
//            }

//            return returnValue;
//        }

//    }
//}