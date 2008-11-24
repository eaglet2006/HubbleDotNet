using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;
using SimpleSearch.News;
using FTAlgorithm;

namespace SimpleSearch
{
    class Index
    {
        public static String INDEX_DIR = @"News";
        private static IndexWriter writer = null;
        static private Lucene.Net.Store.RAMDirectory _RamDirectory = new Lucene.Net.Store.RAMDirectory();

        public static int MaxMergeFactor
        {
            get
            {
                if (writer != null)
                {
                    return writer.GetMergeFactor();
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (writer != null)
                {
                    writer.SetMergeFactor(value);
                }
            }
        }

        public static int MaxMergeDocs
        {
            get
            {
                if (writer != null)
                {
                    return writer.GetMaxMergeDocs();
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (writer != null)
                {
                    writer.SetMaxMergeDocs(value);
                }
            }
        }

        public static int MinMergeDocs
        {
            get
            {
                if (writer != null)
                {
                    return writer.GetMaxBufferedDocs();
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (writer != null)
                {
                    writer.SetMaxBufferedDocs(value);
                }
            }
        }

        public static void CreateIndex(String indexDir)
        {
            
            try
            {
                //writer = new RAMDirectory
                writer = new IndexWriter(_RamDirectory, new Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer(), true);
            }
            catch
            {
                writer = new IndexWriter(_RamDirectory, new Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer(), true);
            }

            //writer.Optimize();
            //writer.Close();
        }

        public static void Rebuild(String indexDir)
        {
            writer = new IndexWriter(_RamDirectory, new Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer(), true);
            writer.Optimize();
            writer.Close();
        }

        public static int IndexString(string content)
        {
            //IndexWriter writer = new IndexWriter(indexDir, new Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer(), false);

            Document doc = new Document();

            //Field field = new Field("url", url, Field.Store.YES, Field.Index.NO);
            //doc.Add(field);
            //field = new Field("title", title, Field.Store.YES, Field.Index.TOKENIZED);
            //doc.Add(field);
            //field = new Field("time", time.ToString("yyyyMMdd"), Field.Store.YES, Field.Index.UN_TOKENIZED);
            //doc.Add(field);
            Field field = new Field("contents", content, Field.Store.NO, Field.Index.TOKENIZED);
            doc.Add(field);

            writer.AddDocument(doc);

            int num = writer.DocCount();
            //writer.Optimize();
            //writer.Close();
            return num;
        }

        public static void Close()
        {
            writer.Optimize();
            writer.Close();
        }

        static public List<string> SplitKeyWords(string keywords, Analyzer analyzer)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(FTAlgorithm.General.CStream.WriteStringToStream(keywords,
                Encoding.UTF8), Encoding.UTF8);

            TokenStream tokenStream = analyzer.TokenStream("", reader);

            Lucene.Net.Analysis.Token token = tokenStream.Next();

            List<string> result = new List<string>();

            while (token != null)
            {
                result.Add(keywords.Substring(token.StartOffset(), token.EndOffset() - token.StartOffset()));
                token = tokenStream.Next();
            }

            return result;

        }

        static public string GetKeyWordsSplitBySpace(string keywords, Lucene.Net.Analysis.KTDictSeg.KTDictSegTokenizer ktTokenizer)
        {
            StringBuilder result = new StringBuilder();

            List<T_WordInfo> words = ktTokenizer.SegmentToWordInfos(keywords);

            foreach (T_WordInfo word in words)
            {
                if (word == null)
                {
                    continue;
                }

                result.AppendFormat("{0}^{1}.0 ", word.Word, (int)Math.Pow(3, word.Rank));
            }

            return result.ToString().Trim();
        }


        public static List<TNews> Search(String indexDir, String q, int pageLen, int pageNo, out int recCount)
        {
            string keywords = q;

            IndexSearcher search = new IndexSearcher(indexDir);
            q = GetKeyWordsSplitBySpace(q, new Lucene.Net.Analysis.KTDictSeg.KTDictSegTokenizer());
            QueryParser queryParser = new QueryParser("contents", new Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer(true));

            Query query = queryParser.Parse(q);

            Hits hits = search.Search(query);

            List<TNews> result = new List<TNews>();

            recCount = hits.Length();
            int i = (pageNo - 1) * pageLen;

            while (i < recCount && result.Count < pageLen)
            {
                TNews news = null;

                try
                {
                    news = new TNews();
                    news.Title = hits.Doc(i).Get("title");
                    news.Content = hits.Doc(i).Get("contents");
                    news.Url = hits.Doc(i).Get("url");
                    String strTime = hits.Doc(i).Get("time");
                    news.Time = DateTime.ParseExact(strTime, "yyyyMMdd", null);


                    //// 高亮显示设置
                    ////TermQuery tQuery = new TermQuery(new Term("contents", q));

                    //SimpleHTMLFormatter simpleHTMLFormatter = new SimpleHTMLFormatter("<font color=\"red\">", "</font>");
                    //Highlighter highlighter = new Highlighter(simpleHTMLFormatter, new QueryScorer(query));
                    ////关键内容显示大小设置 
                    //highlighter.SetTextFragmenter(new SimpleFragmenter(50));
                    ////取出高亮显示内容
                    //Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer analyzer = new Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer();
                    //TokenStream tokenStream = analyzer.TokenStream("contents", new StringReader(news.Content));
                    //news.Abstract = highlighter.GetBestFragment(tokenStream, news.Content);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    result.Add(news);
                    i++;
                }
            }

            search.Close();
            return result;
        }
    }
}
