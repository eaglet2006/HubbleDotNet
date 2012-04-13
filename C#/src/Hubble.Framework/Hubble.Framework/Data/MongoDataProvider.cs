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
    /// Mongo自增长ID数据序列
    /// </summary>
    [Serializable]
    public class MongoSequence
    {
        /// <summary>
        /// 存储数据的序列
        /// </summary>
        public string Sequence { get; set; }
        /// <summary>
        /// 对应的Collection名称
        /// </summary>
        public string CollectionName { get; set; }
        /// <summary>
        /// 对应Collection的自增长ID
        /// </summary>
        public string IncrementID { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sequence">序列表名称</param>
        /// <param name="collectionName">集合字段名称</param>
        /// <param name="incrementID">自增长ID字段名称</param>
        public MongoSequence(string sequence, string collectionName, string incrementID)
        {
            Sequence = sequence;
            CollectionName = collectionName;
            IncrementID = incrementID;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public MongoSequence()
        {
            Sequence = "Sequence";
            CollectionName = "CollectionName";
            IncrementID = "IncrementID";
        }
    }
    /// <summary>
    /// MongoDB数据访问连接对象
    /// </summary>
    public class MongoSession : IDisposable
    {
        #region 私有方法

        /// <summary>
        /// MongoDB连接字符串默认配置节
        /// </summary>
        private const string DEFAULT_CONFIG_NODE = "MongoDB";
        /// <summary>
        /// Mongo自增长ID数据序列
        /// </summary>
        private MongoSequence _sequence { get; set; }
        /// <summary>
        /// MongoDB SafeMode
        /// </summary>
        private SafeMode _safeMode { get; set; }
        /// <summary>
        /// MongoServer
        /// </summary>
        private MongoServer _mongoServer { get; set; }
        /// <summary>
        /// MongoDatabase
        /// </summary>
        public MongoDatabase mongoDatabase;
        /// <summary>
        /// collectionTableName
        /// </summary>
        public string collectionTableName = "DefaultName";

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="collectionTableName">连接的表名</param>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return mongoDatabase.GetCollection<T>(collectionTableName);
        }

        /// <summary>
        /// 链接Mongo
        /// </summary>
        public void Conn()
        {
            this._mongoServer.Connect();
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Drop<T>() where T : class, new()
        {
            if (this.GetCollection<T>().Exists())
            {
                this.GetCollection<T>().Drop();
            }
        }

        /// <summary>
        /// 构造函数
        /// <remarks>默认连接串配置节 Web.config > connectionStrings > MongoDB</remarks>
        /// </summary>
        /// <param name="dbName">数据库名称</param>
        /// <param name="configNode">MongoDB连接字符串配置节</param>
        /// <param name="safeMode">SafeMode选项</param>
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        /// <returns></returns>
        public MongoSession(string tableName, string connStr, SafeMode safeMode, MongoSequence sequence)
        {
            //update by zccokie @2011.8.25
            //string _dbName = "";
            //string _connstr = "";
            //if (!string.IsNullOrEmpty(connStr))
            //{
            //    string[] arrConnStr = connStr.Split(';');

            //    if (arrConnStr.Length >= 2)
            //    {
            //        for (int i = 0; i < arrConnStr.Length; i++)
            //        {
            //            if (arrConnStr[i].Split('=')[0].Equals("DataBase", StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                _dbName = arrConnStr[i].Split('=')[1];
            //            }
            //            else if (arrConnStr[i].Split('=')[0].Equals("Servers", StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                _connstr = arrConnStr[i];
            //            }
            //        }
            //    }
            //}


            //var connString = _connstr;

            SqlConnectionStringBuilder sqlConnBuilder = new SqlConnectionStringBuilder(connStr);

            this._safeMode = safeMode ?? SafeMode.False;
            this._sequence = sequence ?? new MongoSequence();

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

            this._mongoServer = MongoServer.Create(connString.ToString());

            this.mongoDatabase = this._mongoServer.GetDatabase(databaseName, this._safeMode);
            this.collectionTableName = tableName;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        public void Dispose()
        {
            this._mongoServer.Disconnect();
        }

        #endregion

        #region 公有方法

        /// <summary>
        /// 创建自增长ID
        /// <remarks>默认自增ID存放 [Sequence] 集合</remarks>
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public long CreateIncID<T>() where T : class, new()
        {
            long id = 1;
            var collection = mongoDatabase.GetCollection(this._sequence.Sequence);

            if (collection.Exists() &&
                collection.Find(MongoDB.Driver.Builders.Query.EQ(this._sequence.CollectionName, typeof(T).Name)).Count() > 0)
            {
                var result = collection.FindAndModify(
                    MongoDB.Driver.Builders.Query.EQ(this._sequence.CollectionName, typeof(T).Name),
                    null,
                    MongoDB.Driver.Builders.Update.Inc(this._sequence.IncrementID, 1),
                    true);

                if (result.Ok && result.ModifiedDocument != null)
                    long.TryParse(result.ModifiedDocument.GetValue(this._sequence.IncrementID).ToString(), out id);
            }
            else
            {
                collection.Insert(
                    new BsonDocument { 
                        { this._sequence.CollectionName, typeof(T).Name },
                        { this._sequence.IncrementID, id }
                    },
                    this._safeMode);
            }

            return id;
        }

        /// <summary>
        /// 查询跟新一条记录后返回该记录
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="sortBy">排序表达式</param>
        /// <param name="update">跟新表达式</param>
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
        /// 创建索引
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="indexKeys">索引字段列表</param>
        public void CreateIndex<T>(string[] indexKeyArray) where T : class, new()
        {
            if (indexKeyArray.Length > 0)
            {
                this.GetCollection<T>().CreateIndex(indexKeyArray);
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="item">待添加数据</param>
        /// <returns></returns>
        public SafeModeResult Insert<T>(T item) where T : class, new()
        {
            return this.GetCollection<T>().Insert(item, this._safeMode);
        }

        /// <summary>
        /// 获取系统当前时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetSysDateTime()
        {
            return mongoDatabase.Eval("new Date()", null).AsDateTime;
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="items">待添加数据集合</param>
        /// <returns></returns>
        public IEnumerable<SafeModeResult> InsertBatch<T>(IEnumerable<T> items) where T : class, new()
        {
            return this.GetCollection<T>().InsertBatch(items, this._safeMode);
        }

        /// <summary>
        /// 更新数据对象
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="item">待更新数据对象</param>
        /// <returns></returns>
        public SafeModeResult Update<T>(T item) where T : class, new()
        {
            return this.GetCollection<T>().Save<T>(item, this._safeMode);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="update">待更新数据表达式</param>
        /// <param name="updateFlag">修改标志[一条或多条]</param>
        /// <returns></returns>
        public SafeModeResult Update<T>(IMongoQuery query, IMongoUpdate update, UpdateFlags updateFlag) where T : class, new()
        {
            return this.GetCollection<T>().Update(query, update, updateFlag, this._safeMode);
        }

        /// <summary>
        /// 自增长数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="name">字段名</param>
        /// <param name="val">自增长的值</param>
        /// <param name="updateFlag">修改标志[一条或多条]</param>
        /// <returns></returns>
        public SafeModeResult Inc<T>(IMongoQuery query, string name, long val, UpdateFlags updateFlag) where T : class, new()
        {
            if (val <= 0) val = 1;
            return this.Update<T>(query, MongoDB.Driver.Builders.Update.Inc(name, val), updateFlag);
        }

        /// <summary>
        /// 添加数据至数组
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="name">数组字段名</param>
        /// <param name="val">待添加值</param>
        /// <param name="updateFlag">修改标志[一条或多条]</param>
        /// <returns></returns>
        public SafeModeResult Push<T>(IMongoQuery query, string name, BsonValue val, UpdateFlags updateFlag) where T : class, new()
        {
            return this.Update<T>(query, MongoDB.Driver.Builders.Update.Push(name, val), updateFlag);
        }

        /// <summary>
        /// 从数组中删除数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="name">数组字段名</param>
        /// <param name="val">待删除值</param>
        /// <param name="updateFlag">修改标志[一条或多条]</param>
        /// <returns></returns>
        public SafeModeResult Pull<T>(IMongoQuery query, string name, BsonValue val, UpdateFlags updateFlag) where T : class, new()
        {
            return this.Update<T>(query, MongoDB.Driver.Builders.Update.Pull(name, val), updateFlag);
        }

        /// <summary>
        /// 添加数据至数组(保证数据唯一)
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="name">数组字段名</param>
        /// <param name="val">待添加值</param>
        /// <param name="updateFlag">修改标志[一条或多条]</param>
        /// <returns></returns>
        public SafeModeResult AddToSet<T>(IMongoQuery query, string name, BsonValue val, UpdateFlags updateFlag) where T : class, new()
        {
            return this.Update<T>(query, MongoDB.Driver.Builders.Update.AddToSet(name, val), updateFlag);
        }

        /// <summary>
        /// 删除数据
        /// <remarks>一般不用</remarks>
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="removeFlag">删除标志[一条或多条]</param>
        public SafeModeResult Remove<T>(IMongoQuery query, RemoveFlags removeFlag) where T : class, new()
        {
            return this.GetCollection<T>().Remove(query, removeFlag, this._safeMode);
        }

        /// <summary>
        /// 获取多条数据
        /// <remarks>所有或分页数据</remarks>
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="sortBy">排序表达式</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">每页数据数</param>
        /// <returns></returns>
        public MongoCursor<T> Query<T>(IMongoQuery query, IMongoSortBy sortBy, int pageIndex, int pageSize) where T : class, new()
        {
            var cursor = this.GetCollection<T>().Find(query);

            if (sortBy != null)
                cursor = cursor.SetSortOrder(sortBy);
            if (pageSize != 0)
                cursor.SetSkip(pageIndex * pageSize).SetLimit(pageSize);

            return cursor;
        }

        /// <summary>
        /// 获取前几条数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="sortBy">排序表达式</param>
        /// <param name="topCount">数据条数</param>
        /// <returns></returns>
        public MongoCursor<T> Top<T>(IMongoQuery query, IMongoSortBy sortBy, int topCount) where T : class, new()
        {
            return this.Query<T>(query, sortBy, 0, topCount);
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="sortBy">排序表达式</param>
        /// <returns></returns>
        public T Get<T>(IMongoQuery query, IMongoSortBy sortBy) where T : class, new()
        {
            T obj = null;

            foreach (var item in this.Top<T>(query, sortBy, 1))
                obj = item;

            return obj;
        }

        /// <summary>
        /// Distinct数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">字段名</param>
        /// <param name="query">查询表达式</param>
        /// <returns></returns>
        public IEnumerable<BsonValue> Distinct<T>(string key, IMongoQuery query) where T : class, new()
        {
            return this.GetCollection<T>().Distinct(key, query);
        }

        /// <summary>
        /// 获取数据数
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <returns></returns>
        public long Count<T>(IMongoQuery query) where T : class, new()
        {
            return this.GetCollection<T>().Count(query);
        }

        /// <summary>
        /// 二维空间搜索最近的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="query">查询表达式</param>
        /// <param name="x">坐标X</param>
        /// <param name="y">坐标Y</param>
        /// <param name="limit">数据条数</param>
        /// <param name="geoNearOptions">geoNearOptions选项</param>
        /// <returns></returns>
        public GeoNearResult<T> GeoNear<T>(IMongoQuery query, double x, double y, int limit, IMongoGeoNearOptions geoNearOptions) where T : class, new()
        {
            #region Command Demo

            //> db.runCommand({geoNear:"asdf", near:[50,50]})  
            //{  
            //    "ns" : "test.places",  
            //    "near" : "1100110000001111110000001111110000001111110000001111",  
            //    "results" : [  
            //            {  
            //                    "dis" : 69.29646421910687,  
            //                    "obj" : {  
            //                            "_id" : ObjectId("4b8bd6b93b83c574d8760280"),  
            //                            "y" : [  
            //                                    1,  
            //                                    1  
            //                            ],  
            //                            "category" : "Coffee"  
            //                    }  
            //            },  
            //            {  
            //                    "dis" : 69.29646421910687,  
            //                    "obj" : {  
            //                            "_id" : ObjectId("4b8bd6b03b83c574d876027f"),  
            //                            "y" : [  
            //                                    1,  
            //                                    1  
            //                            ]  
            //                    }  
            //            }  
            //    ],  
            //    "stats" : {  
            //            "time" : 0,  
            //            "btreelocs" : 1,  
            //            "btreelocs" : 1,  
            //            "nscanned" : 2,  
            //            "nscanned" : 2,  
            //            "objectsLoaded" : 2,  
            //            "objectsLoaded" : 2,  
            //            "avgDistance" : 69.29646421910687  
            //    },  
            //    "ok" : 1  
            //}  

            #endregion

            return this.GetCollection<T>().GeoNear(query, x, y, limit, geoNearOptions);
        }


        /// <summary>
        /// Mapreduce
        /// </summary>
        public MapReduceResult Mapreduce<T>(IMongoQuery query, BsonJavaScript map, BsonJavaScript reduce) where T : class, new()
        {
            return this.GetCollection<T>().MapReduce(query, map, reduce);
        }

        #endregion
    }
}
