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

using Hubble.Core.SFQL.LexicalAnalysis;
using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Core.Data;
using Hubble.Framework.DataStructure;

using Hubble.SQLClient;

namespace Hubble.Core.SFQL.Parse
{
    public class SFQLParse
    {
        List<TSFQLSentence> _SFQLSentenceList;
        TSFQLSentence _SFQLSentence;
        private bool _NeedCollect = false;
        private Dictionary<string, List<Document>> _InsertTables = new Dictionary<string, List<Document>>();

        private void InputLexicalToken(Lexical.Token token)
        {
            if (token.SyntaxType == SyntaxType.Space)
            {
                return;
            }

            if (_SFQLSentence == null)
            {
                _SFQLSentence = new TSFQLSentence();
            }

            DFAResult result = _SFQLSentence.Input((int)token.SyntaxType, token);

            switch (result)
            {
                case DFAResult.Quit:
                case DFAResult.ElseQuit:
                    _SFQLSentenceList.Add(_SFQLSentence);
                    _SFQLSentence = null;
                    break;
            }

        }

        private void SyntaxAnalyse(string text)
        {
            Lexical lexical = new Lexical(text);

            DFAResult dfaResult;

            for (int i = 0; i < text.Length; i++)
            {
                dfaResult = lexical.Input(text[i], i);

                switch (dfaResult)
                {
                    case DFAResult.Continue:
                        continue;
                    case DFAResult.Quit:
                        InputLexicalToken(lexical.OutputToken);
                        break;
                    case DFAResult.ElseQuit:
                        InputLexicalToken(lexical.OutputToken);
                        i--;
                        break;
                }

            }


            dfaResult = lexical.Input(0, text.Length);

            switch (dfaResult)
            {
                case DFAResult.Continue:
                    throw new Hubble.Core.SFQL.LexicalAnalysis.LexicalException("Lexical abort at the end of sql");
                case DFAResult.Quit:
                    InputLexicalToken(lexical.OutputToken);
                    break;
                case DFAResult.ElseQuit:
                    InputLexicalToken(lexical.OutputToken);
                    break;
            }

            InputLexicalToken(new Lexical.Token());

        }

        private QueryResult ExcuteExec(TSFQLSentence sentence)
        {
            SyntaxAnalysis.Exec.Exec exec = sentence.SyntaxEntity as
                SyntaxAnalysis.Exec.Exec;
            StoredProcedure.IStoredProc sp = DBProvider.GetStoredProc(exec.StoredProcedureName);

            if (sp == null)
            {
                throw new ParseException(string.Format("Stored procedure {0} does not exist.",
                    exec.StoredProcedureName));
            }

            foreach (SyntaxAnalysis.Exec.ExecParameter para in exec.ExecParameters)
            {
                sp.Parameters.Add(para.Value);
            }

            sp.Run();

            return sp.Result;
        }



        private QueryResult ExcuteUpdate(TSFQLSentence sentence)
        {
            SyntaxAnalysis.Update.Update update = sentence.SyntaxEntity as
                SyntaxAnalysis.Update.Update;

            ParseWhere parseWhere = new ParseWhere(update.TableName);

            parseWhere.Begin = update.Begin;
            parseWhere.End = update.End;
            Query.DocumentResult[] result;

            if (update.Where == null)
            {
                result = parseWhere.Parse(null);
            }
            else
            {
                result = parseWhere.Parse(update.Where.ExpressionTree);
            }

            Data.DBProvider dBProvider = Data.DBProvider.GetDBProvider(update.TableName);

            if (dBProvider == null)
            {
                throw new DataException(string.Format("Table: {0} does not exist!", update.TableName));
            }

            List<Data.FieldValue> fieldValues = new List<Hubble.Core.Data.FieldValue>();

            foreach (SyntaxAnalysis.Update.UpdateField field in update.Fields)
            {
                fieldValues.Add(new Hubble.Core.Data.FieldValue(field.Name, field.Value)); 
            }

            dBProvider.Update(fieldValues, result);

            QueryResult qresult = new QueryResult();

            qresult.PrintMessages.Add(string.Format("({0} Row(s) affected)", result.Length));

            return qresult;

        }

        private QueryResult ExcuteDelete(TSFQLSentence sentence)
        {
            SyntaxAnalysis.Delete.Delete delete = sentence.SyntaxEntity as
                SyntaxAnalysis.Delete.Delete;

            ParseWhere parseWhere = new ParseWhere(delete.TableName);

            parseWhere.Begin = delete.Begin;
            parseWhere.End = delete.End;
            Query.DocumentResult[] result;

            if (delete.Where == null)
            {
                result = parseWhere.Parse(null);
            }
            else
            {
                result = parseWhere.Parse(delete.Where.ExpressionTree);
            }

            Data.DBProvider dBProvider = Data.DBProvider.GetDBProvider(delete.TableName);

            if (dBProvider == null)
            {
                throw new DataException(string.Format("Table: {0} does not exist!", delete.TableName));
            }

            dBProvider.Delete(result);

            QueryResult qresult = new QueryResult();

            qresult.PrintMessages.Add(string.Format("({0} Row(s) affected)", result.Length));

            return qresult;
        }

        private void ExcuteInsert(TSFQLSentence sentence)
        {
            Document document = new Document();

            SyntaxAnalysis.Insert.Insert insert = sentence.SyntaxEntity as SyntaxAnalysis.Insert.Insert;

            string tableName = insert.TableName.ToLower().Trim();
            
            Data.DBProvider dBProvider =  Data.DBProvider.GetDBProvider(tableName);

            if (dBProvider == null)
            {
                throw new DataException(string.Format("Table: {0} does not exist!", tableName));
            }

            if (insert.Fields.Count == 0)
            {
                List<Field> fields = dBProvider.GetAllFields();

                if (dBProvider.IndexOnly)
                {
                    SyntaxAnalysis.Insert.InsertField insertField = new SyntaxAnalysis.Insert.InsertField();
                    insertField.Name = "docid";
                    insert.Fields.Add(insertField);
                }

                foreach(Field field in fields)
                {
                    SyntaxAnalysis.Insert.InsertField insertField = new SyntaxAnalysis.Insert.InsertField();
                    insertField.Name = field.Name;
                    insert.Fields.Add(insertField);
                }


            }



            if (insert.Fields.Count > insert.Values.Count)
            {
                throw new DataException("There are more columns in the INSERT statement than values specified in the VALUES clause. The number of values in the VALUES clause must match the number of columns specified in the INSERT statement.");
            }
            else if (insert.Fields.Count < insert.Values.Count)
            {
                throw new DataException("There are fewer columns in the INSERT statement than values specified in the VALUES clause. The number of values in the VALUES clause must match the number of columns specified in the INSERT statement.");
            }


            for(int i = 0; i < insert.Fields.Count; i++)
            {
                if (insert.Fields[i].Name.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    document.DocId = long.Parse(insert.Values[i].Value);
                }
                else
                {
                    document.FieldValues.Add(new FieldValue(insert.Fields[i].Name, insert.Values[i].Value));
                }
            }

            List<Document> docs;
            if (!_InsertTables.TryGetValue(tableName, out docs))
            {
                docs = new List<Document>();
                _InsertTables.Add(tableName, docs);
            }

            docs.Add(document);
        }

        private QueryResult CreateTable(TSFQLSentence sentence)
        {
            Table table = new Table();

            string directory = null;

            SyntaxAnalysis.CreateTable.CreateTable createtable = sentence.SyntaxEntity as
                SyntaxAnalysis.CreateTable.CreateTable;

            table.Name = createtable.TableName;

            //Read attributes

            foreach (TSFQLAttribute attr in sentence.Attributes)
            {
                string attrName = attr.Name.ToLower();

                switch (attrName)
                {
                    case "directory":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("Directory attribute must have one parameter!");
                        }

                        directory = attr.Parameters[0];
                        if (!System.IO.Directory.Exists(directory))
                        {
                            System.IO.Directory.CreateDirectory(directory);
                        }
                        break;
                    case "indexonly":
                        table.IndexOnly = true;
                        break;
                    case "forcecollectcount":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("ForceCollectCount attribute must have one parameter!");
                        }

                        int count;

                        if (!int.TryParse(attr.Parameters[0], out count))
                        {
                            throw new ParseException(string.Format("Invalid count ={0} in ForceCollectCount attribute!", attr.Parameters[0]));
                        }

                        if (count <= 0)
                        {
                            throw new ParseException("ForceCollectCount must be large then 0!");
                        }

                        table.ForceCollectCount = count;
                        break;
                    case "dbtablename":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("DBTableName attribute must have one parameter!");
                        }

                        table.DBTableName = attr.Parameters[0];

                        break;
                    case "dbadapter":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("DBAdapter attribute must have one parameter!");
                        }

                        table.DBAdapterTypeName = attr.Parameters[0];
                        break;

                    case "dbconnect":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("DBConnect attribute must have one parameter!");
                        }

                        table.ConnectionString = attr.Parameters[0];
                        break;
                }
            }

            Global.Database database = null;

            //Verify parameters
            if (directory == null)
            {
                if (database == null)
                {
                    string curDatabaseName = Service.CurrentConnection.ConnectionInfo.DatabaseName;
                    database = Global.Setting.GetDatabase(curDatabaseName);
                }

                if (string.IsNullOrEmpty(database.DefaultPath))
                {
                    throw new ParseException("Hasn't Directory attribute!");
                }
                else
                {
                    directory = Hubble.Framework.IO.Path.AppendDivision(database.DefaultPath, '\\') + 
                        table.Name + @"\";
                }
            }

            if (table.DBAdapterTypeName == null)
            {
                if (database == null)
                {
                    string curDatabaseName = Service.CurrentConnection.ConnectionInfo.DatabaseName;
                    database = Global.Setting.GetDatabase(curDatabaseName);
                }

                if (string.IsNullOrEmpty(database.DefaultDBAdapter))
                {
                    throw new ParseException("Hasn't DBAdapter attribute!");
                }
                else
                {
                    table.DBAdapterTypeName = database.DefaultDBAdapter;
                }

                
            }

            if (table.ConnectionString == null)
            {
                if (database == null)
                {
                    string curDatabaseName = Service.CurrentConnection.ConnectionInfo.DatabaseName;
                    database = Global.Setting.GetDatabase(curDatabaseName);
                }

                if (string.IsNullOrEmpty(database.DefaultConnectionString))
                {
                    throw new ParseException("Hasn't DBConnect attribute!");
                }
                else
                {
                    table.ConnectionString = database.DefaultConnectionString;
                }
            }

            //Init fields

            if (createtable.Fields.Count <= 0)
            {
                throw new ParseException("Table must have one field at least!");
            }

            foreach (SyntaxAnalysis.CreateTable.CreateTableField tfield in createtable.Fields)
            {
                Data.Field field = new Field();

                field.Name = tfield.FieldName;
                field.Mode = Field.IndexMode.Complex;
                field.IndexType = tfield.IndexType;
                field.Mode = tfield.IndexMode;
                field.Store = true;
                field.AnalyzerName = tfield.AnalyzerName;
                field.CanNull = tfield.CanNull;
                field.DataLength = tfield.DataLength;
                field.DataType = tfield.DataType;
                field.DefaultValue = tfield.Default;
                field.PrimaryKey = tfield.PrimaryKey;

                if (field.Name.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ParseException("DocId can not be field name!");
                }

                if (field.IndexType != Field.Index.None && field.CanNull)
                {
                    throw new ParseException("Can not be NULL when index type is Tokenized or Untokenized!");
                }

                if (string.IsNullOrEmpty(field.AnalyzerName) && field.IndexType == Field.Index.Tokenized)
                {
                    throw new ParseException("Must set analyzer name when index type is Tokenized!");
                }

                table.Fields.Add(field);
            }

            if (table.DBTableName == null)
            {
                table.DBTableName = table.Name;
            }

            DBProvider.CreateTable(table, directory);

            QueryResult result = new QueryResult();
            result.PrintMessages.Add(string.Format("create table {0} successful!", table.Name));

            return result;
        }

        private string GetWhereSql(SyntaxAnalysis.Select.Select select)
        {
            StringBuilder sb = new StringBuilder();

            if (select.Where != null)
            {
                sb.Append(select.Where.ToString());
            }

            sb.Append(" ");

            if (select.OrderBys != null)
            {
                foreach (Hubble.Core.SFQL.SyntaxAnalysis.Select.OrderBy orderby in select.OrderBys)
                {
                    sb.AppendFormat("order by {0} {1} ", orderby.Name, orderby.Order);
                }
            }

            return sb.ToString();
        }

        private QueryResult ExcuteSelect(TSFQLSentence sentence)
        {
            QueryResult qResult = new QueryResult();

            SyntaxAnalysis.Select.Select select = sentence.SyntaxEntity as
                SyntaxAnalysis.Select.Select;

            string tableName = select.SelectFroms[0].Name;

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new DataException(string.Format("Table: {0} does not exist!", select.SelectFroms[0].Name));
            }

            long lastModifyTicks = dbProvider.LastModifyTicks;

            //Process data cache
            if (Service.CurrentConnection.ConnectionInfo.CurrentCommandContent != null)
            {
                if (Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.NeedDataCache)
                {

                    if (dbProvider != null)
                    {
                        long datacacheTicks = Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.DataCache.GetTicks(
                            dbProvider.TableName);

                        qResult.PrintMessages.Add(string.Format(@"<TableTicks>{0}={1};</TableTicks>",
                            dbProvider.TableName, lastModifyTicks));

                        if (lastModifyTicks <= datacacheTicks)
                        {
                            System.Data.DataTable table = new System.Data.DataTable();
                            table.MinimumCapacity = int.MaxValue;
                            qResult.DataSet.Tables.Add(table);
                            return qResult;
                        }
                    }
                }
            }

            //Begin to select

            ParseWhere parseWhere = new ParseWhere(tableName);

            parseWhere.Begin = select.Begin;
            parseWhere.End = select.End;
            Query.DocumentResult[] result;

            //Query cache
            Cache.QueryCache queryCache = null;
            string whereSql = "";
            bool noQueryCache = true;
            Cache.QueryCacheDocuments qDocs = null;

            if (dbProvider.QueryCacheEnabled)
            {
                queryCache = dbProvider.QueryCache;
            }

            if (queryCache != null)
            {
                whereSql = GetWhereSql(select);
                
                DateTime expireTime;
                int hitCount;
                object tag;

                if (queryCache.TryGetValue(whereSql, out qDocs, out expireTime, out hitCount, out tag))
                {
                    Cache.QueryCacheInformation qInfo = (Cache.QueryCacheInformation)tag;

                    if ((qInfo.Count >= select.End + 1 && select.End  >= 0) || (qInfo.All && select.End < 0))
                    {
                        if (DateTime.Now <= expireTime)
                        {
                            noQueryCache = false;
                        }
                        else
                        {
                            if (qInfo.CacheTicks >= lastModifyTicks)
                            {
                                noQueryCache = false;
                            }
                        }
                    }
                }
            }

            //Get document result
            if (noQueryCache)
            {
                if (select.Where == null)
                {
                    result = parseWhere.Parse(null);
                }
                else
                {
                    result = parseWhere.Parse(select.Where.ExpressionTree);
                }

                //Sort
                //Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(select.SelectFroms[0].Name);

                //Document rank
                //If has rank field and datatype is int32, do document rank

                Data.Field rankField = dbProvider.GetField("Rank");

                if (rankField != null)
                {
                    if (rankField.DataType == Hubble.Core.Data.DataType.Int &&
                        rankField.IndexType == Hubble.Core.Data.Field.Index.Untokenized)
                    {
                        int rankTab = rankField.TabIndex;

                        foreach (Query.DocumentResult dr in result)
                        {
                            int docRank = dbProvider.GetPayload(dr.DocId).Data[rankTab];
                            if (docRank < 0)
                            {
                                docRank = 0;
                            }
                            else if (docRank > 65535)
                            {
                                docRank = 65535;
                            }

                            dr.Score *= docRank;
                        }
                    }
                }

                QueryResultSort qSort = new QueryResultSort(select.OrderBys, dbProvider);

                //qSort.Sort(result);
                qSort.Sort(result, select.End + 1); // using part quick sort can reduce performance 40%

                if (queryCache != null)
                {
                    int count;

                    if (select.End < 0)
                    {
                        count = result.Length;
                    }
                    else
                    {
                        count = Math.Min(result.Length, select.End + 1);
                    }

                    queryCache.Insert(whereSql,
                        new Hubble.Core.Cache.QueryCacheDocuments(count, result),
                        DateTime.Now.AddSeconds(dbProvider.QueryCacheTimeout),
                        new Cache.QueryCacheInformation(count, lastModifyTicks, select.End < 0));
                }

            }
            else
            {
                result = qDocs.Documents;
            }

            List<Data.Field> selectFields = new List<Data.Field>();

            int allFieldsCount = 0;

            foreach (SyntaxAnalysis.Select.SelectField selectField in select.SelectFields)
            {
                if (selectField.Name == "*")
                {
                    List<Data.Field> allFields = dbProvider.GetAllSelectFields();
                    selectFields.AddRange(allFields);
                    allFieldsCount += allFields.Count;
                    continue;
                }

                Data.Field field = dbProvider.GetField(selectField.Name);

                if (field == null)
                {

                    if (selectField.Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                    {
                        selectFields.Add(new Data.Field("DocId", Hubble.Core.Data.DataType.BigInt));
                    }
                    else if (selectField.Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                    {
                        selectFields.Add(new Data.Field("Score", Hubble.Core.Data.DataType.BigInt));
                    }
                    else
                    {
                        throw new ParseException(string.Format("Unknown field name:{0}", selectField.Name));
                    }
                }
                else
                {
                    selectFields.Add(field);
                }
            }

            List<Data.Document> docResult = dbProvider.Query(selectFields, result, select.Begin, select.End);
            System.Data.DataSet ds = Data.Document.ToDataSet(selectFields, docResult);
            ds.Tables[0].TableName = select.SelectFroms[0].Name;

            for (int i = 0; i < select.SelectFields.Count; i++)
            {
                if (select.SelectFields[i].Name == "*")
                {
                    continue;
                }

                ds.Tables[0].Columns[i].ColumnName = select.SelectFields[i + allFieldsCount].Alias;
            }

            if (noQueryCache)
            {
                ds.Tables[0].MinimumCapacity = result.Length;
            }
            else
            {
                ds.Tables[0].MinimumCapacity = qDocs.ResultLength;
            }

            qResult.DataSet = ds;

            return qResult;
        }

        private QueryResult ExecuteTSFQLSentence(TSFQLSentence sentence)
        {
            ParseOptimize pOptimize = new ParseOptimize();
            sentence = pOptimize.Optimize(sentence);

            switch (sentence.SentenceType)
            {
                case SentenceType.SELECT:
                    return ExcuteSelect(sentence);
                case SentenceType.DELETE:
                    return ExcuteDelete(sentence);
                case SentenceType.UPDATE:
                    return ExcuteUpdate(sentence);
                case SentenceType.INSERT:
                    _NeedCollect = true;
                    ExcuteInsert(sentence);
                    break;
                case SentenceType.EXEC:
                    return ExcuteExec(sentence);
                case SentenceType.CREATETABLE:
                    return CreateTable(sentence);
            }

            return null;
        }

        public void ExecuteNonQuery(string sql)
        {
            Query(sql);
        }

        public QueryResult Query(string sql)
        {
            _NeedCollect = false;
            _InsertTables.Clear();

            _SFQLSentenceList = new List<TSFQLSentence>();

            QueryResult result = new QueryResult();

            SyntaxAnalyse(sql);
            int tableNum = 0;

            foreach (TSFQLSentence sentence in _SFQLSentenceList)
            {
                QueryResult queryResult = ExecuteTSFQLSentence(sentence);

                if (queryResult != null)
                {
                    List<System.Data.DataTable> tables = new List<System.Data.DataTable>();

                    foreach (System.Data.DataTable table in queryResult.DataSet.Tables)
                    {
                        tables.Add(table);
                    }

                    foreach (System.Data.DataTable table in tables)
                    {
                        queryResult.DataSet.Tables.Remove(table);
                    }

                    foreach (System.Data.DataTable table in tables)
                    {
                        table.TableName = "Table" + tableNum.ToString();
                        tableNum++;
                        result.DataSet.Tables.Add(table);
                    }

                    foreach (string message in queryResult.PrintMessages)
                    {
                        result.PrintMessages.Add(message);
                    }
                }
            }

            //Collect
            if (_NeedCollect)
            {
                _NeedCollect = false;
                int affectedCount = 0;

                foreach (string tableName in _InsertTables.Keys)
                {
                    DBProvider dbProvider = DBProvider.GetDBProvider(tableName);

                    List<Document> docs = _InsertTables[tableName];

                    affectedCount += docs.Count;

                    dbProvider.Insert(docs);
                    dbProvider.Collect();

                }

                result.PrintMessages.Add(string.Format("({0} Row(s) affected)", affectedCount));
            }


            return result;
        }
    }
}
