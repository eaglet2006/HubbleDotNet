using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Hubble.Framework.DataStructure;

namespace Hubble.Core.Analysis
{
    public class EnglishAnalyzer : IAnalyzer, Data.INamedExternalReference
    {
        const int OriginalRank = 500;
        const int LowerRank = 50;
        const int StemRank = 10;
        LinkedList<Framework.WordInfo> _Tokenes = null;

        private static Dictionary<string, string> _InfinitiveVerbTable = null;


        private LinkedList<Framework.WordInfo> GetInitSegment(string text)
        {
            LinkedList<Framework.WordInfo> result = new LinkedList<Framework.WordInfo>();

            Framework.Lexical lexical = new Framework.Lexical(text);

            DFAResult dfaResult;

            for (int i = 0; i < text.Length; i++)
            {
                dfaResult = lexical.Input(text[i], i);

                switch (dfaResult)
                {
                    case DFAResult.Continue:
                        continue;
                    case DFAResult.Quit:
                        result.AddLast(lexical.OutputToken);
                        break;
                    case DFAResult.ElseQuit:
                        result.AddLast(lexical.OutputToken);
                        if (lexical.OldState != 255)
                        {
                            i--;
                        }

                        break;
                }

            }

            dfaResult = lexical.Input(0, text.Length);

            switch (dfaResult)
            {
                case DFAResult.Continue:
                    break;
                case DFAResult.Quit:
                    result.AddLast(lexical.OutputToken);
                    break;
                case DFAResult.ElseQuit:
                    result.AddLast(lexical.OutputToken);
                    break;
            }

            return result;
        }

        private void InitInfinitiveVerbTable()
        {
            if (_InfinitiveVerbTable != null)
            {
                return;
            }

            _InfinitiveVerbTable = new Dictionary<string, string>();

            using (StringReader sr = new StringReader(Hubble.Analyzer.AnalyzerResource.INFINITIVE))
            {

                string line = sr.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    string[] strs = Hubble.Framework.Text.Regx.Split(line, "\t+");

                    if (strs.Length != 3)
                    {
                        continue;
                    }

                    for (int i = 1; i < 3; i++)
                    {
                        string key = strs[i].ToLower().Trim();

                        if (!_InfinitiveVerbTable.ContainsKey(key))
                        {
                            _InfinitiveVerbTable.Add(key, strs[0].Trim().ToLower());
                        }
                    }

                    line = sr.ReadLine();
                }
            }

        }

        private string GetStem(string word)
        {
            string stem;
            if (_InfinitiveVerbTable.TryGetValue(word, out stem))
            {
                return stem;
            }

            porter.Stemmer s = new porter.Stemmer();

            foreach (char ch in word)
            {
                if (char.IsLetter((char)ch))
                {
                    s.add(ch);
                }
            }

            s.stem();

            return s.ToString();

        }

        public EnglishAnalyzer()
        {
        }


        #region IAnalyzer Members

        public int Count
        {
            get
            {
                if (_Tokenes == null)
                {
                    throw new Exception("Tokenes is null. Count property should be called after Tokenize");
                }
                else
                {
                    return _Tokenes.Count;
                }
                
            }
        }

        public IEnumerable<Hubble.Core.Entity.WordInfo> Tokenize(string text)
        {
            _Tokenes = GetInitSegment(text);

            foreach (Framework.WordInfo wi in _Tokenes)
            {
                if (wi == null)
                {
                    continue;
                }

                if (wi.Word == null)
                {
                    continue;
                }

                if (wi.WordType == Hubble.Core.Analysis.Framework.WordType.None ||
                    wi.WordType == Hubble.Core.Analysis.Framework.WordType.Space)
                {
                    continue;
                }

                Hubble.Core.Entity.WordInfo wordinfo = new Hubble.Core.Entity.WordInfo();
                wordinfo.Word = wi.Word;
                wordinfo.Position = wi.Position;
                wordinfo.Rank = OriginalRank;
                yield return wordinfo;

                string lower = wordinfo.Word.ToLower();

                if (lower != wordinfo.Word)
                {
                    Hubble.Core.Entity.WordInfo lowerWord = wordinfo;
                    lowerWord.Word = lower;
                    lowerWord.Rank = LowerRank;
                    yield return lowerWord;
                }

                string stem = GetStem(lower);

                if (!string.IsNullOrEmpty(stem))
                {
                    if (lower != stem)
                    {
                        Hubble.Core.Entity.WordInfo stemWord = wordinfo;
                        stemWord.Word = stem;
                        stemWord.Rank = StemRank;
                        yield return stemWord;
                    }
                }

            }
        }

        public IEnumerable<Hubble.Core.Entity.WordInfo> TokenizeForSqlClient(string text)
        {
            return Tokenize(text);
        }


        public void Init()
        {
            InitInfinitiveVerbTable();

            Framework.Lexical.Initialize();
        }

        #endregion

        #region INamedExternalReference Members

        public string Name
        {
            get 
            { 
                return "EnglishAnalyzer"; 
            }
        }

        #endregion


    }
}
