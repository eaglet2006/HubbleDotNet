/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Framework.DataStructure;

namespace Hubble.Core.Analysis
{
    public class SimpleAnalyzer : IAnalyzer, Data.INamedExternalReference
    {
        private bool _Lowercase;
        List<Hubble.Core.Entity.WordInfo> _Tokenes = null;
        static private object _InitLockObj = new object();
        static private bool _Inited = false;

        static private Int16[] _CharSetTable;
        static private Int16[] _NGramChar;


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

        public SimpleAnalyzer()
        {
            _Lowercase = true;
        }

        public SimpleAnalyzer(bool lowercase)
        {
            _Lowercase = lowercase;
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
            lock (_InitLockObj)
            {
                if (!_Inited)
                {
                    Init();
                    _Inited = true;
                }
            }

            int start = -1;
            _Tokenes = new List<Hubble.Core.Entity.WordInfo>();
            bool needToLower = false;

            for(int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (_NGramChar[c] == Int16.MaxValue)
                {
                    Hubble.Core.Entity.WordInfo wordinfo = new Hubble.Core.Entity.WordInfo();
                    wordinfo.Word = c.ToString();
                    wordinfo.Rank = 1;
                    wordinfo.Position = i;
                    start = -1;
                    _Tokenes.Add(wordinfo);
                }
                else if (_CharSetTable[c] == Int16.MaxValue)
                {
                    if (start < 0)
                    {
                        start = i;
                    }
                }
                else if (_CharSetTable[c] == 0)
                {
                    if (start >= 0)
                    {
                        Hubble.Core.Entity.WordInfo wordinfo = new Hubble.Core.Entity.WordInfo();
                        wordinfo.Word = text.Substring(start, i - start);
                     
                        if (needToLower)
                        {
                            wordinfo.Word = wordinfo.Word.ToLower();
                        }

                        wordinfo.Rank = 1;
                        wordinfo.Position = start;
                        start = -1;
                        needToLower = false;
                        _Tokenes.Add(wordinfo);
                    }
                }
                else
                {
                    if (start < 0)
                    {
                        start = i;
                    }

                    needToLower = true;
                }
            }

            if (start >= 0)
            {
                Hubble.Core.Entity.WordInfo wordinfo = new Hubble.Core.Entity.WordInfo();
                wordinfo.Word = text.Substring(start, text.Length - start);
                
                if (needToLower)
                {
                    wordinfo.Word = wordinfo.Word.ToLower();
                }

                wordinfo.Rank = 1;
                wordinfo.Position = start;
                start = -1;
                needToLower = false;
                _Tokenes.Add(wordinfo);
            }

            return _Tokenes;

            //_Tokenes = GetInitSegment(text);

            //foreach (Framework.WordInfo wi in _Tokenes)
            //{
            //    if (wi == null)
            //    {
            //        continue;
            //    }

            //    if (wi.Word == null)
            //    {
            //        continue;
            //    }

            //    if (wi.WordType == Hubble.Core.Analysis.Framework.WordType.None ||
            //        wi.WordType == Hubble.Core.Analysis.Framework.WordType.Space)
            //    {
            //        continue;
            //    }

            //    Hubble.Core.Entity.WordInfo wordinfo = new Hubble.Core.Entity.WordInfo();
            //    wordinfo.Word = wi.Word;
            //    if (_Lowercase)
            //    {
            //        wordinfo.Word = wordinfo.Word.ToLower();
            //    }

            //    wordinfo.Position = wi.Position;
            //    wordinfo.Rank = 1;
            //    yield return wordinfo;
            //}
        }

        public IEnumerable<Hubble.Core.Entity.WordInfo> TokenizeForSqlClient(string text)
        {
            return Tokenize(text);
        }


        public void Init()
        {
            _CharSetTable = new Int16[65536];
            _NGramChar = new Int16[65536];

            for (int i = 0; i < _CharSetTable.Length; i++)
            {
                if ((i >= 'a' && i <= 'z') ||
                    (i >= '0' && i <= '9') ||
                    (i >= '_' && i <= '_') ||
                    (i >= 0x410 && i <= 0x42F))
                {
                    _CharSetTable[i] = Int16.MaxValue;
                }
                else if (i >= 'A' && i <= 'Z')
                {
                    _CharSetTable[i] = 'A' - 'a'; // 'A' to 'a'
                }
                else if (i >= 0x430 && i <= 0x44F)
                {
                    _CharSetTable[i] = 0x430 - 0x410; // 'A' to 'a' lartin char
                }
            }

            for (int i = 0; i < _NGramChar.Length; i++)
            {
                if (i >= 0x3000 && i <= 0x9FFF)
                {
                    _NGramChar[i] = Int16.MaxValue;
                }
            }

            //Framework.Lexical.Initialize();
        }

        #endregion

        #region INamedExternalReference Members

        public string Name
        {
            get 
            { 
                return "SimpleAnalyzer"; 
            }
        }

        #endregion
    }
}
