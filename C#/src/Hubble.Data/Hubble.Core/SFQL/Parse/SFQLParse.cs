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
using Hubble.Core.Right;

namespace Hubble.Core.SFQL.Parse
{
    public class SFQLParse
    {
        internal class UpdateEntity
        {
            internal List<FieldValue> FieldValues;
            internal List<List<FieldValue>> DocValues;
            internal Query.DocumentResultForSort[] Docs;

            internal UpdateEntity (List<FieldValue> fieldValues, Query.DocumentResultForSort[] docs)
            {
                FieldValues = fieldValues;
                Docs = docs;
                DocValues = null;
            }

            internal UpdateEntity(List<FieldValue> fieldValues, List<List<FieldValue>> docValues, Query.DocumentResultForSort[] docs)
            {
                FieldValues = fieldValues;
                Docs = docs;
                DocValues = docValues;
            }

        }

        List<TSFQLSentence> _SFQLSentenceList;
        TSFQLSentence _SFQLSentence;
        private bool _NeedCollect = false;
        private Dictionary<string, List<Document>> _InsertTables = new Dictionary<string, List<Document>>();

        private bool _NeedCollectUpdate = false;
        private Dictionary<string, List<UpdateEntity>> _UpdateTables = new Dictionary<string, List<UpdateEntity>>();

        private bool _UnionSelect = false;
        private bool _Union = false;
        private TSFQLAttribute _UnionAttribute = null;

        private List<SyntaxAnalysis.Select.Select> _UnionSelects = new List<Hubble.Core.SFQL.SyntaxAnalysis.Select.Select>();

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



        private void ExcuteUpdate(TSFQLSentence sentence)
        {
            SyntaxAnalysis.Update.Update update = sentence.SyntaxEntity as
                SyntaxAnalysis.Update.Update;

            ParseWhere parseWhere = new ParseWhere(update.TableName, DBProvider.GetDBProvider(update.TableName), false, "");

            parseWhere.Begin = update.Begin;
            parseWhere.End = update.End;
            Query.DocumentResultForSort[] result;

            ICollection<int> groupByCollection;

            if (update.Where == null)
            {
                result = parseWhere.Parse(null, out groupByCollection);
            }
            else
            {
                result = parseWhere.Parse(update.Where.ExpressionTree, out groupByCollection);
            }

            Data.DBProvider dBProvider = Data.DBProvider.GetDBProvider(update.TableName);

            if (dBProvider == null)
            {
                throw new DataException(string.Format("Table: {0} does not exist!", update.TableName));
            }

            if (dBProvider.Table.TableSynchronization)
            {
                throw new DataException(string.Format("Table: {0} is setting to synchronize with database now. Can't do update by sql.!", update.TableName));
            }


            List<Data.FieldValue> fieldValues = new List<Hubble.Core.Data.FieldValue>();

            foreach (SyntaxAnalysis.Update.UpdateField field in update.Fields)
            {
                fieldValues.Add(new Hubble.Core.Data.FieldValue(field.Name, field.Value)); 
            }

            List<UpdateEntity> updateEntityList;
            string tableName = update.TableName.Trim().ToLower();

            if (!_UpdateTables.TryGetValue(tableName, out updateEntityList))
            {
                updateEntityList = new List<UpdateEntity>();
                _UpdateTables.Add(tableName, updateEntityList);
            }

            updateEntityList.Add(new UpdateEntity(fieldValues, result));

            if (dBProvider.DocIdReplaceField != null)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i].Score = dBProvider.GetDocIdReplaceFieldValue(result[i].DocId); //use score to store id casually.
                }
            }

            //dBProvider.Update(fieldValues, result);

            //QueryResult qresult = new QueryResult();

            //qresult.AddPrintMessage(string.Format("({0} Row(s) affected)", result.Length));

            //return qresult;

        }

        private QueryResult ExcuteDelete(TSFQLSentence sentence)
        {
            SyntaxAnalysis.Delete.Delete delete = sentence.SyntaxEntity as
                SyntaxAnalysis.Delete.Delete;

            ParseWhere parseWhere = new ParseWhere(delete.TableName, DBProvider.GetDBProvider(delete.TableName), false, "");

            parseWhere.Begin = delete.Begin;
            parseWhere.End = delete.End;
            Query.DocumentResultForSort[] result;

            ICollection<int> groupByCollection;

            if (delete.Where == null)
            {
                result = parseWhere.Parse(null, out groupByCollection);
            }
            else
            {
                result = parseWhere.Parse(delete.Where.ExpressionTree, out groupByCollection);
            }

            Data.DBProvider dBProvider = Data.DBProvider.GetDBProvider(delete.TableName);

            if (dBProvider == null)
            {
                throw new DataException(string.Format("Table: {0} does not exist!", delete.TableName));
            }

            if (dBProvider.Table.TableSynchronization)
            {
                throw new DataException(string.Format("Table: {0} is setting to synchronize with database now. Can't do delete by sql.!", delete.TableName));
            }


            dBProvider.Delete(result);

            QueryResult qresult = new QueryResult();

            qresult.AddPrintMessage(string.Format("({0} Row(s) affected)", result.Length));

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

            if (dBProvider.Table.TableSynchronization)
            {
                throw new DataException(string.Format("Table: {0} is setting to synchronize with database now. Can't do insert by sql.!", tableName));
            }


            if (insert.Fields.Count == 0)
            {
                List<Field> fields = dBProvider.GetAllFields();

                if (dBProvider.IndexOnly && dBProvider.DocIdReplaceField == null)
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
                    document.DocId = int.Parse(insert.Values[i].Value);
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
                    case "docid":
                        table.DocIdReplaceField = attr.Parameters[0];
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

                    case "sqlforcreate":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("SQLForCreate attribute must have one parameter!");
                        }

                        table.SQLForCreate = attr.Parameters[0];
                        break;

                    case "mirrordbtablename":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("MirrorDBTableName attribute must have one parameter!");
                        }

                        table.MirrorDBTableName = attr.Parameters[0];

                        break;
                    case "mirrordbadapter":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("MirrorDBAdapter attribute must have one parameter!");
                        }

                        table.MirrorDBAdapterTypeName = attr.Parameters[0];
                        break;

                    case "mirrordbconnect":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("MirrorDBConnect attribute must have one parameter!");
                        }

                        table.MirrorConnectionString = attr.Parameters[0];
                        break;

                    case "mirrorsqlforcreate":
                        if (attr.Parameters.Count != 1)
                        {
                            throw new ParseException("MirrorSQLForCreate attribute must have one parameter!");
                        }

                        table.MirrorSQLForCreate = attr.Parameters[0];
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

            if (table.DocIdReplaceField != null)
            {
                if (!table.IndexOnly)
                {
                    throw new ParseException("DocId attribute only can be set at IndexOnly mode!");
                }
            }

            //Init fields

            if (createtable.Fields.Count <= 0)
            {
                throw new ParseException("Table must have one field at least!");
            }

            bool passCheckWhenSetDocIdAttr = false;

            foreach (SyntaxAnalysis.CreateTable.CreateTableField tfield in createtable.Fields)
            {
                Data.Field field = new Field();

                field.Name = tfield.FieldName;
                field.Mode = Field.IndexMode.Complex;
                field.IndexType = tfield.IndexType;
                field.Mode = tfield.IndexMode;
                field.Store = true;
                field.AnalyzerName = tfield.AnalyzerName;

                if (field.AnalyzerName != null)
                {
                    if (DBProvider.GetAnalyzer(field.AnalyzerName) == null)
                    {
                        throw new ParseException(string.Format("Unknown Analyzer name={0}!",
                            field.AnalyzerName));
                    }
                }

                field.CanNull = tfield.CanNull;
                field.DataLength = tfield.DataLength;
                field.DataType = tfield.DataType;
                field.DefaultValue = tfield.Default;
                field.PrimaryKey = tfield.PrimaryKey;

                if (table.DocIdReplaceField != null)
                {
                    if (field.Name.Equals(table.DocIdReplaceField, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (field.IndexType != Field.Index.Untokenized)
                        {
                            throw new ParseException("DocId Replace Field must be Untokenized field!");
                        }

                        switch (field.DataType)
                        {
                            case DataType.TinyInt:
                            case DataType.SmallInt:
                            case DataType.Int:
                            case DataType.BigInt:
                                passCheckWhenSetDocIdAttr = true;
                                break;
                            default:
                                throw new ParseException("Can't set DocId attribute to a non-numeric field!");
                        }
                    }
                }

                if (field.Name.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ParseException("DocId can not be field name!");
                }

                if (field.IndexType != Field.Index.None && field.CanNull && field.DefaultValue == null)
                {
                    throw new ParseException("Can not be NULL when index type is Tokenized or Untokenized and hasn't default value!");
                }

                if (string.IsNullOrEmpty(field.AnalyzerName) && field.IndexType == Field.Index.Tokenized)
                {
                    throw new ParseException("Must set analyzer name when index type is Tokenized!");
                }

                table.Fields.Add(field);
            }

            if (table.DocIdReplaceField != null && !passCheckWhenSetDocIdAttr)
            {
                throw new ParseException("Can't set DocId attribute to an unknown field!");
            }

            if (table.DBTableName == null)
            {
                table.DBTableName = table.Name;
            }

            //Check mirror table
            if (table.HasMirrorTable)
            {
                if (table.DBAdapterTypeName == table.MirrorDBAdapterTypeName &&
                    table.DBTableName == table.MirrorDBTableName &&
                    table.ConnectionString == table.MirrorConnectionString)
                {
                    throw new ParseException("DBTable is same as MirrorDBTable!");
                }

                table.MirrorTableEnabled = true;
            }

            DBProvider.CreateTable(table, directory);

            QueryResult result = new QueryResult();
            result.AddPrintMessage(string.Format("create table {0} successful!", table.Name));

            return result;
        }

        private string GetWhereSql(SyntaxAnalysis.Select.Select select)
        {
            StringBuilder sb = new StringBuilder();
            
            if (select.Sentence != null)
            {
                foreach (TSFQLAttribute attribute in select.Sentence.Attributes)
                {
                    if (attribute.Name.Equals("Distinct", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sb.Append(attribute.ToString());
                    }
                    else if (attribute.Name.Equals("NotIn", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sb.Append(attribute.ToString());
                    }

                    sb.AppendLine();
                }
            }

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

        private void GetSelectFields(SyntaxAnalysis.Select.Select select, Data.DBProvider dbProvider,
            out List<Data.Field> selectFields, out int allFieldsCount)
        {
            selectFields = new List<Data.Field>();

            allFieldsCount = 0;

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

        }

        private void GetQueryResult(SyntaxAnalysis.Select.Select select, Data.DBProvider dbProvider,
             Query.DocumentResultForSort[] result, List<Data.Field> selectFields, int allFieldsCount,
            out System.Data.DataSet ds)
        {
            List<Data.Document> docResult = dbProvider.Query(selectFields, result, select.Begin, select.End);
            ds = Data.Document.ToDataSet(selectFields, docResult);
            ds.Tables[0].TableName = "Select_" + select.SelectFroms[0].Alias;

            for (int i = 0; i < select.SelectFields.Count; i++)
            {
                if (select.SelectFields[i].Name == "*")
                {
                    continue;
                }

                ds.Tables[0].Columns[i].ColumnName = select.SelectFields[i + allFieldsCount].Alias;
            }
        }

        private bool OrderByFromDatabase(SyntaxAnalysis.Select.Select select, 
            Data.DBProvider dbProvider)
        {
            if (select.OrderBys == null)
            {
                return false;
            }

            if (select.OrderBys.Count == 0)
            {
                return false;
            }

            bool withScoreOrDocId = false;

            foreach (SFQL.SyntaxAnalysis.Select.OrderBy orderBy in select.OrderBys)
            {
                if (orderBy.Name.Equals("score", StringComparison.CurrentCultureIgnoreCase) ||
                    orderBy.Name.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    withScoreOrDocId = true;
                }
            }

            foreach (SFQL.SyntaxAnalysis.Select.OrderBy orderBy in select.OrderBys)
            {
                if (orderBy.Name.Equals("score", StringComparison.CurrentCultureIgnoreCase) ||
                    orderBy.Name.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                Data.Field field = dbProvider.GetField(orderBy.Name);

                if (field == null)
                {
                    if (withScoreOrDocId)
                    {
                        throw new ParseException("Can't order by none index fields with score or docid");
                    }

                    return true;
                }

                if (field.IndexType != Field.Index.Untokenized)
                {
                    if (withScoreOrDocId)
                    {
                        throw new ParseException("Can't order by none index fields with score or docid");
                    }

                    return true;
                }
            }

            return false;
        }

        private IDistinct GetDistinct(TSFQLSentence sentence, Data.DBProvider dbProvider,
            SyntaxAnalysis.ExpressionTree expressionTree)
        {
            foreach (TSFQLAttribute attribute in sentence.Attributes)
            {
                if (attribute.Name.Equals("Distinct", StringComparison.CurrentCultureIgnoreCase))
                {
                    return new ParseDistinct(attribute, dbProvider, expressionTree);
                }
            }

            return null;
        }


        private List<IGroupBy> GetGroupBy(TSFQLSentence sentence, Data.DBProvider dbProvider,
            SyntaxAnalysis.ExpressionTree expressionTree)
        {
            List<IGroupBy> groupByList = new List<IGroupBy>();

            foreach (TSFQLAttribute attribute in sentence.Attributes)
            {
                if (attribute.Name.Equals("GroupBy", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (attribute.Parameters.Count > 1)
                    {
                        if (attribute.Parameters[0].Equals("Count", StringComparison.CurrentCultureIgnoreCase))
                        {
                            groupByList.Add(new ParseGroupByCount(attribute, dbProvider, expressionTree));
                        }
                    }
                }
            }

            return groupByList;
        }

        private bool CheckDataCacheTicksForUnionSelect(List<SyntaxAnalysis.Select.Select> selects, long datacacheTicks, 
            out long lastModifyTicks, out long totalDocumentCounts)
        {
            lastModifyTicks = 0;
            totalDocumentCounts = 0;

            bool result = true;

            foreach (SyntaxAnalysis.Select.Select select in selects)
            {
                DBProvider dbProvider = DBProvider.GetDBProvider(select.SelectFroms[0].Name);

                if (dbProvider != null)
                {
                    totalDocumentCounts += dbProvider.DocumentCount;

                    if (dbProvider.LastModifyTicks > lastModifyTicks)
                    {
                        lastModifyTicks = dbProvider.LastModifyTicks;
                    }

                    if (dbProvider.LastModifyTicks > datacacheTicks)
                    {
                        result = false;
                    }
                }
                else
                {
                    throw new ParseException(string.Format("table:{0} does not in current database",
                        select.SelectFroms[0].Name));
                }
            }

            return result;
        }

        unsafe private Hubble.Core.Query.DocumentResultForSort[] GetGroupByResult(DBProvider dbProvider, ICollection<int> groupByCollection)
        {
            int len = Math.Min(dbProvider.Table.GroupByLimit, groupByCollection.Count);
            Hubble.Core.Query.DocumentResultForSort[] result = new Hubble.Core.Query.DocumentResultForSort[len];

            int i = 0;

            foreach (int docid in groupByCollection)
            {
                int* payload = dbProvider.GetPayloadData(docid);

                if (payload != null)
                {
                    result[i] = new Hubble.Core.Query.DocumentResultForSort(
                        docid, 0, payload);
                    i++;
                }

                if (i >= len)
                {
                    break;
                }
            }

            return result;

        }

        private Cache.QueryCache GetQueryCache(Data.DBProvider dbProvider,
            List<IGroupBy> groupByList, IDistinct distinct)
        {
            if (dbProvider.QueryCacheEnabled && groupByList.Count <= 0)
            {
                return dbProvider.QueryCache;
            }

            return null;
        }

        unsafe private QueryResult ExcuteSelect(SyntaxAnalysis.Select.Select select, 
            Data.DBProvider dbProvider, string tableName, Service.ConnectionInformation connInfo)
        {
            QueryResult qResult = new QueryResult();

            if (dbProvider == null)
            {
                throw new DataException(string.Format("Table: {0} does not exist!", select.SelectFroms[0].Name));
            }

            if (dbProvider.IsBigTable)
            {
                BigTable.IBigTableParse bigTableParse = (BigTable.IBigTableParse)Hubble.Framework.Reflection.Instance.CreateInstance(DBProvider.BigTableParseType);
                bigTableParse.SFQLParse = this;

                return bigTableParse.Parse(select, dbProvider, connInfo);
            }

            List<IGroupBy> groupByList = GetGroupBy(select.Sentence, dbProvider,
                select.Where == null ? null : select.Where.ExpressionTree);

            IDistinct distinct = GetDistinct(select.Sentence, dbProvider,
                select.Where == null ? null : select.Where.ExpressionTree);
            ParseNotIn notIn = new ParseNotIn(select.Sentence);

            if (distinct != null)
            {
                if (groupByList.Count > 0)
                {
                    throw new ParseException("Group by and Distinct can't be in one sql statement");
                }
            }

            long lastModifyTicks = dbProvider.LastModifyTicks;

            //Process data cache
            if (connInfo.CurrentCommandContent != null)
            {
                if (connInfo.CurrentCommandContent.NeedDataCache)
                {
                    if (dbProvider != null)
                    {
                        long datacacheTicks = connInfo.CurrentCommandContent.DataCache.GetTicks(
                            dbProvider.TableName);

                        qResult.AddPrintMessage(string.Format(@"<TableTicks>{0}={1};</TableTicks>",
                            dbProvider.TableName, lastModifyTicks));

                        if (lastModifyTicks <= datacacheTicks)
                        {
                            System.Data.DataTable table = new System.Data.DataTable();
                            table.MinimumCapacity = int.MaxValue;
                            table.TableName = "Select_" + select.SelectFroms[0].Alias;
                            qResult.DataSet.Tables.Add(table);
                            qResult.AddPrintMessage(string.Format("TotalDocuments:{0}", 
                                dbProvider.DocumentCount));
                            return qResult;
                        }
                    }
                }
            }

            //Begin to select

            bool orderByScore = false;
            if (select.OrderBys.Count == 0)
            {
                orderByScore = true;
            }
            else if (select.OrderBys.Count == 1)
            {
                if (select.OrderBys[0].Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (select.OrderBys[0].Order.Equals("Desc", StringComparison.CurrentCultureIgnoreCase))
                    {
                        orderByScore = true;
                    }
                }
            }

            ParseWhere parseWhere = new ParseWhere(tableName, dbProvider, orderByScore, GetOrderByString(select));

            parseWhere.Begin = select.Begin;
            parseWhere.End = select.End;

            if (parseWhere.Begin < 0)
            {
                //Means only return count
                parseWhere.Begin = 0;
                parseWhere.End = 0;
            }

            parseWhere.NeedGroupBy = groupByList.Count > 0 || distinct != null;
            parseWhere.NotInDict = notIn.NotInDict;

            Query.DocumentResultForSort[] result;

            //Query cache
            Cache.QueryCache queryCache = null;
            string whereSql = "";
            bool noQueryCache = true;
            Cache.QueryCacheDocuments qDocs = null;

            //Get QueryCache instance
            queryCache = GetQueryCache(dbProvider, groupByList, distinct);

            if (queryCache != null)
            {
                whereSql = GetWhereSql(select);
                
                DateTime expireTime;
                int hitCount;
                Cache.QueryCacheInformation qInfo;

                if (queryCache.TryGetValue(whereSql, out qDocs, out expireTime, out hitCount, out qInfo))
                {
                    if ((qInfo.Count >= select.End + 1 && select.End  >= 0) || qInfo.All)
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

            int relTotalCount = 0;

            ICollection<int> groupByCollection = null;

            //Get document result
            if (noQueryCache)
            {
                if (select.Where == null)
                {
                    result = parseWhere.Parse(null, out relTotalCount, out groupByCollection);
                    //relTotalCount = result.Length;
                }
                else
                {
                    result = parseWhere.Parse(select.Where.ExpressionTree, out relTotalCount, out groupByCollection);
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

                        for(int i = 0; i< result.Length; i++)
                        {
                            int docRank = dbProvider.GetPayloadData(result[i].DocId)[rankTab];
                            if (docRank < 0)
                            {
                                docRank = 0;
                            }
                            else if (docRank > 65535)
                            {
                                docRank = 65535;
                            }

                            result[i].Score *= docRank;
                        }
                    }
                }

                //Begin sort

                int sortLen = select.End + 1;

                if (sortLen > 0)
                {
                    sortLen = ((sortLen - 1) / 100 + 1) * 100;
                }

                if (OrderByFromDatabase(select, dbProvider))
                {
                    Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("GetDocumentResultForSortList from DB");

                    IList<Query.DocumentResultForSort> dbResult = dbProvider.DBAdapter.GetDocumentResultForSortList(sortLen - 1, result, GetOrderByString(select));

                    performanceReport.Stop();

                    for (int i= 0; i < dbResult.Count; i++)
                    {
                        result[i] = dbResult[i];
                        result[i].PayloadData = dbProvider.GetPayloadData(result[i].DocId);
                    }

                }
                else
                {

                    QueryResultSort qSort = new QueryResultSort(select.OrderBys, dbProvider);

                    //qSort.Sort(result);

                    if (distinct == null)
                    {
                        qSort.Sort(result, sortLen); // using part quick sort can reduce 40% time
                    }
                    else
                    {
                        //Do distinct
                        qSort.Sort(result);
                        System.Data.DataTable distinctTable;
                        result = distinct.Distinct(result, out distinctTable);
                        relTotalCount = result.Length;
                    }
                }

                if (queryCache != null)
                {
                    int count;
                    bool all = false;

                    if (select.End < 0)
                    {
                        count = result.Length;
                        all = true;
                    }
                    else
                    {
                        if (result.Length <= sortLen)
                        {
                            if (select.Where != null)
                            {
                                if (select.Where.ExpressionTree.NeedTokenize)
                                {
                                    all = true;
                                }
                            }
                        }

                        count = Math.Min(result.Length, sortLen);
                    }

                    if (count <= 10000)
                    {
                        queryCache.Insert(whereSql,
                            new Hubble.Core.Cache.QueryCacheDocuments(count, result, relTotalCount),
                            DateTime.Now.AddSeconds(dbProvider.QueryCacheTimeout),
                            new Cache.QueryCacheInformation(count, lastModifyTicks, all));
                    }
                }

            }
            else
            {
                result = qDocs.Documents;
            }

            List<Data.Field> selectFields;
            int allFieldsCount;

            GetSelectFields(select, dbProvider, out selectFields, out allFieldsCount);

            System.Data.DataSet ds;

            if (select.Begin < 0)
            {
                //Only return count
                result = new Hubble.Core.Query.DocumentResultForSort[0];
            }

            GetQueryResult(select, dbProvider, result, selectFields, allFieldsCount, out ds);

            if (noQueryCache)
            {
                ds.Tables[0].MinimumCapacity = relTotalCount;
            }
            else
            {
                ds.Tables[0].MinimumCapacity = qDocs.ResultLength;
            }

            qResult.DataSet = ds;

            Hubble.Core.Query.DocumentResultForSort[] groupByResult = result;

            if (groupByCollection != null)
            {
                if (groupByCollection.Count > 0)
                {
                    groupByResult = GetGroupByResult(dbProvider, groupByCollection);
                }
            }

            foreach (IGroupBy groupBy in groupByList)
            {
                qResult.AddDataTable(groupBy.GroupBy(groupByResult, dbProvider.Table.GroupByLimit));
            }

            qResult.AddPrintMessage(string.Format("TotalDocuments:{0}",
                dbProvider.DocumentCount));
            return qResult;

        }

        private QueryResult ExcuteSelect(TSFQLSentence sentence)
        {
            SyntaxAnalysis.Select.Select select = sentence.SyntaxEntity as
                SyntaxAnalysis.Select.Select;

            select.Sentence = sentence;

            if (sentence.Attributes.Count > 0)
            {
                if (sentence.Attributes[0].Equals(new TSFQLAttribute("UnionSelect")))
                {
                    _UnionSelect = true;
                }
                else if (sentence.Attributes[0].Equals(new TSFQLAttribute("Union")))
                {
                    _Union = true;
                    _UnionAttribute = sentence.Attributes[0];
                }
            }

            if (_UnionSelect || _Union)
            {
                _UnionSelects.Add(select);
                return null;
            }

            string tableName = select.SelectFroms[0].Name;

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            return ExcuteSelect(select, dbProvider, tableName, Service.CurrentConnection.ConnectionInfo);
        }

        private List<QueryResult> _UnionQueryResult;
        private object _UnionLock = new object();
        private void AddUnionQueryResult(QueryResult qResult)
        {
            lock (_UnionLock)
            {
                _UnionQueryResult.Add(qResult);
            }
        }


        private void ProcUnionSelectQuery(object para)
        {
            try
            {
                SyntaxAnalysis.Select.Select select = ((object[])para)[0] as SyntaxAnalysis.Select.Select;
                Service.ConnectionInformation connInfo = ((object[])para)[1] as Service.ConnectionInformation;
                
                SyntaxAnalysis.Select.Select qSelect = new Hubble.Core.SFQL.SyntaxAnalysis.Select.Select();
                
                //qSelect.Begin = select.Begin;
                qSelect.Begin = 0;
                qSelect.End = select.End;
                qSelect.Where = select.Where;
                qSelect.OrderBys = select.OrderBys;
                qSelect.SelectFroms = select.SelectFroms;
                qSelect.Sentence = select.Sentence;

                SyntaxAnalysis.Select.SelectField selectField = new SyntaxAnalysis.Select.SelectField();
                selectField.Name = "DocId";
                selectField.Alias = selectField.Name;

                qSelect.SelectFields.Add(selectField);

                bool hasScore = false;

                foreach (SyntaxAnalysis.Select.OrderBy orderby in select.OrderBys)
                {
                    selectField = new SyntaxAnalysis.Select.SelectField();

                    if (orderby.Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (orderby.Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                    {
                        hasScore = true;
                    }

                    selectField.Name = orderby.Name;
                    selectField.Alias = selectField.Name;
                    qSelect.SelectFields.Add(selectField);
                }

                if (!hasScore)
                {
                    selectField = new SyntaxAnalysis.Select.SelectField();
                    selectField.Name = "Score";
                    selectField.Alias = selectField.Name;
                    qSelect.SelectFields.Add(selectField);
                }

                string tableName = select.SelectFroms[0].Name;

                Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(connInfo.DatabaseName, tableName);

                QueryResult qResult = ExcuteSelect(qSelect, dbProvider, tableName, connInfo);

                System.Data.DataColumn col = new System.Data.DataColumn();
                col.ColumnName = "TableName";
                col.DefaultValue = tableName;

                qResult.DataSet.Tables[0].Columns.Add(col);

                AddUnionQueryResult(qResult);
            }
            catch (Exception e)
            {
                Global.Report.WriteErrorLog(string.Format("ProcUnionSelectQuery fail! err:{0} stack:{1}",
                    e.Message, e.StackTrace));
            }

        }

        private SyntaxAnalysis.Select.Select GetUnionSelectByTableName(IList<SyntaxAnalysis.Select.Select> selects, string tableName)
        {
            foreach (SyntaxAnalysis.Select.Select select in selects)
            {
                if (select.SelectFroms[0].Name.Equals(tableName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return select;
                }
            }

            return null;
        }

        private string GetOrderByString(SyntaxAnalysis.Select.Select select)
        {
            if (select == null)
            {
                return "";
            }

            if (select.OrderBys == null)
            {
                return "";
            }

            StringBuilder sortString = new StringBuilder();

            int i = 0;
            foreach (SyntaxAnalysis.Select.OrderBy orderBy in select.OrderBys)
            {
                if (i == 0)
                {
                    sortString.AppendFormat("{0} ", orderBy.ToString());
                }
                else
                {
                    sortString.AppendFormat(",{0} ", orderBy.ToString());
                }

                i++;
            }

            return sortString.ToString();
        }

        private void CheckUnionSelectSqlStatement()
        {
            CheckUnionSelectSqlStatement(true);
        }

        private void CheckUnionSelectSqlStatement(bool checkOrderBy)
        {
            if (_UnionSelects.Count <= 1)
            {
                throw new ParseException("UnionSelect need at least two select statements!");
            }

            List<string> _FirstSelectFieldNames = new List<string>(32);
            int _FirstSelectBegin;
            int _FirstSelectEnd;
            List<string> _FirstSelectOrderBy = new List<string>(32);

            foreach (SyntaxAnalysis.Select.SelectField field in _UnionSelects[0].SelectFields)
            {
                if (field.Alias == "*")
                {
                    throw new ParseException("Can't use * as the field name in UnionSelect!");
                }

                _FirstSelectFieldNames.Add(field.Alias);
            }

            foreach (SyntaxAnalysis.Select.OrderBy orderBy in _UnionSelects[0].OrderBys)
            {
                _FirstSelectOrderBy.Add(orderBy.Name);
            }

            _FirstSelectBegin = _UnionSelects[0].Begin;
            _FirstSelectEnd = _UnionSelects[0].End;

            for (int i = 1; i < _UnionSelects.Count; i++)
            {
                SyntaxAnalysis.Select.Select select = _UnionSelects[i];

                if (_FirstSelectFieldNames.Count != select.SelectFields.Count)
                {
                    throw new ParseException("All select statement must have some count of select fields!");
                }

                if (select.Begin != _FirstSelectBegin)
                {
                    throw new ParseException("All select statement must have some begin row number!");
                }

                if (select.End != _FirstSelectEnd)
                {
                    throw new ParseException("All select statement must have some end row number!");
                }

                for(int j = 0; j < select.SelectFields.Count; j++)
                {
                    SyntaxAnalysis.Select.SelectField field = select.SelectFields[j];

                    if (field.Alias == "*")
                    {
                        throw new ParseException("Can't use * as the field name in UnionSelect!");
                    }

                    if (!field.Alias.Equals(_FirstSelectFieldNames[j], StringComparison.CurrentCultureIgnoreCase))
                    {
                        throw new ParseException("All select statement must have some order of select fields!");
                    }
                }

                if (checkOrderBy)
                {
                    if (_FirstSelectOrderBy.Count != select.OrderBys.Count)
                    {
                        throw new ParseException("All select statement must have some count of order by fields!");
                    }

                    for (int j = 0; j < select.OrderBys.Count; j++)
                    {
                        SyntaxAnalysis.Select.OrderBy orderBy = select.OrderBys[j];

                        if (!orderBy.Name.Equals(_FirstSelectOrderBy[j], StringComparison.CurrentCultureIgnoreCase))
                        {
                            throw new ParseException("All select statement must have some order of orderby fields!");
                        }
                    }
                }
            }
        }

        private string GetUnionSelectTableName()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UnionSelect");

            foreach (SyntaxAnalysis.Select.Select select in _UnionSelects)
            {
                sb.AppendFormat("_{0}", select.SelectFroms[0].Name);
            }

            return sb.ToString();
        }

        public List<QueryResult> UnionSelectDistribute(Service.ConnectionInformation connInfo,
            List<SyntaxAnalysis.Select.Select> unionSelects, out string unionSelectTableName,
            out string tableTicksReturn, out QueryResult datacacheResult)
        {
            _UnionSelects = unionSelects;

            CheckUnionSelectSqlStatement(); //Check sql statement.

            unionSelectTableName = GetUnionSelectTableName();
            tableTicksReturn = null;

            datacacheResult = null;

            //Process data cache
            if (connInfo.CurrentCommandContent != null)
            {
                if (connInfo.CurrentCommandContent.NeedDataCache)
                {
                    long datacacheTicks = connInfo.CurrentCommandContent.DataCache.GetTicks(
                            unionSelectTableName);

                    long lastModifyTicks;
                    long totalDocumentCounts;

                    if (CheckDataCacheTicksForUnionSelect(_UnionSelects, datacacheTicks, out lastModifyTicks,
                        out totalDocumentCounts))
                    {
                        QueryResult qResult = new QueryResult();

                        tableTicksReturn = string.Format(@"<TableTicks>{0}={1};</TableTicks>",
                            unionSelectTableName, lastModifyTicks);

                        qResult.AddPrintMessage(tableTicksReturn);

                        System.Data.DataTable tbl = new System.Data.DataTable();
                        tbl.MinimumCapacity = int.MaxValue;
                        tbl.TableName = unionSelectTableName;
                        qResult.DataSet.Tables.Add(tbl);
                        qResult.AddPrintMessage(string.Format("TotalDocuments:{0}",
                            totalDocumentCounts));
                        datacacheResult = qResult;
                        return null;

                    }

                    tableTicksReturn = string.Format(@"<TableTicks>{0}={1};</TableTicks>",
                        unionSelectTableName, lastModifyTicks);

                }
            }

            _UnionQueryResult = new List<QueryResult>();

            Hubble.Framework.Threading.MultiThreadCalculate mCalc =
                new Hubble.Framework.Threading.MultiThreadCalculate(ProcUnionSelectQuery);

            //Start multi thread to get the order by fields and docids from each tables
            foreach (SyntaxAnalysis.Select.Select select in _UnionSelects)
            {
                object[] para = new object[3];
                para[0] = select;
                para[1] = Service.CurrentConnection.ConnectionInfo;
                mCalc.Add(para);
            }

            mCalc.Start(8);

            if (_UnionQueryResult.Count == 0)
            {
                throw new Data.DataException("Union query fail! Please check the error log file");
            }

            //Get the result of combining.
            return _UnionQueryResult;
        }

        internal QueryResult ExcuteUnionSelect(string unionSelectTableName, string tableTicksReturn, 
            List<QueryResult> unionQueryResult, Service.ConnectionInformation connInfo)
        {
            _UnionQueryResult = unionQueryResult;

            if (_UnionQueryResult.Count == 0)
            {
                throw new Data.DataException("Union query fail! Please check the error log file");
            }

            int begin = _UnionSelects[0].Begin;
            int end = _UnionSelects[0].End;

            //Get the result of combining.
            System.Data.DataTable table = _UnionQueryResult[0].DataSet.Tables[0];
            int count = table.MinimumCapacity;

            System.Data.DataTable statisticTable = new System.Data.DataTable(); //This table statistic the count of records for each tables

            statisticTable.Columns.Add(new System.Data.DataColumn("TableName", typeof(string)));
            statisticTable.Columns.Add(new System.Data.DataColumn("Count", typeof(int)));

            //Statistic count of records for first table
            System.Data.DataRow sRow = statisticTable.NewRow();

            sRow["TableName"] = _UnionQueryResult[0].DataSet.Tables[0].Columns["TableName"].DefaultValue;
            sRow["Count"] = _UnionQueryResult[0].DataSet.Tables[0].MinimumCapacity;
            statisticTable.Rows.Add(sRow);

            //Merge

            bool hasScoreField = false;

            long documentCountSum = 0;

            if (_UnionQueryResult.Count > 0)
            {
                foreach (System.Data.DataColumn col in _UnionQueryResult[0].DataSet.Tables[0].Columns)
                {
                    if (col.ColumnName.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                    {
                        hasScoreField = true;
                    }
                }
            }

            if (hasScoreField)
            {
                foreach (QueryResult qResult in _UnionQueryResult)
                {
                    documentCountSum += qResult.GetDocumentCount();
                }

                int firstDocCount = _UnionQueryResult[0].GetDocumentCount();

                if (documentCountSum > 0)
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        table.Rows[i]["Score"] = (long)(double.Parse(table.Rows[i]["Score"].ToString())
                            * Math.Sqrt((double)firstDocCount / (double)documentCountSum));
                    }
                }
                else
                {
                    hasScoreField = false;
                }
            }

            for (int i = 1; i < _UnionQueryResult.Count; i++)
            {
                int docCount = _UnionQueryResult[i].GetDocumentCount();

                count += _UnionQueryResult[i].DataSet.Tables[0].MinimumCapacity;

                //Statistic count of records for this table
                System.Data.DataRow sRow1 = statisticTable.NewRow();

                sRow1["TableName"] = _UnionQueryResult[i].DataSet.Tables[0].Columns["TableName"].DefaultValue;
                sRow1["Count"] = _UnionQueryResult[i].DataSet.Tables[0].MinimumCapacity;
                statisticTable.Rows.Add(sRow1);

                foreach (System.Data.DataRow srcRow in _UnionQueryResult[i].DataSet.Tables[0].Rows)
                {
                    System.Data.DataRow row = table.NewRow();

                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        row[j] = srcRow[j];
                    }

                    if (hasScoreField && docCount > 0)
                    {
                        row["Score"] = (long)(double.Parse(row["Score"].ToString())
                            * Math.Sqrt((double)docCount / (double)documentCountSum));
                    }

                    table.Rows.Add(row);
                }
            }

            System.Data.DataRow[] ResultsRowArray;

            //Sort by order by fields
            if (_UnionSelects[0].OrderBys.Count > 0)
            {
                StringBuilder sortString = new StringBuilder();

                int i = 0;

                foreach (SyntaxAnalysis.Select.OrderBy orderBy in _UnionSelects[0].OrderBys)
                {
                    if (i == 0)
                    {
                        sortString.AppendFormat("{0} ", orderBy.ToString());
                    }
                    else
                    {
                        sortString.AppendFormat(",{0} ", orderBy.ToString());
                    }

                    i++;
                }

                ResultsRowArray = table.Select("", sortString.ToString());
            }
            else
            {
                ResultsRowArray = table.Select("", "DocId");
            }

            table = table.Clone();

            //Get the result of combining that has beed sorted.
            foreach (System.Data.DataRow srcRow in ResultsRowArray)
            {
                System.Data.DataRow row = table.NewRow();

                for (int j = 0; j < table.Columns.Count; j++)
                {
                    row[j] = srcRow[j];
                }

                table.Rows.Add(row);
            }

            System.Data.DataSet ds = new System.Data.DataSet();
            table.MinimumCapacity = count;

            ds.Tables.Add(table);

            //Get final result 

            System.Data.DataTable finalTable = null;

            for (int i = begin; i <= end; i++)
            {
                if (table.Rows.Count <= i)
                {
                    if (finalTable == null)
                    {
                        finalTable = table.Clone();
                    }

                    break;
                }

                System.Data.DataRow row = table.Rows[i];

                SyntaxAnalysis.Select.Select select = _UnionSelects[0];
                List<Data.Field> selectFields;
                int allFieldsCount;
                Query.DocumentResultForSort[] result = new Hubble.Core.Query.DocumentResultForSort[1];
                result[0] = new Hubble.Core.Query.DocumentResultForSort(int.Parse(row["DocId"].ToString()),
                    long.Parse(row["Score"].ToString()));

                string tableName = row["TableName"].ToString();
                Data.DBProvider dbProvider = DBProvider.GetDBProvider(tableName);

                GetSelectFields(GetUnionSelectByTableName(_UnionSelects, tableName),
                    dbProvider, out selectFields, out allFieldsCount);

                System.Data.DataSet dataset;
                select.Begin = 0;
                GetQueryResult(select, dbProvider, result, selectFields, allFieldsCount, out dataset);

                if (finalTable == null)
                {
                    finalTable = dataset.Tables[0].Clone();

                    System.Data.DataColumn col = new System.Data.DataColumn();
                    col.ColumnName = "TableName";
                    finalTable.Columns.Add(col);
                }

                foreach (System.Data.DataRow srcRow in dataset.Tables[0].Rows)
                {
                    System.Data.DataRow destRow = finalTable.NewRow();

                    for (int j = 0; j < dataset.Tables[0].Columns.Count; j++)
                    {
                        destRow[j] = srcRow[j];
                    }

                    destRow["TableName"] = tableName;
                    finalTable.Rows.Add(destRow);
                }
            }

            finalTable.MinimumCapacity = count;
            finalTable.TableName = unionSelectTableName;



            ds = new System.Data.DataSet();
            ds.Tables.Add(finalTable);
            ds.Tables.Add(statisticTable);

            QueryResult qr = new QueryResult(ds);

            if (tableTicksReturn != null)
            {
                qr.AddPrintMessage(tableTicksReturn);
            }

            return qr;


        }

        private QueryResult ExcuteUnion(Service.ConnectionInformation connInfo)
        {
            if (_UnionAttribute.Parameters.Count != 2)
            {
                throw new ParseException("Union Attribute must have two parameters. First one is begin row, last one is end row");
            }

            int begin = int.Parse(_UnionAttribute.Parameters[0]);
            int end = int.Parse(_UnionAttribute.Parameters[1]);

            if (begin < 0 || begin > end)
            {
                throw new ParseException("Invalid Union attribute parameter");
            }

            int outputCount = end - begin + 1;

            CheckUnionSelectSqlStatement(false);

            List<QueryResult> results = new List<QueryResult>();

            foreach (SyntaxAnalysis.Select.Select select in _UnionSelects)
            {
                select.Begin = begin;
                select.End = end;
                

                string tableName = select.SelectFroms[0].Name;

                Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

                QueryResult qResult = ExcuteSelect(select, dbProvider, tableName, Service.CurrentConnection.ConnectionInfo);

                results.Add(qResult);

                if ((qResult.DataSet.Tables[0].Rows.Count == outputCount) || begin == -1)
                {
                    begin = -1;
                    end = 0;
                }
                else
                {
                    begin -= qResult.DataSet.Tables[0].MinimumCapacity;

                    if (begin < 0)
                    {
                        begin = 0;
                    }

                    outputCount -= qResult.DataSet.Tables[0].Rows.Count;
                    end = begin + outputCount - 1;
                }
            }

            QueryResult result = results[0];

            for (int i = 1; i < results.Count; i++)
            {
                System.Data.DataTable table = result.DataSet.Tables[0];

                foreach (System.Data.DataRow srcRow in results[i].DataSet.Tables[0].Rows)
                {
                    System.Data.DataRow row = table.NewRow();

                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        row[j] = srcRow[j];
                    }

                    table.Rows.Add(row);
                }

                result.DataSet.Tables[0].MinimumCapacity += results[i].DataSet.Tables[0].MinimumCapacity;
            }

            return result;
        }

        private QueryResult ExcuteUnionSelect(Service.ConnectionInformation connInfo)
        {
            string unionSelectTableName;
            string tableTicksReturn = null;
            QueryResult datacacheQuery = null;

            _UnionQueryResult = UnionSelectDistribute(connInfo, _UnionSelects, out unionSelectTableName, 
                out tableTicksReturn, out datacacheQuery);

            if (datacacheQuery != null)
            {
                return datacacheQuery;
            }

            return ExcuteUnionSelect(unionSelectTableName, tableTicksReturn, _UnionQueryResult, connInfo);
        }

        private QueryResult ExecuteTSFQLSentence(TSFQLSentence sentence)
        {
            ParseOptimize pOptimize = new ParseOptimize();
            sentence = pOptimize.Optimize(sentence);

            switch (sentence.SentenceType)
            {
                case SentenceType.SELECT:
                    Global.UserRightProvider.CanDo(RightItem.Select);
                    return ExcuteSelect(sentence);
                case SentenceType.DELETE:
                    Global.UserRightProvider.CanDo(RightItem.Delete);
                    return ExcuteDelete(sentence);
                case SentenceType.UPDATE:
                    Global.UserRightProvider.CanDo(RightItem.Update);
                    _NeedCollectUpdate = true;
                    ExcuteUpdate(sentence);
                    break;
                case SentenceType.INSERT:
                    Global.UserRightProvider.CanDo(RightItem.Insert);
                    _NeedCollect = true;
                    ExcuteInsert(sentence);
                    break;
                case SentenceType.EXEC:
                    Global.UserRightProvider.CanDo(RightItem.ExcuteStoreProcedure);
                    return ExcuteExec(sentence);
                case SentenceType.CREATETABLE:
                    Global.UserRightProvider.CanDo(RightItem.CreateTable);
                    return CreateTable(sentence);
            }

            return null;
        }

        public void ExecuteNonQuery(string sql)
        {
            Query(sql);
        }

        private void AppendQueryResult(QueryResult queryResult, ref int tableNum, ref QueryResult result)
        {
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
                    result.AddDataTable(table);
                }

                //foreach (System.Data.DataTable table in tables)
                //{
                //    table.TableName = "Table" + tableNum.ToString();
                //    tableNum++;
                //    result.DataSet.Tables.Add(table);
                //}

                foreach (string message in queryResult.PrintMessages)
                {
                    result.AddPrintMessage(message);
                }
            }
        }

        private void CheckQueryResult(QueryResult result)
        {
            if (result.DataSet != null)
            {
                if (result.DataSet.Tables != null)
                {
                    foreach (System.Data.DataTable table in result.DataSet.Tables)
                    {
                        foreach (System.Data.DataColumn col in table.Columns)
                        {
                            try
                            {
                                Hubble.SQLClient.QueryResultSerialization.GetDataType(col.DataType);
                            }
                            catch
                            {
                                throw new ParseException(string.Format("The data type: {0} of column {1} in table: {2} is not supported by hubble.net",
                                    col.DataType.ToString(), col.ColumnName, table.TableName));
                            }
                        }
                    }
                }
            }
        }

        private bool SameUpdateFields(UpdateEntity entity1, UpdateEntity entity2)
        {
            if (entity1.FieldValues.Count != entity2.FieldValues.Count)
            {
                return false;
            }

            for (int i = 0; i < entity1.FieldValues.Count; i++)
            {
                if (entity1.FieldValues[i].FieldName != entity2.FieldValues[i].FieldName)
                {
                    return false;
                }
            }

            return true;
        }

        private int CollectUpdate(string tableName, List<UpdateEntity> updateEntityList)
        {
            int affectedCount = 0;

            DBProvider dbProvider = DBProvider.GetDBProvider(tableName);
            dbProvider.Update(updateEntityList);

            foreach (UpdateEntity updateEntity in updateEntityList)
            {
                affectedCount += updateEntity.Docs.Length;
            }

            return affectedCount;
        }

        private void CollectUpdate(QueryResult result)
        {
            _NeedCollectUpdate = false;
            int affectedCount = 0;

            foreach (string tableName in _UpdateTables.Keys)
            {
                List<UpdateEntity> updateEntityList = _UpdateTables[tableName];

                affectedCount += CollectUpdate(tableName, updateEntityList);

            }

            result.AddPrintMessage(string.Format("({0} Row(s) updated)", affectedCount));

        }

        public QueryResult Query(string sql)
        {
            _NeedCollect = false;
            _NeedCollectUpdate = false;
            _InsertTables.Clear();

            _SFQLSentenceList = new List<TSFQLSentence>();

            QueryResult result = new QueryResult();

            SyntaxAnalyse(sql);
            int tableNum = 0;

            if (_SFQLSentenceList.Count > 0)
            {
                TSFQLSentence sentence = _SFQLSentenceList[0];
                if (sentence.Attributes.Count > 0)
                {
                    if (sentence.Attributes.Contains(new TSFQLAttribute("PerformanceReport")))
                    {
                        Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.PerformanceReport = true;
                    }
                    else
                    {
                        Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.PerformanceReport = false;
                    }
                }
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("SFQLParase.Query");

#if PerformanceTest
            System.Diagnostics.Stopwatch sw = null;
            long totalMem = 0;
            int[] gcCounts = null;

            if (Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.PerformanceReport)
            {
                totalMem = GC.GetTotalMemory(false);

                gcCounts = new int[GC.MaxGeneration + 1];
                for (int i = 0; i <= GC.MaxGeneration; i++)
                {
                    gcCounts[i] = GC.CollectionCount(i);
                }

                sw = new System.Diagnostics.Stopwatch();
                sw.Start();
            }
#endif

            foreach (TSFQLSentence sentence in _SFQLSentenceList)
            {
                QueryResult queryResult = ExecuteTSFQLSentence(sentence);

                AppendQueryResult(queryResult, ref tableNum, ref result);
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

                    //dbProvider.Collect();

                    if (dbProvider.TooManyIndexFiles)
                    {
                        result.AddPrintMessage("TooManyIndexFiles");
                    }
                }

                result.AddPrintMessage(string.Format("({0} Row(s) affected)", affectedCount));

            }

            //Update collect

            if (_NeedCollectUpdate)
            {
                CollectUpdate(result);
            }

            //Union select
            if (_UnionSelect)
            {
                QueryResult queryResult = ExcuteUnionSelect(Service.CurrentConnection.ConnectionInfo);

                AppendQueryResult(queryResult, ref tableNum, ref result);
            }
            else if (_Union)
            {
                QueryResult queryResult = ExcuteUnion(Service.CurrentConnection.ConnectionInfo);
                AppendQueryResult(queryResult, ref tableNum, ref result);
            }

            performanceReport.Stop();

#if PerformanceTest
            if (Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.PerformanceReport)
            {
                sw.Stop();

                result.AddPrintMessage("PerformanceReport", string.Format("SFQLParase.Query time={0} ms", sw.ElapsedMilliseconds));
                result.AddPrintMessage("PerformanceReport", string.Format("{0} bytes memory alloced.", GC.GetTotalMemory(false) - totalMem));

                Console.WriteLine(string.Format("SFQLParase.Query time={0} ms", sw.ElapsedMilliseconds));

                Console.WriteLine(string.Format("{0} bytes memory alloced.", GC.GetTotalMemory(false) - totalMem));

                for (int i = 0; i <= GC.MaxGeneration; i++)
                {
                    int count = GC.CollectionCount(i) - gcCounts[i];
                    result.AddPrintMessage("PerformanceReport", "\tGen " + i + ": \t\t" + count);

                    Console.WriteLine("\tGen " + i + ": \t\t" + count);
                }
            }
#endif

            if (Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.PerformanceReport)
            {
                result.AddPrintMessage(Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.PerformanceReportText);
            }

            CheckQueryResult(result);

            return result;
        }
    }
}
