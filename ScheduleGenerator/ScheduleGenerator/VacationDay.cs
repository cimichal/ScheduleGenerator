﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleGenerator
{
    public class VacationDay : IDay
    {
        public DateTime Day { get; set; }
        public List<Employee> Employees { get; set; }
    }
}
