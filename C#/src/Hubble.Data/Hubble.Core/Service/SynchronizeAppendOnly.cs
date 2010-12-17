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
    class SynchronizeAppendOnly
    {
        TableSynchronize _TableSync;
        DBProvider _DBProvider;
        int _Step;
        OptimizationOption _OptimizeOption;

        private string GetSqlServerSelectSql(long from)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Select top {0} [DocId] ", _Step);

            foreach (Field field in _DBProvider.Table.Fields)
            {
                if (field.IndexType == Field.Index.None)
                {
                    continue;
                }

                sb.AppendFormat(", [{0}] ", field.Name);
            }

            sb.AppendFormat(" from {0} where DocId >= {1} order by DocId", _DBProvider.Table.DBTableName, from);

            return sb.ToString();
        }

        private string GetOracleFieldNameList(long from)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" DocId ");

            foreach (Field field in _DBProvider.Table.Fields)
            {
                if (field.IndexType == Field.Index.None)
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

            sb.AppendFormat("select {0} from {1} where DocId >= {2} and rownum <= {3} order by DocId ",
                fields, _DBProvider.Table.DBTableName, from, _Step);


            return sb.ToString();
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

        private Document GetOneRowDocument(System.Data.DataRow row)
        {
            Document document = new Document();

            document.DocId = int.Parse(row["DocId"].ToString());

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
                from = long.Parse(row["DocId"].ToString()) + 1;
            }

            return result;
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
                System.Threading.Thread.Sleep(2000);

                double progress = GetMergeRate();

                if (progress >= 100)
                {
                    break;
                }

                if (showProgress)
                {
                    _TableSync.SetProgress(90 + progress * 10 / 100);
                }

                System.Threading.Thread.Sleep(1000);
            }
        }


        public SynchronizeAppendOnly(TableSynchronize tableSync, DBProvider dbProvider, int step, OptimizationOption option)
        {
            _TableSync = tableSync;
            _DBProvider = dbProvider;
            _Step = step;
            _OptimizeOption = option;
        }

        public void Do()
        {
            long from = _DBProvider.LastDocIdForIndexOnly;

            DBAdapter.IDBAdapter dbAdapter = (DBAdapter.IDBAdapter)Hubble.Framework.Reflection.Instance.CreateInstance(
                Data.DBProvider.GetDBAdapter(_DBProvider.Table.DBAdapterTypeName));

            string sql = string.Format("select count(*) from {0} where docid >= {1}",
                _DBProvider.Table.DBTableName, from);

            dbAdapter.Table = _DBProvider.Table;

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

            _TableSync.SetProgress(10);

            int insertCount = 0;

            while (insertCount < count)
            {
                if (_TableSync.Stopping)
                {
                    _TableSync.SetProgress(-1);
                    _TableSync.ResetStopping();
                    return;
                }

                List<Document> documents = GetDocumentsForInsert(dbAdapter, ref from);

                if (documents.Count == 0)
                {
                    Global.Report.WriteAppLog(string.Format("Exit before finish when synchronized {0}. Some records deleted during synchronization. insertCount={1} count={2}",
                        _DBProvider.Table.Name, insertCount, count));
                    _TableSync.SetProgress(90);
                    break;
                }
                else
                {
                    insertCount += documents.Count;

                    Global.Report.WriteAppLog(string.Format("SynchronizeCanUpdate Insert to DBProvider, count = {0} table={1}",
                        documents.Count, _DBProvider.TableName));

                    _DBProvider.Insert(documents);

                    double progress = (double)insertCount * 80 / (double)count;

                    if (progress > 80)
                    {
                        progress = 80;
                    }

                    _TableSync.SetProgress(10 + progress);

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
