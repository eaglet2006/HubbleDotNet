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

    public enum InsertFieldFunction
    {
        None = 0,
        Name = 1,
    }

    class InsertFieldState : SyntaxState<InsertFieldFunction>
    {
        public InsertFieldState(int id, bool isQuit, InsertFieldFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == InsertFieldFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public InsertFieldState(int id, bool isQuit, InsertFieldFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public InsertFieldState(int id, bool isQuit) :
            this(id, isQuit, InsertFieldFunction.None, null)
        {

        }

        public InsertFieldState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, InsertFieldFunction.None, nextStateIdDict)
        {

        }

        public InsertFieldState(int id) :
            this(id, false, InsertFieldFunction.None, null)
        {

        }

        public InsertFieldState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, InsertFieldFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, InsertFieldFunction> dfa)
        {
            InsertField insertField = dfa as InsertField;

            switch (Func)
            {
                case InsertFieldFunction.Name:
                    insertField.Name = dfa.CurrentToken.Text;
                    break;
            }
        }
    }

    public class InsertField : Syntax<InsertFieldFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<InsertFieldFunction> s0 = AddSyntaxState(new InsertFieldState(0)); //Start state;
        private static SyntaxState<InsertFieldFunction> squit = AddSyntaxState(new InsertFieldState(30, true)); //quit state;

        /*****************************************************
         * s0 -- ( , -- s1
         * s1 -- Identifer -- s2
         * s2 -- , ) --squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<InsertFieldFunction> s1 = AddSyntaxState(new InsertFieldState(1)); //
            SyntaxState<InsertFieldFunction> s2 = AddSyntaxState(new InsertFieldState(2, false, InsertFieldFunction.Name));

            s0.AddNextState(new int[] { (int)SyntaxType.LBracket, (int)SyntaxType.Comma }, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s2.Id);

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

        public string Name;

        #endregion

    }


}
