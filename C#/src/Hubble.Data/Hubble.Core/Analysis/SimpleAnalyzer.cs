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

        public IEnumerable<Hubble.Core.Entity.WordInfo> Tokenize(string text)
        {
            foreach (Framework.WordInfo wi in GetInitSegment(text))
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
                if (_Lowercase)
                {
                    wordinfo.Word = wordinfo.Word.ToLower();
                }

                wordinfo.Position = wi.Position;
                wordinfo.Rank = 1;
                yield return wordinfo;
            }
        }

        public void Init()
        {
            Framework.Lexical.Initialize();
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
