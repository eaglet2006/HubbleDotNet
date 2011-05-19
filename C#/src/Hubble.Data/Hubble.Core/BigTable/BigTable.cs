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
    public class ServerInfo
    {
        public string ServerName = "";
        public string ConnectionString = "";

        public ServerInfo()
        {

        }

        public ServerInfo(string serverName, string connectionString)
        {
            ServerName = serverName;
            ConnectionString = connectionString;
        }

        public override bool Equals(object obj)
        {
            ServerInfo dest = obj as ServerInfo;
            if (dest == null)
            {
                return false;
            }

            if (dest.ServerName == null || this.ServerName == null)
            {
                return this.ServerName == dest.ServerName;
            }
            else
            {
                return this.ServerName.ToLower() == dest.ServerName.ToLower();
            }
        }

        public override int GetHashCode()
        {
            if (this.ServerName == null)
            {
                return base.GetHashCode();
            }
            else
            {
                return this.ServerName.ToLower().GetHashCode();
            }
        }

        public override string ToString()
        {
            return this.ServerName;
        }
    }

    public class TabletInfo
    {
        public string TableName = "";

        /// <summary>
        /// server name list of balance servers
        /// </summary>
        public List<string> BalanceServers = new List<string>();

        /// <summary>
        /// server name list of failover servers
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

    /// <summary>
    /// Configure of BigTable
    /// </summary>
    public class BigTable
    {
        public List<TabletInfo> Tablets = new List<TabletInfo>();

        public List<ServerInfo> ServerList = new List<ServerInfo>();

        public BigTable()
        {
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
