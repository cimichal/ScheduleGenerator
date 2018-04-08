using System;
using System.Collections.Generic;
using System.Linq;

namespace ScheduleGenerator
{
    public class ScheduleGenerator
    {
        public ScheduleGenerator(int maxNumberOfPersonOnVacationPerDay, int maxNumberOfWorkingDay,
            List<DateTime> holidays,
            List<Employee> employess, int avaliableHolidayDays, int numberOfEmployeeDutiesPerWeek)
        {
            MaxNumberOfPersonOnVacationPerDay = maxNumberOfPersonOnVacationPerDay;
            MaxNumberOfWorkingDay = maxNumberOfWorkingDay;
            Holidays = holidays;
            Employess = employess;
            AvaliableHolidayDays = avaliableHolidayDays;
            NumberOfEmployeeDutiesPerWeek = numberOfEmployeeDutiesPerWeek;
        }

        public int MaxNumberOfPersonOnVacationPerDay { get; }
        public int MaxNumberOfWorkingDay { get; }
        public int AvaliableHolidayDays { get; }
        public List<DateTime> Holidays { get; }
        public List<Employee> Employess { get; }
        public int NumberOfEmployeeDutiesPerWeek { get; }

        public Tuple<int, int, Calendar<VacationDay>> MonthlyVacationGenerator(int month, int year)
        {
            var calendar = new Calendar<VacationDay>
            {
                Month = month,
                Year = year,
                Days = GetRandomDays(month, year, Employess)
            };

            var vacationCalendar = new Tuple<int, int, Calendar<VacationDay>>(month, year, calendar);

            return vacationCalendar;
        }

        private IEnumerable<VacationDay> GetRandomDays(int month, int year, List<Employee> employees)
        {
            if (employees == null) throw new ArgumentNullException(nameof(employees));

            var avaliableHolidayDays = AvaliableHolidayDays;
            var numberOfDaysInMonth = DateTime.DaysInMonth(year, month);
            var generatedVacationDays = new List<VacationDay>();

            foreach (var employee in employees)
            {
                var employeeHolidayDays = new List<DateTime>();

                while (avaliableHolidayDays > 0)
                {
                    var rnd = new Random();
                    var rndDay = rnd.Next(1, numberOfDaysInMonth);

                    var randomDay = new DateTime(year, month, rndDay);

                    // Condition when employee can't go to work 
                    if ((randomDay.DayOfWeek != DayOfWeek.Sunday) && (randomDay.DayOfWeek != DayOfWeek.Saturday) &&
                        !Holidays.Exists(x => x == randomDay) && !employeeHolidayDays.Exists(x => x == randomDay))
                    {
                        employeeHolidayDays.Add(randomDay);

                        // Employee cannot be twice on the single day at work. 
                        if (generatedVacationDays.Exists(x => x.Day == randomDay) &&
                            (generatedVacationDays.Where(x => x.Day == randomDay).FirstOrDefault().Employees.Count <
                             MaxNumberOfPersonOnVacationPerDay))
                        {
                            generatedVacationDays.Where(x => x.Day == randomDay)
                                .FirstOrDefault()
                                .Employees.Add(employee);
                            avaliableHolidayDays--;
                        }
                        else if (!generatedVacationDays.Exists(x => x.Day == randomDay))
                        {
                            generatedVacationDays.Add(new VacationDay
                            {
                                Day = randomDay,
                                Employees = new List<Employee> {employee}
                            });
                            avaliableHolidayDays--;
                        }
                    }
                }

                avaliableHolidayDays = 2;
            }

            return generatedVacationDays;
        }

        public Tuple<int, int, Calendar<WorkingDay>> CalendarGeneratorForEmployees(
            Tuple<int, int, Calendar<VacationDay>> vacationCalendar)
        {
            var calendar = new Calendar<WorkingDay>
            {
                Month = vacationCalendar.Item1,
                Year = vacationCalendar.Item2,
                Days = GetWorkingDays(vacationCalendar)
            };

            var workingCalendar = new Tuple<int, int, Calendar<WorkingDay>>(vacationCalendar.Item1,
                vacationCalendar.Item2, calendar);

            return workingCalendar;
        }

        private List<WorkingDay> GetWorkingDays(Tuple<int, int, Calendar<VacationDay>> vacationCalendar)
        {
            var numberOfDays = DateTime.DaysInMonth(vacationCalendar.Item2, vacationCalendar.Item1);
            var currentDayCounter = 1;
            var workingCalendar = new List<WorkingDay>();
            var dutiesEmployee = new Dictionary<Employee, int>();

            foreach (var employee in Employess)
                dutiesEmployee.Add(employee, 0);

            while (currentDayCounter <= numberOfDays)
            {
                var currentDay = new DateTime(vacationCalendar.Item2, vacationCalendar.Item1, currentDayCounter);

                if ((currentDay.DayOfWeek == DayOfWeek.Sunday) || (currentDay.DayOfWeek == DayOfWeek.Saturday) ||
                    Holidays.Exists(x => x == currentDay))
                {
                    workingCalendar.Add(new WorkingDay
                    {
                        Day = currentDay,
                        IsVacation = true
                    });
                }
                else
                {
                    var availableEmployees = new List<Employee>();
                    var employeeOnHolidayInCurrentDay =
                        vacationCalendar.Item3.Days.FirstOrDefault(y => y.Day == currentDay);

                    foreach (var employee in Employess)
                        if ((employeeOnHolidayInCurrentDay == null) ||
                            !employeeOnHolidayInCurrentDay.Employees.Exists(x => x == employee))
                            availableEmployees.Add(employee);

                    // Smallest number of duties
                    var potentialCandidates = GetPotentialCandidates(availableEmployees, dutiesEmployee);

                    var rnd = new Random();
                    var randomEmployeeId = rnd.Next(potentialCandidates.Count);
                    var potentialEmployeeForWorking = potentialCandidates[randomEmployeeId];

                    if (currentDayCounter > NumberOfEmployeeDutiesPerWeek)
                    {
                        var checkIfEmployeeWorkSecondDay =
                            workingCalendar.Exists(
                                x => (x.Day == currentDay.AddDays(-1)) && (x.Employee == potentialEmployeeForWorking));
                        var checkIfEmployeeWorkThirdTimes =
                            workingCalendar.Exists(
                                x => (x.Day == currentDay.AddDays(-2)) && (x.Employee == potentialEmployeeForWorking));

                        if (checkIfEmployeeWorkSecondDay == false)
                        {
                            workingCalendar.Add(new WorkingDay
                            {
                                Day = currentDay,
                                Employee = potentialEmployeeForWorking,
                                IsVacation = false
                            });
                            dutiesEmployee[potentialEmployeeForWorking]++;
                        }
                        else if (checkIfEmployeeWorkThirdTimes && (potentialCandidates.Count == 1))
                        {
                            workingCalendar.Add(new WorkingDay
                            {
                                Day = currentDay,
                                Employee = potentialEmployeeForWorking,
                                IsVacation = false
                            });
                        }
                        else
                        {
                            potentialEmployeeForWorking =
                                potentialCandidates.FirstOrDefault(x => x.Id != potentialEmployeeForWorking.Id);
                            workingCalendar.Add(new WorkingDay
                            {
                                Day = currentDay,
                                Employee = potentialEmployeeForWorking,
                                IsVacation = false
                            });
                            if (potentialEmployeeForWorking != null) dutiesEmployee[potentialEmployeeForWorking]++;
                        }
                    }
                    else
                    {
                        workingCalendar.Add(new WorkingDay
                        {
                            Day = currentDay,
                            Employee = potentialEmployeeForWorking,
                            IsVacation = false
                        });
                        dutiesEmployee[potentialEmployeeForWorking]++;
                    }
                }

                currentDayCounter++;
            }

            return workingCalendar;
        }

        private List<Employee> GetPotentialCandidates(List<Employee> availableEmployees,
            Dictionary<Employee, int> dutiesEmployee)
        {
            if (availableEmployees == null) throw new ArgumentNullException(nameof(availableEmployees));

            var minDuties = dutiesEmployee.Values.Min();
            var potentialEmployees = new List<Employee>();

            while ((potentialEmployees.Count < 1) && (minDuties < MaxNumberOfWorkingDay))
            {
                potentialEmployees = availableEmployees.Where(
                    x => dutiesEmployee.Where(y => y.Value == minDuties).ToList().Exists(z => z.Key == x)).ToList();

                minDuties++;
            }

            if (potentialEmployees.Count == 0)
                throw new Exception("Didn't fond avaliable employee.");

            return potentialEmployees;
        }

        public void UpdateWokringCalendar(Calendar<VacationDay> vacationCalendar, Calendar<WorkingDay> workingCalendar,
            Employee employee, DateTime employeeVacation)
        {
            // Added new vacation day to current employee vacation harmonogram
            if (vacationCalendar.Days.ToList().Exists(x => x.Day.Day.Equals(employeeVacation.Day)))
                vacationCalendar.Days.FirstOrDefault(x => x.Day.Day.Equals(employeeVacation.Day))?
                    .Employees.Add(employee);
            else
                vacationCalendar.Days.ToList().Add(new VacationDay
                {
                    Day = employeeVacation,
                    Employees = new List<Employee> {employee}
                });

            //Todo Consideration vacation of in current working calendar
        }
    }
}