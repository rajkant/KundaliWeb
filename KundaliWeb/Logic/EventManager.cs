
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace VedAstro.Library
//{
//    /// <summary>
//    /// All logic dealing with creating events is here
//    /// NOTE: Parallel already built in
//    /// </summary>
//    public static class EventManager
//    {
//        /** FIELDS **/

//        /// <summary>
//        /// Placed here to reduce overhead when being accessed by different methods in this class
//        /// </summary>
//        private static List<Event> EventList { get; set; } = new List<Event>();

//        //we use direct storage URL for fast access & solid
//        private const string AzureStorage = "vedastrowebsitestorage.z5.web.core.windows.net";



//        /** PUBLIC METHODS **/

//        /// <summary>
//        /// Get list of events occuring in a time period for all the
//        /// inputed event types aka "event data"
//        /// Note : Cancellation token caught here
//        /// </summary>
//        public static List<Event> GetEventsInTimePeriod(DateTimeOffset startStdTime, DateTimeOffset endStdTime, GeoLocation geoLocation, Person person, double precisionInHours, List<EventData> eventDataList)
//        {

//            //get data to instantiate muhurtha time period
//            //get start & end times
//            var startTime = new Time(startStdTime, geoLocation);
//            var endTime = new Time(endStdTime, geoLocation);

//            //place to store all generated events
//            List<Event> eventList = new();

//            //split time into slices based on precision
//            var timeList = Time.GetTimeListFromRange(startTime, endTime, precisionInHours);

//            //return calculated event list
//            return eventList;

//        }

//        //takes time list and checks if event is occuring for each time slice,
//        //and returns it in dictionary list, done in parallel
//        public static EventSlice[] ConvertToEventSlice(List<Time> timeList, EventData eventData, Person person, bool useParallel = true)
//        {
//            EventSlice[] returnList = new EventSlice[timeList.Count];

//            if (useParallel)
//            {
//                Parallel.For(0, timeList.Count, i =>
//                {
//                    //event data list is recreated because the data
//                    //inside is possibly changed when calculators are run (dynamic yeah!)
//                    //var timeTemp = timeList[i];

//                    //get if event occurring at the inputed time (heavy computation)
//                    var updatedEventData = ConvertToEventSlice(timeList, eventData, person);

//                    //note: fills null if not occuring
//                    returnList[i] = updatedEventData[i];
//                });
//            }
//            else
//            {
//                for (int i = 0; i < timeList.Count; i++)
//                {
//                    //var timeTemp = timeList[i];
//                    var updatedEventData = ConvertToEventSlice(timeList, eventData, person);
//                    returnList[i] = updatedEventData[i];
//                }
//            }

//            return returnList;
//        }

//        public static HoroscopeCalculatorDelegate GetHoroscopeCalculatorMethod(HoroscopeName inputEventName)
//        {
//            //get all event calculator methods
//            var horoscopeCalculatorList = typeof(HoroscopeName).GetMethods();

//            //loop through all calculators
//            foreach (var horoscopeCalculator in horoscopeCalculatorList)
//            {
//                //try to get attribute attached to the calculator method
//                var horoscopeCalculatorAttribute = (HoroscopeCalculatorAttribute)Attribute.GetCustomAttribute(horoscopeCalculator,
//                    typeof(HoroscopeCalculatorAttribute));

//                //if attribute not found
//                if (horoscopeCalculatorAttribute == null)
//                {   //go to next method
//                    continue;
//                }

//                //if attribute name matches input event name
//                if (horoscopeCalculatorAttribute.HoroscopeName == inputEventName)
//                {
//                    //convert calculator reference to a delegate instance
//                    var horoscopeCalculatorDelegate = (HoroscopeCalculatorDelegate)Delegate.CreateDelegate(typeof(HoroscopeCalculatorDelegate), horoscopeCalculator);

//                    //return calculator delegate
//                    return horoscopeCalculatorDelegate;
//                }
//            }


//            //if control reaches here than failure
//            //if calculator method not found, raise error
//            throw new Exception($"Calculator method not found! : {inputEventName.ToString()}");

//        }

//        /// <summary>
//        /// Splits events that span across 2 days or more
//        /// </summary>
//        public static List<Event> SplitEventsByDay(List<Event> events)
//        {
//            //clone the list
//            var returnList = new List<Event>(events);

            
//            //remove 0 minutes events
//            //Note:during splitting 0 minute events are sometimes created it is removed here
//            var filteredEvents = EventManager.FilterOutShortEvents(returnList, 0);


//            //return list to caller 
//            return filteredEvents;


//            /// checks if an event spans more than 1 day
//            /// event starts on a day & ends on another day return true
//            /// Note : Event can be less then 24 hours and still span more than 1 day
//            bool IsEventSpanMoreThan1Day(Event _event)
//            {
//                //gets the start date number of the month for the event
//                var startDay = _event.GetStartTime().GetStdDateTimeOffset().Day;

//                //gets the end date number of the month for the event
//                var endDay = _event.GetEndTime().GetStdDateTimeOffset().Day;

//                //if the date number does not match, return event spans more (true)
//                if (startDay != endDay)
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            }
//        }

//        /// <summary>
//        /// Removes events that are shorter or equal minimum minutes specified
//        /// </summary>
//        public static List<Event> FilterOutShortEvents(List<Event> splittedEvents, int minimumMinutes)
//        {
//            //make a hard copy of the event list
//            var returnList = new List<Event>(splittedEvents);

//            //find events in list that are too short
//            var foundList = from x in splittedEvents where x.GetDurationMinutes() <= minimumMinutes select x;

//            //remove found events from return list
//            foreach (var _event in splittedEvents)
//            {
//                //if this is the event to be deleted
//                if (foundList.Contains(_event))
//                {
//                    //remove it
//                    returnList.Remove(_event);
//                }

//            }

//            //return the cleaned list to caller
//            return returnList;

//        }

//        /// <summary>
//        /// takes an array of booleans and returns a list of tuples,
//        /// where each tuple represents the start and end indices of a range of true values
//        /// Note: written by AI
//        /// </summary>
//        public static List<(int Start, int End)> GetTrueRanges(bool[] array)
//        {
//            var ranges = new List<(int Start, int End)>();
//            for (int i = 0; i < array.Length; i++)
//            {
//                if (array[i])
//                {
//                    int start = i;
//                    while (i < array.Length && array[i])
//                    {
//                        i++;
//                    }
//                    int end = i - 1;
//                    ranges.Add((start, end));
//                }
//            }
//            return ranges;
//        }

//        internal static List<Event> CalculateEvents(int precisionInHours, Time startTime, Time endTime, Person johnDoe, List<EventTag> tagList)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
