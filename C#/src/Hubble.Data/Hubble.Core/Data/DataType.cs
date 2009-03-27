using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Data
{
    public enum DataType
    {
        Int32 = 0,
        Int64 = 1,
        Float = 2,
        Date  = 3, //From 1-1-1 to 9999-12-31
        SmallDateTime = 4, //From 1980-01-01 0:0:0 to 2047-12-31 23:59:59
        DateTime=5,
        String= 100,
        Data  = 200,
    }

    public class DataTypeConvert
    {
        public static int DateToInt(DateTime date)
        {
            TimeSpan s = date.Date - DateTime.Parse("1-01-01");

            return (int)s.TotalDays;
        }

        public static int SmallDateTimeToInt(DateTime date)
        {
            TimeSpan s = date - DateTime.Parse("1980-01-01");

            return (int)s.TotalSeconds;
        }

        public static DateTime IntToDate(int days)
        {
            return DateTime.Parse("1-01-01").AddDays(days);
        }

        public static DateTime IntToSmallDatetime(int seconds)
        {
            return DateTime.Parse("1980-01-01").AddSeconds(seconds);
        }
    }

}
