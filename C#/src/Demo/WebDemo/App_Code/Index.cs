using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Hubble.SQLClient;
using Hubble.Core;
using Hubble.Core.Analysis.HighLight;
using Hubble.Analyzer;

namespace Hubble.WebDemo
{
    public class Index
    {
        const int CacheTimeout = 0; //In seconds

        static public string GetKeyWordsSplit(string keywords, Hubble.Core.Analysis.IAnalyzer iAnalyzer)
        {
            StringBuilder result = new StringBuilder();
            
            iAnalyzer.Init();
            IEnumerable<Hubble.Core.Entity.WordInfo> words = iAnalyzer.Tokenize(keywords);

            foreach (Hubble.Core.Entity.WordInfo word in words)
            {
                result.AppendFormat("{0}^{1}^{2} ", word.Word, word.Rank, word.Position);
            }

            return result.ToString().Trim();
        }


        public static List<TNews> Search(String indexDir, String q, int pageLen, int pageNo, string sortBy,
            out int recCount, out long elapsedMilliseconds, out string sql)
        {
            List<TNews> result = new List<TNews>();

            string keywords = q;

            string matchString = GetKeyWordsSplit(q, new PanGuAnalyzer());

            System.Configuration.ConnectionStringSettings connString =
                System.Web.Configuration.WebConfigurationManager.ConnectionStrings["News"];

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            string connectString = connString.ConnectionString;
            System.Data.DataSet ds;

            sw.Start();

            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();

                if (string.IsNullOrEmpty(sortBy))
                {
                    sortBy = "score";
                }

                SqlCommand cmd = new SqlCommand("select between {0} to {1} * from News where content match {2} or title^2 match {2} order by " + sortBy, 
                    conn, (pageNo-1) * pageLen, pageNo * pageLen - 1, matchString);

                sql = cmd.Sql;

                ds = cmd.Query(CacheTimeout);
            }

            sw.Stop();
            elapsedMilliseconds = sw.ElapsedMilliseconds;

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

                Highlighter highlighter =
                    new Highlighter(simpleHTMLFormatter, new PanGuAnalyzer());

                highlighter.FragmentSize = 50;

                news.Abstract = highlighter.GetBestFragment(keywords, news.Content);
                news.TitleHighLighter = highlighter.GetBestFragment(keywords, news.Title);
                if (string.IsNullOrEmpty(news.TitleHighLighter))
                {
                    news.TitleHighLighter = news.Title;
                }

                result.Add(news);
            }


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
