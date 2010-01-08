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
    public enum OrderByStateFunction
    {
        None = 0,
        Name = 1,
        Order = 2,
    }

    class OrderByState : SyntaxState<OrderByStateFunction>
    {
        public OrderByState(int id, bool isQuit, OrderByStateFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == OrderByStateFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public OrderByState(int id, bool isQuit, OrderByStateFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public OrderByState(int id, bool isQuit) :
            this(id, isQuit, OrderByStateFunction.None, null)
        {

        }

        public OrderByState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, OrderByStateFunction.None, nextStateIdDict)
        {

        }

        public OrderByState(int id) :
            this(id, false, OrderByStateFunction.None, null)
        {

        }

        public OrderByState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, OrderByStateFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, OrderByStateFunction> dfa)
        {
            OrderBy orderby = dfa as OrderBy;

            switch (Func)
            {
                case OrderByStateFunction.Name:
                    orderby.Name = dfa.CurrentToken.Text;
                    orderby.Order = "asc";
                    break;
                case OrderByStateFunction.Order:
                    orderby.Order = dfa.CurrentToken.Text;
                    break;
            }
        }
    }

    public class OrderBy : Syntax<OrderByStateFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<OrderByStateFunction> s0 = AddSyntaxState(new OrderByState(0)); //Start state;
        private static SyntaxState<OrderByStateFunction> squit = AddSyntaxState(new OrderByState(30, true)); //quit state;

        /*****************************************************
         * s0 -- by , -- s1
         * s1 -- Identitier -- s2
         * s2 -- ASC DESC -- s3
         * s2 -- , Eof -- squit
         * s3 -- , Eof -- squit
         * **************************************************/
        private static void InitDFAStates()
        {
            SyntaxState<OrderByStateFunction> s1 = AddSyntaxState(new OrderByState(1)); //Attribute start state;
            SyntaxState<OrderByStateFunction> s2 = AddSyntaxState(new OrderByState(2, false, OrderByStateFunction.Name));
            SyntaxState<OrderByStateFunction> s3 = AddSyntaxState(new OrderByState(3, false, OrderByStateFunction.Order));

            s0.AddNextState(new int[] { (int)SyntaxType.BY, (int)SyntaxType.Comma }, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s2.Id);

            s2.AddNextState(new int[] { (int)SyntaxType.ASC, (int)SyntaxType.DESC}, s3.Id);
            s2.AddNextState(new int[] { (int)SyntaxType.Eof, (int)SyntaxType.Comma, (int)SyntaxType.Semicolon}, squit.Id);

            s3.AddNextState(new int[] { (int)SyntaxType.Eof, (int)SyntaxType.Comma, (int)SyntaxType.Semicolon }, squit.Id);

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

        public string Order;

        #endregion


        public override string ToString()
        {
            return string.Format("{0} {1}", this.Name, this.Order);
        }
    }
}


