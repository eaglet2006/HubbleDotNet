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
using System.Diagnostics;

using Hubble.Core.Data;
using Hubble.Core.Entity;

namespace Hubble.Core.StoredProcedure
{
    class SP_Test : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_Test";
            }
        }

        unsafe private void TestFillPayloadRank(string tableName)
        {
            OutputMessage("TestGetDocIdReplaceFieldValue");

            AddColumn("Times");
            AddColumn("Elapse(ms)");
            AddColumn("ElapseOneTime(ms)");

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0], false);

            if (dbProvider == null)
            {
                throw new DataException(string.Format("Table name {0} does not exist!", Parameters[0]));
            }

            Random rand = new Random();
            int count = 1000000;
            int lastDocId = dbProvider.LastDocId;
            OriginalDocumentPositionList[] payloads = new OriginalDocumentPositionList[count];

            for (int i = 0; i < payloads.Length; i++)
            {
                payloads[i] = new OriginalDocumentPositionList(i * 10);
                payloads[i].CountAndWordCount = 1;
            }

            payloads[0].DocumentId = 8 * payloads.Length;

            Data.Field rankField = dbProvider.GetField("Rank");
            int tab = rankField.TabIndex;

            Stopwatch sw = new Stopwatch();
            //int docid = rand.Next(lastDocId);
            int data;
            sw.Start();

            for (int j = 0; j < count / payloads.Length; j++)
            {
                dbProvider.FillPayloadRank(tab, count, payloads);
            }
            sw.Stop();

            OutputValue("Times", count);
            OutputValue("Elapse(ms)", sw.ElapsedMilliseconds);
            OutputValue("ElapseOneTime(ms)", (double)sw.ElapsedMilliseconds / count);
        }

        unsafe private void TestGetDocIdReplaceFieldValue(string tableName)
        {
            OutputMessage("TestGetDocIdReplaceFieldValue");

            AddColumn("Times");
            AddColumn("Elapse(ms)");
            AddColumn("ElapseOneTime(ms)");

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0], false);

            if (dbProvider == null)
            {
                throw new DataException(string.Format("Table name {0} does not exist!", Parameters[0]));
            }

            Random rand = new Random();
            int count = 1000000;
            int lastDocId = dbProvider.LastDocId;

            Stopwatch sw = new Stopwatch();
            //int docid = rand.Next(lastDocId);
            //int data;
            sw.Start();

            for (int i = 0; i < count; i++)
            {
                int* pData = dbProvider.TestGetPayloadData(i * 10);
                //if (pData != null)
                //{
                //    data = *(pData + 1);
                //}
            }

            sw.Stop();

            OutputValue("Times", count);
            OutputValue("Elapse(ms)", sw.ElapsedMilliseconds);
            OutputValue("ElapseOneTime(ms)", (double)sw.ElapsedMilliseconds / count);
        }


        public void Run()
        {

            if (Parameters.Count != 1)
            {
                throw new ArgumentException("the number of parameters must be 1. Parameter 1 is table name.");
            }

            string tableName = Parameters[0];
            //System.Threading.Thread.Sleep(400000);
            //TestGetDocIdReplaceFieldValue(tableName);
            //TestFillPayloadRank(tableName);

        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "This store procedure is used to test. Parameter 1 is table name.";
            }
        }

        #endregion
    }
}
