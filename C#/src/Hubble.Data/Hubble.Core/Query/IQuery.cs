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
using Hubble.Core.Data;

namespace Hubble.Core.Query
{
    public interface IQuery
    {
        //Input parameters
        string FieldName { get; set;}
        int TabIndex { get; set; }

        string Command { get; }

        DBProvider DBProvider { get; set; }

        IList<Entity.WordInfo> QueryWords { get; set; }

        //Inner parameters
        //need not set by caller
        Index.InvertedIndex InvertedIndex { get; set;}
        //Analysis.IAnalyzer Analyzer { get; set;}

        //output
        Dictionary<long, DocumentResult> Search();
        IEnumerable<DocumentRank> GetRankEnumerable();
        //IList<Entity.WordInfo> GetQueryWords();
        //IList<Entity.WordInfo> GetNextHitWords(out long docId);
    }
}
