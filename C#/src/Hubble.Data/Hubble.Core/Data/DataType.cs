/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
        static public Type GetClrType(DataType type)
        {
            switch (type)
            {
                case DataType.Data:

                    throw new Exception("Not finished!");

                case DataType.String:
                    return typeof(string);

                case DataType.Int32:
                    return typeof(int);

                case DataType.Int64:
                    return typeof(long);

                case DataType.Date:
                case DataType.SmallDateTime:
                case DataType.DateTime:
                    return typeof(DateTime);


                case DataType.Float:
                    return typeof(float);
                default:
                    throw new ArgumentException(string.Format("Invalid type:{0}", type));
            }
        }

        static public string GetString(DataType type, int[] buf, int from, int len)
        {
            switch (type)
            {
                case DataType.Data:

                    throw new Exception("Not finished!");

                case DataType.String:

                    StringBuilder str = new StringBuilder();

                    for (int i = from; i < from + len; i++)
                    {
                        char c;

                        if (i % 2 == 0)
                        {
                            c = (char)(buf[i] >> 16);
                        }
                        else
                        {
                            c = (char)(buf[i] % 65536);
                        }

                        if (c == 0)
                        {
                            break;
                        }

                        str.Append(c);
                    }

                    return str.ToString();
                case DataType.Date:
                    {
                        DateTime date = IntToDate(buf[from]);
                        return date.ToString("yyyy-MM-dd");
                    }
                case DataType.SmallDateTime:
                    {
                        DateTime date = IntToSmallDatetime(buf[from]);
                        return date.ToString("yyyy-MM-dd HH:mm:ss") + "." + date.Millisecond.ToString();
                    }
                case DataType.Int32:
                    return buf[from].ToString();

                case DataType.Int64:
                    {
                        long l;
                        l = ((long)buf[from]) << 32 + buf[from + 1];
                        return l.ToString();
                    }

                case DataType.DateTime:
                    {
                        long l;
                        l = ((long)buf[from]) << 32 + buf[from + 1];
                        DateTime date = LongToDateTime(l);
                        return date.ToString("yyyy-MM-dd HH:mm:ss") + "." + date.Millisecond.ToString();
                    }

                case DataType.Float:
                    {
                        long l;
                        l = ((long)buf[from]) << 32 + buf[from + 1];

                        float f = l;
                        l = ((long)buf[from + 2]) << 32 + buf[from + 3];

                        f += (float)l / 1000000000000000000;

                        return f.ToString();
                    }
                default:
                    throw new ArgumentException(string.Format("Invalid type:{0}", type));
            }
        }

        static public int[] GetData(DataType type, string value)
        {
            int[] ret;
            long l;

            switch (type)
            {
                case DataType.Data:

                    throw new Exception("Not finished!");

                case DataType.String:
                    if (value.Length > 32 || value.Length <= 0)
                    {
                        throw new ArgumentException("Data index length must less then 64!");
                    }

                    int len = value.Length % 2 == 0 ? value.Length / 2 : value.Length / 2 + 1;

                    ret = new int[len];

                    int i = 0;
                    int d = 0;

                    foreach(int c in value)
                    {
                        if (i % 2 == 0)
                        {
                            d = c << 16;

                            if (i == value.Length - 1)
                            {
                                ret[i / 2] = d;
                            }
                        }
                        else
                        {
                            ret[i / 2] = d + c;
                            d = 0;
                        }

                        i++;
                    }
                    return ret;

                case DataType.Date:
                case DataType.Int32:
                case DataType.SmallDateTime:
                    ret = new int[1];
                    ret[0] = int.Parse(value);
                    return ret;

                case DataType.Int64:
                    ret = new int[2];
                    l = long.Parse(value);
                    ret[0] = (int)(l >> 32);
                    ret[1] = (int)(l & 0xFFFFFFFF);
                    return ret;

                case DataType.DateTime:
                    ret = new int[2];
                    DateTime t = DateTime.Parse(value);
                    l = t.Ticks;
                    ret[0] = (int)(l >> 32);
                    ret[1] = (int)(l & 0xFFFFFFFF);
                    return ret;

                case DataType.Float:

                    float f = float.Parse(value);
                    long integer = (long)Math.Truncate((double)f);
                    long dec = (long)((f - integer) * 1000000000000000000);

                    ret = new int[4];
                    l = integer;
                    ret[0] = (int)(l >> 32);
                    ret[1] = (int)(l & 0xFFFFFFFF);
                    l = dec;
                    ret[2] = (int)(l >> 32);
                    ret[3] = (int)(l & 0xFFFFFFFF);
                    return ret;
                default:
                    throw new ArgumentException(string.Format("Invalid type:{0}", type));
            }
        }

        /// <summary>
        /// length by sizeof(int)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int GetDataLength(DataType type, int length)
        {
            switch (type)
            {
                case DataType.Data:
                    if (length > 64)
                    {
                        throw new ArgumentException("Data index length must less then 64!");
                    }
                    return length % sizeof(int) == 0 ? length / sizeof(int) : length / sizeof(int) + 1;

                case DataType.String:
                    if (length > 32 || length <= 0)
                    {
                        throw new ArgumentException("Data index length must less then 64!");
                    }
                    return length % 2 == 0 ? length / 2 : length / 2 + 1;
                case DataType.Date:
                case DataType.Int32:
                case DataType.SmallDateTime:
                    length = sizeof(int);
                    return length % sizeof(int) == 0 ? length / sizeof(int) : length / sizeof(int) + 1;

                case DataType.Int64:
                case DataType.DateTime:
                    length = sizeof(long); //ticks
                    return length % sizeof(int) == 0 ? length / sizeof(int) : length / sizeof(int) + 1; 

                case DataType.Float:
                    return sizeof(long) * 2;

                default:
                    throw new ArgumentException(string.Format("Invalid type:{0}", type));
            }
        }

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

        public static long DateTimeToLong(DateTime date)
        {
            return date.Ticks;
        }

        public static DateTime IntToDate(int days)
        {
            return DateTime.Parse("1-01-01").AddDays(days);
        }

        public static DateTime IntToSmallDatetime(int seconds)
        {
            return DateTime.Parse("1980-01-01").AddSeconds(seconds);
        }

        public static DateTime LongToDateTime(long ticks)
        {
            return new DateTime(ticks);
        }
    }

}
