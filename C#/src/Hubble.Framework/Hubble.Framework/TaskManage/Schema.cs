using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManage
{
    /// <summary>
    /// schedule
    /// </summary>
    [Serializable]
    public class Schema
    {
        private int _SchemaId;
        private string _Name;
        private SchemaType _Type;
        private SchemaState _State;
        private SchemaInfo _SchemaInfo;
        private string _UserName;
        private string _Password;
        private string _Database;

        private string _Sql;

        private string _Description;

        /// <summary>
        /// schedule No.
        /// </summary>
        public int SchemaId
        {
            get
            {
                return _SchemaId;
            }
            set
            {
                _SchemaId = value;
            }
        }

        /// <summary>
        /// schedule name
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        /// <summary>
        /// schedule type
        /// </summary>
        public SchemaType Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }

        /// <summary>
        /// schedule statement，
        /// </summary>
        public SchemaState State
        {
            get
            {
                return _State;
            }
            set
            {
                _State = value;
            }
        }

        /// <summary>
        /// schedule information
        /// </summary>
        public SchemaInfo SchemaInfo
        {
            get
            {
                return _SchemaInfo;
            }
            set
            {
                _SchemaInfo = value;
            }
        }

        /// <summary>
        /// User name of hubble
        /// </summary>
        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                _UserName = value;
            }
        }

        /// <summary>
        /// Password of hubble
        /// </summary>
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
            }
        }

        /// <summary>
        /// Database(hubble's database) name
        /// </summary>
        public string Database
        {
            get
            {
                return _Database;
            }
            set
            {
                _Database = value;
            }
        }

        /// <summary>
        /// Sql to excute
        /// </summary>
        public string Sql
        {
            get
            {
                return _Sql;
            }
            set
            {
                _Sql = value;
            }
        }

        /// <summary>
        /// Description of this schema
        /// </summary>
        public string Description
        {
            get
            {
                if (_Description == null)
                {
                    return "";
                }
                else
                {
                    return _Description;
                }
            }

            set
            {
                _Description = value;
            }
        }

        public Schema()
        {
            _Type = SchemaType.RunOnce;
        }

        public DateTime NextTime(DateTime curTime)
        {
            DateTime defaultTime = DateTime.MaxValue;
            DateTime nextTime;
            DateTime datePart;      //NextTime date
            TimeSpan timePart;      //NextTime time

            Schema schema = this;

            if (schema.State == SchemaState.Enable)    //enable
            {
                if (schema.Type == SchemaType.RunOnce)   //run one time only
                {
                    if (schema.SchemaInfo.RunOnceTime < curTime)   //large than one time
                    {
                        return defaultTime;
                    }
                    else
                    {
                        return schema.SchemaInfo.RunOnceTime;
                    }
                }
                else    //repeating
                {
                    datePart = schema.SchemaInfo.RunTime.StartDate.Date;

                    DayFrequency dayFrequency = schema.SchemaInfo.DayFrequency;
                    timePart = GetFristRunTime(dayFrequency);

                    //if (schema.SchemaInfo.RunTime.IsInfinity == true)    //infinity
                    {
                        //date 
                        if (schema.SchemaInfo.Frequency.FrequencyType == FrequencyType.Day)//daily
                        {
                            double DayInterval = (double)schema.SchemaInfo.Frequency.EveryDay.RunInterval;
                            while (datePart < curTime.Date)                  //
                            {
                                datePart = datePart.AddDays(DayInterval);
                            }

                            //is large than the Max value of daily frequency range in next run time 
                            if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                            {
                                datePart = datePart.AddDays(DayInterval);
                                timePart = GetFristRunTime(dayFrequency);
                            }
                        }
                        else if (schema.SchemaInfo.Frequency.FrequencyType == FrequencyType.Week)//weekly
                        {
                            double WeekInterval = (double)(schema.SchemaInfo.Frequency.EveryWeek.RunInterval);
                            DayOfWeek[] daysOfWeek = schema.SchemaInfo.Frequency.EveryWeek.DaysOfWeek;

                            if (daysOfWeek.Length > 0)
                            {
                                if (schema.SchemaInfo.Frequency.EveryWeek.RunInterval <= 1)//every week
                                {
                                    if (datePart < curTime.Date)                 //
                                    {
                                        datePart = curTime.Date;
                                    }

                                    datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);

                                    //is large than the Max value of daily frequency range in next run time
                                    if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                    {
                                        datePart = datePart.AddDays(1.0);
                                        datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);
                                        timePart = GetFristRunTime(dayFrequency);
                                    }
                                }
                                else//inteval in weeks
                                {
                                    if (datePart >= curTime.Date)//less than start date
                                    {
                                        datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);

                                        //is large than the Max value of daily frequency range in next run time

                                        if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                        {
                                            if (datePart.DayOfWeek != DayOfWeek.Sunday)
                                            {
                                                datePart = datePart.AddDays(1.0);
                                                datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);
                                            }
                                            else
                                            {
                                                datePart = datePart.AddDays((WeekInterval - 1) * 7 + 1.0);
                                                datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);
                                            }
                                            timePart = GetFristRunTime(dayFrequency);
                                        }
                                    }
                                    else//current date large than start date
                                    {

                                        for (int s = 0; s < 7; s++)
                                        {
                                            if (datePart.DayOfWeek == DayOfWeek.Monday && s == 0)
                                            {
                                                datePart = datePart.AddDays(7.0);
                                                break;
                                            }

                                            if (datePart.DayOfWeek != DayOfWeek.Monday)
                                            {
                                                datePart = datePart.AddDays(1.0);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        if (datePart.AddDays(-1.0) >= curTime.Date)
                                        {
                                            datePart = GetNextChooseDaysOfWeek(curTime.Date, daysOfWeek, WeekInterval);

                                            //is large than the Max value of daily frequency range in next run time
                                            if (datePart == curTime.Date)
                                            {
                                                if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                                {
                                                    if (datePart.DayOfWeek != DayOfWeek.Sunday)
                                                    {
                                                        datePart = datePart.AddDays(1.0);
                                                        datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);
                                                    }
                                                    else
                                                    {
                                                        datePart = datePart.AddDays((WeekInterval - 1) * 7 + 1.0);
                                                        datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);
                                                    }
                                                    timePart = GetFristRunTime(dayFrequency);
                                                }
                                            }
                                            else
                                            {
                                                timePart = GetFristRunTime(dayFrequency);
                                            }
                                        }
                                        else
                                        {
                                            datePart = datePart.AddDays((WeekInterval - 1.0) * 7.0);

                                            if (datePart < curTime.Date)
                                            {
                                                while (datePart < curTime.Date)
                                                {
                                                    datePart = datePart.AddDays((WeekInterval) * 7.0);
                                                }

                                                datePart = datePart.AddDays(-WeekInterval * 7.0);

                                                if (datePart.AddDays(7.0) >= curTime.Date)
                                                {
                                                    datePart = curTime.Date;
                                                }
                                                else
                                                {
                                                    datePart = datePart.AddDays(WeekInterval * 7.0);
                                                }
                                            }



                                            datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);

                                            //is large than the Max value of daily frequency range in next run time
                                            if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                            {
                                                if (datePart.DayOfWeek != DayOfWeek.Sunday)
                                                {
                                                    datePart = datePart.AddDays(1.0);
                                                    datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);
                                                }
                                                else
                                                {
                                                    datePart = datePart.AddDays((WeekInterval - 1) * 7 + 1.0);
                                                    datePart = GetNextChooseDaysOfWeek(datePart, daysOfWeek, WeekInterval);
                                                }
                                                timePart = GetFristRunTime(dayFrequency);
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        else if (schema.SchemaInfo.Frequency.FrequencyType == FrequencyType.Month)//monthly
                        {
                            int MonthInterval = schema.SchemaInfo.Frequency.EveryMonth.RunInterval;
                            int nDays = schema.SchemaInfo.Frequency.EveryMonth.WhichDay;//which day
                            int nWeeks = schema.SchemaInfo.Frequency.EveryMonth.WhichWeek;//which week
                            DaysOfWeek dayName = schema.SchemaInfo.Frequency.EveryMonth.DayOfWeek;//day of week

                            if (schema.SchemaInfo.Frequency.EveryMonth.RunInterval <= 1)//monthly
                            {
                                //the ? day of each month 
                                if (schema.SchemaInfo.Frequency.EveryMonth.Option == 1)//which day
                                {
                                    if (datePart <= curTime.Date)                 //
                                    {
                                        if (nDays >= curTime.Day)//this month
                                        {
                                            if (nDays == curTime.Day)
                                            {
                                                datePart = curTime.Date;

                                                //is large than the Max value of daily frequency range in next run time
                                                if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                                {
                                                    datePart = curTime.AddMonths(1);
                                                    //datePart = new DateTime(datePart.Year, datePart.Month, nDays);
                                                    datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                                    timePart = GetFristRunTime(dayFrequency);
                                                }
                                            }
                                            else//nDays is lager
                                            {
                                                //datePart = new DateTime(curTime.Year, curTime.Month, nDays);
                                                datePart = GetRunDayInMonth(curTime, nDays, MonthInterval);
                                                timePart = GetFristRunTime(dayFrequency);
                                            }
                                        }
                                        else//next month
                                        {
                                            datePart = curTime.AddMonths(1);
                                            //datePart = new DateTime(datePart.Year, datePart.Month, nDays);
                                            datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                            timePart = GetFristRunTime(dayFrequency);
                                        }
                                    }
                                    else//dataPart is large
                                    {
                                        if (nDays >= datePart.Day)// current month
                                        {
                                            //datePart = new DateTime(datePart.Year, datePart.Month, nDays);
                                            datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                            timePart = GetFristRunTime(dayFrequency);
                                        }
                                        else//next month
                                        {
                                            datePart = datePart.AddMonths(1);
                                            //datePart = new DateTime(datePart.Year, datePart.Month, nDays);
                                            datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                            timePart = GetFristRunTime(dayFrequency);
                                        }
                                    }
                                }
                                else//every month ,week ,day
                                {
                                    if (datePart < curTime.Date)
                                    {
                                        datePart = curTime.Date;
                                    }

                                    while (GetDaysOfWeekInMonth(datePart, nWeeks, dayName) < datePart)
                                    {
                                        DateTime temp = datePart.AddMonths(1);
                                        datePart = new DateTime(temp.Year, temp.Month, 1);
                                    }

                                    datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);

                                    if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                    {
                                        datePart = datePart.AddDays(1.0);
                                        datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);
                                    }
                                }

                            }
                            else//interval month
                            {
                                if (datePart < curTime.Date)//scheduel date is smaller than current date 
                                {
                                    if (datePart.Year == curTime.Year && datePart.Month == curTime.Month)//same month and year
                                    {
                                        //in ?day of the year
                                        if (schema.SchemaInfo.Frequency.EveryMonth.Option == 1)//which day
                                        {
                                            if (curTime.Day <= nDays)
                                            {
                                                //datePart = new DateTime(curTime.Year, curTime.Month, nDays);
                                                datePart = GetRunDayInMonth(curTime, nDays, MonthInterval);
                                            }
                                            else
                                            {
                                                datePart = curTime.AddMonths(MonthInterval);
                                                datePart = new DateTime(datePart.Year, datePart.Month, nDays);
                                            }

                                            if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                            {
                                                datePart = curTime.AddMonths(MonthInterval);
                                                //datePart = new DateTime(datePart.Year, datePart.Month, nDays);
                                                datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                            }
                                        }
                                        else//which day which wee
                                        {
                                            datePart = curTime.Date;

                                            while (GetDaysOfWeekInMonth(datePart, nWeeks, dayName) < datePart)
                                            {
                                                DateTime temp = datePart.AddMonths(MonthInterval);
                                                datePart = new DateTime(temp.Year, temp.Month, 1);
                                            }

                                            datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);

                                            if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                            {
                                                DateTime temp = datePart.AddMonths(MonthInterval);
                                                datePart = new DateTime(temp.Year, temp.Month, 1);
                                                datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);
                                            }

                                        }

                                    }
                                    else//different year or month ,curTime is large
                                    {

                                        //while (datePart.Day != 1)
                                        //{
                                        //    datePart = datePart.AddDays(1.0);
                                        //}
                                        DateTime tt = datePart.AddMonths(1);
                                        datePart = new DateTime(tt.Year, tt.Month, 1);

                                        datePart = datePart.AddMonths(MonthInterval - 1);

                                        while (datePart < curTime.Date)                 //
                                        {
                                            datePart = datePart.AddMonths(MonthInterval);
                                        }

                                        datePart = datePart.AddMonths(-MonthInterval);
                                        //the first day of datePart，
                                        if (datePart.Month == curTime.Month)//datepart of month is same as current time of month
                                        {
                                            //the ?day of the month
                                            if (schema.SchemaInfo.Frequency.EveryMonth.Option == 1)//which day
                                            {
                                                datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);

                                                if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                                {
                                                    datePart = datePart.AddMonths(MonthInterval);
                                                    //datePart = new DateTime(datePart.Year,datePart.Month,nDays);
                                                    datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                                }
                                            }
                                            else//which day of which week
                                            {
                                                datePart = curTime.Date;

                                                while (GetDaysOfWeekInMonth(datePart, nWeeks, dayName) < datePart)
                                                {
                                                    DateTime temp = datePart.AddMonths(MonthInterval);
                                                    datePart = new DateTime(temp.Year, temp.Month, 1);
                                                }

                                                datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);

                                                if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                                {
                                                    DateTime temp = datePart.AddMonths(MonthInterval);
                                                    datePart = new DateTime(temp.Year, temp.Month, 1);
                                                    datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);
                                                }
                                            }
                                        }
                                        else//current month is out of weekinmonth
                                        {
                                            datePart = datePart.AddMonths(MonthInterval);
                                            //the ?day of the month
                                            if (schema.SchemaInfo.Frequency.EveryMonth.Option == 1)//which day
                                            {
                                                datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                                //datePart = datePart.AddDays(nDays - 1);
                                                timePart = GetFristRunTime(dayFrequency);
                                            }
                                            else//which day of which week
                                            {
                                                //while (GetDaysOfWeekInMonth(datePart, nWeeks, dayName) < datePart)
                                                //{
                                                //    DateTime temp = datePart.AddMonths(MonthInterval);
                                                //    datePart = new DateTime(temp.Year, temp.Month, 1);
                                                //}

                                                datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);

                                                timePart = GetFristRunTime(dayFrequency);
                                            }
                                        }
                                    }

                                }


                                else//scheduel start date is large than current date
                                {
                                    //the ? day of interval month
                                    if (schema.SchemaInfo.Frequency.EveryMonth.Option == 1)//which day
                                    {
                                        if (nDays >= datePart.Day)
                                        {
                                            //datePart = new DateTime(datePart.Year,datePart.Month,nDays);
                                            datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                        }
                                        else
                                        {
                                            datePart = datePart.AddMonths(MonthInterval);
                                            //datePart = new DateTime(datePart.Year, datePart.Month, nDays);
                                            datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                        }

                                        if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                        {
                                            datePart = datePart.AddMonths(MonthInterval);
                                            //datePart = new DateTime(datePart.Year, datePart.Month, nDays);
                                            datePart = GetRunDayInMonth(datePart, nDays, MonthInterval);
                                        }
                                    }
                                    else//which day of which month
                                    {
                                        while (GetDaysOfWeekInMonth(datePart, nWeeks, dayName) < datePart)
                                        {
                                            DateTime temp = datePart.AddMonths(MonthInterval);
                                            datePart = new DateTime(temp.Year, temp.Month, 1);
                                        }

                                        datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);


                                        if (IsOverLastRunTime(datePart, curTime, dayFrequency, out timePart))
                                        {
                                            DateTime temp = datePart.AddMonths(MonthInterval);
                                            datePart = new DateTime(temp.Year, temp.Month, 1);
                                            datePart = GetDaysOfWeekInMonth(datePart, nWeeks, dayName);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (schema.SchemaInfo.RunTime.IsInfinity == false)    //infinity
                    {
                        if (datePart != defaultTime)
                        {
                            if (schema.SchemaInfo.RunTime.EndDate.Date < datePart)
                            {
                                //over infinity
                                return defaultTime;
                            }
                        }
                        else
                        {
                            return defaultTime;
                        }
                    }

                    nextTime = new DateTime(datePart.Year, datePart.Month, datePart.Day, timePart.Hours, timePart.Minutes, timePart.Seconds);
                    return nextTime;
                }
            }

            return defaultTime;
        }


        /// <summary>
        ///current time  is over than last run time
        /// </summary>
        /// <param name="curTimeSpan">current date time</param>
        /// <param name="dayFrequency">daily frequency</param>
        /// <param name="nextTimeSpan">next run time</param>
        /// <returns></returns>
        private bool IsOverLastRunTime(DateTime datePart, DateTime curTime, DayFrequency dayFrequency, out TimeSpan nextTimeSpan)
        {
            bool isOverLastRunTime;
            TimeSpan curTimeSpan = curTime.TimeOfDay;
            TimeSpan startTime = dayFrequency.StartTime;
            TimeSpan endTime = dayFrequency.EndTime;
            TimeSpan runIntervalTimeSpan;
            nextTimeSpan = startTime;
            int nIntervalSeconds = 1;

            if (dayFrequency.TimeUnit == TimeUnit.Hour)
            {
                runIntervalTimeSpan = new TimeSpan(dayFrequency.RunInterval, 0, 0);
                nIntervalSeconds = dayFrequency.RunInterval * 3600;
            }
            else
            {
                runIntervalTimeSpan = new TimeSpan(0, dayFrequency.RunInterval, 0);
                nIntervalSeconds = dayFrequency.RunInterval * 60;
            }

            if (datePart <= curTime.Date)
            {
                if (dayFrequency.Option == 1)//run one time only
                {
                    if (curTimeSpan > dayFrequency.RunOnceTime)
                    {
                        isOverLastRunTime = true;
                    }
                    else
                    {
                        isOverLastRunTime = false;
                        nextTimeSpan = dayFrequency.RunOnceTime;
                    }
                }
                else//interval run time
                {
                    if (curTimeSpan > endTime)//is over current date EndTime
                    {
                        isOverLastRunTime = true;
                    }
                    else
                    {
                        if (curTimeSpan < startTime)//before current date StartTime
                        {
                            isOverLastRunTime = false;
                            nextTimeSpan = startTime;
                        }
                        else//after current date StartTime
                        {
                            int nCurTimeSeconds = (int)curTime.TimeOfDay.TotalSeconds;
                            int nStartTimeSeconds = (int)startTime.TotalSeconds;
                            int nEndTimeSeconds = (int)endTime.TotalSeconds;
                            int nNextTimeSpanSeconds = nCurTimeSeconds + (nIntervalSeconds - ((nCurTimeSeconds - nStartTimeSeconds) % nIntervalSeconds));

                            if (nNextTimeSpanSeconds > nEndTimeSeconds)
                            {
                                isOverLastRunTime = true;
                                nextTimeSpan = startTime;
                            }
                            else
                            {
                                int hours = nNextTimeSpanSeconds / 3600;
                                int m = nNextTimeSpanSeconds % 3600;
                                int minutes = m / 60;
                                int seconds = m % 60;

                                nextTimeSpan = new TimeSpan(hours, minutes, seconds);
                                isOverLastRunTime = false;
                            }
                        }
                    }
                }
            }
            else
            {
                isOverLastRunTime = false;
                if (dayFrequency.Option == 1)
                {
                    nextTimeSpan = dayFrequency.RunOnceTime;
                }
                else
                {
                    nextTimeSpan = startTime;
                }
            }

            return isOverLastRunTime;
        }

        /// <summary>
        /// get the first time of run time of the day 
        /// </summary>
        /// <param name="dayFrequency">daily frequency</param>
        /// <returns></returns>
        private TimeSpan GetFristRunTime(DayFrequency dayFrequency)
        {
            if (dayFrequency.Option == 1)
            {
                return dayFrequency.RunOnceTime;
            }
            else
            {
                return dayFrequency.StartTime;
            }
        }

        /// <summary>
        /// the nearest day of the week  after start date inc start date of the cycle 
        /// </summary>
        /// <param name="datePart">start date</param>
        /// <param name="daysOfWeek">param of the days of week</param>
        /// <param name="WeekInterval">interval week</param>
        /// <returns></returns>
        private DateTime GetNextChooseDaysOfWeek(DateTime datePart, DayOfWeek[] daysOfWeek, double WeekInterval)
        {
            int s = 0;
            DateTime tempDate = datePart;
        WeekStart:
            for (int j = 0; j < 7; j++)
            {
                if (WeekInterval > 1.0)
                {
                    if (tempDate.DayOfWeek == DayOfWeek.Monday && j != 0)
                    {
                        tempDate = tempDate.AddDays((WeekInterval - 1.0) * 7.0);
                        datePart = tempDate;
                        goto WeekStart;
                    }
                }

                for (int k = 0; k < daysOfWeek.Length; k++)
                {
                    if (tempDate.DayOfWeek == daysOfWeek[k])    //find out the right  day of the week 
                    {
                        s = j;
                        j = 7;
                        break;
                    }
                }
                if (j == 7)
                {
                    break;
                }

                tempDate = tempDate.AddDays(1.0);
            }

            datePart = datePart.AddDays((double)s);
            return datePart;
        }

        /// <summary>
        /// get the start date of begninning month of the cycle 
        /// </summary>
        /// <param name="datePart">start month</param>
        /// <param name="days">which day</param>
        /// <param name="monthInterval">interval month</param>
        /// <returns></returns>
        private DateTime GetRunDayInMonth(DateTime datePart, int days, int monthInterval)
        {

            for (int i = 0; i < 4 && GetMaxDayInMonth(datePart) < days; i++)//
            {
                datePart = datePart.AddMonths(monthInterval);
            }

            if (GetMaxDayInMonth(datePart) < days)//add n month or less than days 
            {
                datePart = new DateTime(1900, 1, 1);
            }
            else
            {
                datePart = new DateTime(datePart.Year, datePart.Month, days);
            }

            return datePart;
        }

        /// <summary>
        /// which day of ? week
        /// </summary>
        /// <param name="dt">date</param>
        /// <param name="which">which one</param>
        /// <param name="week">the day of week </param>
        /// <returns>date of the week </returns>
        private DateTime GetDaysOfWeekInMonth(DateTime dt, int which, DaysOfWeek week)
        {
            DateTime dtDate = dt;
            DateTime tempDate = dt.AddMonths(1);
            tempDate = new DateTime(tempDate.Year, tempDate.Month, 1);
            int MaxDay = tempDate.AddDays((double)(-1)).Day;

            if (week == DaysOfWeek.Day)
            {
                if (which == 9)
                {
                    dtDate = new DateTime(dt.Year, dt.Month, MaxDay);
                }
                else
                {
                    dtDate = new DateTime(dt.Year, dt.Month, which);
                }
            }
            else
            {
                if (which == 9)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        DateTime temp = new DateTime(dt.Year, dt.Month, MaxDay - i);
                        if ((int)(temp.DayOfWeek) == (int)week)
                        {
                            dtDate = temp;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = (which - 1) * 7 + 1; i <= MaxDay; i++)
                    {
                        DateTime temp = new DateTime(dt.Year, dt.Month, i);
                        if ((int)(temp.DayOfWeek) == (int)week)
                        {
                            dtDate = temp;
                            break;
                        }
                    }
                }
            }
            return dtDate;
        }

        /// <summary>
        /// return the biggest date of the month
        /// </summary>
        /// <param name="dt">date</param>
        /// <returns></returns>
        private int GetMaxDayInMonth(DateTime dt)
        {
            DateTime tempDate = dt.AddMonths(1);
            tempDate = new DateTime(tempDate.Year, tempDate.Month, 1);
            int MaxDay = tempDate.AddDays((double)(-1)).Day;
            return MaxDay;
        }

        public string ConvertToString()
        {
            System.IO.Stream stream = Hubble.Framework.Serialization.XmlSerialization<Schema>.Serialize(this);
            string xmlStr;
            stream.Position = 0;
            Hubble.Framework.IO.Stream.ReadStreamToString(stream, out xmlStr, Encoding.UTF8);
            return xmlStr;
        }

        static public Schema ConvertFromString(string xmlStr)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(
                Encoding.UTF8.GetBytes(xmlStr));
            ms.Position = 0;

            return Hubble.Framework.Serialization.XmlSerialization<TaskManage.Schema>.Deserialize(ms);
        }

    }

}
