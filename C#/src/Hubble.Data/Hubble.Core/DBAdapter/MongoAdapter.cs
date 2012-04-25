using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;
using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Core.SFQL.SyntaxAnalysis.Select;

using Hubble.Framework.Data;

using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Hubble.Core.DBAdapter
{
    public class MongoAdapter : IDBAdapter, INamedExternalReference
    {
        const string DocIdFieldName = "DocId";

        #region IDBAdapter Members

        Data.Table _Table;
        public Hubble.Core.Data.Table Table
        {
            get
            {
                return _Table;
            }
            set
            {
                _Table = value; ;
            }
        }

        string _ConnectionString = null;
        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
            set
            {
                _ConnectionString = value;
            }
        }


        public void Drop()
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);
            mongoProvider.Drop<BsonDocument>();
        }

        public void Create()
        {
            using (MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString))
            {
                mongoProvider.CreateIndex<BsonDocument>(new string[] { DocIdFieldName }, true);
            }
        }

        public void CreateMirrorTable()
        {
            using (MongoDataProvider mongoProvider = new MongoDataProvider(Table.MirrorDBTableName, Table.ConnectionString))
            {
                mongoProvider.CreateIndex<BsonDocument>(new string[] { Table.DocIdReplaceField }, true);
            }
        }

        
        public void Truncate()
        {
            Truncate(Table.DBTableName);
        }
        
        public void Truncate(string tableName)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(tableName, Table.ConnectionString);
            mongoProvider.Remove<BsonDocument>(null, MongoDB.Driver.RemoveFlags.None);
        }
        
        public void Insert(IList<Document> docs)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);
            List<BsonDocument> list = new List<BsonDocument>();
            foreach (Hubble.Core.Data.Document doc in docs)
            {
                BsonDocument document = new BsonDocument();
                document.Add(DocIdFieldName, new BsonInt32(doc.DocId));

                foreach (Data.FieldValue fv in doc.FieldValues)
                {
                    if (fv.Value == null)
                    {
                        continue;
                    }
                    BsonValue bsonvalue = null;
                    switch (fv.Type)
                    {
                        case Hubble.Core.Data.DataType.Varchar:
                        case Hubble.Core.Data.DataType.NVarchar:
                        case Hubble.Core.Data.DataType.Char:
                        case Hubble.Core.Data.DataType.NChar:
                            bsonvalue = new BsonString(fv.Value);
                            break;
                        case Hubble.Core.Data.DataType.DateTime:
                        case Hubble.Core.Data.DataType.Date:
                        case Hubble.Core.Data.DataType.SmallDateTime:
                            bsonvalue = new BsonDateTime(Convert.ToDateTime(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Int:
                        case Hubble.Core.Data.DataType.SmallInt:
                        case Hubble.Core.Data.DataType.TinyInt:
                            bsonvalue = new BsonInt32(Convert.ToInt32(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.BigInt:
                            bsonvalue = new BsonInt64(Convert.ToInt64(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Float:
                            bsonvalue = new BsonDouble(Convert.ToDouble(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Data:
                            bsonvalue = new BsonBinaryData(Hubble.Core.Data.DataTypeConvert.StringToData(fv.Value));
                            break;
                    }

                    document.Add(fv.FieldName, bsonvalue);
                }

                list.Add(document);
            }

            mongoProvider.InsertBatch<BsonDocument>(list);
        }
        
        public void MirrorInsert(IList<Document> docs)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.MirrorDBTableName, 
                Table.MirrorConnectionString);

            List<BsonDocument> list = new List<BsonDocument>();
            foreach (Hubble.Core.Data.Document doc in docs)
            {
                BsonDocument document = new BsonDocument();
                foreach (Data.FieldValue fv in doc.FieldValues)
                {
                    if (fv.Value == null)
                    {
                        continue;
                    }
                    BsonValue bsonvalue = null;
                    switch (fv.Type)
                    {
                        case Hubble.Core.Data.DataType.Varchar:
                        case Hubble.Core.Data.DataType.NVarchar:
                        case Hubble.Core.Data.DataType.Char:
                        case Hubble.Core.Data.DataType.NChar:
                            bsonvalue = new BsonString(fv.Value);
                            break;
                        case Hubble.Core.Data.DataType.DateTime:
                        case Hubble.Core.Data.DataType.Date:
                        case Hubble.Core.Data.DataType.SmallDateTime:
                            bsonvalue = new BsonDateTime(Convert.ToDateTime(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Int:
                        case Hubble.Core.Data.DataType.SmallInt:
                            bsonvalue = new BsonInt32(Convert.ToInt32(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.TinyInt:
                            {
                                int temp;

                                if (fv.Value.Equals("True", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    temp = 1;
                                }
                                else if (fv.Value.Equals("False", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    temp = 0;
                                }
                                else if (!int.TryParse(fv.Value, out temp))
                                {
                                    temp = (int)double.Parse(fv.Value);
                                }

                                bsonvalue = new BsonInt32(temp);
                            }
                            break;
                        case Hubble.Core.Data.DataType.BigInt:
                            bsonvalue = new BsonInt64(Convert.ToInt64(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Float:
                            bsonvalue = new BsonDouble(Convert.ToDouble(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Data:
                            bsonvalue = new BsonBinaryData(Hubble.Core.Data.DataTypeConvert.StringToData(fv.Value));
                            break;
                    }

                    document.Add(fv.FieldName, bsonvalue);
                }

                list.Add(document);
            }

            mongoProvider.InsertBatch<BsonDocument>(list);
        }
        
        public void Delete(IList<int> docIds)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);

            BsonArray bsonArray = new BsonArray();

            //Get docid set
            foreach (int docId in docIds)
            {
                long replaceFieldValue;

                if (DocIdReplaceField != null)
                {
                    replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                }
                else
                {
                    replaceFieldValue = docId;
                }

                bsonArray.Add(new BsonInt64(replaceFieldValue));
            }


            MongoDB.Driver.Builders.QueryComplete query;

            if (DocIdReplaceField != null)
            {
                query = MongoDB.Driver.Builders.Query.In(DocIdReplaceField, bsonArray);
            }
            else
            {
                query = MongoDB.Driver.Builders.Query.In(DocIdFieldName, bsonArray);
            }
            
            mongoProvider.Remove<BsonDocument>(query, MongoDB.Driver.RemoveFlags.None);
        }
        
        public void MirrorDelete(IList<int> docIds)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.MirrorDBTableName, 
                Table.MirrorConnectionString);

            BsonArray bsonArray = new BsonArray();

            //Get docid set
            foreach (int docId in docIds)
            {
                long replaceFieldValue;

                if (DocIdReplaceField != null)
                {
                    replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                }
                else
                {
                    replaceFieldValue = docId;
                }

                bsonArray.Add(new BsonInt64(replaceFieldValue));
            }


            MongoDB.Driver.Builders.QueryComplete query;

            if (DocIdReplaceField != null)
            {
                query = MongoDB.Driver.Builders.Query.In(DocIdReplaceField, bsonArray);
            }
            else
            {
                query = MongoDB.Driver.Builders.Query.In(DocIdFieldName, bsonArray);
            }

            mongoProvider.Remove<BsonDocument>(query, MongoDB.Driver.RemoveFlags.None);
        }
        

        public void Update(Document doc, IList<Query.DocumentResultForSort> docs)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);
            MongoDB.Driver.Builders.UpdateBuilder update = new MongoDB.Driver.Builders.UpdateBuilder();
            MongoDB.Driver.Builders.QueryComplete query = null;

            foreach (Data.FieldValue fv in doc.FieldValues)
            {
                BsonValue bsonvalue = null;
                if (fv.Value == null)
                {
                    bsonvalue = new BsonString("");
                }
                else
                {
                    switch (fv.Type)
                    {
                        case Hubble.Core.Data.DataType.Varchar:
                        case Hubble.Core.Data.DataType.NVarchar:
                        case Hubble.Core.Data.DataType.Char:
                        case Hubble.Core.Data.DataType.NChar:
                            bsonvalue = new BsonString(fv.Value);
                            break;
                        case Hubble.Core.Data.DataType.DateTime:
                        case Hubble.Core.Data.DataType.Date:
                        case Hubble.Core.Data.DataType.SmallDateTime:
                            bsonvalue = new BsonDateTime(Convert.ToDateTime(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Int:
                        case Hubble.Core.Data.DataType.SmallInt:
                        case Hubble.Core.Data.DataType.TinyInt:
                            bsonvalue = new BsonInt32(Convert.ToInt32(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.BigInt:
                            bsonvalue = new BsonInt64(Convert.ToInt64(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Float:
                            bsonvalue = new BsonDouble(Convert.ToDouble(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Data:
                            bsonvalue = new BsonBinaryData(Hubble.Core.Data.DataTypeConvert.StringToData(fv.Value));
                            break;
                    }
                }

                update.Set(fv.FieldName, bsonvalue);
            }

            string strWhereUpdateID = "";
            BsonArray bsonArray = new BsonArray();
            if (DocIdReplaceField == null)
            {
                strWhereUpdateID = DocIdFieldName;
            }
            else
            {
                strWhereUpdateID = DocIdReplaceField;
            }

            foreach (Query.DocumentResultForSort docResult in docs)
            {
                int docId = docResult.DocId;
                if (DocIdReplaceField == null)
                {
                    bsonArray.Add(new BsonInt32(docId));
                }
                else
                {
                    long replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                    bsonArray.Add(new BsonInt64(replaceFieldValue));
                }
            }

            query = MongoDB.Driver.Builders.Query.In(strWhereUpdateID, bsonArray);
            mongoProvider.Update<BsonDocument>(query, update, MongoDB.Driver.UpdateFlags.Multi);

        }
        

        public void MirrorUpdate(IList<FieldValue> fieldValues, IList<List<FieldValue>> docValues, IList<Query.DocumentResultForSort> docs)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.MirrorDBTableName, 
                Table.MirrorConnectionString);

            MongoDB.Driver.Builders.UpdateBuilder update = new MongoDB.Driver.Builders.UpdateBuilder();
            MongoDB.Driver.Builders.QueryComplete query = null;

            foreach (Data.FieldValue fv in fieldValues)
            {
                BsonValue bsonvalue = null;
                if (fv.Value == null)
                {
                    bsonvalue = new BsonString("");
                }
                else
                {
                    switch (fv.Type)
                    {
                        case Hubble.Core.Data.DataType.Varchar:
                        case Hubble.Core.Data.DataType.NVarchar:
                        case Hubble.Core.Data.DataType.Char:
                        case Hubble.Core.Data.DataType.NChar:
                            bsonvalue = new BsonString(fv.Value);
                            break;
                        case Hubble.Core.Data.DataType.DateTime:
                        case Hubble.Core.Data.DataType.Date:
                        case Hubble.Core.Data.DataType.SmallDateTime:
                            bsonvalue = new BsonDateTime(Convert.ToDateTime(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Int:
                        case Hubble.Core.Data.DataType.SmallInt:
                        case Hubble.Core.Data.DataType.TinyInt:
                            bsonvalue = new BsonInt32(Convert.ToInt32(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.BigInt:
                            bsonvalue = new BsonInt64(Convert.ToInt64(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Float:
                            bsonvalue = new BsonDouble(Convert.ToDouble(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Data:
                            bsonvalue = new BsonBinaryData(Hubble.Core.Data.DataTypeConvert.StringToData(fv.Value));
                            break;
                    }
                }

                update.Set(fv.FieldName, bsonvalue);
            }

            string strWhereUpdateID = "";
            BsonArray bsonArray = new BsonArray();
            if (DocIdReplaceField == null)
            {
                strWhereUpdateID = DocIdFieldName;
            }
            else
            {
                strWhereUpdateID = DocIdReplaceField;
            }

            foreach (Query.DocumentResultForSort docResult in docs)
            {
                int docId = docResult.DocId;
                if (DocIdReplaceField == null)
                {
                    bsonArray.Add(new BsonInt32(docId));
                }
                else
                {
                    long replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                    bsonArray.Add(new BsonInt64(replaceFieldValue));
                }
            }

            query = MongoDB.Driver.Builders.Query.In(strWhereUpdateID, bsonArray);
            mongoProvider.Update<BsonDocument>(query, update, MongoDB.Driver.UpdateFlags.Multi);

        }
        
        public System.Data.DataTable Query(IList<Field> selectFields, IList<Query.DocumentResultForSort> docs)
        {
            return Query(selectFields, docs, 0, docs.Count - 1);
        }
        
        public System.Data.DataTable Query(IList<Field> selectFields, IList<Query.DocumentResultForSort> docs, int begin, int end)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);
            BsonArray bsonArray = new BsonArray();
            MongoDB.Driver.Builders.QueryComplete query = null;
            Dictionary<long, int> replaceFieldValueToDocId = null;
            if (DocIdReplaceField != null)
            {
                replaceFieldValueToDocId = new Dictionary<long, int>();
            }

            for (int j = begin; j <= end; j++)
            {
                if (j >= docs.Count)
                {
                    break;
                }

                int docId = docs[j].DocId;
                long replaceFieldValue;

                if (replaceFieldValueToDocId != null)
                {
                    replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);

                    replaceFieldValueToDocId.Add(replaceFieldValue, docId);
                }
                else
                {
                    replaceFieldValue = docId;
                }

                bsonArray.Add(new BsonInt64(replaceFieldValue));
            }

            if (DocIdReplaceField != null)
            {
                query = MongoDB.Driver.Builders.Query.In(DocIdReplaceField, bsonArray);
            }
            else
            {
                query = MongoDB.Driver.Builders.Query.In(DocIdFieldName, bsonArray);
            }

            //Add selected fields to BsonDocument of query
            List<string> selectedFileds = new List<string>();
            bool hasDocIdReplaceField = false;

            foreach (Hubble.Core.Data.Field field in selectFields)
            {
                if (DocIdReplaceField != null)
                {
                    if (field.Name.Equals(DocIdFieldName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (field.Name.Equals(DocIdReplaceField, StringComparison.CurrentCultureIgnoreCase))
                    {
                        hasDocIdReplaceField = true;
                    }
                }

                selectedFileds.Add(field.Name);
            }

            if (DocIdReplaceField != null)
            {
                if (!hasDocIdReplaceField)
                {
                    selectedFileds.Add(DocIdReplaceField);
                }
            }
            
            MongoDB.Driver.MongoCursor<BsonDocument> list = 
                mongoProvider.Query<BsonDocument>(query, null, 0, 0).SetFields(Fields.Include(selectedFileds.ToArray()));

            System.Data.DataTable table = new System.Data.DataTable();

            foreach (Hubble.Core.Data.Field field in selectFields)
            {
                if (DocIdReplaceField != null)
                {
                    if (field.Name.Equals(DocIdFieldName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                }

                System.Type type = null;
                // System.Data.DbType dbtype = System.Data.DbType.Int32;
                switch (field.DataType)
                {
                    case Hubble.Core.Data.DataType.Varchar:
                    case Hubble.Core.Data.DataType.NVarchar:
                    case Hubble.Core.Data.DataType.Char:
                    case Hubble.Core.Data.DataType.NChar:
                        type = typeof(string);
                        break;
                    case Hubble.Core.Data.DataType.DateTime:
                    case Hubble.Core.Data.DataType.Date:
                    case Hubble.Core.Data.DataType.SmallDateTime:
                        type = typeof(DateTime);
                        break;
                    case Hubble.Core.Data.DataType.Int:
                    case Hubble.Core.Data.DataType.BigInt:
                    case Hubble.Core.Data.DataType.TinyInt:
                        type = typeof(Int64);
                        break;
                    case Hubble.Core.Data.DataType.Float:
                        type = typeof(double);
                        break;
                    case Hubble.Core.Data.DataType.Data:
                        type = typeof(byte[]);
                        break;
                }

                table.Columns.Add(new System.Data.DataColumn(field.Name, type));
            }

            if (DocIdReplaceField != null)
            {
                table.Columns.Add(new System.Data.DataColumn(DocIdReplaceField, typeof(Int64)));
            }

            if (list != null)
            {
                if (list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        System.Data.DataRow dr = table.NewRow();
                        foreach (var bsonElement in item.Elements)
                        {
                            if (table.Columns.Contains(bsonElement.Name))
                            {
                                dr[bsonElement.Name] = MongoDataProvider.ConvertFromBsonValue(bsonElement.Value);;
                            }
                        }

                        table.Rows.Add(dr);
                    }
                }
            }

            if (DocIdReplaceField != null)
            {
                table.Columns.Add(new System.Data.DataColumn(DocIdFieldName, typeof(int)));

                for (var i = 0; i < table.Rows.Count; i++)
                {
                    table.Rows[i][DocIdFieldName] =
                        replaceFieldValueToDocId[long.Parse(table.Rows[i][DocIdReplaceField].ToString())];
                }
            }

            return table;
        }

        public IList<Query.DocumentResultForSort> GetDocumentResultForSortList(int end, Query.DocumentResultForSort[] docResults, string orderby)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);
            BsonArray bsonArray = new BsonArray();
            MongoDB.Driver.Builders.QueryComplete query = null;

            Select select;
            string bsonWhere = SqlWhereToBson("", orderby, out select);

            //Get docid set
            foreach(Query.DocumentResultForSort drfs in docResults)
            {
                int docId = drfs.DocId;
                long replaceFieldValue;

                if (DocIdReplaceField != null)
                {
                    replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                }
                else
                {
                    replaceFieldValue = docId;
                }

                bsonArray.Add(new BsonInt64(replaceFieldValue));
            }

            List<Query.DocumentResultForSort> result = new List<Hubble.Core.Query.DocumentResultForSort>(
                docResults.Length);

            //Get order by 
            SortByBuilder sortByBuilder = null;

            if (select.OrderBys.Count > 0)
            {
                //has order by statement
                sortByBuilder = new SortByBuilder();
            }

            foreach (OrderBy sortOrderBy in select.OrderBys)
            {
                string fieldName = GetMatchedFieldName(sortOrderBy.Name);

                if (sortOrderBy.Order.Equals("Desc", StringComparison.CurrentCultureIgnoreCase))
                {
                    sortByBuilder.Descending(fieldName);
                }
                else
                {
                    sortByBuilder.Ascending(fieldName);
                }
            }

            if (DocIdReplaceField != null)
            {
                query = MongoDB.Driver.Builders.Query.In(DocIdReplaceField, bsonArray);
            }
            else
            {
                query = MongoDB.Driver.Builders.Query.In(DocIdFieldName, bsonArray);
            }

            string idName;

            if (DocIdReplaceField != null)
            {
                idName = DocIdReplaceField;
            }
            else
            {
                idName = DocIdFieldName;
            }

            MongoDB.Driver.MongoCursor<BsonDocument> list =
                mongoProvider.Top<BsonDocument>(query, sortByBuilder, end + 1).SetFields(Fields.Include(idName));

            //Get docid from MongoDB
            if (list != null)
            {
                long listCount = list.Count();

                if (listCount > 0)
                {
                    foreach (var item in list)
                    {
                        int docId = -1;
                        bool find = false;
                        foreach (var bsonElement in item.Elements)
                        {
                            if (bsonElement.Name == idName)
                            {
                                if (this.DocIdReplaceField != null)
                                {
                                    long id = Int64.Parse(bsonElement.Value.RawValue.ToString());
                                    docId = DBProvider.GetDocIdFromDocIdReplaceFieldValue(id);
                                }
                                else
                                {
                                    docId = Int32.Parse(bsonElement.Value.RawValue.ToString());
                                }

                                find = true;
                            }
                        }

                        if (find)
                        {
                            result.Add(new Hubble.Core.Query.DocumentResultForSort(docId));
                        }
                    }
                }
            }

            return result;
        }

        public DocumentResultWhereDictionary GetDocumentResults(int end, string where, string orderby)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);

            List<long> ids = GetIdListForWhereSql(ref where);

            Select select;
            
            string bsonWhere = SqlWhereToBson(where, orderby, out select);

            SortByBuilder sortByBuilder = null;

            if (select.OrderBys.Count > 0)
            {
                //has order by statement
                sortByBuilder = new SortByBuilder();
            }

            foreach (OrderBy sortOrderBy in select.OrderBys)
            {
                string fieldName = GetMatchedFieldName(sortOrderBy.Name);

                if (sortOrderBy.Order.Equals("Desc", StringComparison.CurrentCultureIgnoreCase))
                {
                    sortByBuilder.Descending(fieldName);
                }
                else
                {
                    sortByBuilder.Ascending(fieldName);
                }
            }

            string idName = DocIdFieldName;

            if (this.DocIdReplaceField != null)
            {
                //index only mode
                idName = this.DocIdReplaceField;
            }

            MongoDB.Driver.MongoCursor<BsonDocument> list;

            if (string.IsNullOrEmpty(bsonWhere))
            {
                //no where statement, query all
                list = mongoProvider.TopAll<BsonDocument>(sortByBuilder, end + 1).SetFields(Fields.Include(idName));
            }
            else
            {
                IMongoQuery inQuery = null;

                if (ids != null)
                {
                    //Build bson array
                    BsonArray bsonArray = new BsonArray();

                    foreach (int id in ids)
                    {
                        bsonArray.Add(new BsonInt64(id));

                    }
                    if (DocIdReplaceField != null)
                    {
                        inQuery = MongoDB.Driver.Builders.Query.In(DocIdReplaceField, bsonArray);
                    }
                    else
                    {
                        inQuery = MongoDB.Driver.Builders.Query.In(DocIdFieldName, bsonArray);
                    }
                }

                list = mongoProvider.Top<BsonDocument>(bsonWhere, inQuery,
                                sortByBuilder, end + 1).SetFields(Fields.Include(idName));
            }

            DocumentResultWhereDictionary result = new DocumentResultWhereDictionary();

            //Get docid from MongoDB
            if (list != null)
            {
                long listCount = list.Count();

                if (listCount > 0)
                {
                    foreach (var item in list)
                    {
                        int docId = -1;
                        bool find = false;
                        foreach (var bsonElement in item.Elements)
                        {
                            if (bsonElement.Name == idName)
                            {
                                if (this.DocIdReplaceField != null)
                                {
                                    long id = Int64.Parse(bsonElement.Value.RawValue.ToString());
                                    docId = DBProvider.GetDocIdFromDocIdReplaceFieldValue(id);
                                }
                                else
                                {
                                    docId = Int32.Parse(bsonElement.Value.RawValue.ToString());
                                }

                                find = true;
                            }
                        }

                        if (find)
                        {
                            result.Add(docId, new Hubble.Core.Query.DocumentResult(docId));
                        }
                    }

                    if (string.IsNullOrEmpty(bsonWhere))
                    {
                        result.RelTotalCount = (int)mongoProvider.GetCollection<BsonDocument>().Count();
                    }
                    else
                    {
                        result.RelTotalCount = (int)listCount;
                    }
                }
            }

            return result;

        }

        public int MaxDocId
        {
            get 
            {
                MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);

                MongoDB.Driver.MongoCursor<BsonDocument> list =
                                mongoProvider.TopAll<BsonDocument>(SortBy.Descending(DocIdFieldName), 1).SetFields(Fields.Include(DocIdFieldName));

                if (list != null)
                {
                    if (list.Count() > 0)
                    {
                        foreach (var item in list)
                        {
                            foreach (var bsonElement in item.Elements)
                            {
                                if (bsonElement.Name == DocIdFieldName)
                                {
                                    return Int32.Parse(bsonElement.Value.RawValue.ToString());
                                }
                            }
                        }
                    }

                    return -1;
                }
                else
                {
                    return -1;
                }
            
            }
        }

        public System.Data.DataSet QuerySql(string sql)
        {
            MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString);

            MongoDB.Driver.MongoCursor<BsonDocument> list;

            if (mongoProvider.CheckBson(sql))
            {
                list = mongoProvider.QuerySql<BsonDocument>(sql);
            }
            else
            {
                //Run sql not bson in SP_QuerySql
                
                Select select;

                string bsonWhere = SqlWhereToBson(sql, out select);

                SortByBuilder sortByBuilder = null;

                if (select.OrderBys.Count > 0)
                {
                    //has order by statement
                    sortByBuilder = new SortByBuilder();
                }

                foreach (OrderBy sortOrderBy in select.OrderBys)
                {
                    string fieldName = GetMatchedFieldName(sortOrderBy.Name);

                    if (sortOrderBy.Order.Equals("Desc", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortByBuilder.Descending(fieldName);
                    }
                    else
                    {
                        sortByBuilder.Ascending(fieldName);
                    }
                }

                int top = select.End + 1;

                list = mongoProvider.Top<BsonDocument>(bsonWhere, null, sortByBuilder, top);
            }

            System.Data.DataTable table = new System.Data.DataTable(); 

            if (list != null)
            {
                if (list.Count() > 0)
                {
                    int rows = 0;

                    foreach (var item in list)
                    {
                        if (rows == 0)
                        {
                            //initial table schema
                            foreach (var bsonElement in item.Elements)
                            {
                                System.Data.DataColumn col = new System.Data.DataColumn(
                                    bsonElement.Name, MongoDataProvider.GetTypeFromBsonType(
                                    bsonElement.Value.BsonType));

                                table.Columns.Add(col);
                            }
                        }

                        System.Data.DataRow dr = table.NewRow();
                        foreach (var bsonElement in item.Elements)
                        {
                            if (table.Columns.Contains(bsonElement.Name))
                            {
                                dr[bsonElement.Name] = MongoDataProvider.ConvertFromBsonValue(bsonElement.Value);
                            }
                        }

                        table.Rows.Add(dr);

                        rows++;
                    }
                }
            }
            
            System.Data.DataSet dataSet = new System.Data.DataSet();
            dataSet.Tables.Add(table);

            return dataSet;
        }
        
        public System.Data.DataSet GetSchema(string tableName)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.DataTable dt = new System.Data.DataTable();
            ds.Tables.Add(dt);
            return ds;
        }

        public int ExcuteSql(string sql)
        {
            using (MongoDataProvider mongoProvider = new MongoDataProvider(Table.DBTableName, Table.ConnectionString))
            {
                foreach (string sqltext in sql.Split(new char[] { ';' }))
                {
                    if (sqltext.Trim() == "")
                    {
                        continue;
                    }

                    mongoProvider.ExecuteSql<BsonDocument>(sqltext.Trim());
                }
            }

            return 0;
        }

        public void ConnectionTest()
        {
            string connectionString;
            if (Table == null)
            {
                connectionString = this.ConnectionString;
            }
            else
            {
                connectionString = Table.ConnectionString;
            }

            using (MongoDataProvider mongoProvider = new MongoDataProvider(null, connectionString))
            {
                mongoProvider.Conn();
            }

        }
        string _DocIdReplaceField = null;
        public string DocIdReplaceField
        {
            get
            {
                return _DocIdReplaceField;
            }
            set
            {
                _DocIdReplaceField = value;
            }
        }
        DBProvider _DBProvder;
        public DBProvider DBProvider
        {
            get
            {
                return _DBProvder;
            }
            set
            {
                _DBProvder = value;
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Get docid or id list from where sql if it exist
        /// </summary>
        /// <param name="where">where statement</param>
        /// <returns>if does not include in statement, return null.</returns>
        private List<long> GetIdListForWhereSql(ref string where)
        {
            string inStatement;

            if (DocIdReplaceField != null)
            {
                inStatement = string.Format("and {0} in (", DocIdReplaceField);
            }
            else
            {
                inStatement = string.Format("and {0} in (", DocIdFieldName);
            }

            int index = where.IndexOf(inStatement, 0, StringComparison.CurrentCultureIgnoreCase);

            if (index < 0)
            {
                return null;
            }

            List<long> result = new List<long>();

            string numbers = where.Substring(index + inStatement.Length, where.Length - index - inStatement.Length - 1);

            where = where.Substring(0, index);

            foreach (string str in numbers.Split(new char[] { ',' }))
            {
                result.Add(long.Parse(str));
            }

            return result;
        }

        private string GetMatchedFieldName(string originalFieldName)
        {
            return GetMatchedFieldName(originalFieldName, false);
        }

        private string GetMatchedFieldName(string originalFieldName, bool exceptionWhenNotMatch)
        {
            if (DBProvider == null)
            {
                if (originalFieldName.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    return DocIdFieldName;
                }
                else
                {
                    return originalFieldName;
                }
            }

            if (string.IsNullOrEmpty(originalFieldName))
            {
                throw new ParseException("Field name can't be empty!");
            }

            Field field = DBProvider.GetField(originalFieldName);

            string fieldName;

            if (field == null)
            {
                if (originalFieldName.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    fieldName = DocIdFieldName;
                }
                else
                {
                    fieldName = originalFieldName;
                }
            }
            else
            {
                fieldName = field.Name;
            }

            return fieldName;
        }

        private string ToBson(ExpressionTree tree)
        {
            StringBuilder sb = new StringBuilder();

            if (tree.OrChild != null)
            {
                if (tree.AndChild != null)
                {
                    if (tree.Expression is ExpressionTree)
                    {
                        sb.AppendFormat(@"{{$or:[{0}, {1}, {2}]}}", ToBson(tree.Expression as ExpressionTree),
                            ToBson(tree.AndChild), ToBson(tree.OrChild));
                    }
                    else
                    {
                        sb.AppendFormat(@"{{$or:[{0}, {1}, {2}]}}", ToBson(tree.Expression as Expression),
                            ToBson(tree.AndChild), ToBson(tree.OrChild));
                    }
                }
                else
                {
                    if (tree.Expression is ExpressionTree)
                    {
                        sb.AppendFormat(@"{{$or:[{0}, {1}]}}", ToBson(tree.Expression as ExpressionTree),
                            ToBson(tree.OrChild));
                    }
                    else
                    {
                        sb.AppendFormat(@"{{$or:[{0}, {1}]}}", ToBson(tree.Expression as Expression),
                            ToBson(tree.OrChild));
                    }
                }
            }
            else
            {
                if (tree.AndChild != null)
                {
                    if (tree.Expression is ExpressionTree)
                    {
                        sb.AppendFormat(@"{{$and:[{0}, {1}]}}", ToBson(tree.Expression as ExpressionTree),
                            ToBson(tree.AndChild));
                    }
                    else
                    {
                        sb.AppendFormat(@"{{$and:[{0}, {1}]}}", ToBson(tree.Expression as Expression),
                            ToBson(tree.AndChild));
                    }
                }
                else
                {
                    if (tree.Expression is ExpressionTree)
                    {
                        sb.AppendFormat(@"{0}", ToBson(tree.Expression as ExpressionTree));
                    }
                    else
                    {
                        sb.AppendFormat(@"{0}", ToBson(tree.Expression as Expression));
                    }

                }
            }

            return sb.ToString();
        }

        private string GetQueryValue(ref string fieldName, string originalValue)
        {
            if (fieldName.Equals(DocIdFieldName, StringComparison.CurrentCultureIgnoreCase))
            {
                fieldName = DocIdFieldName;
                return originalValue;
            }

            Field field = DBProvider.GetField(fieldName);

            if (field == null)
            {
                throw new ParseException(string.Format("Query field:{0} is not created in table info", fieldName));
            }

            fieldName = field.Name;

            switch (field.DataType)
            {
                case DataType.Char:
                case DataType.NChar:
                case DataType.Varchar:
                case DataType.NVarchar:
                    return @"""" + originalValue + @"""";
                case DataType.Date:
                case DataType.DateTime:
                case DataType.SmallDateTime:
                    DateTime time = DateTime.Parse(originalValue);
                    BsonDateTime bTime = new BsonDateTime(time);

                    //var query = MongoDB.Driver.Builders.Query.EQ("Time", bTime);
                    //Console.WriteLine(query);
                    
                    return string.Format("ISODate(\"{0}\")", bTime);
                default:
                    return originalValue;
            }
        }

        private string ToBson(Expression expression)
        {
            string opr = "";

            switch(expression.Operator.SyntaxType)
            {
                case SyntaxType.Largethan:
                    opr = "$gt";
                    break;
                case SyntaxType.LargethanEqual:
                    opr = "$gte";
                    break;
                case SyntaxType.Lessthan:
                    opr = "$lt";
                    break;
                case SyntaxType.LessthanEqual:
                    opr = "$lte";
                    break;
                case SyntaxType.Equal:
                    opr = "";
                    break;
                case SyntaxType.NotEqual:
                    opr = "$ne";
                    break;
                default:
                    throw new ParseException(string.Format("Unknown operator={0}",
                        expression.Operator.SyntaxType));
            }

            string fieldName = expression.Left[0].Text;
            string value = GetQueryValue(ref fieldName, expression.Right[0].Text);

            if (opr == "")
            {
                return string.Format(@"{{{0}:{1}}}", fieldName, value);
            }
            else
            {
                return string.Format(@"{{ {0}:{{ {1}:{2} }} }}", fieldName, opr, value);
                //return string.Format(@"{{{0}\: {{{1}\:{2}}}}}", expression.Left[0], opr, expression.Right[0]);
            }            
        }

        /// <summary>
        /// where statement to Bson
        /// </summary>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        private string SqlWhereToBson(string where, string orderBy, out Select select)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("select * from t ");

            if (!string.IsNullOrEmpty(where))
            {
                sql.AppendFormat("where {0} ", where);
            }

            if (!string.IsNullOrEmpty(orderBy))
            {
                sql.AppendFormat("order by {0} ", orderBy);
            }

            return SqlWhereToBson(sql.ToString(), out select);
        }

        /// <summary>
        /// where statement to Bson
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        private string SqlWhereToBson(string sql, out Select select)
        {
            SyntaxAnalyse syntaxAnalyse = new SyntaxAnalyse(sql);

            if (syntaxAnalyse.SFQLSentenceList.Count == 0)
            {
                throw new ParseException("Can't use empty sql.");
            }

            select = syntaxAnalyse.SFQLSentenceList[0] .SyntaxEntity as Select;

            try
            {
                if (this.DBProvider == null)
                {
                    //try to get DBProvider.
                    //If can't find, use the field name and table name as the input. 
                    string tableName = select.SelectFroms[0].Name;

                    this.DBProvider = DBProvider.GetDBProvider(tableName);
                }
            }
            catch
            {
            }

            if (select.Where == null)
            {
                return "";
            }
            else if (select.Where.ExpressionTree == null)
            {
                return "";
            }
            else
            {
                syntaxAnalyse.OptimizeReverse(select.Where.ExpressionTree);

                return ToBson(select.Where.ExpressionTree);
            }
        }

        #endregion

        #region INamedExternalReference Members

        public string Name
        {
            get
            {
                return "MongoDB";
            }
        }

        #endregion
    }
}
