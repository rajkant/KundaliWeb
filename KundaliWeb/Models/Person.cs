using System.ComponentModel.DataAnnotations;

namespace KundaliWeb.Models
{
    public class TimelineEvent
    {
        public int Year { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    public class Persons
    {
        public string PersonId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public DateTime BirthDateTime { get; set; }
        public string Details { get; set; }
        public string lat { get; set; }
        public string longt { get; set; }

        public string AscendantE { get; set; }
        public string AscendantW { get; set; }

        public List<Positions> positions { get; set; }

        public List<HouseAlign> align { get; set; }
        
        public List<TimelineEvent> Timeline { get; set; } = new List<TimelineEvent>();
    }
}

