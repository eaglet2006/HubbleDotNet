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
using System.IO;
using System.IO.Compression;
using System.Data;

namespace Hubble.SQLClient
{
    public class QueryResultSerialization : Hubble.Framework.Serialization.IMySerialization
    {
        public enum DataType : byte
        {
            Null = 0,
            Byte = 1,
            Short= 2,
            Int  = 3,
            Long = 4,
            DateTime=5,
            Float= 6,
            Double=7,
            Decimal=8,
            String =100,
            Data = 127,
        }

        #region static members

        static public DataType GetDataType(Type type)
        {
            if (type == typeof(string))
            {
                return DataType.String;
            }
            else if (type == typeof(bool)) //TinyInt
            {
                return DataType.Byte;
            }
            else if (type == typeof(byte)) //TinyInt
            {
                return DataType.Byte;
            }
            else if (type == typeof(short)) //SmaillInt
            {
                return DataType.Short;
            }
            else if (type == typeof(int)) //Int
            {
                return DataType.Int;
            }
            else if (type == typeof(long)) //BigInt
            {
                return DataType.Long;
            }
            else if (type == typeof(DateTime)) //DateTime
            {
                return DataType.DateTime;
            }
            else if (type == typeof(float)) //Float, Real
            {
                return DataType.Float;
            }
            else if (type == typeof(double)) //Float
            {
                return DataType.Double;
            }
            else if (type == typeof(decimal)) //Float
            {
                return DataType.Decimal;
            }
            else if (type == typeof(byte[])) //Data
            {
                return DataType.Data;
            }
            else
            {
                throw new Exception(string.Format("QueryResultSerialization fail! Unknown data type:{0}",
                    type.ToString()));
            }
        }

        static public Type GetType(DataType type)
        {
            switch (type)
            {
                case DataType.Byte:
                    return typeof(byte);
                case DataType.Data:
                    return typeof(byte[]);
                case DataType.DateTime:
                    return typeof(DateTime);
                case DataType.Decimal:
                    return typeof(double);
                case DataType.Double:
                    return typeof(double);
                case DataType.Float:
                    return typeof(float);
                case DataType.Int:
                    return typeof(int);
                case DataType.Long:
                    return typeof(long);
                case DataType.Short:
                    return typeof(short);
                case DataType.String:
                    return typeof(string);
                default:
                    throw new Exception(string.Format("QueryResultDeserialization fail! Unknown data type:{0}",
                        type.ToString()));
            }

        }

        public static byte[] ReadToBuf(Stream stream, byte[] buf)
        {
            int offset = 0;
            int readBytes = 0;

            while (offset < buf.Length)
            {
                readBytes = stream.Read(buf, offset, buf.Length - offset);
                if (readBytes == 0)
                {
                    throw new System.IO.IOException("Stream had been closed!");
                }

                offset += readBytes;
            }

            return buf;
        }

        public static string ToString(Stream stream)
        {
            byte[] buf = ReadToBuf(stream, new byte[sizeof(int)]);
            int count = BitConverter.ToInt32(buf, 0);
            buf = ReadToBuf(stream, new byte[count]);
            return Encoding.UTF8.GetString(buf);
        }

        public static int ToInt32(Stream stream)
        {
            byte[] buf = ReadToBuf(stream, new byte[sizeof(int)]);
            return BitConverter.ToInt32(buf, 0);
        }

        public static object Read(Stream stream, DataType type)
        {
            switch (type)
            {
                case DataType.Byte:
                    {
                        return (byte)stream.ReadByte();
                    }
                case DataType.Data:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(int)]);
                        int count = BitConverter.ToInt32(buf, 0);
                        return ReadToBuf(stream, new byte[count]);
                    }

                case DataType.DateTime:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(long)]);
                        long ticks = BitConverter.ToInt64(buf, 0);
                        return new DateTime(ticks);
                    }
                case DataType.Decimal:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(double)]);
                        return (decimal)BitConverter.ToDouble(buf, 0);
                    }
                case DataType.Double:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(double)]);
                        return BitConverter.ToDouble(buf, 0);
                    }
                case DataType.Float:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(float)]);
                        return BitConverter.ToSingle(buf, 0);
                    }
                case DataType.Int:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(int)]);
                        return BitConverter.ToInt32(buf, 0);
                    }
                case DataType.Long:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(long)]);
                        return BitConverter.ToInt64(buf, 0);
                    }
                case DataType.Short:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(short)]);
                        return BitConverter.ToInt16(buf, 0);
                    }
                case DataType.String:
                    {
                        byte[] buf = ReadToBuf(stream, new byte[sizeof(int)]);
                        int count = BitConverter.ToInt32(buf, 0);
                        buf = ReadToBuf(stream, new byte[count]);
                        return Encoding.UTF8.GetString(buf);

                    }
                case DataType.Null:
                    return DBNull.Value;
                default:
                    throw new Exception(string.Format("QueryResultDeserialization fail! Unknown data type:{0}",
                        type.ToString()));
            }
        }


        public static void Write(Stream stream, Type type, object data)
        {
            if (type == typeof(string))
            {
                string str = data as string;

                byte[] strBuf = Encoding.UTF8.GetBytes(str);

                stream.Write(BitConverter.GetBytes(strBuf.Length), 0, sizeof(int));
                stream.Write(strBuf, 0, strBuf.Length);
            }
            else if (type == typeof(bool)) //TinyInt
            {
                if ((bool)data)
                {
                    stream.WriteByte((byte)1);
                }
                else
                {
                    stream.WriteByte((byte)0);
                }
            }
            else if (type == typeof(byte)) //TinyInt
            {
                stream.WriteByte((byte)data);
            }
            else if (type == typeof(short)) //SmaillInt
            {
                stream.Write(BitConverter.GetBytes((short)data), 0, sizeof(short));
            }
            else if (type == typeof(int)) //Int
            {
                stream.Write(BitConverter.GetBytes((int)data), 0, sizeof(int));
            }
            else if (type == typeof(long)) //BigInt
            {
                stream.Write(BitConverter.GetBytes((long)data), 0, sizeof(long));
            }
            else if (type == typeof(DateTime)) //DateTime
            {
                stream.Write(BitConverter.GetBytes(((DateTime)data).Ticks), 0, sizeof(long));
            }
            else if (type == typeof(float)) //Float, Real
            {
                stream.Write(BitConverter.GetBytes((float)data), 0, sizeof(float));
            }
            else if (type == typeof(double)) //Float
            {
                stream.Write(BitConverter.GetBytes((double)data), 0, sizeof(double));
            }
            else if (type == typeof(decimal)) //Float
            {
                stream.Write(BitConverter.GetBytes((double)((decimal)data)), 0, sizeof(double));
            }
            else if (type == typeof(byte[])) //Data
            {
                byte[] databuf = data as byte[];

                stream.Write(BitConverter.GetBytes(databuf.Length), 0, sizeof(int));
                stream.Write(databuf, 0, databuf.Length);
            }
            else
            {
                throw new Exception(string.Format("QueryResultSerialization fail! Unknown data type:{0}",
                    type.ToString()));
            }
        }

        private static void SerializeTable(Stream stream, DataTable table)
        {
            //Write table name
            Write(stream, typeof(string), table.TableName);

            //Write MinimumCapacity
            Write(stream, typeof(int), table.MinimumCapacity);
            

            //Columns count
            Write(stream, typeof(int), table.Columns.Count);

            //Write columns
            for(int i = 0; i < table.Columns.Count; i++)
            {
                stream.WriteByte((byte)GetDataType(table.Columns[i].DataType));
                Write(stream, typeof(string), table.Columns[i].ColumnName);
            }

            //Rows count
            Write(stream, typeof(int), table.Rows.Count);

            //Write rows

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    DataType dataType;

                    if (row[i] == System.DBNull.Value)
                    {
                        dataType = DataType.Null;
                    }
                    else
                    {
                        dataType = GetDataType(table.Columns[i].DataType);
                    }

                    stream.WriteByte((byte)dataType);

                    if (dataType != DataType.Null)
                    {
                        Write(stream, table.Columns[i].DataType, row[i]);
                    }
                }
            }
        }

        private static void InnerSeialize(Stream g, QueryResult qResult)
        {
            if (qResult.PrintMessages == null)
            {
                //Write PrintMessages Count
                Write(g, typeof(int), 0);
            }
            else
            {
                //Write PrintMessages Count
                Write(g, typeof(int), qResult.PrintMessageCount);

                //Write PrintMessages
                foreach (string printMsg in qResult.PrintMessages)
                {
                    Write(g, typeof(string), printMsg);
                }
            }

            if (qResult.DataSet == null)
            {
                //Write table count
                Write(g, typeof(int), 0);
            }
            else
            {
                //Write table count
                Write(g, typeof(int), qResult.DataSet.Tables.Count);

                //Serialize tables
                foreach (DataTable table in qResult.DataSet.Tables)
                {
                    SerializeTable(g, table);
                }
            }
        }

        private static DataTable DeseializeTable(Stream g)
        {
            DataTable table = new DataTable();

            //Read table name
            table.TableName = ToString(g);

            //Write MinimumCapacity
            int minimumCapacity = ToInt32(g);

            //Read columns count
            int columnCount = ToInt32(g);

            //Get columns
            for (int i = 0; i < columnCount; i++)
            {
                DataColumn col = new DataColumn();

                DataType dataType = (DataType)g.ReadByte();

                col.DataType = GetType(dataType);

                col.ColumnName = ToString(g);

                table.Columns.Add(col);
            }

            //Read rows count
            int rowCount = ToInt32(g);

            //Get rows
            for(int i = 0; i < rowCount; i++)
            {
                DataRow row = table.NewRow();

                for (int j = 0; j < table.Columns.Count; j++)
                {
                    DataType dataType = (DataType)g.ReadByte();

                    row[j] = Read(g, dataType);
                }

                table.Rows.Add(row);
            }


            table.MinimumCapacity = minimumCapacity;

            return table;
        }

        private static QueryResult InnerDeseialize(Stream g)
        {
            QueryResult result = new QueryResult();

            //Read PrintMessage Count
            int printMsgCount = ToInt32(g);

            if (printMsgCount > 0)
            {
                for (int i = 0; i < printMsgCount; i++)
                {
                    //Read PrintMessage

                    result.AddPrintMessage(ToString(g));
                }
            }

            //Read table count
            int tableCount = ToInt32(g);

            if (tableCount > 0)
            {
                //Insert table
                for (int i = 0; i < tableCount; i++)
                {
                    result.DataSet.Tables.Add(DeseializeTable(g));
                }
            }

            return result;
        }


        public static void Serialize(Stream stream, QueryResult qResult, bool compress, short version)
        {
            if (compress)
            {
                using (GZipStream g = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    InnerSeialize(g, qResult);
                }
            }
            else
            {
                InnerSeialize(stream, qResult);
            }
        }

        public static QueryResult Deserialize(Stream stream, bool compress, short version)
        {
            if (compress)
            {
                using (GZipStream g = new GZipStream(stream, CompressionMode.Decompress, true))
                {
                    return InnerDeseialize(g);
                }
            }
            else
            {
                return InnerDeseialize(stream);
            }
        }

        #endregion

        bool _Compress = false;
        QueryResult _Result;

        public QueryResultSerialization(QueryResult result)
        {
            _Result = result;
        }

        public QueryResultSerialization(QueryResult result, bool compress)
        {
            _Result = result;
            _Compress = compress;
        }

        #region IMySerialization Members

        public byte Version
        {
            get { return 1; }
        }

        public void Serialize(Stream s)
        {
            Serialize(s, _Result, _Compress, Version);
        }

        public object Deserialize(Stream s, short version)
        {
            return Deserialize(s, _Compress, version);
        }

        #endregion
    }
}
