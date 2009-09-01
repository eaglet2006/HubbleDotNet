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

namespace Hubble.Core.SFQL.SyntaxAnalysis.Delete
{
    public enum DeleteFunction
    {
        None = 0,
        From = 1,
        Where = 2,
    }

    public class DeleteState : SyntaxState<DeleteFunction>
    {
               
        public DeleteState(int id, bool isQuit, DeleteFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == DeleteFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public DeleteState(int id, bool isQuit, DeleteFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public DeleteState(int id, bool isQuit) :
            this(id, isQuit, DeleteFunction.None, null)
        {

        }

        public DeleteState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, DeleteFunction.None, nextStateIdDict)
        {

        }

        public DeleteState(int id) :
            this(id, false, DeleteFunction.None, null)
        {

        }

        public DeleteState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, DeleteFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, DeleteFunction> dfa)
        {
            switch (Func)
            {
                case DeleteFunction.From:
                    ((Delete)dfa).CurrentSyntax = new DeleteFrom();
                    break;
                case DeleteFunction.Where:
                    ((Delete)dfa).CurrentSyntax = new Where();
                    break;
            }
        }

    }

    public class Delete : SyntaxContainer<DeleteFunction>, SyntaxAnalysis.ITokenInput, SyntaxAnalysis.ISyntaxEntity
    {
        private static SyntaxState<DeleteFunction> s0 = AddSyntaxState(new DeleteState(0)); //Start state;
        private static SyntaxState<DeleteFunction> squit = AddSyntaxState(new DeleteState(30, true)); //quit state;

        /******************************************************
         * s0 --Delete-- s1
         * s1 --Eof--squit
         * s1 --Where-- s2
         * s2 --Eof ;--squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<DeleteFunction> s1 = AddSyntaxState(new DeleteState(1, false, DeleteFunction.From)); //Select fields start state;
            SyntaxState<DeleteFunction> s2 = AddSyntaxState(new DeleteState(2, false, DeleteFunction.Where)); //Select from start state;

            s0.AddNextState((int)SyntaxType.DELETE, s1.Id);

            s1.AddNextState((int)SyntaxType.Eof, squit.Id);
            s1.AddNextState((int)SyntaxType.WHERE, s2.Id);

            s2.AddNextState((int)SyntaxType.Eof, squit.Id);
            s2.AddNextState((int)SyntaxType.Semicolon, squit.Id);

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
                if (obj is DeleteFrom)
                {
                    DeleteFrom = obj as DeleteFrom;
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

        public DeleteFrom DeleteFrom = new DeleteFrom();
        public Where Where;

        public string TableName
        {
            get
            {
                return DeleteFrom.Name;
            }
        }
        #endregion

    }

}
