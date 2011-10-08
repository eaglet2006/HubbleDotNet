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
using System.Linq;
using System.Text;

using Hubble.SQLClient.Linq.Entities;

namespace Hubble.SQLClient.Linq.StoredProcedure
{
    public partial class SPDataContext : DataContext
    {
        /// <summary>
        /// Get TF-IDF from hubble table base on specified field.
        /// Text is segmented.
        /// Text format is like "abc^5^0 news^5^4" or "abc news"
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="fieldName">field name must be tokenized index type</param>
        /// <param name="formattedText">text segmentd</param>
        /// <returns>TF IDF List</returns>
        public HashSet<TFIDF> SP_GetTFIDFWithSegmentedText(string tableName,
            string fieldName, string segmentdText)
        {
            SPExecutor<TFIDF> spExecutor = new SPExecutor<TFIDF>(this);

            return spExecutor.ReturnAsHashSet("exec SP_GetTFIDF {0}, {1}, {2}", tableName,
                fieldName, segmentdText);
        }

        /// <summary>
        /// Get TF-IDF from hubble table base on specified field.
        /// Text is not segmented. It is the original input text.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="fieldName">field name must be tokenized index type</param>
        /// <param name="text">text</param>
        /// <returns>TF IDF List</returns>
        public HashSet<TFIDF> SP_GetTFIDF(string tableName,
            string fieldName, string text)
        {
            SPExecutor<TFIDF> spExecutor = new SPExecutor<TFIDF>(this);

            StringBuilder segmentdText = new StringBuilder();
            foreach (var wordinfo in SP_FieldAnalyze(tableName, fieldName, text))
            {
                segmentdText.AppendFormat("{0}^{1}^{2} ", wordinfo.Word,
                    wordinfo.Rank, wordinfo.Position);
            }

            return SP_GetTFIDFWithSegmentedText(tableName,
                fieldName, segmentdText.ToString().Trim());
        }
    }
}
