using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Hubble.Core")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Hubble Star Pty")]
#if HubblePro
[assembly: AssemblyProduct("HubblePro 2012 Beta 2 Edition")]
[assembly: AssemblyCopyright("Copyright © Hubble Star Pty 2011-2012")]
#else
[assembly: AssemblyProduct("HubbleDotNet Community Edition")]
[assembly: AssemblyCopyright("Copyright © Hubble Star Pty 2009-2011")]
#endif
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6936932f-1a62-4b02-b274-ee985480ad82")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.2.8.5")]
[assembly: AssemblyFileVersion("1.2.8.5")]

/*****************************************************************************************
 * Hubble.Core modification records
 * ***************************************************************************************
 * 1.1.6.8 Bo Xiao
 * If sorted, don't sort again.
 * 1.1.6.9 Bo Xiao
 * Modify AndMergeForNot. Set the RelCount = Count;
 * When we execute sql with the condition looks like:
 * title match 'abc news' and title not contains 'new' order by score 
 * Wrong count returned.
 * 1.1.7.0 Bo Xiao
 * Fix a bug of distinct. This bug is the cause of modification of 1.1.6.8
 * 1.1.7.1 Bo Xiao
 * if the condition of select statement looks like id = xxx, don't query database and get the data
 * from hubble index directly.
 * This modification can improve the performance of the sql statement such as following:
 * select * from table where id = 1
 * update table set price = 1 where id = 1;
 * 1.1.7.2 Bo Xiao
 * Add a store produce: SP_GetTFIDF.
 * 1.1.7.3 Bo Xiao
 * Add the framework of Linq for hubble (Hubble.SQLClient.Linq)
 * Add some store procedure for linq.
 * We will improve it in the latter version.
 * 1.1.7.4 Bo Xiao
 * Modify SP_TableList store procedure in Linq
 * 1.1.7.5 Modify some data type of the store procedures in Linq
 * 1.1.7.6 Fix a bug when condition is where id = 1000000000000. The old version doesn't support
 * bigint value here.
 * 1.1.7.7 Bo Xiao
 * Improve the performance for one word query normal order by condition.
 * The old version only optimized the performance for order by score desc.
 * From this version any order by condition which is less than or equal 3 and query one word will be optimized.
 * And group by for above condition is also optimized.
 * 1.1.7.8 Bo Xiao
 * Add some features to synchronize function.
 * Can synchronize only for insert or update or delete.
 * Can synchronize with nolock.
 * Can do WaitForExit so that run many synchronize store procedure sequently. 
 * 1.1.7.9 Bo Xiao
 * Improve the performance for normal order by of contains query.
 * 1.1.8.0 
 * Improve performance for mix condition such as where title contains 'xxx' and price > 10 and price < 100.
 * For one word optimize and contains optimize.
 * Fix a bug of PriorQueue that Last value is wrong when _Start > 0. It will effect order by result.
 * 1.1.8.1
 * Increase the max value of step of synchronize from 10000 to 100000
 * 1.1.8.2
 * Add NoneSegment analyzer
 * 1.1.8.3
 * Fix two bugs.
 * return wrong result when execute select * from table where id < n 
 * wrong result when execute the statement like
 * select top 10 * from news where title match 'abc' and title1 match 'abc' and time > '2005-1-1' and content match 'abc'
 * 1.1.8.4
 * Fix a bug:
 * if execute a select statement like:
 * select * from table where a > 0 order by a desc, score.
 * and a is untokenized index type. it will return wrong order of result.
 * Fix a bug of QueryAnalyzer:
 * It will raise exception when we save the task schedule as we set the default time format
 * in control panel as AM and PM.
 * Fix a bug that return wrong order of result when order by from untokenized index fields
 * and one word query. Like where title match 'abc' order by time desc.
 * 1.1.8.5
 * Add SP_GCCollect store procedure.
 * This store procedure can let user collect GC of hubbletask mannually.
 * 1.1.8.6
 * Fix a bug of query cache. It will return empty result when go to next page.
 * 1.1.8.7
 * Imporve the performance of multi-words match order by score desc. and group by with above statement.
 * Such as select top 10 * from table where title match 'abc news' order by score desc.
 * 1.1.8.8
 * Move sqlite db adapter to OtherDBAdapter.sln. The bone version of hubble will never install 
 * sqlite dbadapter by default. If you want to use it, please install it via SP_AddExternalReference
 * manually.
 * 1.1.8.9
 * Fix a bug that will return wrong result when order by asc and contains multi words.
 * Such as following sql
 * select top 10 time from news where title contains 'abc news' order by time asc
 * it will return desc result whatever asc or desc.
 * 1.1.9.0
 * Fix two bugs.
 * one of them is when we execute sql statement without top or between and match more than one words and order by score desc.
 * such as select id, score from news where title match 'abc news' order by score desc. It will raise a out of memory exception.
 * the other one is: there are some problem of compare varchar untokenized fields. 
 * Such as select top 10 * from news where title match 'abc' and a > '1234' and a < '2345'.
 * 1.1.9.1
 * Fix a bug when match more than one words and not order by score desc, it will return wrong order.
 * Such as select top 10 * from news where title match 'abc news' order by time desc
 * 1.1.9.2
 * Improve performance of match query for normal order by and more than one words.
 * for example: select top 10 * from news where title match 'abc bcd' order by time desc
 * 1.1.9.3
 * Add SP_Rebuild store procedure
 * exec SP_Rebuild 'News'
 * exec SP_Rebuild 'News', 5000, 1
 * exec SP_Rebuild 'News', 5000, 2
 * 1.1.9.4
 * Change System.Data.DataSet, DataTable to Hubble.Framework.Data.DataSet, DataTable
 * 1.1.9.5
 * Fix a bug of Hubble.Framework.Data.DataRow that doesn't convert other datatype to the specify datatype
 * Fix a problem of score value of one word match when order by normal fields and including score.
 * For example select top 10 * from news where title contains 'abc' order by time desc, score desc
 * 1.1.9.6
 * Add connection pool for async connection. Add Min Pool Size parameter in connection string of Hubble can 
 * set the connection pool size. Default value is 1.
 * 1.1.9.7
 * Fix a bug of SP_Rebuild for updatable table that will raise exception when run rebuild.
 * Add a static function named Cancel for HubbleAsyncConnection class.
 * This function can be used to cancel the tcp connection.
 * 1.1.9.8
 * Add a item named QueryQueueWaitingTimeout in setting.xml. This setting specifies the 
 * timeout when the sql statement was waiting in the query queue. It is in second and default value is 300.
 * If we set it less than zero, it means infinite timeout.
 * Fix a bug of SynchronizeCanUpdate for MySql and Sqlite. In GetMySqlSelectSql and GetSqliteSelectSql function,
 * " from {0} where {1} >= {2} order by {1} limit {3}" should be 
 * " from {0} where {1} > {2} order by {1} limit {3}"
 * 1.1.9.9
 * Fix a bug when index file large than 2GB and using RamIndex, it will raise a exception said can't 
 * set System.IO.FileStream.set_Position a negative number.
 * 1.2.0.0
 * Fix a bug when None index field include Null value, it raise "is not a valid value for Int32" exception.
 * top optimize if MultiWordsDocIdEnumerator will effect search result, disable it.
 * 1.2.1.0
 * Fix a bug, if GetDocumentsForInsert fail in Synchornize, it will occur a unhandled exception and stop the service.
 * Check the field name in create table. Can't use score as the field name.
 * 1.2.3.0
 * First version can support MongoDB.
 * 1.2.3.1
 * Fix a bug of mirror table insert that will cause exception when the source table is providered from sql server and there are
 * at least one field which Data Type is bit.
 * 1.2.3.2
 * Fix a bug of Mongodb database adpter that will cause a error when we use Mongodb as mirror table and run non-sql query.
 * 1.2.3.4
 * Fix two bugs of mongodb adapter. One is truncate table will use a lot of memory and very slow because of I use remove all before. 
 * Use drop instead of removeall is ok. The other one is MirrorSQLForCreate dose not work before. 
 * 1.2.4.0
 * Load mongodb primary id index to memory at the begining of table openning.
 * Fix a import bug of AsyncHubbleConnection that will hang up the caller at high loading environment.
 * Please replace hubble.sqlclient.dll at the bin folder of asp.net or any client side.
 * 1.2.4.1
 * Can create mirror table at append only mode
 * Fix some bugs of Mongodb adapter when it used as a mirror table.
 * Some of them is case sensitive problem and some of them is mirror table for append only.
 * 1.2.4.2
 * Fix a bug of keywords as a table name like select top 10 * from 'check' where 'title' match 'abc' order by score desc
 * 1.2.5.0
 * Read all index file once before it prepare to using. This optimization can increase the performance of 
 * IO read when the ram index is none cached.
 * 1.2.6.0
 * Add a feature of Distinct. We can specify the count of distinct and default count is 1.
 * For example. Following sql can output same group id 4 times in the result.
 * [Distinct('GroupId',4)]
   select top 10 * from News where title match 'abc' order by score desc
 * 1.2.6.1
 * Optimize the IO read and write for index data.
 * 1.2.7.0
 * Add external distinct interface so that user can load his customer distinct algorithm into hubbledotnet core.
 * Add external distinct: SP_AddExternalReference 'distinct', 'ExternalDistinct.dll'
 * Delete external distinct: SP_DeleteExternalReference 'distinct', 'ExternalDistinct.dll'
 * List external all references that are already in the system: SP_ExternalReference
 * [Distinct('time', 20, 'Demo')] --Third parameter is external distinct name.
 * select top 10 * from News where title match 'abc' order by score desc
 * 1.2.7.1
 * fix a bug of mongodb adapter for mirror update. It will raise an exception when the master database is 
 * sql server and has some fields need to be updated are bit data type.
 * 1.2.7.2
 * Add a exception in MongoAdapter.Query to report the reduplicated insert.
 * 1.2.7.3
 * Fix a bug of distinct that will not return whole results while non-fulltext query
 * Increase the sql length to 4096 for SQLTrace
 * 1.2.7.4
 * Fix a bug of synchronize that will return a progress more than 100 percent.
 * 1.2.7.5
 * Fix a bug of MongoAdapter for mirror table update
 * Improve the performance of updatable synronize for update. Can merge the same id in the trigger table
 * and can batch update
 * 1.2.7.7
 * Fix the right click issue.
 * Expand treeview after truncate table.
 * Fix the prompt error. More then is More than.
 * Add a enabled feature in Bigtable serverinfo so we can enable or disable specified server or tablet.
 * 1.2.7.8
 * Add SP_EnableTablet and SP_DisableTablet
 * 1.2.8.0
 * Fix a bug that if the system throw exception during indexing, the indexwriter won't be closed.
 * Fix a bug of SP_Rebuild that can't execute the statement like this SP_Rebuild 'News',5000,1,'WaitForExit'
 * 1.2.8.1
 * Fix a bug of bigtable setting interface that will show the server name does not including the specified tablet in the balance or failover server combox.
 * Fix a bug of Mongodb adapter that will throw a exception when execute select statement and there has smallint data type in select fields
 * Fix a bug of insert statement that will not insert default value when the insert field is not completed. eg.
 * insert testdatatype (Title) values('bcd')
 * Fix a bug of append only that will search dateabase when we only want to get the result for some specified docid.
 * e.g select * from NewsAppendOnly where docid = 100002
 * 1.2.8.2
 * Fix a bug of add server in bigtable setting interface. 
 * 1.2.8.3
 * Fix a bug of SelectWatchDog. deadlock when select watch dog want to abort some thread that is running select statement and timeout.
 * 1.2.8.4
 * Fix a bug that if payload.db large than 4GB, will throw a exception.
 * 1.2.8.5
 * change lock (this) to lock (_SyncObj) of DBProvider class. 
 *****************************************************************************************/