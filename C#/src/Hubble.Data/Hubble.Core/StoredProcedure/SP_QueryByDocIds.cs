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
    /// <summary>
    /// SP_QueryByDocIds 'select title, content from table',0,1,2,3
    /// </summary>
    class SP_QueryByDocIds : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_QueryByDocIds";
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo(Right.RightItem.Select);

            if (Parameters.Count < 2)
            {
                throw new ArgumentException("Parameter 1 is select statement before where. Parameter 2 - n is docids.");
            }

            string sql = Parameters[0];

            SFQL.Parse.SFQLParse sfqlParse = new Hubble.Core.SFQL.Parse.SFQLParse();
            sfqlParse.SyntaxAnalyse(sql);


            SFQL.SyntaxAnalysis.Select.Select select = sfqlParse.SFQLSentenceList[0].SyntaxEntity as
                SFQL.SyntaxAnalysis.Select.Select;

            string tableName = select.SelectFroms[0].Name;

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new Data.DataException(string.Format("Invalid table:{0}", tableName));
            }

            List<Data.Field> fields;
            int allFieldsCount;
            sfqlParse.GetSelectFields(select, dbProvider, out fields, out allFieldsCount);

            Query.DocumentResultForSort[] docs = new Hubble.Core.Query.DocumentResultForSort[Parameters.Count - 1];

            for(int i = 1; i < Parameters.Count; i++)
            {
                docs[i - 1] = new Hubble.Core.Query.DocumentResultForSort(int.Parse(Parameters[i]));
            }

            List<Data.Document> docResult = dbProvider.Query(fields, docs);

            Hubble.Framework.Data.DataSet ds = Data.Document.ToDataSet(fields, docResult);
            ds.Tables[0].TableName = "Select_" + select.SelectFroms[0].Alias;
            _QueryResult.DataSet = ds;
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Query by docids. Parameter 1 is select statement before where. Parameter 2 - n is docids.";
            }
        }

        #endregion 
    }
}

