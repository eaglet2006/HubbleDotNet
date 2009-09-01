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
    public enum InsertFunction
    {
        None = 0,
        Table = 1,
        Field = 2,
        Value = 3,
    }

    public class InsertState : SyntaxState<InsertFunction>
    {

        public InsertState(int id, bool isQuit, InsertFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == InsertFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public InsertState(int id, bool isQuit, InsertFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public InsertState(int id, bool isQuit) :
            this(id, isQuit, InsertFunction.None, null)
        {

        }

        public InsertState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, InsertFunction.None, nextStateIdDict)
        {

        }

        public InsertState(int id) :
            this(id, false, InsertFunction.None, null)
        {

        }

        public InsertState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, InsertFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, InsertFunction> dfa)
        {
            switch (Func)
            {
                case InsertFunction.Table:
                    ((Insert)dfa).CurrentSyntax = new InsertTableName();
                    break;
                case InsertFunction.Field:
                    ((Insert)dfa).CurrentSyntax = new InsertField();
                    break;
                case InsertFunction.Value:
                    ((Insert)dfa).CurrentSyntax = new InsertValue();
                    break;
            }
        }

    }

    public class Insert : SyntaxContainer<InsertFunction>, SyntaxAnalysis.ITokenInput, SyntaxAnalysis.ISyntaxEntity
    {
        private static SyntaxState<InsertFunction> s0 = AddSyntaxState(new InsertState(0)); //Start state;
        private static SyntaxState<InsertFunction> squit = AddSyntaxState(new InsertState(30, true)); //quit state;

        /******************************************************
         * s0 --Insert-- s1
         * s1 --Into--s2
         * s1 --Identifer--s3
         * s2 --Identifer--s3
         * s3 --(-- s4
         * s4 --,--s4
         * s4 --)--s8
         * s3 --Values -- s5
         * s8 --Values -- s5
         * s5 --(--s6
         * s6 --, --s6
         * s6 --)--s7
         * s7 --EOF ;--squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<InsertFunction> s1 = AddSyntaxState(new InsertState(1)); //
            SyntaxState<InsertFunction> s2 = AddSyntaxState(new InsertState(2)); //
            SyntaxState<InsertFunction> s3 = AddSyntaxState(new InsertState(3, false, InsertFunction.Table)); //
            SyntaxState<InsertFunction> s4 = AddSyntaxState(new InsertState(4, false, InsertFunction.Field)); //
            SyntaxState<InsertFunction> s5 = AddSyntaxState(new InsertState(5)); //
            SyntaxState<InsertFunction> s6 = AddSyntaxState(new InsertState(6, false, InsertFunction.Value)); //
            SyntaxState<InsertFunction> s7 = AddSyntaxState(new InsertState(7)); //
            SyntaxState<InsertFunction> s8 = AddSyntaxState(new InsertState(8)); //

            s0.AddNextState((int)SyntaxType.INSERT, s1.Id);

            s1.AddNextState((int)SyntaxType.INTO, s2.Id);
            s1.AddNextState((int)SyntaxType.Identifer, s3.Id);

            s2.AddNextState((int)SyntaxType.Identifer, s3.Id);

            s3.AddNextState((int)SyntaxType.LBracket, s4.Id);
            s3.AddNextState((int)SyntaxType.VALUES, s5.Id);

            s4.AddNextState((int)SyntaxType.Comma, s4.Id);
            s4.AddNextState((int)SyntaxType.RBracket, s8.Id);

            s8.AddNextState((int)SyntaxType.VALUES, s5.Id);
            s5.AddNextState((int)SyntaxType.LBracket, s6.Id);

            s6.AddNextState((int)SyntaxType.Comma, s6.Id);
            s6.AddNextState((int)SyntaxType.RBracket, s7.Id);

            s7.AddNextState((int)SyntaxType.Eof, squit.Id);
            s7.AddNextState((int)SyntaxType.Semicolon, squit.Id);

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
                if (obj is InsertTableName)
                {
                    InsertTableName = obj as InsertTableName;
                }
                else if (obj is InsertField)
                {
                    Fields.Add(obj as InsertField);
                }
                else if (obj is InsertValue)
                {
                    Values.Add(obj as InsertValue);
                }
            }
        }

        #region public Fields

        public int Begin = 0;
        public int End = -1;

        public InsertTableName InsertTableName = new InsertTableName();
        public List<InsertField> Fields = new List<InsertField>();
        public List<InsertValue> Values = new List<InsertValue>();

        public string TableName
        {
            get
            {
                return InsertTableName.Name;
            }
        }
        #endregion

    }
}
