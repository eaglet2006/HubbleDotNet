using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Hubble.SQLClient;
using Hubble.Core.Analysis.HighLight;
using Hubble.Analyzer;

namespace Hubble.WebDemo
{
    public class Index
    {
        const int CacheTimeout = 0; //In seconds

        //static public string GetKeyWordsSplit(string keywords, Hubble.Core.Analysis.IAnalyzer iAnalyzer, out string bySpace)
        //{
        //    StringBuilder result = new StringBuilder();
        //    StringBuilder bySpaceSb = new StringBuilder();
            
        //    iAnalyzer.Init();
        //    IEnumerable<Hubble.Core.Entity.WordInfo> words = iAnalyzer.Tokenize(keywords);

        //    foreach (Hubble.Core.Entity.WordInfo word in words)
        //    {
        //        bySpaceSb.AppendFormat("{0} ", word.Word, word.Rank, word.Position);
        //        result.AppendFormat("{0}^{1}^{2} ", word.Word, word.Rank, word.Position);
        //    }

        //    bySpace = bySpaceSb.ToString().Trim();
        //    return result.ToString().Trim();
        //}

        private static string _TitleAnalyzerName = null;
        private static string _ContentAnalyzerName = null;

        private static void GetAnalyzerName(HubbleAsyncConnection conn, string tableName)
        {
            if (_TitleAnalyzerName != null && _ContentAnalyzerName != null)
            {
                return;
            }

            string sql = string.Format("exec SP_Columns '{0}'", tableName.Replace("'", "''"));

            HubbleCommand cmd = new HubbleCommand(sql, conn);

            foreach (System.Data.DataRow row in cmd.Query().Tables[0].Rows)
            {
                if (row["FieldName"].ToString().Equals("Title", StringComparison.CurrentCultureIgnoreCase))
                {
                    _TitleAnalyzerName = row["Analyzer"].ToString();
                }

                if (row["FieldName"].ToString().Equals("Content", StringComparison.CurrentCultureIgnoreCase))
                {
                    _ContentAnalyzerName = row["Analyzer"].ToString();
                }
            }

        }

        public static List<TNews> Search(String indexDir, string searchType, String q, int pageLen, int pageNo, string sortBy,
            out int recCount, out long elapsedMilliseconds, out string sql)
        {
            List<TNews> result = new List<TNews>();

            string keywords = q;

            //string matchString = GetKeyWordsSplit(q, new PanGuAnalyzer(), out wordssplitbyspace);

            System.Configuration.ConnectionStringSettings connString =
                System.Web.Configuration.WebConfigurationManager.ConnectionStrings["News"];

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            string connectString = connString.ConnectionString;
            System.Data.DataSet ds;
            //System.Data.DataTable titleWordsPositions;
            //System.Data.DataTable contentWordsPositions;

            sw.Start();

            using (HubbleAsyncConnection conn = new HubbleAsyncConnection(connectString))
            {
                conn.Open();

                GetAnalyzerName(conn, "News");

                if (string.IsNullOrEmpty(sortBy))
                {
                    sortBy = "score";
                }

                string wordssplitbyspace;

                HubbleCommand matchCmd = new HubbleCommand(conn);

                string matchString = matchCmd.GetKeywordAnalyzerStringFromServer("News",
                    "Content", keywords, int.MaxValue, out wordssplitbyspace);

                //HubbleCommand cmd = new HubbleCommand("select between {0} to {1} * from News where content match {2} or title^2 match {2} order by " + sortBy,
                //    conn, (pageNo - 1) * pageLen, pageNo * pageLen - 1, matchString);


                HubbleDataAdapter adapter = new HubbleDataAdapter();

                if (searchType == "Precise")
                {
                    adapter.SelectCommand = new HubbleCommand("select between @begin to @end * from News where content contains @matchString or title^2 contains @matchString order by " + sortBy,
                        conn);
                }
                else if (searchType == "Fuzzy")
                {
                    adapter.SelectCommand = new HubbleCommand("select between @begin to @end * from News where content match @matchString or title^2 match @matchString order by " + sortBy,
                        conn);
                }
                else if (searchType == "Like")
                {
                    adapter.SelectCommand = new HubbleCommand("select between @begin to @end * from News where content like @likeString or title^2 like @likeString order by " + sortBy,
                        conn);
                }
                else
                {
                    throw new ArgumentException(string.Format("Invalid search type: {0}", searchType));
                }


                adapter.SelectCommand.Parameters.Add("@begin", (pageNo - 1) * pageLen);
                adapter.SelectCommand.Parameters.Add("@end", pageNo * pageLen - 1);
                adapter.SelectCommand.Parameters.Add("@matchString", matchString);
                adapter.SelectCommand.Parameters.Add("@likeString", "*" + q.Trim() + "*");

                adapter.SelectCommand.CacheTimeout = CacheTimeout;

                sql = adapter.SelectCommand.Sql;

                ds = new System.Data.DataSet();
                //adapter.Fill(ds);
                
                HubbleCommand cmd = adapter.SelectCommand;

                ds = cmd.Query(CacheTimeout);

                long[] docids = new long[ds.Tables[0].Rows.Count];

                int i = 0;

                foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                {
                    docids[i++] = (long)row["DocId"];
                }
             
                //titleWordsPositions = cmd.GetWordsPositions(wordssplitbyspace, "News", "Title", docids, int.MaxValue);
                //contentWordsPositions = cmd.GetWordsPositions(wordssplitbyspace, "News", "Content", docids, int.MaxValue);
            }

            recCount = ds.Tables[0].MinimumCapacity;

            foreach (System.Data.DataRow row in ds.Tables[0].Rows)
            {
                TNews news = new TNews();
                news.Title = row["Title"].ToString();
                news.Content = row["Content"].ToString();
                news.Url = row["Url"].ToString();
                news.Time = (DateTime)row["Time"];

                SimpleHTMLFormatter simpleHTMLFormatter =
                    new SimpleHTMLFormatter("<font color=\"red\">", "</font>");

                Highlighter titleHighlighter;
                Highlighter contentHighlighter;

                if (_TitleAnalyzerName.Equals("PanGuSegment", StringComparison.CurrentCultureIgnoreCase))
                {
                    titleHighlighter =
                    new Highlighter(simpleHTMLFormatter, new PanGuAnalyzer());
                }
                else if (_TitleAnalyzerName.Equals("EnglishAnalyzer", StringComparison.CurrentCultureIgnoreCase))
                {
                    titleHighlighter = new Highlighter(simpleHTMLFormatter, new Hubble.Core.Analysis.EnglishAnalyzer());
                }
                else
                {
                    titleHighlighter = new Highlighter(simpleHTMLFormatter, new Hubble.Core.Analysis.SimpleAnalyzer());
                }

                if (_ContentAnalyzerName.Equals("PanGuSegment", StringComparison.CurrentCultureIgnoreCase))
                {
                    contentHighlighter =
                    new Highlighter(simpleHTMLFormatter, new PanGuAnalyzer());
                }
                else if (_ContentAnalyzerName.Equals("EnglishAnalyzer", StringComparison.CurrentCultureIgnoreCase))
                {
                    contentHighlighter = new Highlighter(simpleHTMLFormatter, new Hubble.Core.Analysis.EnglishAnalyzer());
                }
                else
                {
                    contentHighlighter = new Highlighter(simpleHTMLFormatter, new Hubble.Core.Analysis.SimpleAnalyzer());
                }

                titleHighlighter.FragmentSize = 50;
                contentHighlighter.FragmentSize = 50;

                //news.Abstract = highlighter.GetBestFragment(contentWordsPositions, news.Content, (long)row["DocId"]);
                //news.TitleHighLighter = highlighter.GetBestFragment(titleWordsPositions, news.Title, (long)row["DocId"]);

                news.Abstract = contentHighlighter.GetBestFragment(keywords, news.Content);
                news.TitleHighLighter = titleHighlighter.GetBestFragment(keywords, news.Title);
                if (string.IsNullOrEmpty(news.TitleHighLighter))
                {
                    news.TitleHighLighter = news.Title;
                }

                result.Add(news);
            }

            sw.Stop();
            elapsedMilliseconds = sw.ElapsedMilliseconds;

            return result;


            //QueryParser queryParser = new QueryParser("contents", new PanGuAnalyzer(true));
            //Query query = queryParser.Parse(q);

            //QueryParser titleQueryParser = new QueryParser("title", new PanGuAnalyzer(true));
            //Query titleQuery = titleQueryParser.Parse(q);

            //BooleanQuery bq = new BooleanQuery();
            //bq.Add(query, BooleanClause.Occur.SHOULD);
            //bq.Add(titleQuery, BooleanClause.Occur.SHOULD);

            //Hits hits = search.Search(bq);

            //List<TNews> result = new List<TNews>();

            //recCount = hits.Length();
            //int i = (pageNo - 1) * pageLen;

            //while (i < recCount && result.Count < pageLen)
            //{
            //    TNews news = null;

            //    try
            //    {
            

            //        //// 高亮显示设置
            //        ////TermQuery tQuery = new TermQuery(new Term("contents", q));

            //        //SimpleHTMLFormatter simpleHTMLFormatter = new SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            //        //Highlighter highlighter = new Highlighter(simpleHTMLFormatter, new QueryScorer(query));
            //        ////关键内容显示大小设置 
            //        //highlighter.SetTextFragmenter(new SimpleFragmenter(50));
            //        ////取出高亮显示内容
            //        //Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer analyzer = new Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer();
            //        //TokenStream tokenStream = analyzer.TokenStream("contents", new StringReader(news.Content));
            //        //news.Abstract = highlighter.GetBestFragment(tokenStream, news.Content);

            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //    finally
            //    {
            //        result.Add(news);
            //        i++;
            //    }
            //}

            //search.Close();
            //return result;
        }
    }
}
