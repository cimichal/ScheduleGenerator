using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleGenerator
{
    class Program
    {
        private static void Main(string[] args)
        {
            var maxNumberOfPersonOnVacationPerDay = 2;
            var maxNumberOfWorkingDay = 3;
            var avaliableHolidayDays = 2;
            var numberOfEmployeeDutiesPerWeek = 3;

            var holidays = new List<DateTime>
            {
                new DateTime(2018, 4, 2),
                new DateTime(2018, 4, 19),
                new DateTime(2018, 4, 9),
                new DateTime(2018, 4, 8)
            };

            var employees = new List<Employee>
            {
                new Employee {Id = 1, Name = "Heniu"},
                new Employee {Id = 2, Name = "Krzysiu"},
                new Employee {Id = 3, Name = "Heniu1"},
                new Employee {Id = 4, Name = "Krzysiu1"},
                new Employee {Id = 5, Name = "Heniu2"},
                new Employee {Id = 6, Name = "Krzysiu2"}
            };

            var scheduleGenerator = new ScheduleGenerator(maxNumberOfPersonOnVacationPerDay, maxNumberOfWorkingDay, holidays, employees, avaliableHolidayDays, numberOfEmployeeDutiesPerWeek);

            var vacationCalendar = scheduleGenerator.MonthlyVacationGenerator(4, 2018);

            var workingCalendar = scheduleGenerator.CalendarGeneratorForEmployees(vacationCalendar);

            DisplaySchedule(workingCalendar);

            Console.WriteLine("Add new vacation for employee.");
            string addNewEmployeeVacationRaw = Console.ReadLine();
            bool addNewEmployeeVacation = false;
            Boolean.TryParse(addNewEmployeeVacationRaw, out addNewEmployeeVacation);

            if (addNewEmployeeVacation)
            {
                Console.Write("Employee id: ");

                var employeeId = Console.ReadLine();
                DateTime employeeVacation;
                Console.WriteLine("Enter date of vacation in format MM/DD/YYYY: ");
                DateTime.TryParse(Console.ReadLine(), out employeeVacation);

                Console.WriteLine($"Employee id:{employeeId} |New vacation day {employeeVacation}");
                var employee = employees.FirstOrDefault(x => x.Id.Equals(Convert.ToInt16(employeeId)));

                scheduleGenerator.UpdateWokringCalendar(vacationCalendar.Item3, workingCalendar.Item3, employee, employeeVacation);
                Console.WriteLine();

                DisplaySchedule(workingCalendar);
            }

            Console.WriteLine("Break app.");
            Console.ReadLine();
        }

        private static void DisplaySchedule(Tuple<int, int, Calendar<WorkingDay>> workingCalendar)
        {
            if (workingCalendar == null) throw new ArgumentNullException(nameof(workingCalendar));

            var employeeWorkingDay = from c in workingCalendar.Item3.Days
                                     group c.Day by c.Employee
                into e
                                     where e.Key != null
                                     select new { Employee = new { e.Key.Id, e.Key.Name }, Days = e.ToList() };

            Console.WriteLine($"{workingCalendar.Item1}/{workingCalendar.Item2}");

            foreach (var employee in employeeWorkingDay)
            {
                Console.WriteLine($"Employee id: {employee.Employee.Id} {employee.Employee.Name} ");
                employee.Days.ForEach(x => Console.WriteLine($"{x.Day} | {x.DayOfWeek}"));

                Console.WriteLine();
            }

        }
    }
}
