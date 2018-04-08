using System;

namespace ScheduleGenerator
{
    public class WorkingDay : IDay
    {
        public DateTime Day { get; set; }
        public Employee Employee { get; set; }
        public bool IsVacation { get; set; }
    }
}