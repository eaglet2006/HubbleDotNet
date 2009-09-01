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

    public enum UpdateTableNameStateFunction
    {
        None = 0,
        Name = 1,
    }

    class UpdateTableNameState : SyntaxState<UpdateTableNameStateFunction>
    {

        public UpdateTableNameState(int id, bool isQuit, UpdateTableNameStateFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == UpdateTableNameStateFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public UpdateTableNameState(int id, bool isQuit, UpdateTableNameStateFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public UpdateTableNameState(int id, bool isQuit) :
            this(id, isQuit, UpdateTableNameStateFunction.None, null)
        {

        }

        public UpdateTableNameState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, UpdateTableNameStateFunction.None, nextStateIdDict)
        {

        }

        public UpdateTableNameState(int id) :
            this(id, false, UpdateTableNameStateFunction.None, null)
        {

        }

        public UpdateTableNameState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, UpdateTableNameStateFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, UpdateTableNameStateFunction> dfa)
        {
            UpdateTableName deleteFrom = dfa as UpdateTableName;

            switch (Func)
            {
                case UpdateTableNameStateFunction.Name:
                    deleteFrom.Name = dfa.CurrentToken.Text;
                    break;
            }
        }

    }

    public class UpdateTableName : Syntax<UpdateTableNameStateFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<UpdateTableNameStateFunction> s0 = AddSyntaxState(new UpdateTableNameState(0)); //Start state;
        private static SyntaxState<UpdateTableNameStateFunction> squit = AddSyntaxState(new UpdateTableNameState(30, true)); //quit state;

        /*****************************************************
         * s0 -- Update , -- s1
         * s1 -- Identitier -- s2
         * s2 -- Set -- squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<UpdateTableNameStateFunction> s1 = AddSyntaxState(new UpdateTableNameState(1));
            SyntaxState<UpdateTableNameStateFunction> s2 = AddSyntaxState(new UpdateTableNameState(2, false, UpdateTableNameStateFunction.Name));

            s0.AddNextState((int)SyntaxType.UPDATE, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s2.Id);

            s2.AddNextState((int)SyntaxType.SET, squit.Id);
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
