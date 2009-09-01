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

namespace Hubble.Core.SFQL.SyntaxAnalysis.Insert
{

    public enum InsertTableNameStateFunction
    {
        None = 0,
        Name = 1,
    }

    class InsertTableNameState : SyntaxState<InsertTableNameStateFunction>
    {

        public InsertTableNameState(int id, bool isQuit, InsertTableNameStateFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == InsertTableNameStateFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public InsertTableNameState(int id, bool isQuit, InsertTableNameStateFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public InsertTableNameState(int id, bool isQuit) :
            this(id, isQuit, InsertTableNameStateFunction.None, null)
        {

        }

        public InsertTableNameState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, InsertTableNameStateFunction.None, nextStateIdDict)
        {

        }

        public InsertTableNameState(int id) :
            this(id, false, InsertTableNameStateFunction.None, null)
        {

        }

        public InsertTableNameState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, InsertTableNameStateFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, InsertTableNameStateFunction> dfa)
        {
            InsertTableName deleteFrom = dfa as InsertTableName;

            switch (Func)
            {
                case InsertTableNameStateFunction.Name:
                    deleteFrom.Name = dfa.CurrentToken.Text;
                    break;
            }
        }

    }

    public class InsertTableName : Syntax<InsertTableNameStateFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<InsertTableNameStateFunction> s0 = AddSyntaxState(new InsertTableNameState(0)); //Start state;
        private static SyntaxState<InsertTableNameStateFunction> squit = AddSyntaxState(new InsertTableNameState(30, true)); //quit state;

        /*****************************************************
         * s0 -- Identitier -- s1
         * s1 -- ( Values -- squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<InsertTableNameStateFunction> s1 = AddSyntaxState(new InsertTableNameState(1, false, InsertTableNameStateFunction.Name));

            s0.AddNextState((int)SyntaxType.Identifer, s1.Id);

            s1.AddNextState(new int[] { (int)SyntaxType.LBracket, (int)SyntaxType.VALUES }, squit.Id);
        }

        public static void Initialize()
        {
            lock (InitLockObj)
            {
                if (!Inited)
                {
                    InitDFAStates();
                    Inited = true;
                }
            }
        }

        protected override void Init()
        {
            Initialize();
        }

        #region public Fields

        public string Name;

        #endregion

    }


}
