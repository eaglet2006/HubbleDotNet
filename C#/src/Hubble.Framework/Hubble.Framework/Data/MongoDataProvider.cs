using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using System.Data.SqlClient;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Hubble.Framework.Data
{
    /// <summary>
    /// MongoDB Session
    /// </summary>
    public class MongoDataProvider : IDisposable
    {
        private enum MongoFunction
        {
            None = 0,
            RunCommand = 1,
            EnsureIndex = 2,
            Find = 3,
        }

        #region Fields

        /// <summary>
        /// MongoDB SafeMode
        /// </summary>
        private SafeMode _SafeMode;
        /// <summary>
        /// MongoServer
        /// </summary>
        private MongoServer _MongoServer;
        /// <summary>
        /// MongoDatabase
        /// </summary>
        private MongoDatabase _MongoDatabase;
        /// <summary>
        /// collectionTableName
        /// </summary>
        readonly private string _TableName;

        #endregion 

        #region Private methods

        private MongoFunction ParseSql(string sql, out BsonDocument bson)
        {
            string function = null;
            int bsonBegin = -1;

            for (int i = 0; i < sql.Length; i++)
            {
                if (sql[i] == '(')
                {
                    bsonBegin = i + 1;
                    function = sql.Substring(0, i);
                    break;
                }
            }

            if (function == null)
            {
                throw new BsonException("no '(' in the sql");
            }

            string bsonString = null;

            for (int i = sql.Length - 1; i >= 0; i--)
            {
                if (sql[i] == ')')
                {
                    bsonString = sql.Substring(bsonBegin, i - bsonBegin);
                    break;
                }                
            }

            if (bsonString == null)
            {
                throw new BsonException("no ')' in the sql");
            }

            bson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(bsonString);


            //bson = bsonString.ToBsonDocument();

            function = function.Trim().ToLower();

            switch (function)
            {
                case "runcommand":
                    return MongoFunction.RunCommand;

                case "ensureindex":
                    return MongoFunction.EnsureIndex;
                case "find":
                    return MongoFunction.Find;
                default:
                    throw new BsonException(string.Format("invalid function:{0}", function));
            }
        }

        /// <summary>
        /// Get the collection 
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <returns>the collection contains with the current table name</returns>
        public MongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return _MongoDatabase.GetCollection<T>(_TableName);
        }

        /// <summary>
        /// Connect Mongo
        /// </summary>
        public void Conn()
        {
            this._MongoServer.Connect();
        }

        /// <summary>
        /// Drop current table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Drop<T>() where T : class, new()
        {
            if (this.GetCollection<T>().Exists())
            {
                this.GetCollection<T>().Drop();
            }
        }

        public MongoDataProvider(string tableName, string connectionString)
            : this(tableName, connectionString, SafeMode.True)
        {

        }

        /// <summary>
        /// Constractor 
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="conntionString">connection string. As same as sql server connection string</param>
        /// <param name="safeMode">Safe mode</param>
        /// <returns></returns>
        public MongoDataProvider(string tableName, string conntionString, SafeMode safeMode)
        {
            SqlConnectionStringBuilder sqlConnBuilder = new SqlConnectionStringBuilder(conntionString);

            this._SafeMode = safeMode ?? SafeMode.False;

            StringBuilder connString = new StringBuilder();
            connString.Append("mongodb://");

            string userName = sqlConnBuilder.UserID ?? "";
            string password = sqlConnBuilder.Password ?? "";

            if (!string.IsNullOrEmpty(userName))
            {
                connString.AppendFormat("{0}:{1}@", userName, password);
            }

            connString.Append(sqlConnBuilder.DataSource);

            string databaseName = sqlConnBuilder.InitialCatalog;

            this._MongoServer = MongoServer.Create(connString.ToString());

            this._MongoDatabase = this._MongoServer.GetDatabase(databaseName, this._SafeMode);
            this._TableName = tableName;
        }

        /// <summary>
        /// Dispose function
        /// </summary>
        public void Dispose()
        {
            this._MongoServer.Disconnect();
        }

        #endregion

        #region public metheds

        /// <summary>
        /// Execute sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        public void ExecuteSql<T>(string sql) where T : class, new()
        {
            BsonDocument bDoc;
            switch (ParseSql(sql, out bDoc))
            {
                case MongoFunction.EnsureIndex:
                    IndexKeysDocument indexKeysDoc = new IndexKeysDocument(bDoc.Elements);
                    this.GetCollection<T>().CreateIndex(indexKeysDoc);
                    break;

                case MongoFunction.RunCommand:
                    _MongoDatabase.RunCommand(new CommandDocument(bDoc.Elements));
                    break;
            }
        }

        public bool CheckBson(string bson)
        {
            try
            {
                BsonDocument bDoc;
                ParseSql(bson, out bDoc);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public MongoCursor<T> QuerySql<T>(string sql) where T : class, new()
        {
            BsonDocument bDoc;
            MongoFunction function = ParseSql(sql, out bDoc);
            switch (function)
            {
                case MongoFunction.Find:
                    return this.GetCollection<T>().Find(new QueryDocument(bDoc));

                default:
                    throw new BsonException(string.Format("invalid function:{0}", function));
            }
        }



        /// <summary>
        /// Find and modify
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="query">Query</param>
        /// <param name="sortBy">Sort by</param>
        /// <param name="update">update</param>
        /// <returns></returns>
        public T FindAndModify<T>(IMongoQuery query, IMongoSortBy sortyBy, IMongoUpdate update) where T : class, new()
        {
            T obj = null;

            var result = this.GetCollection<T>().FindAndModify(query, sortyBy, update, true);
            if (result.Ok && result.ModifiedDocument != null)
            {
                obj = result.GetModifiedDocumentAs<T>();
            }
            return obj;
        }

        /// <summary>
        /// create index
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="indexKeys">fields for index</param>
        public void CreateIndex<T>(string[] indexKeys, bool unique) where T : class, new()
        {
            if (indexKeys.Length > 0)
            {
                var keys = IndexKeys.Ascending(indexKeys);

                var options = IndexOptions.SetUnique(unique);

                StringBuilder indexName = new StringBuilder();
                indexName.Append("Index");

                foreach(string key in indexKeys)
                {
                    indexName.AppendFormat("_{0}", key);
                }

                options.SetName(indexName.ToString());

                this.GetCollection<T>().CreateIndex(keys, options);
            }
        }

        /// <summary>
        /// insert data
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="item">insert item</param>
        /// <returns></returns>
        public SafeModeResult Insert<T>(T item) where T : class, new()
        {
            return this.GetCollection<T>().Insert(item, this._SafeMode);
        }

        /// <summary>
        /// Get current system date time
        /// </summary>
        /// <returns></returns>
        public DateTime GetSysDateTime()
        {
            return _MongoDatabase.Eval("new Date()", null).AsDateTime;
        }

        /// <summary>
        /// batch insert
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="items">items</param>
        /// <returns></returns>
        public IEnumerable<SafeModeResult> InsertBatch<T>(IEnumerable<T> items) where T : class, new()
        {
            return this.GetCollection<T>().InsertBatch(items, this._SafeMode);
        }

        /// <summary>
        /// Update data
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="item">Item</param>
        /// <returns></returns>
        public SafeModeResult Update<T>(T item) where T : class, new()
        {
            return this.GetCollection<T>().Save<T>(item, this._SafeMode);
        }

        /// <summary>
        /// update
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="query">query information</param>
        /// <param name="update">update information</param>
        /// <param name="updateFlag">update flag</param>
        /// <returns></returns>
        public void Update<T>(IMongoQuery query, IMongoUpdate update, UpdateFlags updateFlag) where T : class, new()
        {
            //this.GetCollection<T>().FindAndModify(query, null, update);
            this.GetCollection<T>().Update(query, update, updateFlag, this._SafeMode);
        }


        /// <summary>
        /// Delete 
        /// </summary>
        /// <typeparam name="T">Date Type</typeparam>
        /// <param name="query">Query information</param>
        /// <param name="removeFlag">remove flag</param>
        public SafeModeResult Remove<T>(IMongoQuery query, RemoveFlags removeFlag) where T : class, new()
        {
            return this.GetCollection<T>().Remove(query, removeFlag, this._SafeMode);
        }

        /// <summary>
        /// Get all data from the collection
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="sortBy">sort by fields</param>
        /// <param name="pageIndex">current page index. start with zero</param>
        /// <param name="pageSize">size of page</param>
        /// <returns>Cursor</returns>
        public MongoCursor<T> QueryAll<T>(IMongoSortBy sortBy, int pageIndex, int pageSize) where T : class, new()
        {
            var cursor = this.GetCollection<T>().FindAll();

            if (sortBy != null)
                cursor = cursor.SetSortOrder(sortBy);
            if (pageSize != 0)
                cursor.SetSkip(pageIndex * pageSize).SetLimit(pageSize);

            return cursor;
        }

        /// <summary>
        /// Query data
        /// <remarks>if pageSize == 0, return all of the data contains of this query</remarks>
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="query">query</param>
        /// <param name="sortBy">sort by fields</param>
        /// <param name="pageIndex">current page index. start with zero</param>
        /// <param name="pageSize">size of page</param>
        /// <returns>Cursor</returns>
        public MongoCursor<T> Query<T>(IMongoQuery query, IMongoSortBy sortBy, int pageIndex, int pageSize) where T : class, new()
        {
            var cursor = this.GetCollection<T>().Find(query);

            if (sortBy != null)
                cursor = cursor.SetSortOrder(sortBy);
            if (pageSize != 0)
                cursor.SetSkip(pageIndex * pageSize).SetLimit(pageSize);

            return cursor;
        }

        public MongoCursor<T> Top<T>(string bsonWhere, IMongoQuery inQuery, IMongoSortBy sortBy, int topCount) where T : class, new()
        {
            return this.Query<T>(bsonWhere, inQuery, sortBy, 0, topCount);
        }

        /// <summary>
        /// Query by bson where and inQuery
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="bsonWhere">bson format of where statement</param>
        /// <param name="inQuery">ids in this query. If don't exist, inQuery==null</param>
        /// <param name="sortBy">sort by information</param>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns>cursor</returns>
        public MongoCursor<T> Query<T>(string bsonWhere, IMongoQuery inQuery, IMongoSortBy sortBy, int pageIndex, int pageSize) where T : class, new()
        {
            BsonDocument bson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(bsonWhere);
            MongoCursor<T> cursor;

            if (inQuery != null)
            {
                 cursor = this.GetCollection<T>().Find(MongoDB.Driver.Builders.Query.And(inQuery, new QueryDocument(bson)));
            }
            else
            {
                cursor = this.GetCollection<T>().Find(new QueryDocument(bson));
            }

            if (sortBy != null)
                cursor = cursor.SetSortOrder(sortBy);
            if (pageSize != 0)
                cursor.SetSkip(pageIndex * pageSize).SetLimit(pageSize);

            return cursor;
        }

        /// <summary>
        /// Get top of data for the query
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="query">query</param>
        /// <param name="sortBy">sort by fields</param>
        /// <param name="topCount">number of top</param>
        /// <returns>Cursor</returns>
        public MongoCursor<T> Top<T>(IMongoQuery query, IMongoSortBy sortBy, int topCount) where T : class, new()
        {
            return this.Query<T>(query, sortBy, 0, topCount);
        }

        /// <summary>
        /// Get Top of all of the data 
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="sortBy">sort by fields</param>
        /// <param name="topCount">number of top</param>
        /// <returns>Cursor</returns>
        public MongoCursor<T> TopAll<T>(IMongoSortBy sortBy, int topCount) where T : class, new()
        {
            return this.QueryAll<T>(sortBy, 0, topCount);
        }


        /// <summary>
        /// Get the count of query
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="query">Query information</param>
        /// <returns></returns>
        public long Count<T>(IMongoQuery query) where T : class, new()
        {
            return this.GetCollection<T>().Count(query);
        }
       
        #endregion


        #region Static methods

        /// <summary>
        /// Get type from Bson type
        /// </summary>
        /// <param name="bType">bson type</param>
        /// <returns>.net type</returns>
        public static Type GetTypeFromBsonType(BsonType bType)
        {
            Type type = typeof(string);

            switch (bType)
            {
                case BsonType.Array:
                case BsonType.Binary:
                    type = typeof(byte[]);
                    break;
                case BsonType.Boolean:
                    type = typeof(bool);
                    break;
                case BsonType.DateTime:
                    type = typeof(DateTime);
                    break;
                case BsonType.Document:
                    type = typeof(string);
                    break;
                case BsonType.Double:
                    type = typeof(double);
                    break;
                case BsonType.Int32:
                    type = typeof(int);
                    break;
                case BsonType.Int64:
                    type = typeof(long);
                    break;
                case BsonType.JavaScript:
                    type = typeof(string);
                    break;
                case BsonType.String:
                    type = typeof(string);
                    break;
                default:
                    type = typeof(string);
                    break;
            }

            return type;
        }

        /// <summary>
        /// Bson GUID to dotnet GUID
        /// </summary>
        /// <param name="guidString">bson guid string</param>
        /// <returns>.net GUID string</returns>
        private static string BsonGUIDToDotNetGUID(string guidString)
        {
            return guidString.Substring(0, 8) + "-" +
                guidString.Substring(8, 4) + "-" +
                guidString.Substring(12, 4) + "-" +
                guidString.Substring(16, 4) + "-" +
                guidString.Substring(24, 8);
        }

        /// <summary>
        /// Convert Bson value to the right data type
        /// </summary>
        /// <param name="value">bson value</param>
        /// <returns>value</returns>
        public static object ConvertFromBsonValue(BsonValue value)
        {
            switch (value.BsonType)
            {
                case BsonType.Array:
                case BsonType.Binary:
                    return value.AsByteArray;
                case BsonType.Boolean:
                    return value.AsBoolean;
                case BsonType.DateTime:
                    return value.AsDateTime.ToLocalTime();
                case BsonType.Document:
                    return value.ToString();
                case BsonType.Double:
                    return value.AsDouble;
                case BsonType.Int32:
                    return value.AsInt32;
                case BsonType.Int64:
                    return value.AsInt64;
                case BsonType.String:
                    return value.AsString;
                case BsonType.Null:
                    return DBNull.Value;
                case BsonType.ObjectId:
                    return value.ToString();
                default:
                    return value.ToString();
            }
        }


        #endregion
    }
}
