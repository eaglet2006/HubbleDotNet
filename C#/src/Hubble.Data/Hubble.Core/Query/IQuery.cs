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
using Hubble.Core.SFQL.Parse;

namespace Hubble.Core.Query
{
    public class QueryParameter
    {
        /// <summary>
        /// Between begin to end
        /// </summary>
        public int End = -1;

        /// <summary>
        /// Order by fields
        /// </summary>
        public string OrderBy = null;

        /// <summary>
        /// Need do group by
        /// </summary>
        public bool NeedGroupBy = false;

        /// <summary>
        /// Need do distinct
        /// </summary>
        public bool NeedDistinct = false;

        private int _FieldRank = 1;

        /// <summary>
        /// Rank of this field
        /// </summary>
        public int FieldRank
        {
            get
            {
                return _FieldRank;
            }
            set
            {
                _FieldRank = value;
                if (_FieldRank <= 0)
                {
                    _FieldRank = 1;
                }
            }
        }

        /// <summary>
        /// If this query need output not match result set it to true.
        /// </summary>
        public bool Not = false;

        /// <summary>
        /// Can load part of docs for file that configure by 
        /// max doc count
        /// </summary>
        public bool CanLoadPartOfDocs = false;

        /// <summary>
        /// No and expression
        /// </summary>
        public bool NoAndExpression = false;

    }

    public interface IQuery
    {
        //Input parameters
        string FieldName { get; set;}

        int TabIndex { get; set; }

        string Command { get; }

        QueryParameter QueryParameter { get; }
        
        DBProvider DBProvider { get; set; }

        IList<Entity.WordInfo> QueryWords { get; set; }

        //Inner parameters
        //need not set by caller
        Index.InvertedIndex InvertedIndex { get; set;}
        //Analysis.IAnalyzer Analyzer { get; set;}

        /// <summary>
        /// The dictionary in up and condition. 
        /// </summary>
        Core.SFQL.Parse.DocumentResultWhereDictionary UpDict { get; set; }

        /// <summary>
        /// Not in dictionary.
        /// For Attribute(NotIn)
        /// </summary>
        Dictionary<int, int> NotInDict { get; set; }

        //output
        Core.SFQL.Parse.DocumentResultWhereDictionary Search();
    }
}
