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
    public enum ServerType
    {
        Balance  = 0,
        Failover = 1,
    }

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


        public ServerInfo Clone()
        {
            ServerInfo serverInfo = new ServerInfo();
            serverInfo.ConnectionString = this.ConnectionString;
            serverInfo.ServerName = this.ServerName;
            return serverInfo;
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

        public TabletInfo(string tableName)
        {
            this.TableName = tableName;
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
        public DateTime TimeStamp = DateTime.Parse("1900-01-01");

        public int ExecuteTimeout = 300000; //Execute time out, in millisecond

        /// <summary>
        /// If set to true. It will raise a exception when some 
        /// tablets have problem for query.
        /// </summary>
        public bool KeepDataIntegrity = true; 

        public List<TabletInfo> Tablets = new List<TabletInfo>();

        public List<ServerInfo> ServerList = new List<ServerInfo>();

        public BigTable()
        {
        }

        public BigTable Clone()
        {
            BigTable bigTable = new BigTable();

            bigTable.TimeStamp = this.TimeStamp;

            bigTable.ExecuteTimeout = this.ExecuteTimeout;

            bigTable.KeepDataIntegrity = this.KeepDataIntegrity;

            foreach (TabletInfo tabletInfo in Tablets)
            {
                bigTable.Tablets.Add(tabletInfo.Clone());
            }

            foreach (ServerInfo serverInfo in ServerList)
            {
                bigTable.ServerList.Add(serverInfo.Clone());
            }

            return bigTable;
        }

        public void RemoveServerInfo(ServerInfo serverInfo)
        {
            if (ServerList.Contains(serverInfo))
            {
                ServerList.Remove(serverInfo);

                foreach (TabletInfo tablet in Tablets)
                {
                    tablet.BalanceServers.Remove(serverInfo.ServerName);
                    tablet.FailoverServers.Remove(serverInfo.ServerName);
                }
            }

        }

        public void Add(string tableName, ServerType serverType, string serverName)
        {
            TabletInfo tablet = new TabletInfo(tableName);

            int index = Tablets.IndexOf(tablet);
            if (index >= 0)
            {
                tablet = Tablets[index];

                switch (serverType)
                {
                    case ServerType.Balance:
                        if (tablet.BalanceServers.Contains(serverName))
                        {
                            throw new Hubble.Core.Data.DataException("Can insert same table name with same server name into bigtable");
                        }
                        else
                        {
                            tablet.BalanceServers.Add(serverName);
                        }
                        break;

                    case ServerType.Failover:
                        if (tablet.FailoverServers.Contains(serverName))
                        {
                            throw new Hubble.Core.Data.DataException("Can insert same table name with same server name into bigtable");
                        }
                        else
                        {
                            tablet.FailoverServers.Add(serverName);
                        }
                        break;
                }

            }
            else
            {
                switch (serverType)
                {
                    case ServerType.Balance:
                        tablet.BalanceServers.Add(serverName);
                        break;

                    case ServerType.Failover:
                        tablet.FailoverServers.Add(serverName);
                        break;
                }

                Tablets.Add(tablet);
            }
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
