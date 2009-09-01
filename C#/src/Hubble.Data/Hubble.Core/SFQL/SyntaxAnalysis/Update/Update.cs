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

namespace Hubble.Core.SFQL.SyntaxAnalysis.Update
{
    public enum UpdateFunction
    {
        None = 0,
        Table = 1,
        Field = 2,
        Where = 3,
    }

    public class UpdateState : SyntaxState<UpdateFunction>
    {

        public UpdateState(int id, bool isQuit, UpdateFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == UpdateFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public UpdateState(int id, bool isQuit, UpdateFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public UpdateState(int id, bool isQuit) :
            this(id, isQuit, UpdateFunction.None, null)
        {

        }

        public UpdateState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, UpdateFunction.None, nextStateIdDict)
        {

        }

        public UpdateState(int id) :
            this(id, false, UpdateFunction.None, null)
        {

        }

        public UpdateState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, UpdateFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, UpdateFunction> dfa)
        {
            switch (Func)
            {
                case UpdateFunction.Table:
                    ((Update)dfa).CurrentSyntax = new UpdateTableName();
                    break;
                case UpdateFunction.Field:
                    ((Update)dfa).CurrentSyntax = new UpdateField();
                    break;
                case UpdateFunction.Where:
                    ((Update)dfa).CurrentSyntax = new Where();
                    break;
            }
        }

    }

    public class Update : SyntaxContainer<UpdateFunction>, SyntaxAnalysis.ITokenInput, SyntaxAnalysis.ISyntaxEntity
    {
        private static SyntaxState<UpdateFunction> s0 = AddSyntaxState(new UpdateState(0)); //Start state;
        private static SyntaxState<UpdateFunction> squit = AddSyntaxState(new UpdateState(30, true)); //quit state;

        /******************************************************
         * s0 --Update-- s1
         * s1 --Set--s2
         * s2 --,--s2
         * s2 --Where-- s3
         * s2 --Eof ;--squit
         * s3 --Eof ;--squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<UpdateFunction> s1 = AddSyntaxState(new UpdateState(1, false, UpdateFunction.Table)); //
            SyntaxState<UpdateFunction> s2 = AddSyntaxState(new UpdateState(2, false, UpdateFunction.Field)); //
            SyntaxState<UpdateFunction> s3 = AddSyntaxState(new UpdateState(3, false, UpdateFunction.Where)); //

            s0.AddNextState((int)SyntaxType.UPDATE, s1.Id);

            s1.AddNextState((int)SyntaxType.SET, s2.Id);

            s2.AddNextState((int)SyntaxType.Comma, s2.Id);
            s2.AddNextState((int)SyntaxType.WHERE, s3.Id);
            s2.AddNextState((int)SyntaxType.Eof, squit.Id);
            s2.AddNextState((int)SyntaxType.Semicolon, squit.Id);

            s3.AddNextState((int)SyntaxType.Eof, squit.Id);
            s3.AddNextState((int)SyntaxType.Semicolon, squit.Id);

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
                if (obj is UpdateTableName)
                {
                    UpdateTableName = obj as UpdateTableName;
                }
                else if (obj is Where)
                {
                    Where = obj as Where;
                }
                else if(obj is UpdateField)
                {
                    Fields.Add(obj as UpdateField);
                }
            }
        }

        #region public Fields

        public int Begin = 0;
        public int End = -1;

        public UpdateTableName UpdateTableName = new UpdateTableName();
        public List<UpdateField> Fields = new List<UpdateField>();
        public Where Where;

        public string TableName
        {
            get
            {
                return UpdateTableName.Name;
            }
        }
        #endregion

    }
}
