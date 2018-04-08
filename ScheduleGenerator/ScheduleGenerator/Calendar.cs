using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleGenerator
{
    public class Calendar<T>
    where T : IDay
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public IEnumerable<T> Days { get; set; }
    }
}
