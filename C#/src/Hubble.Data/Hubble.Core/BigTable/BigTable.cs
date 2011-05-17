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
    public class TabletInfo
    {
        public string TableName = "";

        /// <summary>
        /// Connection String list of balance servers
        /// </summary>
        public List<string> BalanceServers = new List<string>();

        /// <summary>
        /// Connection String list of failover servers
        /// </summary>
        public List<string> FailoverServers = new List<string>();

        public TabletInfo()
        {
        }

        public TabletInfo(string tableName, string connectionString)
        {
            this.TableName = tableName;
            BalanceServers.Add(connectionString);
        }

        public TabletInfo Clone()
        {
            TabletInfo tableInfo = new TabletInfo();
            tableInfo.TableName = this.TableName;

            foreach (string connectionString in this.BalanceServers)
            {
                tableInfo.BalanceServers.Add(connectionString);
            }

            foreach (string connectionString in this.FailoverServers)
            {
                tableInfo.FailoverServers.Add(connectionString);
            }

            return tableInfo;
        }

        public override bool Equals(object obj)
        {
            TabletInfo dest = obj as TabletInfo;
            if (dest == null)
            {
                return false;
            }

            return this.TableName.Equals(dest.TableName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.TableName.GetHashCode();
        }

        public override string ToString()
        {
            return this.TableName;
        }

    }

    public class TableCollection
    {
        public List<TabletInfo> TableNames;

        public TableCollection()
        {
            TableNames = new List<TabletInfo>();

            //TableNames.Add(new TableInfo("News", "ServerName=127.0.0.1"));
            //TableNames.Add(new TableInfo("TitleNews", "ServerName=127.0.0.1"));
        }
    }

    /// <summary>
    /// Configure of BigTable
    /// </summary>
    public class BigTable
    {
        public List<TabletInfo> Tablets = new List<TabletInfo>();

        public BigTable()
        {
            //TableCollectionList.Add(new TableCollection());
        }

        public void Add(TabletInfo tablet)
        {
            if (Tablets.Contains(tablet))
            {
                throw new Hubble.Core.Data.DataException("Can insert same table name into bigtable");
            }

            Tablets.Add(tablet);
        }
    }
}
