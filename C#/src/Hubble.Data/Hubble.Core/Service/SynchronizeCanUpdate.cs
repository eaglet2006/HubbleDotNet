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

using Hubble.Core.Data;
using Hubble.Core.DBAdapter;
using Hubble.SQLClient;

namespace Hubble.Core.Service
{
    class SynchronizeCanUpdate
    {
        TableSynchronize _TableSync;
        DBProvider _DBProvider;
        int _Step;
        OptimizationOption _OptimizeOption;
        bool _FastestMode;
        bool _HasInsertCount = false;
        Service.SyncFlags _Flags;

        private string GetSqlServerSelectSql(long from)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Select top {0} [{1}] ", _Step, _DBProvider.Table.DocIdReplaceField);

            foreach (Field field in _DBProvider.Table.Fields)
            {
                if (field.Name.Equals(_DBProvider.Table.DocIdReplaceField, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                sb.AppendFormat(", [{0}] ", field.Name);
            }

            if ((_Flags & SyncFlags.NoLock) != 0)
            {
                sb.AppendFormat(" from {0} with(nolock) where [{2}] > {1} order by [{2}]", _DBProvider.Table.DBTableName, from, _DBProvider.Table.DocIdReplaceField);
            }
            else
            {
                sb.AppendFormat(" from {0} where [{2}] > {1} order by [{2}]", _DBProvider.Table.DBTableName, from, _DBProvider.Table.DocIdReplaceField);
            }

            return sb.ToString();
        }

        private string GetSqlServerTriggleSql(long serial, int top, string opr)
        {
            return string.Format("select top {0} Serial, Id, Fields from {1} where Opr = '{2}' and serial > {3} order by Serial", 
                top, _DBProvider.Table.TriggerTableName, opr, serial);
        }


        private string GetOracleFieldNameList(long from)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" {0} ", Oracle8iAdapter.GetFieldName(_DBProvider.Table.DocIdReplaceField));

            foreach (Field field in _DBProvider.Table.Fields)
            {
                if (field.Name.Equals(_DBProvider.Table.DocIdReplaceField, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                sb.AppendFormat(", {0} ", Oracle8iAdapter.GetFieldName(field.Name));
            }

            return sb.ToString();
        }

        private string GetOracleSelectSql(long from)
        {
            string fields = GetOracleFieldNameList(from);

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("select {0} from {1} where {4} > {2} and rownum <= {3} order by {4} ",
                fields, _DBProvider.Table.DBTableName, from, _Step, Oracle8iAdapter.GetFieldName(_DBProvider.Table.DocIdReplaceField));

            return sb.ToString();
        }


        private string GetOracleTriggleSql(long serial, int top, string opr)
        {
            return string.Format("select Serial, Id, Fields from {1} where Opr = '{2}' and serial > {3} and rownum <= {0} order by Serial",
                top, _DBProvider.Table.TriggerTableName, opr, serial);
        }

        private string GetMySqlSelectSql(long from)
        {
            StringBuilder sb = new StringBuilder();

            if ((_Flags & SyncFlags.NoLock) != 0)
            {
                sb.Append("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ;");
            }

            sb.Append("Select ");

            int i = 0;
            foreach (Field field in _DBProvider.Table.Fields)
            {
                if (i == 0)
                {
                    sb.AppendFormat("`{0}` ", field.Name);
                }
                else
                {
                    sb.AppendFormat(", `{0}` ", field.Name);
                }

                i++;
            }

            sb.AppendFormat(" from {0} where {1} > {2} order by {1} limit {3}", _DBProvider.Table.DBTableName,
                _DBProvider.Table.DocIdReplaceField, from, _Step);


            if ((_Flags & SyncFlags.NoLock) != 0)
            {
                sb.Append(";SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;");
            }

            return sb.ToString();
        }

        private string GetSqliteSelectSql(long from)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Select ");

            int i = 0;
            foreach (Field field in _DBProvider.Table.Fields)
            {
                if (i == 0)
                {
                    sb.AppendFormat("[{0}] ", field.Name);
                }
                else
                {
                    sb.AppendFormat(", [{0}] ", field.Name);
                }

                i++;
            }

            sb.AppendFormat(" from {0} where {1} > {2} order by {1} limit {3}", _DBProvider.Table.DBTableName,
                _DBProvider.Table.DocIdReplaceField, from, _Step);

            return sb.ToString();

        }

        private string GetMySqlTriggleSql(long serial, int top, string opr)
        {
            return string.Format("select Serial, Id, Fields from {1} where Opr = '{2}' and serial > {3} order by Serial limit {0}",
                top, _DBProvider.Table.TriggerTableName, opr, serial);
        }

        private string GetSqliteTriggleSql(long serial, int top, string opr)
        {
            return string.Format("select Serial, Id, Fields from {1} where Opr = '{2}' and serial > {3} order by Serial limit {0}",
                top, _DBProvider.Table.TriggerTableName, opr, serial);
        }
        

        private string GetSelectSql(long from)
        {
            if (_DBProvider.Table.DBAdapterTypeName.IndexOf("sqlserver", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetSqlServerSelectSql(from);
            }
            else if (_DBProvider.Table.DBAdapterTypeName.IndexOf("oracle", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetOracleSelectSql(from);
            }
            else if (_DBProvider.Table.DBAdapterTypeName.IndexOf("mysql", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetMySqlSelectSql(from);
            }
            else if (_DBProvider.Table.DBAdapterTypeName.IndexOf("sqlite", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetSqliteSelectSql(from);
            }
            else
            {
                return GetSqlServerSelectSql(from);
            }

        }


        private string GetTriggleSql(long serial, int top, string opr)
        {
            if (_DBProvider.Table.DBAdapterTypeName.IndexOf("sqlserver", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetSqlServerTriggleSql(serial, top, opr);
            }
            else if (_DBProvider.Table.DBAdapterTypeName.IndexOf("oracle", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetOracleTriggleSql(serial, top, opr);
            }
            else if (_DBProvider.Table.DBAdapterTypeName.IndexOf("mysql", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetMySqlTriggleSql(serial, top, opr);
            }
            else if (_DBProvider.Table.DBAdapterTypeName.IndexOf("sqlite", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetSqliteTriggleSql(serial, top, opr);
            }
            else
            {
                return GetSqlServerTriggleSql(serial, top, opr);
            }
        }

        private Document GetOneRowDocument(System.Data.DataRow row)
        {
            Document document = new Document();

            document.DocId = _DBProvider.GetDocIdFromDocIdReplaceFieldValue(int.Parse(row[_DBProvider.Table.DocIdReplaceField].ToString()));

            foreach (Field field in _DBProvider.Table.Fields)
            {
                if (field.IndexType == Field.Index.None)
                {
                    if (!_DBProvider.Table.HasMirrorTable)
                    {
                        continue;
                    }
                }

                string value = null;

                if (row[field.Name] == DBNull.Value)
                {
                    if (!field.CanNull)
                    {
                        throw new DataException(string.Format("Field:{0} in table {1} is not null so that it can't be inserted null value!",
                            field.Name, _DBProvider.Table.Name));
                    }

                    if (field.DefaultValue == null)
                    {
                        if (field.IndexType != Field.Index.None)
                        {
                            throw new DataException(string.Format("Field:{0} in table {1} is null but hasn't default value so that it can't be inserted null value!",
                                field.Name, _DBProvider.Table.Name));
                        }
                    }

                    value = field.DefaultValue;
                }
                else
                {
                    if (row[field.Name] is DateTime)
                    {
                        value = ((DateTime)row[field.Name]).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }
                    else
                    {
                        value = row[field.Name].ToString();
                    }
                }

                document.Add(field.Name, value, field.DataType, field.DataLength, false);
            }


            return document;
        }

        #region DoGetDocuments thread fields

        System.Threading.Thread _GetDocForInsertThread = null;

        object _GetDocForInsertThreadLock = new object();

        List<Document> _DocForInsertResult = null;

        long _LastFrom = -1;

        bool _ReadyToGet = false;

        bool ReadyToGet
        {
            get
            {
                lock (_GetDocForInsertThreadLock)
                {
                    return _ReadyToGet;
                }
            }

            set
            {
                lock (_GetDocForInsertThreadLock)
                {
                    _ReadyToGet = value;

                    if (_ReadyToGet)
                    {
                        _AlreadyGet = false;
                    }
                }
            }
        }

        bool _AlreadyGet = true;

        bool AlreadyGet
        {
            get
            {
                lock (_GetDocForInsertThreadLock)
                {
                    return _AlreadyGet;
                }
            }

            set
            {
                lock (_GetDocForInsertThreadLock)
                {
                    _AlreadyGet = value;

                    if (_AlreadyGet)
                    {
                        _ReadyToGet = false;
                    }
                }
            }
        }

        #endregion

        private void DoGetDocumentsForInsertAsync(object dbAdapter)
        {
            do
            {
                if (_TableSync.Stopping)
                {
                    _DocForInsertResult = new List<Document>();
                    return;
                }

                while (!AlreadyGet)
                {
                    if (_TableSync.Stopping)
                    {
                        _DocForInsertResult = new List<Document>();
                        return;
                    }

                    System.Threading.Thread.Sleep(50);
                }

                try
                {
                    _DocForInsertResult = GetDocumentsForInsert((IDBAdapter)dbAdapter, ref _LastFrom);
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog("Get documents for insert fail!", e);
                    _DocForInsertResult = new List<Document>();
                }
                ReadyToGet = true;

            } while (_DocForInsertResult.Count > 0);
        }

        private List<Document> GetDocumentsForInsertInvoke(IDBAdapter dbAdapter, ref long from)
        {
            if (_GetDocForInsertThread == null)
            {
                _GetDocForInsertThread = new System.Threading.Thread(DoGetDocumentsForInsertAsync);
                _GetDocForInsertThread.IsBackground = true;
                _LastFrom = from;
                _GetDocForInsertThread.Start(dbAdapter);
            }

            while (!ReadyToGet)
            {
                System.Threading.Thread.Sleep(50);
            }

            from = _LastFrom;
            List<Document> result = _DocForInsertResult;
            AlreadyGet = true;

            return result;
        }

        private List<Document> GetDocumentsForInsert(IDBAdapter dbAdapter, ref long from)
        {
            List<Document> result = new List<Document>();

            System.Data.DataSet ds = dbAdapter.QuerySql(GetSelectSql(from));

            StringBuilder sb = new StringBuilder();

            foreach (System.Data.DataRow row in ds.Tables[0].Rows)
            {
                result.Add(GetOneRowDocument(row));
                from = long.Parse(row[_DBProvider.Table.DocIdReplaceField].ToString());
            }

            return result;
        }

        private void SychronizeForDelete(IDBAdapter dbAdapter)
        {
            List<Document> result = new List<Document>();

            long serial = -1;
            int count;
            do
            {
                string sql = GetTriggleSql(serial, 5000, "Delete");
                System.Data.DataSet ds = dbAdapter.QuerySql(sql);
                count = ds.Tables[0].Rows.Count;

                if (count > 0)
                {
                    List<int> docs = new List<int>(5000);

                    foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                    {
                        long id = long.Parse(row["id"].ToString());

                        int docid = _DBProvider.GetDocIdFromDocIdReplaceFieldValue(id);

                        if (docid >= 0)
                        {
                            docs.Add(docid);
                        }

                        serial = long.Parse(row["Serial"].ToString());
                    }

                    _DBProvider.Delete(docs);

                    string deleteSql = string.Format("delete from {0} where Serial <= {1} and Opr = 'Delete'",
                        _DBProvider.Table.TriggerTableName, serial);

                    dbAdapter.ExcuteSql(deleteSql);
                }

            } while (count > 0);
        }

        private void BatchAddToUpdate(System.Data.DataTable table,
            List<SFQL.Parse.SFQLParse.UpdateEntity> updateEntityList)
        {
            List<List<FieldValue>> docValues = new List<List<FieldValue>>();
            List<Query.DocumentResultForSort> docs = new List<Hubble.Core.Query.DocumentResultForSort>();

            foreach(System.Data.DataRow row in table.Rows)
            {
                long id = long.Parse(row[0].ToString());

                int docid = _DBProvider.GetDocIdFromDocIdReplaceFieldValue(id);

                //Get field values
                List<FieldValue> fieldValues = new List<FieldValue>();

                System.Data.DataRow vRow = row;

                for(int i = 1; i < table.Columns.Count; i++)
                {
                    string field = table.Columns[i].ColumnName;

                    if (string.IsNullOrEmpty(field))
                    {
                        continue;
                    }

                    FieldValue fv;

                    if (vRow[field] == DBNull.Value)
                    {
                        fv = new FieldValue(field, null);
                    }
                    else
                    {
                        if (vRow[field] is DateTime)
                        {
                            fv = new FieldValue(field, ((DateTime)vRow[field]).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        }
                        else
                        {
                            fv = new FieldValue(field, vRow[field].ToString());
                        }
                    }

                    fieldValues.Add(fv);
                }

                docValues.Add(fieldValues);
                Query.DocumentResultForSort doc = new Hubble.Core.Query.DocumentResultForSort(docid);
                doc.Score = id; //use score store id casually.

                docs.Add(doc);

                //_DBProvider.Update(fieldValues, docs);
            }

            if (docValues.Count > 0)
            {
                updateEntityList.Add(new Hubble.Core.SFQL.Parse.SFQLParse.UpdateEntity(
                    docValues[0], docValues, docs.ToArray()));
            }
            else
            {
                return;
            }
        }

        private string BuildSelectSql(string sql, List<long> docs)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");

            for(int i = 0; i < docs.Count; i++)
            {
                if (i == 0)
                {
                    sb.AppendFormat("{0}", docs[i]);
                }
                else
                {
                    sb.AppendFormat(",{0}", docs[i]);
                }
            }

            sb.Append(")");

            return sql + sb.ToString();
        }

        private void SychronizeForUpdate(IDBAdapter dbAdapter)
        {
            long serial = -1;
            int count;


            System.Data.DataSet totalCountDS = dbAdapter.QuerySql(
                string.Format("select count(*) from {0} where Opr = 'Update'",
                _DBProvider.Table.TriggerTableName));


            int totalCount = int.Parse(totalCountDS.Tables[0].Rows[0][0].ToString());

            Global.Report.WriteAppLog(string.Format("SynchronizeForUpdate, update total count = {0} table={1}",
                totalCount, _DBProvider.TableName));

            
            int doCount = 0;

            do
            {
                string sql = GetTriggleSql(serial, 2500, "Update");
                System.Data.DataSet ds = dbAdapter.QuerySql(sql);

                count = ds.Tables[0].Rows.Count;

                if (count > 0)
                {
                    Global.Report.WriteAppLog(string.Format("SynchronizeForUpdate, update count = {0}, table={1}",
                        count, _DBProvider.TableName));

                    doCount += count;

                    List<SFQL.Parse.SFQLParse.UpdateEntity> updateEntityList = new List<SFQL.Parse.SFQLParse.UpdateEntity>();

                    string lastSql = null;
                    List<long> ids = new List<long>();

                    foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                    {
                        long id = long.Parse(row["id"].ToString());

                        int docid = _DBProvider.GetDocIdFromDocIdReplaceFieldValue(id);


                        serial = long.Parse(row["Serial"].ToString());

                        if (docid >= 0)
                        {
                            List<FieldValue> fieldValues = new List<FieldValue>();

                            string fields = row["Fields"].ToString();

                            //Get values updated of this id
                            
                            string fieldsSQL = fields.Trim();

                            if (fields.EndsWith(","))
                            {
                                fieldsSQL = fields.Substring(0, fields.Length - 1);
                            }

                            Synchronize.GenerateSelectIDSql generateSelIdSql = new Synchronize.GenerateSelectIDSql(
                                _DBProvider.DocIdReplaceField, _DBProvider.Table.DBAdapterTypeName, fieldsSQL,
                                _DBProvider.Table.DBTableName, _Flags);


                            StringBuilder sb = generateSelIdSql.GetSql();

                            //sb.Append("Select ");

                            //if ((_Flags & SyncFlags.NoLock) != 0)
                            //{
                            //    sb.AppendFormat("{0}, {1} from {2} with(nolock) where {0} in ", _DBProvider.DocIdReplaceField,
                            //        fieldsSQL, _DBProvider.Table.DBTableName);
                            //    // Mark here. Will add a option.
                            //}
                            //else
                            //{
                            //    sb.AppendFormat("{0}, {1} from {2} where {0} in ", _DBProvider.DocIdReplaceField,
                            //        fieldsSQL, _DBProvider.Table.DBTableName);
                            //}

                            if (lastSql == null)
                            {
                                ids.Add(id);
                                lastSql = sb.ToString();
                                continue;
                            }
                            else
                            {
                                if (lastSql == sb.ToString())
                                {
                                    ids.Add(id);
                                    continue;
                                }
                            }

                            Global.Report.WriteAppLog(string.Format("SynchronizeForUpdate, get update details, count = {0}, table={1}",
                                ids.Count, _DBProvider.TableName));

                            System.Data.DataSet vDs = dbAdapter.QuerySql(BuildSelectSql(lastSql, ids));

                            lastSql = sb.ToString();
                            ids.Clear();
                            ids.Add(id);

                            BatchAddToUpdate(vDs.Tables[0], updateEntityList);
                        }

                    }

                    if (lastSql != null)
                    {
                        Global.Report.WriteAppLog(string.Format("SynchronizeForUpdate, get update details, count = {0}, table={1}",
                            ids.Count, _DBProvider.TableName));

                        System.Data.DataSet vDs = dbAdapter.QuerySql(BuildSelectSql(lastSql, ids));

                        Global.Report.WriteAppLog("SynchronizeForUpdate,finish getting update details");

                        BatchAddToUpdate(vDs.Tables[0], updateEntityList);
                    }

                    _DBProvider.Update(updateEntityList);

                    _TableSync.SetProgress(70 + 20 * (double)doCount / (double)totalCount);

                    string deleteSql = string.Format("delete from {0} where Serial <= {1} and Opr = 'Update'",
                        _DBProvider.Table.TriggerTableName, serial);

                    dbAdapter.ExcuteSql(deleteSql);
                }

            } while (count > 0);
        }

        private double GetMergeRate()
        {
            double totalRate = 0;
            int count = 0;

            foreach (Index.InvertedIndex index in _DBProvider.GetInvertedIndexes())
            {
                double rate = index.MergeRate;

                if (rate < 0)
                {
                    rate = 1;
                }

                totalRate += rate * 100;
                count++;
            }

            if (count == 0)
            {
                return 100;
            }

            return totalRate / count;
        }

        private void DoOptimize(OptimizationOption optimization, bool showProgress)
        {
            _DBProvider.Optimize(optimization);

            int times = 0;

            while (times++ < 1800) //Time out is 3600 s
            {
                double progress = GetMergeRate();

                if (progress >= 100)
                {
                    if (times == 1)
                    {
                        System.Threading.Thread.Sleep(200);

                        progress = GetMergeRate();

                        if (progress >= 100)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (showProgress)
                {
                    _TableSync.SetProgress(90 + progress * 10 / 100);
                }

                System.Threading.Thread.Sleep(2000);
            }
        }


        public SynchronizeCanUpdate(TableSynchronize tableSync, DBProvider dbProvider, int step, 
            OptimizationOption option, bool fastestMode, Service.SyncFlags flags)
        {
            _TableSync = tableSync;
            _DBProvider = dbProvider;
            _Step = step;
            _OptimizeOption = option;
            _FastestMode = fastestMode;
            _Flags = flags;
        }

        private void UpdateLastId(long lastId, DBAdapter.IDBAdapter dbAdapter)
        {
            string sql = string.Format("update {0} set id = {1} where Opr = 'Insert'",
                _DBProvider.Table.TriggerTableName, lastId);
            dbAdapter.ExcuteSql(sql);
        }

        private long GetLastId(int lastDocId, DBAdapter.IDBAdapter dbAdapter)
        {
            if ((_Flags & SyncFlags.Rebuild) != 0)
            {
                //Rebuild 
                return long.MaxValue;
            }

            string sql = string.Format("select id from {0} where Opr = 'Insert'",
                _DBProvider.Table.TriggerTableName);

            System.Data.DataSet ds = dbAdapter.QuerySql(sql);

            if (ds.Tables[0].Rows.Count <= 0)
            {
                long from = -1;

                while (lastDocId > 0)
                {
                    from = _DBProvider.GetDocIdReplaceFieldValue(lastDocId);

                    if (from != long.MaxValue)
                    {
                        break;
                    }

                    lastDocId--;
                }

                if (from < 0)
                {
                    from = _DBProvider.GetDocIdReplaceFieldValue(lastDocId);
                }

                if (from == long.MaxValue)
                {
                    sql = string.Format("insert into {0} (id, Opr, Fields) values(-1, 'Insert', '')",
                        _DBProvider.Table.TriggerTableName);
                }
                else
                {
                    sql = string.Format("insert into {0} (id, Opr, Fields) values({1}, 'Insert', '')",
                        _DBProvider.Table.TriggerTableName, from);
                }
                dbAdapter.ExcuteSql(sql);
                return from;
            }
            else
            {
                long lastId = long.Parse(ds.Tables[0].Rows[0][0].ToString());

                if (lastId < 0)
                {
                    return long.MaxValue;
                }
                else
                {
                    return lastId;
                }
            }
        }

        private int GetRemainCount(long from, DBAdapter.IDBAdapter dbAdapter)
        {
            string dbAdapterName = (dbAdapter as INamedExternalReference).Name;
            _HasInsertCount = true;

            if (dbAdapterName.IndexOf("sqlserver", 0, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                //For sql server

                string sql = string.Format("select rows from sysindexes where id = object_id('{0}') and indid in (0,1)",
                    _DBProvider.Table.DBTableName.Replace("'", "''"));

                System.Data.DataSet ds = null;
              
                try
                {
                    ds = dbAdapter.QuerySql(sql);
                }
                catch
                {
                    ds = null;
                }
                
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return int.Parse(ds.Tables[0].Rows[0][0].ToString()) - 
                            _DBProvider.DocumentCount;
                    }
                }
            }

            //For others
            {
                string sql = string.Format("select count(*) from {0} where {2} > {1}",
                   _DBProvider.Table.DBTableName, from, _DBProvider.Table.DocIdReplaceField);

                System.Data.DataSet ds = null;
                int times = 0;

                if (!_FastestMode)
                {
                    while (times < 3)
                    {

                        try
                        {
                            ds = dbAdapter.QuerySql(sql);
                            break;
                        }
                        catch
                        {
                            ds = null;
                            times++;
                        }
                    }
                }

                int count;

                if (ds == null)
                {
                    count = 50000000;
                    _HasInsertCount = false;
                }
                else
                {
                    count = int.Parse(ds.Tables[0].Rows[0][0].ToString());
                }

                return count;
            }
        }


        public void Do()
        {
            DBAdapter.IDBAdapter dbAdapter = (DBAdapter.IDBAdapter)Hubble.Framework.Reflection.Instance.CreateInstance(
                Data.DBProvider.GetDBAdapter(_DBProvider.Table.DBAdapterTypeName));

            int lastDocId = _DBProvider.LastDocIdForIndexOnly;

            dbAdapter.Table = _DBProvider.Table;

            if (lastDocId <= 0 && _DBProvider.DocumentCount <= 0 && (_Flags & SyncFlags.Rebuild) == 0)
            {
                //first time synchronize
                //Truncate trigger table

                dbAdapter.Truncate(_DBProvider.Table.TriggerTableName);
            }

            long from = GetLastId(lastDocId, dbAdapter);

            if (from == long.MaxValue)
            {
                from = -1;
            }

            //Synchronize for delete
            if ((_Flags & SyncFlags.Delete) != 0)
            {
                SychronizeForDelete(dbAdapter);
            }

            _TableSync.SetProgress(5);

           
            int count = GetRemainCount(from, dbAdapter);
           

            _TableSync.SetProgress(10);

            //Synchronize for insert

            if ((_Flags & SyncFlags.Insert) != 0)
            {
                int insertCount = 0;

                Global.Report.WriteAppLog(string.Format("SynchronizeCanUpdate Insert count = {0}, table={1}",
                    count, _DBProvider.TableName));

                while (true)
                {
                    if (_TableSync.Stopping)
                    {
                        _TableSync.SetProgress(-1, insertCount);
                        _TableSync.ResetStopping();

                        try
                        {
                            if (_GetDocForInsertThread != null)
                            {
                                _GetDocForInsertThread.Abort();
                            }
                        }
                        catch
                        {
                        }

                        return;
                    }

                    Global.Report.WriteAppLog(string.Format("SynchronizeCanUpdate GetDocumentsForInsert, from = {0}, table={1}",
                        from, _DBProvider.TableName));

                    List<Document> documents = GetDocumentsForInsertInvoke(dbAdapter, ref from);

                    if (documents.Count == 0)
                    {
                        //Global.Report.WriteAppLog(string.Format("Exit before finish when synchronized {0}. Some records deleted during synchronization. insertCount={1} count={2}",
                        //    _DBProvider.Table.Name, insertCount, count));
                        
                        _TableSync.SetProgress(70, insertCount);
                        break;
                    }

                    insertCount += documents.Count;

                    Global.Report.WriteAppLog(string.Format("SynchronizeCanUpdate Insert to DBProvider, count = {0}, table={1}",
                        documents.Count, _DBProvider.TableName));

                    _DBProvider.Insert(documents);

                    if ((_Flags & SyncFlags.Rebuild) == 0)
                    {
                        UpdateLastId(from, dbAdapter);
                    }

                    double progress;

                    if (!_HasInsertCount)
                    {
                        progress = (double)((insertCount % 100000) * 60) / (double)100000;
                    }
                    else
                    {
                        progress = (double)insertCount * 60 / (double)count;
                    }

                    if (progress > 60)
                    {
                        progress = 60;
                    }

                    _TableSync.SetProgress(10 + progress, insertCount);

                    if (_DBProvider.TooManyIndexFiles)
                    {
                        Global.Report.WriteAppLog(string.Format("Begin optimize:type={0}, table={1}",
                            OptimizationOption.Speedy, _DBProvider.TableName));

                        DoOptimize(OptimizationOption.Speedy, false);
                        DoOptimize(OptimizationOption.Speedy, false);

                        Global.Report.WriteAppLog(string.Format("Finish optimize:type={0}, table={1}",
                            OptimizationOption.Speedy, _DBProvider.TableName));

                    }
                }
            }

            _TableSync.SetProgress(70);

            //Synchronize for update
            if ((_Flags & SyncFlags.Update) != 0)
            {
                SychronizeForUpdate(dbAdapter);
            }

            if (_OptimizeOption != OptimizationOption.Idle)
            {
                Global.Report.WriteAppLog(string.Format("Begin optimize1:type={0}, table={1}",
                    _OptimizeOption, _DBProvider.TableName));

                DoOptimize(_OptimizeOption, true);

                Global.Report.WriteAppLog(string.Format("Begin optimize2:type={0}, table={1}",
                    _OptimizeOption, _DBProvider.TableName));

                DoOptimize(_OptimizeOption, true);
            }

            Global.Report.WriteAppLog(string.Format("Finish Synchronize optimize:type={0}, table={1}",
                _OptimizeOption, _DBProvider.TableName));

        }

    }
}
