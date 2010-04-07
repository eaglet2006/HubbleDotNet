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
using System.Data;
using Hubble.Core.SFQL.Parse;

namespace Hubble.Core.DBAdapter
{
    public interface IDBAdapter
    {
        Data.Table Table { get; set; }

        string ConnectionString { get; set; }

        void Drop();

        void Create();

        void Truncate();

        void Insert(IList<Data.Document> docs);

        void Delete(IList<int> docIds);

        void Update(Data.Document doc, IList<Query.DocumentResultForSort> docs);

        System.Data.DataTable Query(IList<Data.Field> selectFields, IList<Query.DocumentResultForSort> docs);
        System.Data.DataTable Query(IList<Data.Field> selectFields, IList<Query.DocumentResultForSort> docs, int begin, int end);


        IList<Core.Query.DocumentResultForSort> GetDocumentResultForSortList(int end, string where, string orderby);
        IList<Core.Query.DocumentResultForSort> GetDocumentResultForSortList(int end, Core.Query.DocumentResultForSort[] docResults, string orderby);

        Core.SFQL.Parse.DocumentResultWhereDictionary GetDocumentResults(int end, string where, string orderby);

        int MaxDocId { get; }

        DataSet QuerySql(string sql);

        DataSet GetSchema(string tableName);

        int ExcuteSql(string sql);

        void ConnectionTest();

        string DocIdReplaceField { get; set; }

        Data.DBProvider DBProvider { get; set; }
    }
}
