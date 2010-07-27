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

namespace Hubble.SQLClient
{
    [Serializable]
    public class QueryResult
    {
        public System.Data.DataSet DataSet = new System.Data.DataSet();

        private List<string> _PrintMessages = new List<string>();

        public int PrintMessageCount
        {
            get
            {
                lock (_LockObj)
                {
                    return _PrintMessages.Count;
                }
            }
        }

        public string[] PrintMessages
        {
            get
            {
                lock (_LockObj)
                {
                    return _PrintMessages.ToArray();
                }
            }
        }

        private object _LockObj = new object();

        public QueryResult()
        {
 
        }

        public QueryResult(string printMessage, System.Data.DataSet dataSet)
        {
            _PrintMessages.Add(printMessage);
            DataSet = dataSet;
        }

        public QueryResult(System.Data.DataSet dataSet)
        {
            DataSet = dataSet;
        }

        public QueryResult(string printMessage)
        {
            _PrintMessages.Add(printMessage);
        }

        public int GetDocumentCount()
        {
            foreach (string message in _PrintMessages)
            {
                int index = message.IndexOf("TotalDocuments:");

                if (index == 0)
                {
                    return int.Parse(message.Substring("TotalDocuments:".Length));
                }
            }

            return 0;
        }

        public void AddDataTable(System.Data.DataTable table)
        {
            string tableName = table.TableName;

            int serial = 0;
            bool find = false;

            foreach (System.Data.DataTable tb in DataSet.Tables)
            {
                int index = tb.TableName.IndexOf(tableName, 0, StringComparison.CurrentCultureIgnoreCase);

                if (index >= 0)
                {
                    find = true;

                    string serialStr = tb.TableName.Substring(index + tableName.Length);

                    if (serialStr == "")
                    {
                        continue;
                    }
                    else
                    {
                        serial = int.Parse(serialStr);
                    }
                }
            }

            if (find)
            {
                table.TableName = tableName + serial.ToString();
            }

            DataSet.Tables.Add(table);
        }

        public void AddPrintMessage(string message)
        {
            lock (_LockObj)
            {
                _PrintMessages.Add(message);
            }
        }

        public void AddPrintMessage(string section, string message)
        {
            lock (_LockObj)
            {
                _PrintMessages.Add(string.Format("{0}:{1}", section, message));
            }
        }

        public void RemovePrintMessage(string message)
        {
            lock (_LockObj)
            {
                _PrintMessages.Remove(message);
            }
        }
    }
}
