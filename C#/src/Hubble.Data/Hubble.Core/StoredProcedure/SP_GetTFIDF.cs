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
using Hubble.Core.SFQL.Parse;
using Hubble.Core.Entity;
using Hubble.Core.Query;
using Hubble.Core.Index;

namespace Hubble.Core.StoredProcedure
{
    public class SP_GetTFIDF : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_GetTFIDF";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 3)
            {
                throw new StoredProcException("First parameter is table name, second parameter is field name, third parameter is words. SP_GetIDF 'tablename', 'fieldname', 'abc news'");
            }

            string tableName = Parameters[0];
            string fieldName = Parameters[1];

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist!", tableName));
            }

            Hubble.Core.Index.InvertedIndex invertedIndex = dbProvider.GetInvertedIndex(fieldName);

            if (invertedIndex == null)
            {
                throw new StoredProcException(string.Format("Field name {0} does not exist or is not the tokenized index field!", fieldName));
            }

            string queryStr = Parameters[2];
            List<WordInfo> wordInfos = ParseWhere.GetWordInfoList(queryStr);
            Dictionary<string, WordIndexForQuery> wordIndexDict = new Dictionary<string, WordIndexForQuery>();

            foreach (Hubble.Core.Entity.WordInfo wordInfo in wordInfos)
            {
                WordIndexForQuery wifq;

                if (!wordIndexDict.TryGetValue(wordInfo.Word, out wifq))
                {

                    //Hubble.Core.Index.WordIndexReader wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, CanLoadPartOfDocs); //Get whole index

                    Hubble.Core.Index.WordIndexReader wordIndex = invertedIndex.GetWordIndex(wordInfo.Word, false, true); //Only get step doc index

                    if (wordIndex == null)
                    {
                        wordIndexDict.Add(wordInfo.Word, null);
                        continue;
                    }

                    wifq = new WordIndexForQuery(wordIndex,
                        invertedIndex.DocumentCount, wordInfo.Rank, 1);
                    wifq.QueryCount = 1;
                    wifq.FirstPosition = wordInfo.Position;
                    wordIndexDict.Add(wordInfo.Word, wifq);
                }
                else
                {
                    wifq.WordRank += wordInfo.Rank;
                    wifq.QueryCount++;
                }

                //wordIndexList[wordIndexList.Count - 1].Rank += wordInfo.Rank;
            }

            AddColumn("Word");
            AddColumn("TF");
            AddColumn("IDF");
            AddColumn("T_D");
            AddColumn("TotalDoucments");
            AddColumn("TF_IDF");

            int totalDocuments = invertedIndex.DocumentCount;

            foreach (string word in wordIndexDict.Keys)
            {
                NewRow();

                WordIndexForQuery wifq = wordIndexDict[word];
                OutputValue("Word", word);

                if (wifq == null)
                {
                    OutputValue("TF", 0);
                    OutputValue("T_D", 0);
                    OutputValue("TotalDoucments", 0);
                    OutputValue("IDF", 0);
                    OutputValue("TF_IDF", 0);
                }
                else
                {
                    double idf = Math.Log((double) totalDocuments / (double) wifq.RelTotalCount);

                    OutputValue("TF", wifq.QueryCount);
                    OutputValue("T_D", wifq.RelTotalCount);
                    OutputValue("TotalDoucments", totalDocuments);
                    OutputValue("IDF", idf);
                    OutputValue("TF_IDF", wifq.QueryCount * idf);
                }
            }

        }

        #endregion


        #region IHelper Members

        public string Help
        {
            get 
            {
                return "Get inverse document frequency. First parameter is table name, second parameter is field name, third parameter is words. SP_GetIDF 'tablename', 'fieldname', 'abc news'"; 
            }
        }

        #endregion
    }
}
