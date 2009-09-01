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

namespace Hubble.Core.SFQL.SyntaxAnalysis.Select
{
    public enum SelectFieldFunction
    {
        None = 0,
        Name = 1,
        Alias= 2,
        Begin = 3,
        End = 4,
    }

    class SelectFieldState : SyntaxState<SelectFieldFunction>
    {
        public SelectFieldState(int id, bool isQuit, SelectFieldFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == SelectFieldFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public SelectFieldState(int id, bool isQuit, SelectFieldFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public SelectFieldState(int id, bool isQuit) :
            this(id, isQuit, SelectFieldFunction.None, null)
        {

        }

        public SelectFieldState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, SelectFieldFunction.None, nextStateIdDict)
        {

        }

        public SelectFieldState(int id) :
            this(id, false, SelectFieldFunction.None, null)
        {

        }

        public SelectFieldState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, SelectFieldFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, SelectFieldFunction> dfa)
        {
            SelectField selectField = dfa as SelectField;

            switch (Func)
            {
                case SelectFieldFunction.Name:
                    selectField.Name = dfa.CurrentToken.Text;
                    selectField.Alias = selectField.Name;
                    break;
                case SelectFieldFunction.Alias:
                    selectField.Alias = dfa.CurrentToken.Text;
                    break;
                case SelectFieldFunction.End:
                    if (dfa.CurrentToken.SyntaxType == SyntaxType.Numeric)
                    {
                        selectField.BetweenRecord = true;
                        selectField.End = int.Parse(dfa.CurrentToken.Text);
                    }
                    break;
                case SelectFieldFunction.Begin:
                    selectField.BetweenRecord = true;
                    selectField.Begin = int.Parse(dfa.CurrentToken.Text);
                    break;
            }
        }
    }

    public class SelectField : Syntax<SelectFieldFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<SelectFieldFunction> s0 = AddSyntaxState(new SelectFieldState(0)); //Start state;
        private static SyntaxState<SelectFieldFunction> squit = AddSyntaxState(new SelectFieldState(30, true)); //quit state;

        /*****************************************************
         * s0 -- Select , -- s1
         * s1 -- Identitier * -- s2
         * s1 -- Top -- s5
         * s1 -- Between --s6 
         * s2 -- AS -- s3
         * s2 -- Identitier -- s4
         * s2 -- , From -- squit
         * s3 -- Identitier, String -- s4
         * s4 -- , From -- squit
         * s5 -- Numeric -- s1
         * s6 -- Numeric -- s7
         * s7 -- To -- s5
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<SelectFieldFunction> s1 = AddSyntaxState(new SelectFieldState(1, false, SelectFieldFunction.End)); //Attribute start state;
            SyntaxState<SelectFieldFunction> s2 = AddSyntaxState(new SelectFieldState(2, false, SelectFieldFunction.Name));
            SyntaxState<SelectFieldFunction> s3 = AddSyntaxState(new SelectFieldState(3));
            SyntaxState<SelectFieldFunction> s4 = AddSyntaxState(new SelectFieldState(4, false, SelectFieldFunction.Alias));
            SyntaxState<SelectFieldFunction> s5 = AddSyntaxState(new SelectFieldState(5, false));
            SyntaxState<SelectFieldFunction> s6 = AddSyntaxState(new SelectFieldState(6, false));
            SyntaxState<SelectFieldFunction> s7 = AddSyntaxState(new SelectFieldState(7, false, SelectFieldFunction.Begin));

            s0.AddNextState(new int[]{(int)SyntaxType.SELECT, (int)SyntaxType.Comma}, s1.Id);

            s1.AddNextState(new int[] {(int)SyntaxType.Identifer, (int)SyntaxType.Multiply}, s2.Id);
            s1.AddNextState((int)SyntaxType.TOP, s5.Id);
            s1.AddNextState((int)SyntaxType.BETWEEN, s6.Id);

            s2.AddNextState((int)SyntaxType.AS, s3.Id);
            s2.AddNextState((int)SyntaxType.Identifer, s4.Id);
            s2.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.FROM }, squit.Id);

            s3.AddNextState(new int[] {(int)SyntaxType.Identifer, (int)SyntaxType.String}, s4.Id);

            s4.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.FROM }, squit.Id);

            s5.AddNextState((int)SyntaxType.Numeric, s1.Id);

            s6.AddNextState((int)SyntaxType.Numeric, s7.Id);

            s7.AddNextState((int)SyntaxType.TO, s5.Id);

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

        public bool BetweenRecord = false;
        public int Begin = 0;
        public int End = -1;

        public string Name;

        public string Alias;

        #endregion

    }
}
