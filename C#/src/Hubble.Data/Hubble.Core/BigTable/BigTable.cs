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

namespace Hubble.Core.BigTable
{
    public class TableInfo
    {
        public string TableName = "";

        public List<string> BalanceServers = new List<string>();

        public List<string> FailoverServers = new List<string>();

        public TableInfo()
        {
        }

        public TableInfo(string tableName, string serverName)
        {
            this.TableName = tableName;
            BalanceServers.Add(serverName);
        }



        public TableInfo Clone()
        {
            TableInfo tableInfo = new TableInfo();
            tableInfo.TableName = this.TableName;

            foreach (string server in this.BalanceServers)
            {
                tableInfo.BalanceServers.Add(server);
            }

            foreach (string server in this.FailoverServers)
            {
                tableInfo.FailoverServers.Add(server);
            }

            return tableInfo;
        }

    }

    public class TableCollection
    {
        public string ServerName;
        public string ConnectString;
        public List<TableInfo> TableNames;

        public TableCollection()
        {
            ServerName = "";
            ConnectString = "";
            TableNames = new List<TableInfo>();

            //TableNames.Add(new TableInfo("News", "ServerName=127.0.0.1"));
            //TableNames.Add(new TableInfo("TitleNews", "ServerName=127.0.0.1"));
        }

        public TableCollection Clone()
        {
            TableCollection tc = new TableCollection();
            tc.ServerName = this.ServerName;
            tc.ConnectString = this.ConnectString;

            foreach (TableInfo tableInfo in this.TableNames)
            {
                tc.TableNames.Add(tableInfo.Clone());
            }

            return tc;
        }
    }

    /// <summary>
    /// Configure of BigTable
    /// </summary>
    public class BigTable
    {
        public List<TableCollection> TableCollectionList = new List<TableCollection>();

        public BigTable()
        {
            //TableCollectionList.Add(new TableCollection());
        }
    }
}
