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
[assembly: AssemblyProduct("HubblePro 2012 Beta 1 Edition")]
[assembly: AssemblyCopyright("Copyright © Hubble Star Pty 2011")]
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
[assembly: AssemblyVersion("1.1.9.8")]
[assembly: AssemblyFileVersion("1.1.9.8")]

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
 * If we set it less then zero, it means infinite timeout.
 * Fix a bug of SynchronizeCanUpdate for MySql and Sqlite. In GetMySqlSelectSql and GetSqliteSelectSql function,
 * " from {0} where {1} >= {2} order by {1} limit {3}" should be 
 * " from {0} where {1} > {2} order by {1} limit {3}"
 *****************************************************************************************/