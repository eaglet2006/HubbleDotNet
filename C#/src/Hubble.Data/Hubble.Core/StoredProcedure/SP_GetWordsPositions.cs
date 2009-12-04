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

namespace Hubble.Core.StoredProcedure
{
    class SP_GetWordsPositions : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_GetWordsPositions";
            }
        }

        public void Run()
        {
            if (Parameters.Count < 4)
            {
                throw new StoredProcException("First parameter is words. Second is Table name. Third is Field name and after is docid.");
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[1]);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist!", Parameters[1]));
            }

            Hubble.Core.Index.InvertedIndex invertedIndex = dbProvider.GetInvertedIndex(Parameters[2]);

            if (invertedIndex == null)
            {
                throw new StoredProcException(string.Format("Field: {0} isn't tokenized field!", Parameters[2]));
            }

            Dictionary<string, bool> wordDict = new Dictionary<string, bool>();

            foreach (string word in Hubble.Framework.Text.Regx.Split(Parameters[0], @"\s+"))
            {
                if (wordDict.ContainsKey(word))
                {
                    continue;
                }

                wordDict.Add(word, true);
            }

            List<long> docidList = new List<long>();

            for (int i = 3; i < Parameters.Count; i++)
            {
                docidList.Add(long.Parse(Parameters[i]));
            }

            docidList.Sort();

            long[] docids = docidList.ToArray();

            AddColumn("DocId");
            AddColumn("Word");
            AddColumn("Position");

            foreach (string word in wordDict.Keys)
            {
                Hubble.Core.Index.InvertedIndex.WordIndexReader wordIndex = invertedIndex.GetWordIndex(word);

                if (wordIndex == null)
                {
                    continue;
                }

                int j =0;

                for (int i = 0; i < wordIndex.Count; i++)
                {
                    long curDocId = wordIndex[i].DocumentId;

                    //docids[j] does not exist in this word's document list
                    if (curDocId > docids[j])
                    {
                        j++;

                        while (j < docids.Length && wordIndex[i].DocumentId > docids[j])
                        {
                            j++;
                        }

                        if (j >= docids.Length)
                        {
                            break;
                        }
                    }

                    if (curDocId == docids[j])
                    {
                        foreach (int position in wordIndex[i])
                        {
                            NewRow();
                            OutputValue("DocId", curDocId);
                            OutputValue("Word", word);
                            OutputValue("Position", position);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
