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
    class SP_TestAnalyzer : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_TestAnalyzer";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 2)
            {
                throw new ArgumentException("the number of parameters must be 2. Parameter 1 is Analyzer name, Parameter 2 is a text for test.");
            }

            Analysis.IAnalyzer analyzer = Data.DBProvider.GetAnalyzer(Parameters[0]);

            if (analyzer == null)
            {
                throw new Data.DataException(string.Format("Can't find analyzer name : {0}", Parameters[0]));
            }

            AddColumn("Word");
            AddColumn("Position");
            AddColumn("Rank");

            foreach (Entity.WordInfo word in analyzer.Tokenize(Parameters[1]))
            {
                NewRow();
                OutputValue("Word", word.Word);
                OutputValue("Position", word.Position.ToString());
                OutputValue("Rank", word.Rank.ToString());
            }
        }

        #endregion
    }
}
