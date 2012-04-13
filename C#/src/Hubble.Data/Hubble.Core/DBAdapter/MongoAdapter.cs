using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using Hubble.Framework.Data;
using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
namespace Hubble.Core.DBAdapter
{
    public class MongoAdapter : IDBAdapter, INamedExternalReference
    {

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
            MongoSession _session = new MongoSession(Table.DBTableName, Table.ConnectionString, null, null);
            _session.Drop<BsonDocument>();
        }

        public void Create()
        {
            //db.mvnews.ensureIndex({id:1},{unique:true});
            //throw new NotImplementedException();
        }

        public void CreateMirrorTable()
        {
            // throw new NotImplementedException();
        }

        
        public void Truncate()
        {
            Truncate(Table.DBTableName);
        }
        
        public void Truncate(string tableName)
        {
            MongoSession _session = new MongoSession(tableName, Table.ConnectionString, null, null);
            _session.Remove<BsonDocument>(null, MongoDB.Driver.RemoveFlags.None);
            //_session.Drop<BsonDocument>();
        }
        
        public void Insert(IList<Document> docs)
        {
            MongoSession _session = new MongoSession(Table.DBTableName, Table.ConnectionString, null, null);
            List<BsonDocument> list = new List<BsonDocument>();
            foreach (Hubble.Core.Data.Document doc in docs)
            {
                BsonDocument document = new BsonDocument();
                document.Add("DocId", new BsonInt64(doc.DocId));
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
                        case Hubble.Core.Data.DataType.BigInt:
                        case Hubble.Core.Data.DataType.TinyInt:
                            bsonvalue = new BsonInt64(Convert.ToInt64(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Float:
                            bsonvalue = new BsonDouble(Convert.ToDouble(fv.Value));
                            break;
                    }
                    document.Add(fv.FieldName, bsonvalue);
                }
                list.Add(document);
            }

            _session.InsertBatch<BsonDocument>(list);
        }
        
        public void MirrorInsert(IList<Document> docs)
        {
            MongoSession _session = new MongoSession(Table.MirrorDBTableName, Table.MirrorConnectionString, null, null);
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
                        case Hubble.Core.Data.DataType.BigInt:
                        case Hubble.Core.Data.DataType.TinyInt:
                            bsonvalue = new BsonInt64(Convert.ToInt64(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Float:
                            bsonvalue = new BsonDouble(Convert.ToDouble(fv.Value));
                            break;
                    }
                    document.Add(fv.FieldName, bsonvalue);
                }
                list.Add(document);
            }

            _session.InsertBatch<BsonDocument>(list);
        }
        
        public void Delete(IList<int> docIds)
        {
            MongoSession _session = new MongoSession(Table.DBTableName, Table.ConnectionString, null, null);
            MongoDB.Driver.Builders.QueryComplete query = MongoDB.Driver.Builders.Query.EQ(Table.DocIdReplaceField, 0);
            foreach (int docId in docIds)
            {
                long id = 0;
                if (Table.DocIdReplaceField != null)
                {
                    id = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                    if (id == long.MaxValue)
                    {
                        //does not find this record
                        continue;
                    }
                }
                else
                {
                    id = docId;
                }
                query = MongoDB.Driver.Builders.Query.Or(query, MongoDB.Driver.Builders.Query.EQ(Table.DocIdReplaceField, id));
            }
            _session.Remove<BsonDocument>(query, MongoDB.Driver.RemoveFlags.None);
        }
        
        public void MirrorDelete(IList<int> docIds)
        {
            MongoSession _session = new MongoSession(Table.MirrorDBTableName, Table.MirrorConnectionString, null, null);
            MongoDB.Driver.Builders.QueryComplete query = MongoDB.Driver.Builders.Query.EQ(Table.DocIdReplaceField, 0);
            foreach (int docId in docIds)
            {
                long id = 0;
                if (Table.DocIdReplaceField != null)
                {
                    id = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                    if (id == long.MaxValue)
                    {
                        //does not find this record
                        continue;
                    }
                }
                else
                {
                    id = docId;
                }
                query = MongoDB.Driver.Builders.Query.Or(query, MongoDB.Driver.Builders.Query.EQ(Table.DocIdReplaceField, id));
            }
            _session.Remove<BsonDocument>(query, MongoDB.Driver.RemoveFlags.None);
        }
        

        public void Update(Document doc, IList<Query.DocumentResultForSort> docs)
        {
            MongoSession _session = new MongoSession(Table.DBTableName, Table.ConnectionString, null, null);
            MongoDB.Driver.Builders.UpdateBuilder update = new MongoDB.Driver.Builders.UpdateBuilder();
            MongoDB.Driver.Builders.QueryComplete query = null;
            MongoDB.Driver.Builders.Update.AddToSet("", "");
            MongoDB.Driver.Builders.Query.In("", "");
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
                        case Hubble.Core.Data.DataType.BigInt:
                        case Hubble.Core.Data.DataType.TinyInt:
                            bsonvalue = new BsonInt64(Convert.ToInt64(fv.Value));
                            break;
                        case Hubble.Core.Data.DataType.Float:
                            bsonvalue = new BsonDouble(Convert.ToDouble(fv.Value));
                            break;
                    }
                }
                //document.Add(fvFieldName.FieldName, bsonvalue);
                update.AddToSet(fv.FieldName, bsonvalue);
            }

            string strWhereUpdateID = "";
            BsonArray bsonArray = new BsonArray();
            if (DocIdReplaceField == null)
            {
                //sql.Append(" where docId in (");
                strWhereUpdateID = "docId";
            }
            else
            {
                //sql.AppendFormat(" where {0} in (", DocIdReplaceField);
                strWhereUpdateID = DocIdReplaceField;
            }
            foreach (Query.DocumentResultForSort docResult in docs)
            {
                int docId = docResult.DocId;
                if (DocIdReplaceField == null)
                {
                    bsonArray.Add(new BsonInt64(docId));
                }
                else
                {
                    long replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                    bsonArray.Add(new BsonInt64(replaceFieldValue));
                }
            }

            query = MongoDB.Driver.Builders.Query.In(strWhereUpdateID, bsonArray);
            _session.Update<BsonDocument>(query, update, MongoDB.Driver.UpdateFlags.None);

        }
        

        public void MirrorUpdate(IList<FieldValue> fieldValues, IList<List<FieldValue>> docValues, IList<Query.DocumentResultForSort> docs)
        {
            MongoSession _session = new MongoSession(Table.MirrorDBTableName, Table.MirrorConnectionString, null, null);
            for (int index = 0; index < docs.Count; index++)
            {
                // BsonDocument document = new BsonDocument();
                MongoDB.Driver.Builders.UpdateBuilder update = new MongoDB.Driver.Builders.UpdateBuilder();

                for (int i = 0; i < fieldValues.Count; i++)
                {
                    Data.FieldValue fvFieldName = fieldValues[i];

                    Data.FieldValue fv = docValues[index][i];

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
                            case Hubble.Core.Data.DataType.BigInt:
                            case Hubble.Core.Data.DataType.TinyInt:
                                bsonvalue = new BsonInt64(Convert.ToInt64(fv.Value));
                                break;
                            case Hubble.Core.Data.DataType.Float:
                                bsonvalue = new BsonDouble(Convert.ToDouble(fv.Value));
                                break;
                        }
                    }
                    //document.Add(fvFieldName.FieldName, bsonvalue);
                    update.Set(fvFieldName.FieldName, bsonvalue);
                }
                int docid = docs[index].DocId;
                long replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docid);
                MongoDB.Driver.Builders.QueryComplete query = MongoDB.Driver.Builders.Query.EQ(DocIdReplaceField, replaceFieldValue);
                _session.Update<BsonDocument>(query, update, MongoDB.Driver.UpdateFlags.None);
            }

        }
        
        public System.Data.DataTable Query(IList<Field> selectFields, IList<Query.DocumentResultForSort> docs)
        {
            return Query(selectFields, docs, 0, docs.Count - 1);
        }
        
        public System.Data.DataTable Query(IList<Field> selectFields, IList<Query.DocumentResultForSort> docs, int begin, int end)
        {

            MongoSession _session = new MongoSession(Table.DBTableName, Table.ConnectionString, null, null);
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

                long replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);

                replaceFieldValueToDocId.Add(replaceFieldValue, docId);
                bsonArray.Add(new BsonInt64(replaceFieldValue));
            }

            query = MongoDB.Driver.Builders.Query.In(DocIdReplaceField, bsonArray);
            MongoDB.Driver.MongoCursor<BsonDocument> list = _session.Query<BsonDocument>(query, null, 0, 0);

            System.Data.DataTable table = new System.Data.DataTable();

            foreach (Hubble.Core.Data.Field field in selectFields)
            {
                if (DocIdReplaceField != null)
                {
                    if (field.Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
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
                }
                table.Columns.Add(new System.Data.DataColumn(field.Name, type));
            }

            if (DocIdReplaceField != null)
            {
                table.Columns.Add(new System.Data.DataColumn(DocIdReplaceField, typeof(Int64)));
                //sql.AppendFormat(",[{0}]", DocIdReplaceField);
            }


            if (list != null && list.Count() > 0)
            {
                foreach (var item in list)
                {
                    System.Data.DataRow dr = table.NewRow();
                    foreach (var bsonElement in item.Elements)
                    {
                        if (table.Columns.Contains(bsonElement.Name))
                        {
                            dr[bsonElement.Name] = bsonElement.Value;
                        }
                    }
                    table.Rows.Add(dr);
                }
            }
            table.Columns.Add(new System.Data.DataColumn("DocId", typeof(int)));
            for (var i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i]["DocId"] =
                    replaceFieldValueToDocId[long.Parse(table.Rows[i][DocIdReplaceField].ToString())];
            }

            return table;
        }

        public IList<Query.DocumentResultForSort> GetDocumentResultForSortList(int end, string where, string orderby)
        {
            // throw new NotImplementedException();
            return null;
        }

        public IList<Query.DocumentResultForSort> GetDocumentResultForSortList(int end, Query.DocumentResultForSort[] docResults, string orderby)
        {
            //throw new NotImplementedException();
            return null;
        }

        public DocumentResultWhereDictionary GetDocumentResults(int end, string where, string orderby)
        {
            //throw new NotImplementedException();
            return null;
        }

        public int MaxDocId
        {
            get { throw new NotImplementedException(); }
        }

        public System.Data.DataSet QuerySql(string sql)
        {
            //throw new NotImplementedException();
            return GetSchema(Table.DBTableName);
        }
        
        public System.Data.DataSet GetSchema(string tableName)
        {
            if (this._ConnectionString == null)
            {
                if (!string.IsNullOrEmpty(this.Table.ConnectionString))
                {
                    this.ConnectionString = this.Table.ConnectionString;
                }
            }
            MongoSession _session = new MongoSession(tableName, this._ConnectionString, null, null);
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.DataTable dt = new System.Data.DataTable();

            MongoCursor<BsonDocument> list = _session.Query<BsonDocument>(null, null, 0, 0);
            if (list == null || list.Count() == 0) return null;
            var i = 0;
            foreach (var item in list)
            {
                if (i > 0)
                {
                    break;
                }
                foreach (var column in item)
                {
                    switch (column.Value.BsonType)
                    {
                        case BsonType.Int64:
                        case BsonType.Int32:
                            dt.Columns.Add(column.Name, typeof(Int64));
                            break;
                        case BsonType.DateTime:
                            dt.Columns.Add(column.Name, typeof(DateTime));
                            break;
                        case BsonType.String:
                        case BsonType.Array:
                            dt.Columns.Add(column.Name, typeof(String));
                            break;
                        default:
                            dt.Columns.Add(column.Name, typeof(String));
                            break;
                    }
                }
                i++;
            }

            foreach (var item in list)
            {
                System.Data.DataRow dr = dt.NewRow();
                foreach (var column in item)
                {
                    dr[column.Name] = column.Value;
                }
                dt.Rows.Add(dr);
            }
            ds.Tables.Add(dt);
            return ds;
        }

        public int ExcuteSql(string sql)
        {
            //throw new NotImplementedException();
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

            using (MongoSession _session = new MongoSession(null, connectionString, null, null))
            {
                _session.Conn();
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
