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

using Hubble.Core.Right;

namespace Hubble.Core.Global
{
    class UserRightProvider
    {
        static List<UserRight> _UserRightList = null;
        static string _FileName;
        static object _LockObj = new object();
        static Dictionary<string, UserRight> _UserRightDict;

        internal static void CreateUser(string userName, string password, RightItem systemRight)
        {
            lock (_LockObj)
            {
                string key = userName.ToLower().Trim();

                if (_UserRightList == null)
                {
                    _UserRightList = new List<UserRight>();
                    _UserRightDict = new Dictionary<string, UserRight>();
                }

                if (_UserRightDict.ContainsKey(key))
                {
                    throw new UserRightException(string.Format("user name:{0} exist!", userName));
                }

                if (_UserRightList.Count == 0)
                {
                    if ((systemRight & RightItem.ManageUser) == 0)
                    {
                        throw new UserRightException("First user must has ManageUser right");
                    }
                }

                UserRight systemUserRight = new UserRight();

                systemUserRight.UserName = userName.Trim();

                System.Security.Cryptography.MD5CryptoServiceProvider md5 =
                    new System.Security.Cryptography.MD5CryptoServiceProvider();

                byte[] b = new byte[password.Length * 2];

                for (int i = 0; i < password.Length; i++)
                {
                    char c = password[i];

                    b[2 * i] = (byte)(c % 256);
                    b[2 * i + 1] = (byte)(c / 256);
                }

                systemUserRight.PasswordHash = md5.ComputeHash(b);
                systemUserRight.ServerRangeRight = new DatabaseRight("", systemRight);
                systemUserRight.DatabaseRights = new List<DatabaseRight>();

                _UserRightList.Add(systemUserRight);
                _UserRightDict.Add(key, systemUserRight);

                Save();
            }
        }

        internal static void ChangePassword(string userName, string password)
        {
            lock (_LockObj)
            {
                UserRight userRight;
                string key = userName.ToLower().Trim();

                if (!_UserRightDict.TryGetValue(key, out userRight))
                {
                    throw new UserRightException("Invalid username");
                }

                System.Security.Cryptography.MD5CryptoServiceProvider md5 =
                    new System.Security.Cryptography.MD5CryptoServiceProvider();

                byte[] b = new byte[password.Length * 2];

                for (int i = 0; i < password.Length; i++)
                {
                    char c = password[i];

                    b[2 * i] = (byte)(c % 256);
                    b[2 * i + 1] = (byte)(c / 256);
                }

                userRight.PasswordHash = md5.ComputeHash(b);

                Save();
            }
        }


        internal static List<string> UserList
        {
            get
            {
                lock (_LockObj)
                {
                    List<string> result = new List<string>(); 
                    
                    if (_UserRightList == null)
                    {
                        return result;
                    }

                    foreach (UserRight userRight in _UserRightList)
                    {
                        result.Add(userRight.UserName);
                    }

                    return result;
                }
            }
        }

        internal static void DeleteUser(string userName)
        {
            lock (_LockObj)
            {
                if (_UserRightDict == null)
                {
                    return;
                }

                string key = userName.ToLower().Trim();

                if (_UserRightDict.ContainsKey(key))
                {
                    UserRight userRight = _UserRightDict[key];

                    _UserRightDict.Remove(key);
                    _UserRightList.Remove(userRight);

                    Save();
                }
            }
        }

        internal static void Verify(string userName, string password)
        {
            lock (_LockObj)
            {
                if (_UserRightDict == null)
                {
                    return;
                }

                if (_UserRightDict.Count == 0)
                {
                    return;
                }
            }

            UserRight userRight;
            string key = userName.ToLower().Trim();

            if (!_UserRightDict.TryGetValue(key, out userRight))
            {
                throw new UserRightException("Invalid username");
            }

            byte[] b = new byte[password.Length * 2];

            for (int i = 0; i < password.Length; i++)
            {
                char c = password[i];

                b[2 * i] = (byte)(c % 256);
                b[2 * i + 1] = (byte)(c / 256);
            }

            System.Security.Cryptography.MD5CryptoServiceProvider md5 =
                new System.Security.Cryptography.MD5CryptoServiceProvider();

            b = md5.ComputeHash(b);
            
            lock (_LockObj)
            {
                if (b.Length != userRight.PasswordHash.Length)
                {
                    throw new UserRightException("Invalid password");
                }

                for (int i = 0; i < b.Length; i++)
                {
                    if (b[i] != userRight.PasswordHash[i])
                    {
                        throw new UserRightException("Invalid password");
                    }
                }
            }

        }

        internal static List<DatabaseRight> GetUserRights(string userName)
        {
            lock (_LockObj)
            {
                if (_UserRightDict == null)
                {
                    throw new UserRightException("HubbleDotNet is on no authorization mode.");
                }

                string key = userName.ToLower().Trim();
                UserRight userRight;

                if (!_UserRightDict.TryGetValue(key, out userRight))
                {
                    throw new UserRightException(string.Format("user name:{0} does not exist!", userName));
                }

                List<DatabaseRight> result = new List<DatabaseRight>();

                if (userRight.ServerRangeRight != null)
                {
                    result.Add(new DatabaseRight(userRight.ServerRangeRight.DatabaseName,
                        userRight.ServerRangeRight.Right));
                }

                foreach (DatabaseRight dbRight in userRight.DatabaseRights)
                {
                    result.Add(new DatabaseRight(dbRight.DatabaseName,
                        dbRight.Right));
                }

                return result;
            }

        }

        internal static void UpdateDBRight(string databaseName, string userName, RightItem right)
        {
            lock (_LockObj)
            {
                if (_UserRightDict == null)
                {
                    return;
                }

                string key = userName.ToLower().Trim();
                UserRight userRight;

                if (!_UserRightDict.TryGetValue(key, out userRight))
                {
                    throw new UserRightException(string.Format("user name:{0} does not exist!", userName));
                }

                if (databaseName == "")
                {
                    if (_UserRightDict.Count == 1)
                    {
                        if ((right & RightItem.ManageUser) == 0)
                        {
                            throw new UserRightException("Last user must has ManageUser right");
                        }
                    }

                    if (userRight.ServerRangeRight == null)
                    {
                        userRight.ServerRangeRight = new DatabaseRight("", right);
                    }
                    else
                    {
                        userRight.ServerRangeRight.Right = right;
                    }
                }
                else
                {
                    foreach (DatabaseRight dbRight in userRight.DatabaseRights)
                    {
                        if (dbRight.DatabaseName.Equals(databaseName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            dbRight.Right = right;
                            return;
                        }
                    }

                    userRight.DatabaseRights.Add(new DatabaseRight(databaseName, right));
                }

                Save();
            }
        }

        internal static void DeleteDBRight(string databaseName, string userName)
        {
            lock (_LockObj)
            {
                if (_UserRightDict == null)
                {
                    return;
                }

                string key = userName.ToLower().Trim();
                UserRight userRight;

                if (!_UserRightDict.TryGetValue(key, out userRight))
                {
                    throw new UserRightException(string.Format("user name:{0} does not exist!", userName));
                }

                if (databaseName == "")
                {
                    if (_UserRightDict.Count == 1)
                    {
                        throw new UserRightException("Can't delete last user's System right");
                    }

                     userRight.ServerRangeRight = null;
                }
                else
                {
                    DatabaseRight deleteRight = null;

                    foreach (DatabaseRight dbRight in userRight.DatabaseRights)
                    {
                        if (dbRight.DatabaseName.Equals(databaseName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            deleteRight = dbRight;
                            break;
                        }
                    }

                    if (deleteRight != null)
                    {
                        userRight.DatabaseRights.Remove(deleteRight);
                    }
                }

                Save();
            }
        }

        internal static void CanDo(RightItem right)
        {
            Service.ConnectionInformation connInfo = Service.CurrentConnection.ConnectionInfo;
            if (connInfo == null)
            {
                return;
            }

            string userName = connInfo.UserName;

            string databaseName = connInfo.DatabaseName;

            CanDo(databaseName, userName, right);
        }

        internal static void CanDo(string databaseName, RightItem right)
        {
            Service.ConnectionInformation connInfo = Service.CurrentConnection.ConnectionInfo;
            if (connInfo == null)
            {
                return;
            }

            string userName = connInfo.UserName;

            CanDo(databaseName, userName, right);
        }

        internal static void CanDo(string databaseName, string userName, RightItem right)
        {
            lock (_LockObj)
            {
                if (_UserRightDict == null)
                {
                    return;
                }

                if (_UserRightDict.Count == 0)
                {
                    return;
                }

                string key = userName.ToLower().Trim();
                UserRight userRight;

                if (!_UserRightDict.TryGetValue(key, out userRight))
                {
                    throw new UserRightException(string.Format("user name:{0} does not exist!", userName));
                }

                if (userRight.ServerRangeRight != null)
                {
                    if ((right & userRight.ServerRangeRight.Right) != 0)
                    {
                        return;
                    }
                }

                foreach (DatabaseRight dbRight in userRight.DatabaseRights)
                {
                    if (dbRight.DatabaseName.Equals(databaseName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if ((right & dbRight.Right) == 0)
                        {
                            throw new UserRightException(string.Format("Current user hasn't {0} right for this operation",
                                right));
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                throw new UserRightException(string.Format("Current user hasn't {0} right for this operation",
                    right));

            }
        }

        internal static void Load(string fileName)
        {
            lock (_LockObj)
            {
                _FileName = fileName;
                if (!System.IO.File.Exists(_FileName))
                {
                    _UserRightList = null;
                    _UserRightDict = null;
                    return;
                }

                object userRightList;

                Hubble.Framework.Serialization.BinSerialization.DeserializeBinary(
                    new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read,
                         System.IO.FileShare.Read), out userRightList);

                _UserRightList = (List<UserRight>)userRightList;

                _UserRightDict = new Dictionary<string, UserRight>();

                foreach (UserRight userRight in _UserRightList)
                {
                    string key = userRight.UserName.ToLower().Trim();

                    if (!_UserRightDict.ContainsKey(key))
                    {
                        _UserRightDict.Add(key, userRight);
                    }
                }
            }
        }

        private static void Save()
        {
            lock (_LockObj)
            {
                if (_UserRightList != null)
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                    {
                        Hubble.Framework.Serialization.BinSerialization.SerializeBinary(_UserRightList, fs);
                    }
                }
            }
        }

    }
}
