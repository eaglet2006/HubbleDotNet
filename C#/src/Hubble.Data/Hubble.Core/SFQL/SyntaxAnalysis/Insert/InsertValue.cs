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
    public enum InsertValueFunction
    {
        None = 0,
        Value = 1,
    }

    class InsertValueState : SyntaxState<InsertValueFunction>
    {
        public InsertValueState(int id, bool isQuit, InsertValueFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == InsertValueFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public InsertValueState(int id, bool isQuit, InsertValueFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public InsertValueState(int id, bool isQuit) :
            this(id, isQuit, InsertValueFunction.None, null)
        {

        }

        public InsertValueState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, InsertValueFunction.None, nextStateIdDict)
        {

        }

        public InsertValueState(int id) :
            this(id, false, InsertValueFunction.None, null)
        {

        }

        public InsertValueState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, InsertValueFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, InsertValueFunction> dfa)
        {
            InsertValue insertField = dfa as InsertValue;

            switch (Func)
            {
                case InsertValueFunction.Value:
                    insertField.Value = dfa.CurrentToken.Text;
                    break;
            }
        }
    }

    public class InsertValue : Syntax<InsertValueFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<InsertValueFunction> s0 = AddSyntaxState(new InsertValueState(0)); //Start state;
        private static SyntaxState<InsertValueFunction> squit = AddSyntaxState(new InsertValueState(30, true)); //quit state;

        /*****************************************************
         * s0 -- ( , -- s1
         * s1 -- String Numeric -- s2
         * s2 -- , ) --squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<InsertValueFunction> s1 = AddSyntaxState(new InsertValueState(1)); //
            SyntaxState<InsertValueFunction> s2 = AddSyntaxState(new InsertValueState(2, false, InsertValueFunction.Value));

            s0.AddNextState(new int[] { (int)SyntaxType.LBracket, (int)SyntaxType.Comma }, s1.Id);

            s1.AddNextState(new int[] { (int)SyntaxType.String, (int)SyntaxType.Numeric }, s2.Id);

            s2.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);

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

        public string Value;

        #endregion

    }

}
