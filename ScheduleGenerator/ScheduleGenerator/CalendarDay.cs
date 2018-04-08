using System;

namespace ScheduleGenerator
{
    public class CalendarDay : IDay
    {
        public Employee Employee { get; set; }
        public DateTime Day { get; set; }
    }
}