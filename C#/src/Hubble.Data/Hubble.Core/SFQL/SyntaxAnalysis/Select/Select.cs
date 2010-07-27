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
    public enum SelectFunction
    {
        None = 0,
        SelectField = 1,
        From = 2,
        Where = 3,
        GroupBy = 4,
        OrderBy = 5,
    }

    class SelectState : SyntaxState<SelectFunction>
    {
        public SelectState(int id, bool isQuit, SelectFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == SelectFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public SelectState(int id, bool isQuit, SelectFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public SelectState(int id, bool isQuit) :
            this(id, isQuit, SelectFunction.None, null)
        {

        }

        public SelectState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, SelectFunction.None, nextStateIdDict)
        {

        }

        public SelectState(int id) :
            this(id, false, SelectFunction.None, null)
        {

        }

        public SelectState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, SelectFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, SelectFunction> dfa)
        {
            switch (Func)
            {
                case SelectFunction.SelectField:
                    ((Select)dfa).CurrentSyntax = new SelectField();
                    break;
                case SelectFunction.From:
                    ((Select)dfa).CurrentSyntax = new SelectFrom();
                    break;
                case SelectFunction.OrderBy:
                    ((Select)dfa).CurrentSyntax = new OrderBy();
                    break;
                case SelectFunction.Where:
                    ((Select)dfa).CurrentSyntax = new Where();
                    break;

            }
        }
    }

    public class Select : SyntaxContainer<SelectFunction>, SyntaxAnalysis.ITokenInput, SyntaxAnalysis.ISyntaxEntity
    {
        private static SyntaxState<SelectFunction> s0 = AddSyntaxState(new SelectState(0)); //Start state;
        private static SyntaxState<SelectFunction> squit = AddSyntaxState(new SelectState(30, true)); //quit state;

        /******************************************************
         * s0 --Select-- s1
         * s1 --,--s1
         * s1 --From--s2
         * s2 --Eof ;--squit
         * s2 --Order-- s3
         * s2 --Where-- s5
         * s3 --By-- s4
         * s4 --,-- s4
         * s4 --Eof ;--squit
         * s5 --Order-- s3
         * s5 --Eof ;--squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<SelectFunction> s1 = AddSyntaxState(new SelectState(1, false, SelectFunction.SelectField)); //Select fields start state;
            SyntaxState<SelectFunction> s2 = AddSyntaxState(new SelectState(2, false, SelectFunction.From)); //Select from start state;
            SyntaxState<SelectFunction> s3 = AddSyntaxState(new SelectState(3)); //Select from start state;
            SyntaxState<SelectFunction> s4 = AddSyntaxState(new SelectState(4, false, SelectFunction.OrderBy)); //Select from start state;
            SyntaxState<SelectFunction> s5 = AddSyntaxState(new SelectState(5, false, SelectFunction.Where)); //Where start state;

            s0.AddNextState((int)SyntaxType.SELECT, s1.Id);

            s1.AddNextState((int)SyntaxType.Comma, s1.Id);
            s1.AddNextState((int)SyntaxType.FROM, s2.Id);

            s2.AddNextState((int)SyntaxType.Eof, squit.Id);
            s2.AddNextState((int)SyntaxType.Semicolon, squit.Id);
            s2.AddNextState((int)SyntaxType.ORDER, s3.Id);
            s2.AddNextState((int)SyntaxType.WHERE, s5.Id);

            s3.AddNextState((int)SyntaxType.BY, s4.Id);

            s4.AddNextState((int)SyntaxType.Comma, s4.Id);
            s4.AddNextState((int)SyntaxType.Eof, squit.Id);
            s4.AddNextState((int)SyntaxType.Semicolon, squit.Id);

            s5.AddNextState((int)SyntaxType.ORDER, s3.Id);
            s5.AddNextState((int)SyntaxType.Eof, squit.Id);
            s5.AddNextState((int)SyntaxType.Semicolon, squit.Id);

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

        override public void Finish()
        {
            foreach (object obj in SyntaxList)
            {
                if (obj is SelectField)
                {
                    SelectField selectField = obj as SelectField;
                    
                    if (selectField.BetweenRecord)
                    {
                        this.Begin = selectField.Begin;
                        this.End = selectField.End;
                    }

                    SelectFields.Add(selectField);
                }
                else if (obj is SelectFrom)
                {
                    SelectFroms.Add(obj as SelectFrom);
                }
                else if (obj is OrderBy)
                {
                    OrderBys.Add(obj as OrderBy);
                }
                else if (obj is Where)
                {
                    Where = obj as Where;
                }
            }
        }

        #region public Fields

        public int Begin = 0;
        public int End = -1;

        public List<SelectField> SelectFields = new List<SelectField>();
        public List<SelectFrom> SelectFroms = new List<SelectFrom>();
        public List<OrderBy> OrderBys = new List<OrderBy>();
        public Where Where;
        public TSFQLSentence Sentence = null;

        #endregion

    }
}
