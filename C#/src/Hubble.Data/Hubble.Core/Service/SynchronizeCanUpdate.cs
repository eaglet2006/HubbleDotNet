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

            sb.AppendFormat(" from {0} where [{2}] > {1} order by [{2}]", _DBProvider.Table.DBTableName, from, _DBProvider.Table.DocIdReplaceField);

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
                    continue;
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
                        throw new DataException(string.Format("Field:{0} in table {1} is null but hasn't default value so that it can't be inserted null value!",
                            field.Name, _DBProvider.Table.Name));
                    }

                    value = field.DefaultValue;
                }
                else
                {
                    value = row[field.Name].ToString();
                }

                document.Add(field.Name, value, field.DataType, field.DataLength, false);
            }


            return document;
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

                    FieldValue fv = new FieldValue(field, vRow[field].ToString());

                    fieldValues.Add(fv);
                }

                Query.DocumentResultForSort doc = new Hubble.Core.Query.DocumentResultForSort(docid);
                List<Query.DocumentResultForSort> docs = new List<Hubble.Core.Query.DocumentResultForSort>();
                docs.Add(doc);

                updateEntityList.Add(new Hubble.Core.SFQL.Parse.SFQLParse.UpdateEntity(
                    fieldValues, docs.ToArray()));
                //_DBProvider.Update(fieldValues, docs);
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
                            StringBuilder sb = new StringBuilder();

                            sb.Append("Select ");
                            
                            string fieldsSQL = fields.Trim();

                            if (fields.EndsWith(","))
                            {
                                fieldsSQL = fields.Substring(0, fields.Length - 1);
                            }

                            sb.AppendFormat("{0}, {1} from {2} where {0} in ", _DBProvider.DocIdReplaceField,
                                fieldsSQL, _DBProvider.Table.DBTableName);

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
                    break;
                }

                if (showProgress)
                {
                    _TableSync.SetProgress(90 + progress * 10 / 100);
                }

                System.Threading.Thread.Sleep(2000);
            }
        }


        public SynchronizeCanUpdate(TableSynchronize tableSync, DBProvider dbProvider, int step, OptimizationOption option)
        {
            _TableSync = tableSync;
            _DBProvider = dbProvider;
            _Step = step;
            _OptimizeOption = option;
        }

        private void UpdateLastId(long lastId, DBAdapter.IDBAdapter dbAdapter)
        {
            string sql = string.Format("update {0} set id = {1} where Opr = 'Insert'",
                _DBProvider.Table.TriggerTableName, lastId);
            dbAdapter.ExcuteSql(sql);
        }

        private long GetLastId(int lastDocId, DBAdapter.IDBAdapter dbAdapter)
        {
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

        public void Do()
        {
            DBAdapter.IDBAdapter dbAdapter = (DBAdapter.IDBAdapter)Hubble.Framework.Reflection.Instance.CreateInstance(
                Data.DBProvider.GetDBAdapter(_DBProvider.Table.DBAdapterTypeName));

            int lastDocId = _DBProvider.LastDocIdForIndexOnly;

            dbAdapter.Table = _DBProvider.Table;

            long from = GetLastId(lastDocId, dbAdapter);

            if (from == long.MaxValue)
            {
                from = -1;
            }

            string sql = string.Format("select count(*) from {0} where {2} > {1}",
                _DBProvider.Table.DBTableName, from, _DBProvider.Table.DocIdReplaceField);

            System.Data.DataSet ds = null;
            int times = 0;
            
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

            int count;

            if (ds == null)
            {
                count = 50000000;
            }
            else
            {
                count = int.Parse(ds.Tables[0].Rows[0][0].ToString());
            }

            _TableSync.SetProgress(5);

            SychronizeForDelete(dbAdapter);

            _TableSync.SetProgress(10);

            int insertCount = 0;

            Global.Report.WriteAppLog(string.Format("SynchronizeCanUpdate Insert count = {0}, table={1}",
                count, _DBProvider.TableName));

            while (insertCount < count)
            {
                if (_TableSync.Stopping)
                {
                    _TableSync.SetProgress(-1);
                    _TableSync.ResetStopping();
                    return;
                }

                Global.Report.WriteAppLog(string.Format("SynchronizeCanUpdate GetDocumentsForInsert, from = {0}, table={1}",
                    from, _DBProvider.TableName));

                List<Document> documents = GetDocumentsForInsert(dbAdapter, ref from);

                if (documents.Count == 0)
                {
                    Global.Report.WriteAppLog(string.Format("Exit before finish when synchronized {0}. Some records deleted during synchronization. insertCount={1} count={2}",
                        _DBProvider.Table.Name, insertCount, count));
                    _TableSync.SetProgress(70);
                    break;
                }

                insertCount += documents.Count;

                Global.Report.WriteAppLog(string.Format("SynchronizeCanUpdate Insert to DBProvider, count = {0}, table={1}",
                    documents.Count, _DBProvider.TableName));

                _DBProvider.Insert(documents);

                UpdateLastId(from, dbAdapter);

                double progress = (double)insertCount * 60 / (double)count;

                if (progress > 60)
                {
                    progress = 60;
                }

                _TableSync.SetProgress(10 + progress );

                if (_DBProvider.TooManyIndexFiles)
                {
                    DoOptimize(OptimizationOption.Speedy, false);
                    DoOptimize(OptimizationOption.Speedy, false);
                }
            }

            _TableSync.SetProgress(70);

            SychronizeForUpdate(dbAdapter);

            DoOptimize(_OptimizeOption, true);
            DoOptimize(_OptimizeOption, true);
        }

    }
}
